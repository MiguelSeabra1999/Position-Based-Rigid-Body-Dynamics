using System.Dynamic;
using System;
using System.Security.Cryptography;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Threading;
using System.Threading.Tasks;

public class CollisionEngine
{
    public bool selfCollisions = true;
    public double k = 2;
    public PBDCollider[] allColliders;
    public PBDCollider[] excludedColliders;

    private List<PBDCollision> collisions = new List<PBDCollision>();
    private NonPenetrationConstraint nonPenetrationConstraint = new NonPenetrationConstraint();
    private StaticFrictionConstraint staticFrictionConstraint = new StaticFrictionConstraint();
    public int colLen = 0;

    private BVH_node root;

    private List<BVH_node> nodes = new List<BVH_node>();
    private int nodeCount = 0;
    private  PBDCollision  col = new PBDCollision();
    public int count = 0;
    public int colCount = 0;
    bool[,] checkedCols;


    public List<(int, int)> collisionPairs = new List<(int, int)>();
    ThreadDispatcher threadDispatcher = new ThreadDispatcher();
    private Mutex colListMutex = new Mutex();

    private  PBDCollision[]  cols = new PBDCollision[ThreadDispatcher.NTHREADS];
    private NonPenetrationConstraint[] nonPenetrationConstraints = new NonPenetrationConstraint[ThreadDispatcher.NTHREADS];
    private StaticFrictionConstraint[] staticFrictionConstraints = new StaticFrictionConstraint[ThreadDispatcher.NTHREADS];
    private List<CountdownEvent> countdownEvents = new List<CountdownEvent>();
    private int nCountdown = 0;

    public CollisionEngine()
    {
        for (int i = 0; i < cols.Length; i++)
        {
            cols[i] = new PBDCollision();
            nonPenetrationConstraints[i] = new NonPenetrationConstraint();
            staticFrictionConstraints[i] = new StaticFrictionConstraint();
        }
    }

    public BVH_node GetNewNode(int[] colliders)
    {
        if (nodes.Count <= nodeCount)
            nodes.Add(new BVH_node(this));

        nodes[nodeCount].SetNewValues(colliders);

        nodeCount++;
        return nodes[nodeCount - 1];
    }

    public BVH_node GetNewNode(int n)
    {
        if (nodes.Count <= nodeCount)
            nodes.Add(new BVH_node(this));

        nodes[nodeCount].SetNewValues(n);

        nodeCount++;
        return nodes[nodeCount - 1];
    }

    public BVH_node GetNewNode()
    {
        lock (nodes)
        {
            if (nodes.Count <= nodeCount)
                nodes.Add(new BVH_node(this));

            nodeCount++;
            nodes[nodeCount - 1].nColliders = 0;
            return nodes[nodeCount - 1];
        }
    }

    private void ClearNodes()
    {
        nodeCount = 0;
    }

    private CountdownEvent GetNewCountdownEvent()
    {
        if (nCountdown >= countdownEvents.Count)
            countdownEvents.Add(new CountdownEvent(2));
        CountdownEvent result = countdownEvents[nCountdown];
        result.Reset();
        nCountdown++;
        return result;
    }

    private void ClearCountdownEvents()
    {
        nCountdown = 0;
    }

    private void AddCollisionToList(PBDCollision collision)
    {
        if (collisions.Count <= colLen)
            collisions.Add(new PBDCollision());

        collisions[colLen].CopyInto(collision);
        colLen++;
    }

    public void LoopCollisions(Action<PBDCollision> doWork)
    {
        for (int i = 0; i < colLen; i++)
            doWork(collisions[i]);
    }

    public void Clear()
    {
        ClearNodes();
        PBDFrictionCollision.ResetList();

        colLen = 0;
        for (int i = 0; i < allColliders.Length; i++)
        {
            allColliders[i].particle.wasInCollision = false;
            for (int j = 0; j < allColliders.Length; j++)
                checkedCols[i, j] = false;
        }
    }

