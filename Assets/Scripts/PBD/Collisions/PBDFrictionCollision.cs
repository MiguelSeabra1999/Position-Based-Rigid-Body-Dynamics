using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PBDFrictionCollision
{
    public double normalForceLargrangeMult = 0;
    public DoubleVector3 tangencialDir = new DoubleVector3(0, 0, 0);
    public bool appliedStaticFriction = false;
}
