
using System;
using System.Collections.Specialized;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class PBDCollider : MonoBehaviour
{
    [HideInInspector]public Particle particle;
    public Vector3 pos = Vector3.zero;
    public bool isTrigger = false;
    public AABB aabb;


    public virtual void ColliderUpdate(){}
    public virtual void SeparateAdjustment(DoubleVector3 normal){}

    public abstract PBDColliderType GetColliderType();
    public abstract void CalcBoundingBox();
    public abstract bool IntersectRay(DoubleRay ray, ref DoubleRayHit hit);
    public abstract bool CheckCollision(PBDColliderSphere other, ref PBDCollision collision);
    public abstract bool CheckCollision(PBDColliderBox other, ref PBDCollision collision);
    public abstract bool CheckCollision(PBDColliderCapsule other, ref PBDCollision collision);
    public abstract bool CheckCollision(PBDColliderPlaneY other, ref PBDCollision collision);

    public virtual Matrix3x3 GetInertiaTensor()
    {
        return new Matrix3x3();
    }
    public virtual Matrix3x3 GetInertiaTensorInverted()
    {
        return new Matrix3x3();
    }

    public bool CheckCollision(PBDCollider other, ref PBDCollision collision)
    {
        if(isTrigger || other.isTrigger)
            return false;
        if(particle.inverseMass == 0 && other.particle.inverseMass == 0)
            return false;
        switch(other.GetColliderType())
        {
            case PBDColliderType.sphere:
                return CheckCollision((PBDColliderSphere)other, ref collision);
            case PBDColliderType.box:
                return CheckCollision((PBDColliderBox)other, ref collision);
            case PBDColliderType.capsule:
                return CheckCollision((PBDColliderCapsule)other, ref collision);
            case PBDColliderType.planeY:
                return CheckCollision((PBDColliderPlaneY)other, ref collision);
            default:
                return false;
        }
    }


    protected bool CheckCollision(PBDColliderSphere self, PBDColliderSphere other, ref PBDCollision collision)
    {
        if(self.particle == null)
            return false;
        
        double radiusSum = other.radius + self.radius;
        DoubleVector3 deltaPos = other.particle.position - self.particle.position;
        double deltaPosMagnitude = DoubleVector3.Magnitude(deltaPos);

        if(deltaPosMagnitude >= radiusSum)
        {
            return false;
        }
        double penetration = radiusSum - deltaPosMagnitude;

        DoubleVector3 normal = DoubleVector3.Normal(deltaPos);
        DoubleVector3 correction = normal * penetration;
       // Debug.Log("normal " + normal + "radSum " + radiusSum + "delaPosMag" + deltaPosMagnitude + "corr " + correction) ;
        DoubleVector3 collisionPoint = self.particle.position + normal * (self.radius - penetration/2);
        collision = new PBDCollision(self.particle, other.particle, normal, correction, collisionPoint);
        return true;
    }
    protected bool CheckCollision(PBDColliderCapsule self, PBDColliderSphere other, ref PBDCollision collision)
    {
        return CheckCollision(other,self, ref collision);
    }

    protected bool CheckCollision(PBDColliderSphere self, PBDColliderCapsule other, ref PBDCollision collision)
    {
   
        DoubleVector3 point = other.GetNearestPoint(self.particle.position);
        double radiusSum = other.radius + self.radius;
        DoubleVector3 deltaPos = point - self.particle.position;
        double deltaPosMagnitude = DoubleVector3.Magnitude(deltaPos);

       // Debug.DrawLine(self.transform.position,point.ToVector3());

        if(deltaPosMagnitude >= radiusSum)
        {
            return false;
        }
        double penetration = radiusSum - deltaPosMagnitude;
        DoubleVector3 normal = DoubleVector3.Normal(deltaPos);
        DoubleVector3 correction = normal * penetration;
       // Debug.Log("normal " + normal + "radSum " + radiusSum + "delaPosMag" + deltaPosMagnitude + "corr " + correction) ;
        DoubleVector3 collisionPoint = self.particle.position + normal * (self.radius - penetration/2);
        
        collision = new PBDCollision(self.particle, other.particle, normal, correction, collisionPoint);
        return true;


    }

    protected bool CheckCollision(PBDColliderCapsule self, PBDColliderCapsule other, ref PBDCollision collision)
    {
        
        (DoubleVector3 a, DoubleVector3 b) line = other.GetLine();
        DoubleVector3 pointSelf1 = self.GetNearestPoint(line.a);
        DoubleVector3 pointSelf2 = self.GetNearestPoint(line.b);

        double aToP1_length = DoubleVector3.MagnitudeSqr(pointSelf1 - line.a);
        double aToP2_length = DoubleVector3.MagnitudeSqr(pointSelf2 - line.b);

        DoubleVector3 pointSelf, pointOther;
        if(aToP1_length < aToP2_length)
        {
            pointSelf = pointSelf1;
        }
        else if(aToP1_length > aToP2_length)
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

        if(deltaPosMagnitude >= radiusSum)
        {
            return false;
        }
        double penetration = radiusSum - deltaPosMagnitude;

        DoubleVector3 normal = DoubleVector3.Normal(deltaPos);
        DoubleVector3 correction = normal * penetration;
        DoubleVector3 collisionPoint = pointSelf + normal * (self.radius - penetration/2);

        (DoubleVector3 a, DoubleVector3 b) selfLine = self.GetLine();
        DoubleVector3 otherDir = line.b - line.a;
        DoubleVector3 selfDir = selfLine.b - selfLine.a;
        double dot = DoubleVector3.Dot(otherDir, selfDir);
        if(dot == 1 || dot == -1)
        {
            
            collisionPoint =  self.particle.position + (other.particle.position - self.particle.position)*(self.radius - penetration/2);

        }
       // Debug.DrawRay(collisionPoint.ToVector3(), Vector3.up * 10, Color.black, 1f);

        collision = new PBDCollision(self.particle, other.particle, normal, correction, collisionPoint);
        return true;

    }


    protected bool CheckCollision(PBDColliderPlaneY self, PBDColliderPlaneY other, ref PBDCollision collision)
    {
        return false;
    }
    protected bool CheckCollision(PBDColliderSphere self, PBDColliderPlaneY other, ref PBDCollision collision)
    {
        return CheckCollision(other,self, ref collision);
    }
    protected bool CheckCollision(PBDColliderPlaneY self, PBDColliderSphere other, ref PBDCollision collision)
    {
        if(other.particle.position.y - other.radius < self.particle.position.y)
        {
            double yDiff = self.particle.position.y - (other.particle.position.y - other.radius);
            DoubleVector3 normal = new DoubleVector3(0,1,0);
            DoubleVector3 correction = new DoubleVector3(0,yDiff,0);
            DoubleVector3 collisionPoint = new DoubleVector3(other.particle.position.x ,self.particle.position.y ,other.particle.position.z);
            collision = new PBDCollision(self.particle, other.particle, normal, correction, collisionPoint);
            return true;
        }
        return false;
    }
    protected bool CheckCollision(PBDColliderCapsule self, PBDColliderPlaneY other, ref PBDCollision collision)
    {
        return CheckCollision(other,self, ref collision);
    }
    protected bool CheckCollision(PBDColliderPlaneY self, PBDColliderCapsule other, ref PBDCollision collision)
    {
        (DoubleVector3 a, DoubleVector3 b) line = other.GetLine();
        DoubleVector3 otherPos;
        if(line.a.y < line.b.y)
            otherPos = line.a;
        else if(line.a.y > line.b.y)
            otherPos = line.b;
        else
            otherPos = other.particle.position;

        if(otherPos.y - other.radius < self.particle.position.y)
        {
            double yDiff = self.particle.position.y - (otherPos.y - other.radius);
            DoubleVector3 normal = new DoubleVector3(0,1,0);
            DoubleVector3 correction = new DoubleVector3(0,yDiff,0);
            DoubleVector3 collisionPoint = new DoubleVector3(otherPos.x ,self.particle.position.y ,otherPos.z);
            collision = new PBDCollision(self.particle, other.particle, normal, correction, collisionPoint);
            return true;
        }
        return false;
    }
    protected bool CheckCollision(PBDColliderBox self, PBDColliderPlaneY other, ref PBDCollision collision)
    {
        return CheckCollision(other,self, ref collision);
    }
    protected bool CheckCollision(PBDColliderPlaneY self, PBDColliderBox other, ref PBDCollision collision)
    {
   
        DoubleVector3 lessYpoint = other.GetLowestPoint();
     //   DoubleVector3 lessYpoint =  other.GetFurthestPointAlongDirection(new DoubleVector3(Vector3.up));

     //  Debug.DrawRay(lessYpoint.ToVector3(), Vector3.up,Color.blue);
        if(lessYpoint.y > self.particle.position.y)
            return false;
        double yDiff = self.particle.position.y - lessYpoint.y;
        DoubleVector3 normal = new DoubleVector3(0,1,0);
        DoubleVector3 correction = new DoubleVector3(0,yDiff,0);
       
        collision = new PBDCollision(self.particle, other.particle, normal, correction,lessYpoint);
        return true;

    }
    protected bool CheckCollision(PBDColliderBox self, PBDColliderCapsule other, ref PBDCollision collision)
    {
        return CheckCollision(other,self, ref collision);
    }
    protected bool CheckCollision(PBDColliderCapsule self, PBDColliderBox other, ref PBDCollision collision)
    {
        (DoubleVector3 a, DoubleVector3 b) line = self.GetLine();
        DoubleVector3 pointOther1 = other.GetClosestPointOnSurface(line.a);
        DoubleVector3 pointOther2 = other.GetClosestPointOnSurface(line.b);

        double aToP1_length = DoubleVector3.MagnitudeSqr(pointOther1 - line.a);
        double aToP2_length = DoubleVector3.MagnitudeSqr(pointOther2 - line.b);

        DoubleVector3 pointOther, pointSelf;
        if(aToP1_length < aToP2_length)
        {
            pointOther = pointOther1;
        }
        else if(aToP1_length > aToP2_length)
        {
            pointOther = pointOther2;
        }
        else
        {
            pointOther = self.particle.position;
        }
        pointSelf = self.GetNearestPoint(pointOther);

        //Debug.DrawLine(pointSelf.ToVector3(),pointOther.ToVector3());

        DoubleVector3 closestPointOnBox = other.GetClosestPointOnSurface(pointSelf);
        DoubleVector3 offset = closestPointOnBox - pointSelf;
        double magnitude = DoubleVector3.Magnitude(offset);
        if(magnitude > self.radius)
            return false;
        DoubleVector3 normal = DoubleVector3.Normal(offset);
        DoubleVector3 correction = normal * (self.radius - magnitude);
        collision = new PBDCollision(self.particle, other.particle, normal, correction, closestPointOnBox);
        return true;
    }
    protected bool CheckCollision(PBDColliderBox self, PBDColliderSphere other, ref PBDCollision collision)
    {
        return CheckCollision(other,self, ref collision);
    }
    protected bool CheckCollision(PBDColliderSphere self, PBDColliderBox other, ref PBDCollision collision)
    {
        DoubleVector3 closestPointOnBox = other.GetClosestPointOnSurface(self.particle.position);

        
        DoubleVector3 offset = closestPointOnBox - self.particle.position;
        double magnitude = DoubleVector3.Magnitude(offset);
        if(magnitude > self.radius)
            return false;
        DoubleVector3 normal = DoubleVector3.Normal(offset);
        DoubleVector3 correction = normal * (self.radius - magnitude);
        collision = new PBDCollision(self.particle, other.particle, normal, correction, closestPointOnBox);
        return true;

        
    }

    protected bool CheckCollision(PBDColliderBox self, PBDColliderBox other, ref PBDCollision collision)
    {
        double epsilon = 0.0000001;
        Matrix3x3 R = new Matrix3x3();
        Matrix3x3 AbsR  = new Matrix3x3();
        double[] separatingDistances = new double[15];//used for box box intersection
        DoubleVector3[] separatingAxes = new DoubleVector3[15];//used for box box intersection
        DoubleVector3[] selfAxes = self.particle.GetAxes();
        DoubleVector3[] otherAxes = other.particle.GetAxes();

        for(int i = 0; i < 3; i++)
            for(int j = 0; j < 3; j++)
            {
                R[i*3 + j] =  DoubleVector3.Dot(selfAxes[i], otherAxes[j]);
                AbsR[i*3 + j] =  Math.Abs(R[i*3 + j]) + epsilon;
            }

        DoubleVector3 translation = other.particle.position - self.particle.position;
        double magnitude = DoubleVector3.Magnitude(translation);

        for(int i = 0; i < 15; i++)
        {
            separatingDistances[i] = magnitude;
        }
        //Bounding sphere intersection
        /*if(magnitude > self.diagonal + other.diagonal)
            return false;*/

        DoubleVector3 translationInSelfCoords = self.particle.ProjectToSelfCoordinates(translation);
    
        // Test axes L = A0, L = A1, L = A2
        if(!PBDColliderBox.TestFaceParallelAxis(self, other, selfAxes, R, AbsR, separatingDistances, separatingAxes, translationInSelfCoords, 0, 0))
            return false;
        if(!PBDColliderBox.TestFaceParallelAxis(self, other, selfAxes, R, AbsR, separatingDistances, separatingAxes, translationInSelfCoords, 1, 1))
            return false;
        if(!PBDColliderBox.TestFaceParallelAxis(self, other, selfAxes, R, AbsR, separatingDistances, separatingAxes, translationInSelfCoords, 2, 2))
            return false;

        // Test axes L = B0, L = B1, L = B2
        if(!PBDColliderBox.TestFaceParallelAxisB(self, other, otherAxes, R, AbsR, separatingDistances, separatingAxes, translationInSelfCoords, 0, 3))
            return false;
        if(!PBDColliderBox.TestFaceParallelAxisB(self, other, otherAxes, R, AbsR, separatingDistances, separatingAxes, translationInSelfCoords, 1, 4))
            return false;
        if(!PBDColliderBox.TestFaceParallelAxisB(self, other, otherAxes, R, AbsR, separatingDistances, separatingAxes, translationInSelfCoords, 2, 5))
            return false;
        // Test 9 axes L = Ai X Bj    , i = j = 0,1,2
    	if(!PBDColliderBox.TestCrossProductAxises(self, other, R, AbsR, separatingDistances, separatingAxes, translationInSelfCoords, 6))
            return false;

        // Since no separating axis is found, the OBBs must be intersecting

        collision = PBDColliderBox.CreateCollision(self, other, separatingDistances, separatingAxes);
        
     
        return true;
    }

    



  
}

