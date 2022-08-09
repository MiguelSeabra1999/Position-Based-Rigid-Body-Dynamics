using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PBDColliderSphere : PBDCollider
{
    public double radius = 0.5;
    public override void CalcBoundingBox()
    {
        DoubleVector3 diag = new DoubleVector3(radius, radius, radius);
        DoubleVector3 neg = particle.position - diag;
        DoubleVector3 pos = particle.position + diag;
        aabb = new AABB(neg, pos);
    }

    public override Matrix3x3 GetInertiaTensor()
    {
        // return new Matrix3x3();
        // return new Matrix3x3() * particle.mass;
        Matrix3x3 I = Matrix3x3.Identity();
        I = I * (2.0 / 5.0) * particle.mass * radius * radius;

        return I;
    }

    public override Matrix3x3 GetInertiaTensorInverted()
    {
        return GetInertiaTensor().GetInverse();
        /*Matrix3x3 I = new Matrix3x3();
        I = I * (2.0/5.0) * particle.inverseMass * radius * radius;

        return I;*/
    }

    public override bool CheckCollision(PBDColliderSphere other,  PBDCollision collision)
    {
        return CheckCollision(this, other,  collision);
    }

    public override bool CheckCollision(PBDColliderCapsule other,  PBDCollision collision)
    {
        return CheckCollision(this, other,  collision);
    }

    public override bool CheckCollision(PBDColliderBox other,  PBDCollision collision)
    {
        return CheckCollision(this, other,  collision);
    }

    public override bool CheckCollision(PBDColliderPlaneY other,  PBDCollision collision)
    {
        return CheckCollision(other, this ,  collision);
    }

    public override PBDColliderType GetColliderType()
    {
        return PBDColliderType.sphere;
    }

    public override bool IntersectRay(DoubleRay ray, ref DoubleRayHit hit)
    {
        DoubleVector3 sphereToRay = ray.point - particle.position;
        //quadratic formula
        double b = DoubleVector3.Dot(sphereToRay, ray.direction);
        double c = DoubleVector3.Dot(sphereToRay, sphereToRay) - radius * radius;

        if (c > 0.0 && b > 0.0)
            return false;

        double discriminant = b * b - c;
        if (discriminant < 0.0)
            return false;

        //Intersection Confirmed
        double hitDistance = -b - Math.Sqrt(discriminant);
        if (hitDistance < 0.0)
            hitDistance = 0.0f; //ray was inside sphere

        DoubleVector3 hitPoint = ray.point + hitDistance * ray.direction;
        hit = new DoubleRayHit(this, hitPoint, hitDistance);
        return true;
    }

    //  public static bool IntersectRaySphere(DoubleVector3 pos,double radius, )
}
