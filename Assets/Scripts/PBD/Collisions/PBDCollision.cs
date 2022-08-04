using System.Runtime.InteropServices;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public  class PBDCollision
{
    public readonly Particle a;
    public readonly Particle b;
    public readonly DoubleVector3 normal;
    public readonly DoubleVector3 correction;
    public readonly DoubleVector3 collisionPoint;
    public readonly DoubleVector3 otherCollisionPoint;


    public  DoubleVector3 deltaVel;
    public double percent;
    public double massSum;
    public double wa, wb;

    public DoubleVector3 rA, rB, rAworld, rBworld;
    private DoubleVector3 velocityAtCollisionPointA, velocityAtCollisionPointB, relativeVelocity, velocityTangent, prevVelocityAtCollisionPointA, prevVelocityAtCollisionPointB, prevRelativeVelocity, prevVelocityTangent;
    private double velocityNormal, prevVelocityNormal;
    public PBDFrictionCollision frictionCol;
    public bool hasFriction = true;


    public PBDCollision(Particle a, Particle b, DoubleVector3 normal, DoubleVector3 correction, DoubleVector3 collisionPoint)
    {
        this.collisionPoint = collisionPoint;
        this.otherCollisionPoint = collisionPoint;
        this.a = a;
        this.b = b;
        this.normal = normal;
        this.correction = correction;
        InitCollision();
        CalculateRelativeVelocities();
    }

    public PBDCollision(Particle a, Particle b, DoubleVector3 normal, DoubleVector3 correction, DoubleVector3 collisionPoint, DoubleVector3 otherCollisionPoint)
    {
        this.collisionPoint = collisionPoint;
        this.otherCollisionPoint = otherCollisionPoint;
        this.a = a;
        this.b = b;
        this.normal = normal;
        this.correction = correction;
        InitCollision();
        CalculateRelativeVelocities();

        // CalculateRelativeVelocities();
    }

    private void InitCollision()
    {
        frictionCol = new PBDFrictionCollision();

        hasFriction = ((a.staticFrictionCoefficient != 0 && b.staticFrictionCoefficient != 0) || (a.dynamicFrictionCoefficient != 0  && b.dynamicFrictionCoefficient != 0));
        a.wasInCollision = true;
        b.wasInCollision = true;

        rAworld = collisionPoint  - a.position;
        rBworld = otherCollisionPoint  - b.position;
        rA = a.ProjectToSelfCoordinates(rAworld);
        rB = b.ProjectToSelfCoordinates(rBworld);

        deltaVel = new DoubleVector3(0, 0, 0);
        wa = a.inverseMass;
        wb = b.inverseMass;
        massSum = wa + wb;
        percent = wa / massSum;
    }

    public void CalculateRelativeVelocities()
    {
        wa = a.GetGeneralizedInverseMass(normal, rA);
        wb = b.GetGeneralizedInverseMass(-normal, rB);

        massSum = wa + wb;
        percent = wa / massSum;

        velocityAtCollisionPointA = a.GetVelocityAtCollisionPoint(rA);
        velocityAtCollisionPointB = b.GetVelocityAtCollisionPoint(rB);
        relativeVelocity = velocityAtCollisionPointA - velocityAtCollisionPointB;

        velocityNormal = DoubleVector3.Dot(normal, relativeVelocity);
        velocityTangent = relativeVelocity - normal * velocityNormal;

        prevVelocityAtCollisionPointA = a.GetPrevVelocityAtCollisionPoint(rA);
        prevVelocityAtCollisionPointB = b.GetPrevVelocityAtCollisionPoint(rB);
        prevRelativeVelocity = prevVelocityAtCollisionPointA - prevVelocityAtCollisionPointB;

        prevVelocityNormal = DoubleVector3.Dot(normal, prevRelativeVelocity);
        prevVelocityTangent = prevRelativeVelocity - normal * prevVelocityNormal;
/** /
        Debug.DrawLine(a.position.ToVector3(), (a.position + rAworld).ToVector3(), Color.yellow, 0.001f);
        Debug.DrawLine(b.position.ToVector3(), (b.position + rBworld).ToVector3(), Color.yellow, 0.001f);
  /**/
    }

    public PBDCollision GetInverse()
    {
        PBDCollision newCol = new PBDCollision(b, a, -normal, -correction, otherCollisionPoint, collisionPoint);
        return newCol;
    }

    public void Separate()//deprecated
    {
        //Debug.DrawLine(a.transform.position, a.transform.position - (correction * percent).ToVector3()*10 , Color.red, 3f);
        //Debug.DrawLine(b.transform.position, b.transform.position + (correction * percent).ToVector3()*10 , Color.red, 3f);

        if (a.inverseMass != 0)
            MoveParticle(a, correction, percent, -1, rA);

        if (b.inverseMass != 0)
            MoveParticle(b, correction, 1 - percent, 1, rB);
    }

    private void MoveParticle(Particle particle, DoubleVector3 movement, double percent, double sign, DoubleVector3 r)//Deprecaetd
    {
        DoubleVector3 corr = sign * correction * percent;
        particle.position += corr;
        particle.ApplyCorrectionOrientation(corr, r);
    }

    public void HandleCollision(double h)
    {
        double restitution = a.restitution * b.restitution;
        //  Debug.Log(velocityNormal);
        DoubleVector3 p = normal * (-velocityNormal + Math.Min(-restitution * prevVelocityNormal, 0));
        /**/
        if (Math.Abs(velocityNormal) <= 2 * Math.Abs(PhysicsEngine.gravForce) * h)
        {
            restitution = 0;
            return;
        }

        if (hasFriction)
        {
            // Debug.DrawRay(b.position.ToVector3(), -frictionPair.frictionCol2.tangencialDir.ToVector3() * 10, Color.cyan, 0.1f);
            ApplyFriction(a, frictionCol, -1, rA, h);
            ApplyFriction(b, frictionCol, 1, rB, h);
        }


        p = p / massSum;
        a.ApplyRestitution(p * a.inverseMass, 1, rAworld);
        b.ApplyRestitution(p * b.inverseMass, -1, rBworld);
    }

    private void ApplyFriction(Particle particle, PBDFrictionCollision fricCol, double sign, DoubleVector3 r, double h)
    {
        if (!fricCol.appliedStaticFriction)
        {
//                Debug.Log("dynamic");
            ApplyDynamicFriction(particle, r, fricCol, sign, h);
        }
        /**/
        else
        {
            //   Debug.Log("static");
            ApplyStaticFriction(particle, fricCol, r, h, sign);
        }
    }

    private void ApplyDynamicFriction(Particle particle, DoubleVector3 r, PBDFrictionCollision fricCol, double sign, double h)
    {
        double dynamicFrictionCoeffiecient = (a.dynamicFrictionCoefficient + b.dynamicFrictionCoefficient) / 2.0;
        if (dynamicFrictionCoeffiecient == 0)
            return;
        double frictionForce = dynamicFrictionCoeffiecient * fricCol.normalForceLargrangeMult / h;
        DoubleVector3 velocityTangentNormal = sign * DoubleVector3.Normal(fricCol.tangencialDir);
        DoubleVector3 velocity = particle.GetVelocityAtCollisionPoint(r);
        if (DoubleVector3.MagnitudeSqr(velocity) == 0)
            return;
        DoubleVector3 velocityTangent = velocityTangentNormal * DoubleVector3.Dot(velocity, velocityTangentNormal);

        double velocityTangentMag = DoubleVector3.Magnitude(velocityTangent);
        DoubleVector3 frictionVelocity = velocityTangentNormal * Math.Min(Math.Abs(frictionForce), Math.Abs(velocityTangentMag));

        particle.ApplyRestitution(frictionVelocity, 1, particle.ProjectToWorldCoordinates(r));
        Debug.DrawRay(particle.position.ToVector3() + (particle.GetOrientation() * r).ToVector3(), frictionVelocity.ToVector3(), Color.cyan, 0.01f);
//        Debug.Log("dyn fric " + particle.name + " of "+ frictionVelocity);
    }

    private void ApplyStaticFriction(Particle particle, PBDFrictionCollision fricCol, DoubleVector3 r, double h, double sign)
    {
        if (DoubleVector3.Magnitude(particle.velocity) == 0)
            return;


        DoubleVector3 velocityTangentNormal = sign * DoubleVector3.Normal(fricCol.tangencialDir);
        DoubleVector3 velocity = particle.GetVelocityAtCollisionPoint(r);
        // Debug.DrawRay(particle.position.ToVector3() + (particle.GetOrientation() * r).ToVector3(), velocityTangentNormal.ToVector3(), Color.red, 0.01f);

        DoubleVector3 velocityTangent = velocityTangentNormal * DoubleVector3.Dot(velocity, velocityTangentNormal);

        velocityTangent = DoubleVector3.Magnitude(velocity) * -1 * DoubleVector3.Normal(velocityTangent);

        particle.ApplyRestitution(velocityTangent * h, 1, particle.ProjectToWorldCoordinates(r));
        Debug.DrawRay(particle.position.ToVector3() + (particle.GetOrientation() * r).ToVector3(),  velocityTangent.ToVector3(), Color.cyan, 0.01f);
    }

    public override string ToString()
    {
        return "(" + a.gameObject.name + ", " + b.gameObject.name + ", " + normal + ", " + correction + ")";
    }
}
