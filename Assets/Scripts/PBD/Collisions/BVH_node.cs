using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BVH_node
{
    public static int maxColliders = 1;
    public AABB aabb;
    public int[] colliders = null;
    public int nColliders = 0;
    public BVH_node firstSon = null;
    public BVH_node secondSon = null;
    private CollisionEngine collisionEngine;

    public BVH_node(CollisionEngine collisionEngine)
    {
        this.collisionEngine = collisionEngine;
        colliders = new int[maxColliders];
    }

    /*   public BVH_node(CollisionEngine collisionEngine, int[] colliders)
       {
           this.colliders = colliders;
           this.collisionEngine = collisionEngine;
           CalcAABB();
       }
       public BVH_node(CollisionEngine collisionEngine, int n)
       {
           this.colliders = new int[n];
           for(int i = 0; i < n; i++)
               colliders[i] = i;
           this.collisionEngine = collisionEngine;
           CalcAABB();
       }*/
    public void SetNewValues(int[] colliders)
    {
        this.colliders = colliders;

        CalcAABB();
    }

    public void AddCollider(int n)
    {
        colliders[nColliders] = n;
        nColliders++;
    }

    public void SetNewValues(int n)
    {
        // this.colliders = new int[n];
        nColliders = n;
        for (int i = 0; i < n; i++)
            colliders[i] = i;

        CalcAABB();
    }

    public void Branch()
    {
        DoubleVector3 dims = aabb.pos - aabb.neg;
        int index = dims.GetLongestDirection();
        double halfPoint = aabb.neg[index] + dims[index] / 2;

        // int midObj = colliders.Length/2;
        // double halfPoint = colliders[midObj-1].pos[index]  + (colliders[midObj-1].aabb.pos[index] - colliders[midObj].aabb.neg[index])/2;
        /*  if(index == 0)
          Debug.DrawRay(new Vector3((float)halfPoint,0,0), Vector3.up * 100, Color.black, 1f);
          if(index == 1)
          Debug.DrawRay(new Vector3(0,(float)halfPoint,0), Vector3.right * 100, Color.black, 1f);
          if(index == 2)
          Debug.DrawRay(new Vector3(0,0,(float)halfPoint), Vector3.down * 100, Color.black, 1f);*/
        bool foundDiv = false;
        for (int i = 0; i < 3; i++)
        {
            (BVH_node a, BVH_node b)nodes = SeparateColliders(index, halfPoint);

            if (nodes.a != null)
                firstSon = nodes.a;
            // firstSon = new BVH_node(collisionEngine, colliderLists.a);
            if (nodes.b != null)
                secondSon = nodes.b;
            //secondSon = new BVH_node(collisionEngine, colliderLists.b);
            if ((firstSon != null && secondSon != null) && (firstSon.aabb != aabb && secondSon.aabb != aabb))
            {
                foundDiv = true;
                break;
            }

            if (index == 2)
                index = 0;
            else
                index++;

            halfPoint = aabb.neg[index] + dims[index] / 2;
        }
        if (!foundDiv)
        {
            firstSon = null;
            secondSon = null;
        }
    }

    private (BVH_node, BVH_node) SeparateColliders(int index, double halfPoint)
    {
        BVH_node firstSon = null, secondSon = null;
        for (int i = 0; i < nColliders; i++)
        {
            /*  bool isPlane = col.GetColliderType() == ColliderType.planeY;
              if(isPlane)
                  continue;*/
            int col = colliders[i];

            if (collisionEngine.allColliders[col].aabb.pos[index] < halfPoint)
            {
                if (firstSon == null)
                    firstSon = collisionEngine.GetNewNode();
                firstSon.AddCollider(col);
            }
            else if (collisionEngine.allColliders[col].aabb.neg[index] > halfPoint)
            {
                if (secondSon == null)
                    secondSon = collisionEngine.GetNewNode();
                secondSon.AddCollider(col);
            }
            else //somewhere in the middle
            {
                if (firstSon == null)
                    firstSon = collisionEngine.GetNewNode();
                firstSon.AddCollider(col);
                if (secondSon == null)
                    secondSon = collisionEngine.GetNewNode();
                secondSon.AddCollider(col);
            }
        }
        return (firstSon, secondSon);
    }

    public void CalcAABB()
    {
        aabb = AABB.Join(collisionEngine.allColliders, colliders);
    }

    public bool IsValid()
    {
        bool nan = aabb.neg.IsNan() || aabb.pos.IsNan();
        if (nan)
        {
            Debug.LogError("Nan found");
            Debug.Break();
        }
        return !nan;
    }
}
