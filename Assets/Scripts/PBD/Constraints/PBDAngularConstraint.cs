using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public abstract class PBDAngularConstraint : PBDConstraint
{
    public override void Init(Particle[] allParticles)
    {
    }

    protected override double GetLagrangeMultiplier(double error, double deltaTime)
    {
        double denominator = 0;
        for (int i = 0; i < bodies.Count; i++)
        {
            double gradientMag = GetGradientMagnitude(i);
            double wi = bodies[i].GetGeneralizedInverseMass(GetGradient(i));
            denominator += wi * gradientMag * gradientMag;
        }
        denominator += compliance / (deltaTime * deltaTime);

        return error  / denominator;
    }

    public override void Solve(double deltaTime)
    {
        /*  Solve(deltaTime, (i , correctionQ, lagrangeMult) =>
          {
              bodies[i].SetRotation(bodies[i].GetOrientation() + GetSign(i) * correctionQ);
          });*/
        double error = Evaluate();

        if (error == 0)
            return;

        lagrangeMult = GetLagrangeMultiplier(error, deltaTime);


        if (LagrangeMultConstraint(deltaTime))
            return;

        for (int i = 0; i < bodies.Count; i++)
        {
            if (bodies[i].inverseMass == 0)
                continue;
            DoubleVector3 p = GetGradient(i) *  lagrangeMult;
            p = bodies[i].GetOrientation().Inverse() * p;
            DoubleVector3 correction = bodies[i].GetOrientation() * (bodies[i].GetInertiaTensorInverted() * p);
            //DoubleVector3 correction = GetGradient(i) * bodies[i].GetGeneralizedInverseMass(GetGradient(i));
            DoubleQuaternion correctionQ = 0.5 * new DoubleQuaternion(0, correction.x, correction.y, correction.z) * bodies[i].GetOrientation();
            bodies[i].SetRotation(bodies[i].GetOrientation() + GetSign(i) * correctionQ);
        }
    }

    public override void GetContribution(double deltaTime, List<List<Correction>> corrections)
    {
        /*Solve(deltaTime, (i, correctionQ, lagrangeMult) =>
        {
            corrections[bodies[i].indexID].Add(new Correction(new DoubleVector3(0), GetSign(i) * correctionQ));
        });*/
        double error = Evaluate();

        if (error == 0)
            return;

        lagrangeMult = GetLagrangeMultiplier(error, deltaTime);


        if (LagrangeMultConstraint(deltaTime))
            return;

        for (int i = 0; i < bodies.Count; i++)
        {
            if (bodies[i].inverseMass == 0)
                continue;
            DoubleVector3 p = GetGradient(i) *  lagrangeMult;
            p = bodies[i].GetOrientation().Inverse() * p;
            DoubleVector3 correction = bodies[i].GetOrientation() * (bodies[i].GetInertiaTensorInverted() * p);
            //DoubleVector3 correction = GetGradient(i) * bodies[i].GetGeneralizedInverseMass(GetGradient(i));
            DoubleQuaternion correctionQ = 0.5 * new DoubleQuaternion(0, correction.x, correction.y, correction.z) * bodies[i].GetOrientation();
            corrections[bodies[i].indexID].Add(new Correction(new DoubleVector3(0), GetSign(i) * correctionQ, lagrangeMult));
        }
    }

    public override void ParallelGetContribution(double deltaTime, List<List<Correction>> corrections)
    {
        /* Solve(deltaTime, (i, correctionQ, lagrangeMult) =>
         {
             lock (corrections)
                 corrections[bodies[i].indexID].Add(new Correction(new DoubleVector3(0), GetSign(i) * correctionQ));
         });*/
        double error = Evaluate();

        if (error == 0)
            return;

        lagrangeMult = GetLagrangeMultiplier(error, deltaTime);


        if (LagrangeMultConstraint(deltaTime))
            return;

        for (int i = 0; i < bodies.Count; i++)
        {
            if (bodies[i].inverseMass == 0)
                continue;
            DoubleVector3 p = GetGradient(i) *  lagrangeMult;
            p = bodies[i].GetOrientation().Inverse() * p;
            DoubleVector3 correction = bodies[i].GetOrientation() * (bodies[i].GetInertiaTensorInverted() * p);
            //DoubleVector3 correction = GetGradient(i) * bodies[i].GetGeneralizedInverseMass(GetGradient(i));
            DoubleQuaternion correctionQ = 0.5 * new DoubleQuaternion(0, correction.x, correction.y, correction.z) * bodies[i].GetOrientation();
            lock (corrections)
                corrections[bodies[i].indexID].Add(new Correction(new DoubleVector3(0), GetSign(i) * correctionQ, lagrangeMult));
        }
    }

    public  void Solve(double deltaTime, Action<int, DoubleQuaternion, double> updateFunc)
    {
        double error = Evaluate();

        if (error == 0)
            return;

        lagrangeMult = GetLagrangeMultiplier(error, deltaTime);


        if (LagrangeMultConstraint(deltaTime))
            return;

        for (int i = 0; i < bodies.Count; i++)
        {
            if (bodies[i].inverseMass == 0)
                continue;
            DoubleVector3 p = GetGradient(i) *  lagrangeMult;
            p = bodies[i].GetOrientation().Inverse() * p;
            DoubleVector3 correction = bodies[i].GetOrientation() * (bodies[i].GetInertiaTensorInverted() * p);
            //DoubleVector3 correction = GetGradient(i) * bodies[i].GetGeneralizedInverseMass(GetGradient(i));
            DoubleQuaternion correctionQ = 0.5 * new DoubleQuaternion(0, correction.x, correction.y, correction.z) * bodies[i].GetOrientation();
            updateFunc(i, correctionQ, lagrangeMult);
        }
    }

//limits the angle between the axes n1 and n2 of two bodies to be in the interval [a,b] using the common rotation axis n.
    public DoubleVector3 LimitAngle(DoubleVector3 n, DoubleVector3 n1, DoubleVector3 n2, double a, double b)
    {
        DoubleVector3 cross = DoubleVector3.Cross(n1, n2);
        double fi = Math.Asin(DoubleVector3.Dot(cross, n));
        if (DoubleVector3.Dot(n1, n2) < 0)
            fi = 2 * Math.PI - fi;
        if (fi > Math.PI)
            fi = fi - 2 * Math.PI;
        if (fi < -Math.PI)
            fi = fi + 2 * Math.PI;
        if (fi < a || fi > b)
        {
            fi = Math.Clamp(fi, a, b);
            n1 = new DoubleQuaternion(fi, n) * n1;
            DoubleVector3.Cross(n1, n2);
            return DoubleVector3.Cross(n1, n2);
        }
        return new DoubleVector3(0);
    }
}
