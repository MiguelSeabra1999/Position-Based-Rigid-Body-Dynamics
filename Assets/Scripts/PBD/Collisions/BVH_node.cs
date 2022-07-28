using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BVH_node
{
    public AABB aabb;
    public int[] colliders = null;
    public BVH_node firstSon = null;
    public BVH_node secondSon = null;
    private CollisionEngine collisionEngine;

    public BVH_node(CollisionEngine collisionEngine, int[] colliders)
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
    }


    public void Branch()
    {
        DoubleVector3 dims = aabb.pos - aabb.neg;
        int index = dims.GetLongestDirection();
        double halfPoint = aabb.neg[index] + dims[index]/2;

         // int midObj = colliders.Length/2;
       // double halfPoint = colliders[midObj-1].pos[index]  + (colliders[midObj-1].aabb.pos[index] - colliders[midObj].aabb.neg[index])/2;
      /*  if(index == 0)
        Debug.DrawRay(new Vector3((float)halfPoint,0,0), Vector3.up * 100, Color.black, 1f);
        if(index == 1)
        Debug.DrawRay(new Vector3(0,(float)halfPoint,0), Vector3.right * 100, Color.black, 1f);
        if(index == 2)
        Debug.DrawRay(new Vector3(0,0,(float)halfPoint), Vector3.down * 100, Color.black, 1f);*/
        bool foundDiv = false;
        for(int i = 0; i < 3; i++)
        {
            (int[] a, int[] b) colliderLists = SeparateColliders(index, halfPoint);

            if(colliderLists.a.Length > 0)
                firstSon = new BVH_node(collisionEngine, colliderLists.a);
            if(colliderLists.b.Length > 0)
                secondSon = new BVH_node(collisionEngine, colliderLists.b);
            if(firstSon.aabb != aabb && secondSon.aabb != aabb )
            {
                foundDiv = true;
                break;
            }
            
            if(index == 2)
                index = 0;
            else
                index++;
       
            halfPoint = aabb.neg[index] + dims[index]/2;
        }
        if(!foundDiv)
        {
            firstSon = null;
            secondSon = null;
        }
    }

    private (int[], int[]) SeparateColliders(int index, double halfPoint)
    {
       
        List<int> collidersA = new List<int>();
        List<int> collidersB = new List<int>();
        foreach(int col in colliders)
        {
          /*  bool isPlane = col.GetColliderType() == ColliderType.planeY;
            if(isPlane)
                continue;*/

            if(collisionEngine.allColliders[col].aabb.pos[index] < halfPoint )
            {
                collidersA.Add(col);
            }
            else if(collisionEngine.allColliders[col].aabb.neg[index] > halfPoint)
            {
                collidersB.Add(col);
            }
            else //somewhere in the middle
            {
                collidersA.Add(col);
                collidersB.Add(col);
            }
        }
        return(collidersA.ToArray(), collidersB.ToArray());
    }

    public void CalcAABB()
    {
        aabb = AABB.Join(collisionEngine.allColliders, colliders);

    }

    public bool IsValid()
    {
        bool nan = aabb.neg.IsNan() || aabb.pos.IsNan();
        if(nan) 
        {
            Debug.LogError("Nan found");
            Debug.Break();
        }
        return !nan;
    }

}
