using System.Dynamic;
using System;
using System.Security.Cryptography;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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
        if (nodes.Count <= nodeCount)
            nodes.Add(new BVH_node(this));


        nodeCount++;
        nodes[nodeCount - 1].nColliders = 0;
        return nodes[nodeCount - 1];
    }

    private void ClearNodes()
    {
        nodeCount = 0;
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
        {
            doWork(collisions[i]);
        }
    }

    public void Clear()
    {
        ClearNodes();
        PBDFrictionCollision.ResetList();
        //collisions.Clear();
        colLen = 0;
        for (int i = 0; i < allColliders.Length; i++)
            for (int j = 0; j < allColliders.Length; j++)
                checkedCols[i, j] = false;

        for (int i = 0; i < allColliders.Length; i++)
            allColliders[i].particle.wasInCollision = false;
    }

    public void Reset()
    {
        Clear();
    }

    public void StoreColliders(List<PBDCollider> cols)
    {
        List<PBDCollider> excluded = new List<PBDCollider>();

        int count = cols.Count;
        for (int i = 0; i < count; i++)
        {
            if (cols[i].GetColliderType() == PBDColliderType.planeY || cols[i].exclude)
                excluded.Add(cols[i]);
        }
        for (int i = 0; i < excluded.Count; i++)
        {
            cols.Remove(excluded[i]);
        }


        allColliders = cols.ToArray();
        checkedCols = new bool[allColliders.Length, allColliders.Length];
        excludedColliders = excluded.ToArray();

        BVH_node.maxColliders = allColliders.Length;
    }

    public void BroadCollisionDetection()
    {
        if (allColliders.Length == 0)
            return;
        UpdateAABBS();
        root = GetNewNode(allColliders.Length);
        BroadCollisionDetectionRecursive(root, 0);
    }

    public void BroadCollisionDetectionRecursive(BVH_node node, int depth)
    {
        node.Branch();
        /*   node.aabb.DrawWireframe(Color.red);
           Debug.Log("depth: " + depth);*/
        if (node.firstSon != null && node.firstSon.IsValid())
            BroadCollisionDetectionRecursive(node.firstSon, depth + 1);
        if (node.secondSon != null && node.secondSon.IsValid())
            BroadCollisionDetectionRecursive(node.secondSon, depth + 1);
    }

    public void NarrowCollisionDetection(bool createConstraints, double h)
    {
        if (allColliders.Length == 0)
            return;
        count = 0;
        colCount = 0;
        //  Debug.Log("----------");
        if (selfCollisions)
            NarrowCollisionDetectionRecursive(root, createConstraints, h);
        CheckCollisionWithPlane(createConstraints, h);
    }

    public void NarrowCollisionDetection(bool createConstraints, double h, List<List<Correction>> corrections)
    {
        if (allColliders.Length == 0)
            return;
        count = 0;
        colCount = 0;
        //  Debug.Log("----------");
        if (selfCollisions)
            NarrowCollisionDetectionRecursive(root, createConstraints, h, corrections);
        CheckCollisionWithPlane(createConstraints, h, corrections);
    }

    public void NarrowCollisionDetectionRecursive(BVH_node node, bool createConstraints, double h)
    {
        if (node.colliders.Length <= 1)
            return;
        if (node.firstSon == null && node.secondSon == null)
        {
            // node.aabb.DrawWireframe(Color.cyan);
            /*  string str = "";
              for (int c = 0; c < node.nColliders; c++)
                  str += " ," + allColliders[node.colliders[c]].name;
              Debug.Log("check node " + str);*/
            //  Debug.Log(node.colliders.Length);
            FullCollisionDetection(node.colliders, node.nColliders, h, createConstraints);
            //  FullCollisionDetection(node.colliders,  h, createConstraints);
            return;
        }
        if (node.firstSon != null)
            NarrowCollisionDetectionRecursive(node.firstSon, createConstraints, h);
        if (node.secondSon != null)
            NarrowCollisionDetectionRecursive(node.secondSon, createConstraints, h);
    }

    public void NarrowCollisionDetectionRecursive(BVH_node node, bool createConstraints, double h, List<List<Correction>> corrections)
    {
        if (node.nColliders <= 1)
            return;
        if (node.firstSon == null && node.secondSon == null)
        {
            //node.aabb.DrawWireframe(Color.cyan);
            /* string str = "";
             foreach(PBDCollider c in node.colliders)
                 str += " ," + c.name;
             Debug.Log("check node " + str );*/
            // Debug.Log(node.colliders.Length);
            //    FullCollisionDetection(node.colliders, h, createConstraints, corrections);
            FullCollisionDetection(node.colliders, node.nColliders, h, createConstraints);
            return;
        }
        if (node.firstSon != null)
            NarrowCollisionDetectionRecursive(node.firstSon, createConstraints, h, corrections);
        if (node.secondSon != null)
            NarrowCollisionDetectionRecursive(node.secondSon, createConstraints, h, corrections);
    }

    private void UpdateAABBS()
    {
        for (int i = 0; i < allColliders.Length; i++)
        {
            allColliders[i].CalcBoundingBox();
            double expansion = k * Time.deltaTime * DoubleVector3.Magnitude(allColliders[i].particle.velocity);
            if (k > 0)
                allColliders[i].aabb.Expand(expansion);
            // allColliders[i].aabb.DrawWireframe(Color.gray);
        }
    }

    public void FullCollisionDetection(PBDCollider[] colArr, double h, bool createConstraints)
    {
        colCount = 0;
        //    Debug.Log("----------");
        for (int i = 0; i < colArr.Length; i++)
        {
            for (int j = i + 1; j < colArr.Length; j++)
            {
                //       Debug.Log("cheking " + colArr[i].particle.gameObject.name  + "  " + colArr[j].particle.gameObject.name );
                //   if(colArr[i].GetColliderType() == ColliderType.planeY || colArr[j].GetColliderType() == ColliderType.planeY )
                ;
                CheckCollision(colArr[i], colArr[j], createConstraints, h);
                if (colArr[i].GetColliderType() != PBDColliderType.planeY && colArr[j].GetColliderType() != PBDColliderType.planeY)
                {
                    //  Debug.DrawLine(colArr[i].particle.position.ToVector3(), colArr[j].particle.position.ToVector3(), Color.yellow,0.3f);
                    count++;
                }
            }
        }
        for (int i = 0; i < excludedColliders.Length; i++)
        {
            for (int j = 0; j < colArr.Length; j++)
            {
                CheckCollision(excludedColliders[i], colArr[j], createConstraints, h);
            }
        }
        //   if(colCount > 0)
        //   Debug.Log("colCount " + colCount);
        //  Debug.Log(count + " collisions");
    }

    public void FullCollisionDetection(PBDCollider[] colArr, double h, bool createConstraints, List<List<Correction>> corrections)
    {
        colCount = 0;
        //    Debug.Log("----------");
        for (int i = 0; i < colArr.Length; i++)
        {
            for (int j = i + 1; j < colArr.Length; j++)
            {
                //       Debug.Log("cheking " + colArr[i].particle.gameObject.name  + "  " + colArr[j].particle.gameObject.name );
                //   if(colArr[i].GetColliderType() == ColliderType.planeY || colArr[j].GetColliderType() == ColliderType.planeY )
                ;
                CheckCollision(colArr[i], colArr[j], createConstraints, h, corrections);
                if (colArr[i].GetColliderType() != PBDColliderType.planeY && colArr[j].GetColliderType() != PBDColliderType.planeY)
                {
                    //  Debug.DrawLine(colArr[i].particle.position.ToVector3(), colArr[j].particle.position.ToVector3(), Color.yellow,0.3f);
                    count++;
                }
            }
        }
        for (int i = 0; i < excludedColliders.Length; i++)
        {
            for (int j = 0; j < colArr.Length; j++)
            {
                CheckCollision(excludedColliders[i], colArr[j], createConstraints, h, corrections);
            }
        }
        //   if(colCount > 0)
        //   Debug.Log("colCount " + colCount);
        //  Debug.Log(count + " collisions");
    }

    public void FullCollisionDetection(int[] colArr, double h, bool createConstraints)//REAL ONE
    {
        FullCollisionDetection(colArr, colArr.Length, h, createConstraints);
    }

    public void FullCollisionDetection(int[] colArr, int n, double h, bool createConstraints)//REAL ONE
    {
        // Debug.Log(n);
        for (int i = 0; i < n; i++)
        {
            for (int j = i + 1; j < n; j++)
            {
                int minInd = Mathf.Min(colArr[i], colArr[j]);
                int maxInd = Mathf.Max(colArr[i], colArr[j]);


                if (checkedCols[minInd, maxInd])
                    continue;

                checkedCols[minInd, maxInd] = true;
                PBDCollider iCol = allColliders[colArr[i]];
                PBDCollider jCol = allColliders[colArr[j]];
                //     Debug.Log("cheking " + iCol.particle.gameObject.name  + "  " + jCol.particle.gameObject.name );
                if (iCol.aabb.CollidesWith(jCol.aabb))
                    CheckCollision(iCol, jCol, createConstraints, h);

                //  Debug.DrawLine(iCol.particle.position.ToVector3(), jCol.particle.position.ToVector3(), Color.yellow,0.3f);
                count++;
            }
        }

//        Debug.Log(count + " collisions");
    }

    public void FullCollisionDetection(int[] colArr, double h, bool createConstraints, List<List<Correction>> corrections)//REAL ONE
    {
        for (int i = 0; i < colArr.Length; i++)
        {
            for (int j = i + 1; j < colArr.Length; j++)
            {
                int minInd = Mathf.Min(colArr[i], colArr[j]);
                int maxInd = Mathf.Max(colArr[i], colArr[j]);

                /*checkedCols[minInd,maxInd] = true;
                continue;*/


                if (checkedCols[minInd, maxInd])
                    continue;

                checkedCols[minInd, maxInd] = true;
                PBDCollider iCol = allColliders[colArr[i]];
                PBDCollider jCol = allColliders[colArr[j]];
                //     Debug.Log("cheking " + iCol.particle.gameObject.name  + "  " + jCol.particle.gameObject.name );
                if (iCol.aabb.CollidesWith(jCol.aabb))
                    CheckCollision(iCol, jCol, createConstraints, h, corrections);

                //  Debug.DrawLine(iCol.particle.position.ToVector3(), jCol.particle.position.ToVector3(), Color.yellow,0.3f);
                count++;
            }
        }

//        Debug.Log(count + " collisions");
    }

    private void CheckCollisionWithPlane(bool createConstraints, double h)
    {
        for (int i = 0; i < excludedColliders.Length; i++)
        {
            for (int j = 0; j < allColliders.Length; j++)
            {
                CheckCollision(excludedColliders[i], allColliders[j], createConstraints, h);
            }
        }
    }

    private void CheckCollisionWithPlane(bool createConstraints, double h, List<List<Correction>> corrections)
    {
        for (int i = 0; i < excludedColliders.Length; i++)
        {
            for (int j = 0; j < allColliders.Length; j++)
            {
                CheckCollision(excludedColliders[i], allColliders[j], createConstraints, h, corrections);
            }
        }
    }

    private void CheckCollision(PBDCollider a, PBDCollider b, bool createConstraints, double h)
    {
        bool collided = a.CheckCollision(b,  col);
        if (collided)
        {
            colCount++;
            if (createConstraints)
                CreateConstraints(h, col);
//            Debug.Log("col between " + a.name + " " + b.name + " " + col.correction);
            AddCollisionToList(col);
        }
    }

    private void CheckCollision(PBDCollider a, PBDCollider b, bool createConstraints, double h, List<List<Correction>> corrections)
    {
        bool collided = a.CheckCollision(b,  col);
        if (collided)
        {
            colCount++;
            if (createConstraints)
                CreateConstraints(h, col, corrections);
//            Debug.Log("col between " + a.name + " " + b.name + " " + col.correction);
            AddCollisionToList(col);
        }
    }

    public void CreateConstraints(double h, PBDCollision collision)
    {
        nonPenetrationConstraint.SetNewValue(collision);
        // NonPenetrationConstraint nonPenetrationConstraint = new NonPenetrationConstraint(collision);
        nonPenetrationConstraint.Solve(h);

        if (!collision.hasFriction)
            return;

        staticFrictionConstraint.SetNewValue(collision, nonPenetrationConstraint, collision.frictionCol);
        //StaticFrictionConstraint staticFrictionConstraint = new StaticFrictionConstraint(collision, nonPenetrationConstraint, collision.frictionCol);
        staticFrictionConstraint.Solve(h);
    }

    public void CreateConstraints(double h, PBDCollision collision, List<List<Correction>> corrections)
    {
        nonPenetrationConstraint.SetNewValue(collision);
        nonPenetrationConstraint.GetContribution(h, corrections);

        if (!collision.hasFriction)
            return;

        staticFrictionConstraint.SetNewValue(collision, nonPenetrationConstraint, collision.frictionCol);
        staticFrictionConstraint.GetContribution(h, corrections);
    }

    public void SeparateContacts()//TEMPORARY, SHOULD BE PART OF CONSTRAINT SOLVE
    {
        for (int i = 0; i < collisions.Count; i++)
        {
            collisions[i].Separate();
        }
    }
}
