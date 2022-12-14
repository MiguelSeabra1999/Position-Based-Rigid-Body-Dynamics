using System;
using System.Collections.Specialized;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Threading;

public abstract class PBDCollider : MonoBehaviour
{
    [HideInInspector] public Particle particle;
    public bool exclude = false;
    public Vector3 pos = Vector3.zero;
    public bool isTrigger = false;
    public AABB aabb;
    [HideInInspector] public  Mutex mutex = new Mutex();


    public virtual void ColliderUpdate() {}
    public virtual void SeparateAdjustment(DoubleVector3 normal) {}

    public abstract PBDColliderType GetColliderType();
    public abstract void CalcBoundingBox();
    public abstract bool IntersectRay(DoubleRay ray, ref DoubleRayHit hit);
    public abstract bool CheckCollision(PBDColliderSphere other,  PBDCollision collision);
    public abstract bool CheckCollision(PBDColliderBox other,  PBDCollision collision);
    public abstract bool CheckCollision(PBDColliderCapsule other,  PBDCollision collision);
    public abstract bool CheckCollision(PBDColliderPlaneY other,  PBDCollision collision);

    public virtual Matrix3x3 GetInertiaTensor()
    {
        return Matrix3x3.Identity();
    }

    public virtual Matrix3x3 GetInertiaTensorInverted()
    {
        return Matrix3x3.Identity();
    }

    public bool CheckCollision(PBDCollider other,  PBDCollision collision)
    {
        if (isTrigger || other.isTrigger)
            return false;
        if (particle.inverseMass == 0 && other.particle.inverseMass == 0)
            return false;
        switch (other.GetColliderType())
        {
            case PBDColliderType.sphere:
                return CheckCollision((PBDColliderSphere)other,  collision);
            case PBDColliderType.box:
                return CheckCollision((PBDColliderBox)other,  collision);
            case PBDColliderType.capsule:
                return CheckCollision((PBDColliderCapsule)other,  collision);
            case PBDColliderType.planeY:
                return CheckCollision((PBDColliderPlaneY)other,  collision);
            default:
                return false;
        }
    }

    protected bool CheckCollision(PBDColliderSphere self, PBDColliderSphere other,  PBDCollision collision)
    {
        if (self.particle == null)
            return false;

        double radiusSum = other.radius + self.radius;
        DoubleVector3 deltaPos = other.particle.position - self.particle.position;
        double deltaPosMagnitude = DoubleVector3.Magnitude(deltaPos);

        if (deltaPosMagnitude >= radiusSum)
        {
            return false;
        }
        double penetration = radiusSum - deltaPosMagnitude;

        DoubleVector3 normal = DoubleVector3.Normal(deltaPos);
        DoubleVector3 correction = normal * penetration;
        // Debug.Log("normal " + normal + "radSum " + radiusSum + "delaPosMag" + deltaPosMagnitude + "corr " + correction) ;
        DoubleVector3 collisionPoint = self.particle.position + normal * (self.radius - penetration / 2);
        collision.LoadNewValues(self.particle, other.particle, normal, correction, collisionPoint);
        return true;
    }

    protected bool CheckCollision(PBDColliderCapsule self, PBDColliderSphere other,  PBDCollision collision)
    {
        return CheckCollision(other, self,  collision);
    }

    protected bool CheckCollision(PBDColliderSphere self, PBDColliderCapsule other,  PBDCollision collision)
    {
        DoubleVector3 point = other.GetNearestPoint(self.particle.position);
        double radiusSum = other.radius + self.radius;
        DoubleVector3 deltaPos = point - self.particle.position;
        double deltaPosMagnitude = DoubleVector3.Magnitude(deltaPos);

        // Debug.DrawLine(self.transform.position,point.ToVector3());

        if (deltaPosMagnitude >= radiusSum)
        {
            return false;
        }
        double penetration = radiusSum - deltaPosMagnitude;
        DoubleVector3 normal = DoubleVector3.Normal(deltaPos);
        DoubleVector3 correction = normal * penetration;
        // Debug.Log("normal " + normal + "radSum " + radiusSum + "delaPosMag" + deltaPosMagnitude + "corr " + correction) ;
        DoubleVector3 collisionPoint = self.particle.position + normal * (self.radius - penetration / 2);

        collision.LoadNewValues(self.particle, other.particle, normal, correction, collisionPoint);
        return true;
    }