    public void StoreColliders(List<PBDCollider> cols)
    {
        List<PBDCollider> excluded = new List<PBDCollider>();

        int count = cols.Count;
        for (int i = 0; i < count; i++)
            if (cols[i].GetColliderType() == PBDColliderType.planeY || cols[i].exclude)
                excluded.Add(cols[i]);

        for (int i = 0; i < excluded.Count; i++)
            cols.Remove(excluded[i]);

        allColliders = cols.ToArray();
        checkedCols = new bool[allColliders.Length, allColliders.Length];
        excludedColliders = excluded.ToArray();

        BVH_node.maxColliders = allColliders.Length;
    }

    public void BroadCollisionDetection(double deltaTime)
    {
        if (allColliders.Length == 0)
            return;
        UpdateAABBS(0, allColliders.Length, deltaTime, 0);
        root = GetNewNode(allColliders.Length);

        collisionPairs.Clear();
        BroadCollisionDetectionRecursive(root, 0);
    }

    public void ParallelBroadCollisionDetection(double deltaTime)
    {
        if (allColliders.Length == 0)
            return;

        threadDispatcher.DistributeLoad((from, to, h, i) =>
        {UpdateAABBS(from, to, h, i);}, allColliders.Length, deltaTime);

        root = GetNewNode(allColliders.Length);

        collisionPairs.Clear();
        ClearCountdownEvents();

        ParallelBroadCollisionDetectionRecursive(root, 0);
    }

    public void BroadCollisionDetectionRecursive(BVH_node node, int depth)
    {
        node.Branch();
        if (node.firstSon != null && node.firstSon.IsValid())
            BroadCollisionDetectionRecursive(node.firstSon, depth + 1);
        if (node.secondSon != null && node.secondSon.IsValid())
            BroadCollisionDetectionRecursive(node.secondSon, depth + 1);

        if (node.IsLeaf())
        {
            for (int i = 0; i < node.nColliders; i++)
                for (int j = i + 1; j < node.nColliders; j++)
                    collisionPairs.Add((node.colliders[i], node.colliders[j]));
        }
    }

    public void ParallelBroadCollisionDetectionRecursive(BVH_node node, int depth)
    {
        node.Branch();
        if (node.IsLeaf())
        {
            lock (collisionPairs)
            {
                for (int i = 0; i < node.nColliders; i++)
                    for (int j = i + 1; j < node.nColliders; j++)
                        collisionPairs.Add((node.colliders[i], node.colliders[j]));
            }
            return;
        }

        CountdownEvent countdownEvent;
        lock (countdownEvents)
        {
            countdownEvent = GetNewCountdownEvent();
        }

        NewBranch(node.firstSon, countdownEvent, depth);
        NewBranch(node.secondSon, countdownEvent, depth);
        countdownEvent.Wait();
    }

    private void NewBranch(BVH_node nodeSon, CountdownEvent countdown, int depth)
    {
        if (nodeSon != null && nodeSon.IsValid())
        {
            if (depth > 3)
            {
                ParallelBroadCollisionDetectionRecursive(nodeSon , depth + 1);
                countdown.Signal();
            }
            else
            {
                threadDispatcher.InitThread<BVH_node>((n, d) => {ParallelBroadCollisionDetectionRecursive(n, d);}, countdown, depth + 1, nodeSon);
            }
        }
        else
            countdown.Signal();
    }

    public void NarrowCollisionDetection(double h)
    {
        if (allColliders.Length == 0)
            return;

        if (selfCollisions)
            NarrowCollisionDetectionIterative(h);

        CheckCollisionWithPlane(h);
    }

    public void NarrowCollisionDetection(double h, List<List<Correction>> corrections)
    {
        if (allColliders.Length == 0)
            return;

        if (selfCollisions)
            NarrowCollisionDetectionIterative(h, corrections);

        CheckCollisionWithPlane(h, corrections);
    }

    public void ParallelNarrowCollisionDetection(double h, List<List<Correction>> corrections)
    {
        Action<PBDCollider, PBDCollider, int> collisionFunc = (iCol, jCol, index) => {ParallelCheckCollision(iCol, jCol,  h, index, corrections);};
        ParallelNarrowCollisionDetection(h, collisionFunc);
    }

    public void ParallelNarrowCollisionDetection(double h)
    {
        Action<PBDCollider, PBDCollider, int> collisionFunc = (iCol, jCol, index) => {ParallelCheckCollision(iCol, jCol,  h, index);};
        ParallelNarrowCollisionDetection(h, collisionFunc);
    }

