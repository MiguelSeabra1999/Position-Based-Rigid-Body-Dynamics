using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PBDFrictionCollision
{
    public double normalForceLargrangeMult = 0;
    public DoubleVector3 tangencialDir = new DoubleVector3(0, 0, 0);
    public bool appliedStaticFriction = false;

    public static List<PBDFrictionCollision> allFrictionCollisions = new List<PBDFrictionCollision>();
    public static int n = 0;
    public void Reset()
    {
        normalForceLargrangeMult = 0;
        tangencialDir = new DoubleVector3(0, 0, 0);
        appliedStaticFriction = false;
    }

    public static PBDFrictionCollision GetNewFrictionCollision()
    {
        if (allFrictionCollisions.Count <= n)
            allFrictionCollisions.Add(new PBDFrictionCollision());
        n++;
        allFrictionCollisions[n - 1].Reset();
        return allFrictionCollisions[n - 1];
    }

    public static void ResetList()
    {
        n = 0;
    }
}
