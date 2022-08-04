using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PBDColliderCapsule : PBDCollider
{
    public double radius = 0.5;
    public double length = 2;
    public Vector3 center = new Vector3(0, 0, 0);
    private DoubleVector3 doubleCenter;
    private double halfLength;
    private bool shouldDrawGizmoSpheres = false;
    void Awake()
    {
        doubleCenter = new DoubleVector3(center);
        length -= 2 * radius;
        halfLength = length / 2;

        if (transform.localScale.x != transform.localScale.y || transform.localScale.x != transform.localScale.z)
        {
            shouldDrawGizmoSpheres = true;
        }
    }

    void OnDrawGizmos()
    {
        if (Application.isPlaying)
        {
            if (shouldDrawGizmoSpheres)
            {
                (DoubleVector3 a, DoubleVector3 b)line = GetLine();
                Gizmos.DrawSphere(line.a.ToVector3(), (float)radius);
                Gizmos.DrawSphere(line.b.ToVector3(), (float)radius);
            }
        }
    }

    public override void CalcBoundingBox()
    {
        DoubleVector3 diag = new DoubleVector3(radius, radius, radius);

        (DoubleVector3 a, DoubleVector3 b)line = GetLine();
        DoubleVector3 neg = line.a - diag;
        DoubleVector3 pos = line.a  + diag;
        AABB aabb1 = new AABB(neg, pos);


        neg = line.b - diag;
        pos = line.b  + diag;
        AABB aabb2 = new AABB(neg, pos);

        aabb = AABB.Join(aabb1, aabb2);
    }

    public override Matrix3x3 GetInertiaTensor()
    {
        //return new Matrix3x3();
        // return new Matrix3x3() * particle.mass;

        Matrix3x3 I = new Matrix3x3();

        double massCylinder = length * radius * radius * Math.PI;
        double massHemisphere = 2.0 * radius * radius * radius * Math.PI / 3.0;
        double totalMass = massCylinder + massHemisphere * 2.0;
        double ratio1 = massCylinder / totalMass;
        double ratio2 = massHemisphere * 2.0 / totalMass;
        massCylinder = particle.mass * ratio1;
        massHemisphere = particle.mass * ratio2 * 0.5;

        I[0] = massCylinder * (length * length / 12.0 + radius * radius / 4.0) + 2.0 * massHemisphere * (2.0 * radius * radius / 5.0 + length * length / 2.0 + 3.0 * length * radius / 8.0);
        I[1] = 0;
        I[2] = 0;
        I[3] = 0;
        I[4] = massCylinder * (radius * radius / 2.0) + 2.0 * massHemisphere * (2.0 * radius * radius / 5.0);
        I[5] = 0;
        I[6] = 0;
        I[7] = 0;
        I[8] = massCylinder * (length * length / 12.0 + radius * radius / 4.0) + 2.0 * massHemisphere * (2.0 * radius * radius / 5.0 + length * length / 2.0 + 3.0 * length * radius / 8.0);
        return I;
    }

    public override Matrix3x3 GetInertiaTensorInverted()
    {
        //  return new Matrix3x3()* particle.inverseMass;

        return GetInertiaTensor().GetInverse();
        /*Matrix3x3 I = new Matrix3x3();

        double massCylinder = length * radius * radius * Math.PI;
        double massHemisphere = 2 * radius * radius * radius * Math.PI / 3;
        double totalMass = massCylinder + massHemisphere * 2;
        double ratio1 = massCylinder / totalMass;
        double ratio2 = massHemisphere * 2 / totalMass;
        massCylinder = particle.inverseMass * ratio1;
        massHemisphere = particle.inverseMass * ratio2 * 0.5;

        I[0] = massCylinder * (length * length / 12 + radius * radius / 4) + 2 * massHemisphere * (2 * radius * radius / 5 + length * length / 2 + 3 * length * radius / 8);
        I[1] = 0;
        I[2] = 0;
        I[3] = 0;
        I[4] = massCylinder * (radius * radius / 2) + 2 * massHemisphere * (2 * radius * radius / 5);
        I[5] = 0;
        I[6] = 0;
        I[7] = 0;
        I[8] = massCylinder * (length * length / 12 + radius * radius / 4) + 2 * massHemisphere * (2 * radius * radius / 5 + length * length / 2 + 3 * length * radius / 8);
        return I;*/
    }

    public DoubleVector3 GetNearestPoint(DoubleVector3 point)
    {
        (DoubleVector3 a, DoubleVector3 b)line = GetLine();
        return DoubleVector3.ClosestPointOnSegment(line.a, line.b, point);
    }

    public (DoubleVector3 a, DoubleVector3 b) GetLine()
    {
        DoubleVector3 up = particle.up;
        DoubleVector3 a = particle.position + doubleCenter - up * halfLength;
        DoubleVector3 b = particle.position + doubleCenter + up * halfLength;

        return (a, b);
    }

    public override bool CheckCollision(PBDColliderPlaneY other, ref PBDCollision collision)
    {
        return CheckCollision(other, this , ref collision);
    }

    public override bool CheckCollision(PBDColliderSphere other, ref PBDCollision collision)
    {
        return CheckCollision(other, this, ref collision);
    }

    public override bool CheckCollision(PBDColliderCapsule other, ref PBDCollision collision)
    {
        return CheckCollision(this, other, ref collision);
    }

    public override bool CheckCollision(PBDColliderBox other, ref PBDCollision collision)
    {
        return CheckCollision(this, other, ref collision);
    }

    public override PBDColliderType GetColliderType()
    {
        return PBDColliderType.capsule;
    }

    public override bool IntersectRay(DoubleRay ray, ref DoubleRayHit hit)
    {
        DoubleVector3 offset = ray.point - particle.position;
        DoubleVector3 clampedOffset = new DoubleVector3(offset.x, 0, offset.z);
        DoubleVector3 planeNormal = DoubleVector3.Normal(clampedOffset);
        double normalDotDirection = DoubleVector3.Dot(planeNormal, ray.direction);
        if (normalDotDirection >= 0)
            return false;

        double planeD = planeNormal.x * particle.position.x + planeNormal.z * particle.position.z;
        double hitDistance = (planeD - DoubleVector3.Dot(ray.point, planeNormal)) / normalDotDirection;

        DoubleVector3 planeHitPoint = ray.point + hitDistance * ray.direction;

        DoubleVector3 spherePoint = GetNearestPoint(planeHitPoint);
        DoubleVector3 sphereToRay = ray.point - spherePoint;
        //quadratic formula
        double b = DoubleVector3.Dot(sphereToRay, ray.direction);
        double c = DoubleVector3.Dot(sphereToRay, sphereToRay) - radius * radius;

        if (c > 0.0 && b > 0.0)
            return false;

        double discriminant = b * b - c;
        if (discriminant < 0.0)
            return false;

        //Intersection Confirmed
        hitDistance = -b - Math.Sqrt(discriminant);
        if (hitDistance < 0.0)
            hitDistance = 0.0f; //ray was inside sphere

        DoubleVector3 hitPoint = ray.point + hitDistance * ray.direction;
        hit = new DoubleRayHit(this, hitPoint, hitDistance);
        return true;
    }
}