    protected bool CheckCollision(PBDColliderCapsule self, PBDColliderCapsule other,  PBDCollision collision)
    {
        (DoubleVector3 a, DoubleVector3 b)line = other.GetLine();
        DoubleVector3 pointSelf1 = self.GetNearestPoint(line.a);
        DoubleVector3 pointSelf2 = self.GetNearestPoint(line.b);

        double aToP1_length = DoubleVector3.MagnitudeSqr(pointSelf1 - line.a);
        double aToP2_length = DoubleVector3.MagnitudeSqr(pointSelf2 - line.b);

        DoubleVector3 pointSelf, pointOther;
        if (aToP1_length < aToP2_length)
        {
            pointSelf = pointSelf1;
        }
        else if (aToP1_length > aToP2_length)
        {
            pointSelf = pointSelf2;
        }
        else
        {
            pointSelf = self.particle.position;
        }
        pointOther = other.GetNearestPoint(pointSelf);

        //   Debug.DrawLine(pointSelf.ToVector3(),pointOther.ToVector3());
        double radiusSum = other.radius + self.radius;
        DoubleVector3 deltaPos = pointOther - pointSelf;

        double deltaPosMagnitude = DoubleVector3.Magnitude(deltaPos);

        if (deltaPosMagnitude >= radiusSum)
        {
            return false;
        }
        double penetration = radiusSum - deltaPosMagnitude;

        DoubleVector3 normal = DoubleVector3.Normal(deltaPos);
        DoubleVector3 correction = normal * penetration;
        DoubleVector3 collisionPoint = pointSelf + normal * (self.radius - penetration / 2);

        (DoubleVector3 a, DoubleVector3 b)selfLine = self.GetLine();
        DoubleVector3 otherDir = line.b - line.a;
        DoubleVector3 selfDir = selfLine.b - selfLine.a;
        double dot = DoubleVector3.Dot(otherDir, selfDir);
        if (dot == 1 || dot == -1)
        {
            collisionPoint =  self.particle.position + (other.particle.position - self.particle.position) * (self.radius - penetration / 2);
        }
        // Debug.DrawRay(collisionPoint.ToVector3(), Vector3.up * 10, Color.black, 1f);

        collision.LoadNewValues(self.particle, other.particle, normal, correction, collisionPoint);
        return true;
    }

    protected bool CheckCollision(PBDColliderPlaneY self, PBDColliderPlaneY other,  PBDCollision collision)
    {
        return false;
    }

    protected bool CheckCollision(PBDColliderSphere self, PBDColliderPlaneY other,  PBDCollision collision)
    {
        return CheckCollision(other, self,  collision);
    }

    protected bool CheckCollision(PBDColliderPlaneY self, PBDColliderSphere other,  PBDCollision collision)
    {
        if (other.particle.position.y - other.radius < self.particle.position.y)
        {
            double yDiff = self.particle.position.y - (other.particle.position.y - other.radius);
            DoubleVector3 normal = new DoubleVector3(0, 1, 0);
            DoubleVector3 correction = new DoubleVector3(0, yDiff, 0);
            DoubleVector3 collisionPoint = new DoubleVector3(other.particle.position.x , self.particle.position.y , other.particle.position.z);
            collision.LoadNewValues(self.particle, other.particle, normal, correction, collisionPoint);
            return true;
        }
        return false;
    }

    protected bool CheckCollision(PBDColliderCapsule self, PBDColliderPlaneY other,  PBDCollision collision)
    {
        return CheckCollision(other, self,  collision);
    }

    protected bool CheckCollision(PBDColliderPlaneY self, PBDColliderCapsule other,  PBDCollision collision)
    {
        (DoubleVector3 a, DoubleVector3 b)line = other.GetLine();
        DoubleVector3 otherPos;
        if (line.a.y < line.b.y)
            otherPos = line.a;
        else if (line.a.y > line.b.y)
            otherPos = line.b;
        else
            otherPos = other.particle.position;

        if (otherPos.y - other.radius < self.particle.position.y)
        {
            double yDiff = self.particle.position.y - (otherPos.y - other.radius);
            DoubleVector3 normal = new DoubleVector3(0, 1, 0);
            DoubleVector3 correction = new DoubleVector3(0, yDiff, 0);
            DoubleVector3 collisionPoint = new DoubleVector3(otherPos.x , self.particle.position.y , otherPos.z);
            collision.LoadNewValues(self.particle, other.particle, normal, correction, collisionPoint);
            return true;
        }
        return false;
    }