    public void ParallelNarrowCollisionDetection(double h, Action<PBDCollider, PBDCollider, int> collisionFunc)
    {
        if (allColliders.Length == 0)
            return;

        if (selfCollisions && collisionPairs.Count > 0)
            threadDispatcher.DistributeLoad((from, to, h, i) => {CollisionThread(from, to, i, collisionFunc);}, collisionPairs.Count, h);

        threadDispatcher.DistributeLoad((from, to, h, i) => {ExcludedCollisionThread(from, to, i, collisionFunc);}, allColliders.Length, h);
    }

    private void CollisionThread(int from, int to,  int index, Action<PBDCollider, PBDCollider, int> collisionFunc)
    {
        for (int i = from; i < to; i++)
        {
            int a = collisionPairs[i].Item1;
            int b = collisionPairs[i].Item2;

            lock (checkedCols)
            {
                if (IsAlreadyChecked(a, b))
                    continue;
            }

            PBDCollider iCol = allColliders[a];
            PBDCollider jCol = allColliders[b];

            if (iCol.aabb.CollidesWith(jCol.aabb))
            {
                Mutex[] mutexes = {iCol.mutex, jCol.mutex};
                Mutex.WaitAll(mutexes);
                collisionFunc(iCol, jCol, index);
                //ParallelCheckCollision(iCol, jCol,  h, index);
                mutexes[0].ReleaseMutex();
                mutexes[1].ReleaseMutex();
            }
        }
    }

    private void ExcludedCollisionThread(int from, int to, int index, Action<PBDCollider, PBDCollider, int> collisionFunc)
    {
        for (int i = 0; i < excludedColliders.Length; i++)
            for (int j = from; j < to; j++)
                collisionFunc(excludedColliders[i], allColliders[j], index);
        //ParallelCheckCollision(excludedColliders[i], allColliders[j], h, index);
    }

    public void NarrowCollisionDetectionIterative(double h)
    {
        for (int i = 0; i < collisionPairs.Count; i++)
        {
            CheckCollision(collisionPairs[i].Item1, collisionPairs[i].Item2, h);
        }
    }

    public void NarrowCollisionDetectionIterative(double h, List<List<Correction>> corrections)
    {
        for (int i = 0; i < collisionPairs.Count; i++)
        {
            CheckCollision(collisionPairs[i].Item1, collisionPairs[i].Item2, h, corrections);
        }
    }

    public void NarrowCollisionDetectionRecursive(BVH_node node, double h, Action<BVH_node> nextCall)
    {
        if (node.colliders.Length <= 1)
            return;
        if (node.firstSon == null && node.secondSon == null)
        {
            FullCollisionDetectionNoDuplicates(node.colliders, node.nColliders, h);
            return;
        }
        if (node.firstSon != null)
            nextCall(node.firstSon);
        if (node.secondSon != null)
            nextCall(node.secondSon);
    }

    public void NarrowCollisionDetectionRecursive(BVH_node node, double h)
    {
        // NarrowCollisionDetectionRecursive(node, h, (nd) => {NarrowCollisionDetectionRecursive(nd, h);});
        if (node.colliders.Length <= 1)
            return;
        if (node.firstSon == null && node.secondSon == null)
        {
            FullCollisionDetectionNoDuplicates(node.colliders, node.nColliders, h);
            return;
        }
        if (node.firstSon != null)
            NarrowCollisionDetectionRecursive(node.firstSon, h);
        if (node.secondSon != null)
            NarrowCollisionDetectionRecursive(node.secondSon, h);
    }

    public void NarrowCollisionDetectionRecursive(BVH_node node, double h, List<List<Correction>> corrections)
    {
        //NarrowCollisionDetectionRecursive(node, h, (nd) => {NarrowCollisionDetectionRecursive(nd, h, corrections);});
        if (node.colliders.Length <= 1)
            return;
        if (node.firstSon == null && node.secondSon == null)
        {
            FullCollisionDetectionNoDuplicates(node.colliders, node.nColliders, h);
            return;
        }
        if (node.firstSon != null)
            NarrowCollisionDetectionRecursive(node.firstSon, h, corrections);
        if (node.secondSon != null)
            NarrowCollisionDetectionRecursive(node.secondSon, h, corrections);
    }

