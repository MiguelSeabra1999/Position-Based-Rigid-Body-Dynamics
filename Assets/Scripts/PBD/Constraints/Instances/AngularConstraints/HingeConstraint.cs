using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

[Serializable]
public class HingeConstraint : PBDAngularConstraint
{
    public PBDRigidbody body;
    public PBDRigidbody otherBody;
    public double bound;


    public Vector3 _a1 = Vector3.up;
    public DoubleVector3 a1;

    public Vector3 _a2 = Vector3.up;
    public DoubleVector3 a2;
    public Vector3 _rotAxis = Vector3.forward;
    public DoubleVector3 rotAxis;


    private DoubleVector3 deltaRotTarget = new DoubleVector3(0);


    public override void Init(Particle[] allParticles)
    {
        a1 = DoubleVector3.Normal(new DoubleVector3(_a1));
        a2 = DoubleVector3.Normal(new DoubleVector3(_a2));
        rotAxis = DoubleVector3.Normal(new DoubleVector3(_rotAxis));


        bodies.Add(body);
        bodies.Add(otherBody);
        base.Init(allParticles);
    }

    protected override double GetGradientMagnitude(int i)
    {
        return 1;
    }

    public override double Evaluate()
    {
        DoubleVector3 worldA1 = bodies[0].GetOrientation() * a1;
        DoubleVector3 worldA2 = bodies[1].GetOrientation() * a2;
        DoubleVector3 worldRotAxis = bodies[0].GetOrientation() * rotAxis;

        DoubleVector3 error = DoubleVector3.Cross(worldA1, worldA2);
        deltaRotTarget = DoubleVector3.Normal(error);


        return DoubleVector3.Magnitude(error);
    }

    protected override DoubleVector3 GetGradient(int i)
    {
        return deltaRotTarget;
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
         double newError = Evaluate();
         if (newError >= 0.01)
         {
             Debug.Log("Hinge diverge" + newError );
             // Debug.Break();
         }
     }*/
}