    protected bool CheckCollision(PBDColliderBox self, PBDColliderPlaneY other,  PBDCollision collision)
    {
        return CheckCollision(other, self,  collision);
    }

    protected bool CheckCollision(PBDColliderPlaneY self, PBDColliderBox other,  PBDCollision collision)
    {
        if (other.particle.position.y - other.diagonal > self.particle.position.y)
            return false;

        DoubleVector3 lessYpoint = other.GetLowestPoint();
        //   DoubleVector3 lessYpoint =  other.GetFurthestPointAlongDirection(new DoubleVector3(Vector3.up));

        //  Debug.DrawRay(lessYpoint.ToVector3(), Vector3.up,Color.blue);
        if (lessYpoint.y > self.particle.position.y)
            return false;
        double yDiff = self.particle.position.y - lessYpoint.y;
        DoubleVector3 normal = new DoubleVector3(0, 1, 0);
        DoubleVector3 correction = new DoubleVector3(0, yDiff, 0);
        //other.pos = lessYpoint.ToVector3();
        collision.LoadNewValues(self.particle, other.particle, normal, correction, lessYpoint);
        return true;
    }

    protected bool CheckCollision(PBDColliderBox self, PBDColliderCapsule other,  PBDCollision collision)
    {
        return CheckCollision(other, self,  collision);
    }

    protected bool CheckCollision(PBDColliderCapsule self, PBDColliderBox other,  PBDCollision collision)
    {
        (DoubleVector3 a, DoubleVector3 b)line = self.GetLine();
        DoubleVector3 pointOther1 = other.GetClosestPointOnSurface(line.a);
        DoubleVector3 pointOther2 = other.GetClosestPointOnSurface(line.b);

        double aToP1_length = DoubleVector3.MagnitudeSqr(pointOther1 - line.a);
        double aToP2_length = DoubleVector3.MagnitudeSqr(pointOther2 - line.b);

        DoubleVector3 pointOther, pointSelf;
        if (aToP1_length < aToP2_length)
        {
            pointOther = pointOther1;
        }
        else if (aToP1_length > aToP2_length)
        {
            pointOther = pointOther2;
        }
        else
        {
            pointOther = self.particle.position;
        }
        pointSelf = self.GetNearestPoint(pointOther);

        //Debug.DrawLine(pointSelf.ToVector3(),pointOther.ToVector3());

        DoubleVector3 closestPointOnBox = other.GetClosestPoint(pointSelf);
        DoubleVector3 offset = closestPointOnBox - pointSelf;
        double magnitude = DoubleVector3.Magnitude(offset);
        if (magnitude > self.radius)
            return false;
        DoubleVector3 normal = DoubleVector3.Normal(offset);
        DoubleVector3 correction = normal * (self.radius - magnitude);
        collision.LoadNewValues(self.particle, other.particle, normal, correction, closestPointOnBox);
        return true;
    }

    protected bool CheckCollision(PBDColliderBox self, PBDColliderSphere other,  PBDCollision collision)
    {
        return CheckCollision(other, self,  collision);
    }

    protected bool CheckCollision(PBDColliderSphere self, PBDColliderBox other,  PBDCollision collision)
    {
        DoubleVector3 closestPointOnBox = other.GetClosestPoint(self.particle.position);


        DoubleVector3 offset = closestPointOnBox - self.particle.position;
        double magnitude = DoubleVector3.Magnitude(offset);
        if (magnitude > self.radius)
            return false;
        DoubleVector3 normal = DoubleVector3.Normal(offset);
        DoubleVector3 correction = normal * (self.radius - magnitude);
        collision.LoadNewValues(self.particle, other.particle, normal, correction, closestPointOnBox);
        return true;
    }

    protected bool CheckCollision(PBDColliderBox self, PBDColliderBox other,  PBDCollision collision)
    {
        return PBDColliderBox.CheckCollisionBoxBox(self, other, collision);
    }
}
