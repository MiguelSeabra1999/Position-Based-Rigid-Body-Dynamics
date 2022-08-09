using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PBDColliderPlaneY : PBDCollider
{
    public override void CalcBoundingBox()
    {
        DoubleVector3 diag = new DoubleVector3(particle.transform.localScale.x, 0, particle.transform.localScale.z);
        DoubleVector3 neg = particle.position - diag;
        DoubleVector3 pos = particle.position + diag;
        aabb = new AABB(neg, pos);
    }

    public override bool CheckCollision(PBDColliderSphere other,  PBDCollision collision)
    {
        return CheckCollision(this, other ,  collision);
    }

    public override bool CheckCollision(PBDColliderPlaneY other,  PBDCollision collision)
    {
        return CheckCollision(this, other ,  collision);
    }

    public override bool CheckCollision(PBDColliderCapsule other,  PBDCollision collision)
    {
        return CheckCollision(this, other ,  collision);
    }

    public override bool CheckCollision(PBDColliderBox other,  PBDCollision collision)
    {
        return CheckCollision(this, other,  collision);
    }

    public override PBDColliderType GetColliderType()
    {
        return PBDColliderType.planeY;
    }

    public override bool IntersectRay(DoubleRay ray, ref DoubleRayHit hit)
    {
        return false;
        /*DoubleVector3 planeNormal = new DoubleVector3(0,1,0);
        double normalDotDirection = DoubleVector3.Dot(planeNormal, ray.direction);
        if(normalDotDirection >= 0)
            return false;

        double planeD = particle.position.y;
        double hitDistance = (planeD - ray.point.y) / normalDotDirection;

        DoubleVector3 hitPoint = ray.point + hitDistance * ray.direction;
        hit = new DoubleRayHit(this, hitPoint, hitDistance);
        return true;*/
    }
}
