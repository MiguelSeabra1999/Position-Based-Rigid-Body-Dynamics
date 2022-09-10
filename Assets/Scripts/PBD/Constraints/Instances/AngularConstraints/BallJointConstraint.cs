using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

[Serializable]
public class BallJointConstraint : PBDAngularConstraint
{
    public PBDRigidbody body;
    public PBDRigidbody otherBody;
    public double bound;


    public Vector3 _a1 = Vector3.up;
    public DoubleVector3 a1;
    public Vector3 _b1;
    public DoubleVector3 b1;

    public Vector3 _a2 = Vector3.up;
    public DoubleVector3 a2;
    public Vector3 _b2;
    public DoubleVector3 b2;


    private DoubleVector3 deltaRotTarget = new DoubleVector3(0);

    public override void Init(Particle[] allParticles)
    {
        base.Init(allParticles);
        bound *= Constants.Deg2Rad;

        a1 = DoubleVector3.Normal(new DoubleVector3(_a1));
        b1 = DoubleVector3.Normal(new DoubleVector3(_b1));

        a2 = DoubleVector3.Normal(new DoubleVector3(_a2));
        b2 = DoubleVector3.Normal(new DoubleVector3(_b2));

        bodies.Add(body);
        bodies.Add(otherBody);
    }

    protected override double GetGradientMagnitude(int i)
    {
        return 1;
    }

    public override double Evaluate()
    {
        DoubleVector3 worldA1 = bodies[0].GetOrientation() * a1;
        DoubleVector3 worldA2 = bodies[1].GetOrientation() * a2;
        DoubleVector3 worldB1 = bodies[0].GetOrientation() * b1;
        DoubleVector3 worldB2 = bodies[1].GetOrientation() * b2;
        if (worldA1 == worldA2)
            return 0;

        DoubleVector3 error = new DoubleVector3(0);
        bool isError = false;
        //limit swing
        DoubleVector3 n = DoubleVector3.Cross(worldA1, worldA2);
        double angle = DoubleVector3.AngleBetween(worldA1, worldA2);
        if (angle > bound)
        {
            error = n * (angle - bound);
            isError = true;
        }

        //limit twist
        /*   DoubleVector3 sum = worldA1 + worldA2;
           n = sum / DoubleVector3.Magnitude(sum);
           DoubleVector3 n1 = DoubleVector3.Normal(worldB1 - DoubleVector3.Dot(n, worldB1) * n);
           DoubleVector3 n2 = DoubleVector3.Normal(worldB2 - DoubleVector3.Dot(n, worldB2) * n);
           angle = DoubleVector3.AngleBetween(n1, n2);
           if (angle > 1)
           {
               error += n * (angle - bound);
               isError = true;
           }*/

        if (!isError)
            return 0;
        deltaRotTarget = DoubleVector3.Normal(error);
        return DoubleVector3.Magnitude(error);
    }

    protected override DoubleVector3 GetGradient(int i)
    {
        return DoubleVector3.Normal(deltaRotTarget);
    }

    protected override double GetSign(int i)
    {
        if (i == 0)
            return 1;
        return -1;
    }

    /* public override void Solve(double deltaTime)
     {
         base.Solve(deltaTime);
         if (errorBackup != 0)
         {
             DoubleVector3 worldA1 = bodies[0].GetOrientation() * a1;
             DoubleVector3 worldA2 = bodies[1].GetOrientation() * a2;
             Debug.Log(DoubleVector3.AngleBetween(worldA1, worldA2) * Constants.Rad2Deg);
         }
         double newError = Evaluate();
         if (newError >= 0.1)
         {
             Debug.Log("Ball Joint diverge" + newError);
             // Debug.Break();
         }
     }*/
}
