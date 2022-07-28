using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public struct AABB
{
    public static double EPSILON = 0.01;
    public DoubleVector3 neg;//negative point, -x,-y,-z
    public DoubleVector3 pos;//positiove point, x, y, z

    public AABB(DoubleVector3 neg, DoubleVector3 pos)
    {
        this.neg = neg - new DoubleVector3(EPSILON);
        this.pos = pos + new DoubleVector3(EPSILON);
    }

    public static AABB Join(AABB a, AABB b)
    {
        DoubleVector3 neg = DoubleVector3.MinValues(a.neg, b.neg);
        DoubleVector3 pos = DoubleVector3.MaxValues(a.pos, b.pos);
        return new AABB(neg, pos);
    }

/*
    public static AABB Join(AABB[] aabbs)
    {
        AABB result = aabbs[0];

        for(int i = 0; i < aabbs.Length; i++)
        {
            result = AABB.Join(result, aabbs[i]);
        }

        return result;
    }
    public static AABB Join(List<AABB> aabbs)
    {
        AABB result = aabbs[0];

        for(int i = 0; i < aabbs.Count; i++)
        {
            result = AABB.Join(result, aabbs[i]);
        }

        return result;
    }*/
    public static AABB Join(PBDCollider[] aabbs)
    {
        AABB result = aabbs[0].aabb;

        for (int i = 0; i < aabbs.Length; i++)
        {
            result = AABB.Join(result, aabbs[i].aabb);
        }

        return result;
    }

    public static AABB Join(PBDCollider[] collisionList, int[] indexes)
    {
        AABB result = collisionList[indexes[0]].aabb;

        for (int i = 0; i < indexes.Length; i++)
        {
            result = AABB.Join(result, collisionList[indexes[i]].aabb);
        }

        return result;
    }

    public static AABB Join(List<PBDCollider> aabbs)
    {
        AABB result = aabbs[0].aabb;

        for (int i = 0; i < aabbs.Count; i++)
        {
            result = AABB.Join(result, aabbs[i].aabb);
        }

        return result;
    }

    public void DrawWireframe(Color color)
    {
        DoubleVector3 offset = pos - neg;
        DoubleVector3 u = new DoubleVector3(offset.x, 0, 0);
        DoubleVector3 v = new DoubleVector3(0, offset.y, 0);
        DoubleVector3 w = new DoubleVector3(0, 0, offset.z);

        DrawCornerWireframe(neg, pos, color);
        DrawCornerWireframe(neg + u, pos - u, color);
        DrawCornerWireframe(neg + v, pos - v, color);
        DrawCornerWireframe(neg + w, pos - w, color);
    }

    private void DrawCornerWireframe(DoubleVector3 a, DoubleVector3 b, Color color)
    {
        float time = 0.05f;
        DoubleVector3 offset = b - a;
        DoubleVector3 u = new DoubleVector3(offset.x, 0, 0);
        DoubleVector3 v = new DoubleVector3(0, offset.y, 0);
        DoubleVector3 w = new DoubleVector3(0, 0, offset.z);
        Debug.DrawRay(a.ToVector3(), u.ToVector3(), color, time);
        Debug.DrawRay(a.ToVector3(), v.ToVector3(), color, time);
        Debug.DrawRay(a.ToVector3(), w.ToVector3(), color, time);

        Debug.DrawRay(b.ToVector3(), (-u).ToVector3(), color, time);
        Debug.DrawRay(b.ToVector3(), (-v).ToVector3(), color, time);
        Debug.DrawRay(b.ToVector3(), (-w).ToVector3(), color, time);
    }

    public static bool operator==(AABB a, AABB b)
    {
        return a.neg == b.neg && a.pos == b.pos;
    }

    public static bool operator!=(AABB a, AABB b)
    {
        return a.neg != b.neg || a.pos != b.pos;
    }

    public override bool Equals(object o)
    {
        if (o == null)
            return false;
        return (AABB)o == this;
    }

    public override int GetHashCode()
    {
        return (this.ToString()).GetHashCode();
    }
}
