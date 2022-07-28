using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public struct DoubleRayHit
{
    public PBDCollider collider;
    public DoubleVector3 point;
    public double hitDistance;

    public DoubleRayHit(PBDCollider collider, DoubleVector3 point, double hitDistance)
    {
        this.collider = collider;
        this.point = point;
        this.hitDistance = hitDistance;
    }
}