    private void UpdateAABBS(int from, int to, double deltaTime, int index)
    {
        for (int i = from; i < to; i++)
        {
            allColliders[i].CalcBoundingBox();
            double expansion = k * deltaTime * DoubleVector3.Magnitude(allColliders[i].particle.velocity);
            if (k > 0)
                allColliders[i].aabb.Expand(expansion);
            // allColliders[i].aabb.DrawWireframe(Color.gray);
        }
    }

    public void FullCollisionDetection(PBDCollider[] colArr, Action<PBDCollider, PBDCollider> collisionFunc)
    {
        colCount = 0;
        for (int i = 0; i < colArr.Length; i++)
            for (int j = i + 1; j < colArr.Length; j++)
                collisionFunc(colArr[i], colArr[j]);

        for (int i = 0; i < excludedColliders.Length; i++)
            for (int j = 0; j < colArr.Length; j++)
                collisionFunc(excludedColliders[i], colArr[j]);
    }

    public void FullCollisionDetection(PBDCollider[] colArr, double h)
    {
        FullCollisionDetection(colArr, (iCol, jCol) => {CheckCollision(iCol, jCol, h);});
    }

    public void FullCollisionDetection(PBDCollider[] colArr, double h, List<List<Correction>> corrections)
    {
        FullCollisionDetection(colArr, (iCol, jCol) => {CheckCollision(iCol, jCol, h, corrections);});
    }

    public void FullCollisionDetection(int[] colArr, double h)//REAL ONE
    {
        FullCollisionDetectionNoDuplicates(colArr, colArr.Length, h);
    }

    public void FullCollisionDetectionNoDuplicates(int[] colArr, int n, Action<PBDCollider, PBDCollider> collisionFunc)//REAL ONE
    {
        for (int i = 0; i < n; i++)
        {
            for (int j = i + 1; j < n; j++)
            {
                if (IsAlreadyChecked(i, j))
                    continue;

                PBDCollider iCol = allColliders[colArr[i]];
                PBDCollider jCol = allColliders[colArr[j]];

                if (iCol.aabb.CollidesWith(jCol.aabb))
                    collisionFunc(iCol, jCol);
                count++;
            }
        }
    }

    public void FullCollisionDetectionNoDuplicates(int[] colArr, int n, double h)//REAL ONE
    {
        FullCollisionDetectionNoDuplicates(colArr, n, (iCol, jCol) => {CheckCollision(iCol, jCol, h);});
    }

    public void FullCollisionDetection(int[] colArr, double h, List<List<Correction>> corrections)//REAL ONE
    {
        FullCollisionDetectionNoDuplicates(colArr, colArr.Length, (iCol, jCol) => {CheckCollision(iCol, jCol, h, corrections);});
    }

    private void CheckCollisionWithPlane(Action<PBDCollider, PBDCollider> collisionFunc)
    {
        for (int i = 0; i < excludedColliders.Length; i++)
            for (int j = 0; j < allColliders.Length; j++)
                collisionFunc(excludedColliders[i], allColliders[j]);
    }

    private void CheckCollisionWithPlane(double h)
    {
        //CheckCollisionWithPlane((iCol, jCol) => {CheckCollision(iCol, jCol, h);});
        for (int i = 0; i < excludedColliders.Length; i++)
            for (int j = 0; j < allColliders.Length; j++)
                CheckCollision(excludedColliders[i], allColliders[j], h);
    }

    private void CheckCollisionWithPlane(double h, List<List<Correction>> corrections)
    {
        //CheckCollisionWithPlane((iCol, jCol) => {CheckCollision(iCol, jCol, h, corrections);});
        for (int i = 0; i < excludedColliders.Length; i++)
            for (int j = 0; j < allColliders.Length; j++)
                CheckCollision(excludedColliders[i], allColliders[j], h, corrections);
    }

    private void CheckCollision(PBDCollider a, PBDCollider b, double h)
    {
        bool collided = a.CheckCollision(b,  col);
        if (collided)
        {
            CreateConstraints(h, col, nonPenetrationConstraint, staticFrictionConstraint);
            AddCollisionToList(col);
        }
    }

    private void ParallelCheckCollision(PBDCollider a, PBDCollider b, double h, int index)
    {
        bool collided = a.CheckCollision(b,  cols[index]);
        if (collided)
        {
            CreateConstraints(h, cols[index], nonPenetrationConstraints[index], staticFrictionConstraints[index]);
            lock (collisions)
                AddCollisionToList(cols[index]);
        }
    }

    private void ParallelCheckCollision(PBDCollider a, PBDCollider b, double h, int index, List<List<Correction>> corrections)
    {
        bool collided = a.CheckCollision(b,  cols[index]);
        if (collided)
        {
            ParallelCreateConstraints(h, cols[index], nonPenetrationConstraints[index], staticFrictionConstraints[index], corrections);
            lock (collisions)
                AddCollisionToList(cols[index]);
        }
    }

    private void CheckCollision(int i, int j,  double h)
    {
        if (IsAlreadyChecked(i, j))
            return;

        PBDCollider iCol = allColliders[i];
        PBDCollider jCol = allColliders[j];

        if (iCol.aabb.CollidesWith(jCol.aabb))
            CheckCollision(iCol, jCol,  h);
    }

    private void CheckCollision(int i, int j,  double h, List<List<Correction>> correction)
    {
        if (IsAlreadyChecked(i, j))
            return;

        PBDCollider iCol = allColliders[i];
        PBDCollider jCol = allColliders[j];

        if (iCol.aabb.CollidesWith(jCol.aabb))
            CheckCollision(iCol, jCol,  h, correction);
    }

    private bool IsAlreadyChecked(int i, int j)
    {
        int minInd = Mathf.Min(i, j);
        int maxInd = Mathf.Max(i, j);

        if (checkedCols[minInd, maxInd])
            return true;
        checkedCols[minInd, maxInd] = true;
        return false;
    }

    private void CheckCollision(PBDCollider a, PBDCollider b, double h, List<List<Correction>> corrections)
    {
        bool collided = a.CheckCollision(b,  col);
        if (collided)
        {
            colCount++;
            CreateConstraints(h, col, corrections);
            AddCollisionToList(col);
        }
    }

    public void CreateConstraints(double h, PBDCollision collision, NonPenetrationConstraint nonPenetrationConstraint, StaticFrictionConstraint staticFrictionConstraint)
    {
        nonPenetrationConstraint.SetNewValue(collision);
        nonPenetrationConstraint.Solve(h);

        if (!collision.hasFriction)
            return;

        staticFrictionConstraint.SetNewValue(collision, nonPenetrationConstraint, collision.frictionCol);
        staticFrictionConstraint.Solve(h);
    }

    public void CreateConstraints(double h, PBDCollision collision, List<List<Correction>> corrections)
    {
        CreateConstraints(h, col, nonPenetrationConstraint, staticFrictionConstraint, corrections);
    }

    public void CreateConstraints(double h, PBDCollision collision, NonPenetrationConstraint nonPenetrationConstraint, StaticFrictionConstraint staticFrictionConstraint, List<List<Correction>> corrections)
    {
        nonPenetrationConstraint.SetNewValue(collision);
        nonPenetrationConstraint.GetContribution(h, corrections);

        if (!collision.hasFriction)
            return;

        staticFrictionConstraint.SetNewValue(collision, nonPenetrationConstraint, collision.frictionCol);
        staticFrictionConstraint.GetContribution(h, corrections);
    }

    public void ParallelCreateConstraints(double h, PBDCollision collision, NonPenetrationConstraint nonPenetrationConstraint, StaticFrictionConstraint staticFrictionConstraint, List<List<Correction>> corrections)
    {
        nonPenetrationConstraint.SetNewValue(collision);
        nonPenetrationConstraint.ParallelGetContribution(h, corrections);

        if (!collision.hasFriction)
            return;

        staticFrictionConstraint.SetNewValue(collision, nonPenetrationConstraint, collision.frictionCol);
        staticFrictionConstraint.ParallelGetContribution(h, corrections);
    }

    public void IncreaseColCount()
    {
        BVH_node.maxColliders += 1;
        foreach (BVH_node n in nodes)
        {
            n.colliders = new int[BVH_node.maxColliders];
        }
        checkedCols = new bool[allColliders.Length, allColliders.Length];
    }
}
