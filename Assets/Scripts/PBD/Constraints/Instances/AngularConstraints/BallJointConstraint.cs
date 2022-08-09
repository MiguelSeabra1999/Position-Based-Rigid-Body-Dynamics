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
    /*  public Vector3 _b1;
      public DoubleVector3 b1;
      public Vector3 _c1;
      public DoubleVector3 c1;*/
    public Vector3 _a2 = Vector3.up;
    public DoubleVector3 a2;
    /* public Vector3 _b2;
     public DoubleVector3 b2;
     public Vector3 _c2;
     public DoubleVector3 c2;
     public Vector3 _r1;
     public DoubleVector3 r1;
     public Vector3 _r2;
     public DoubleVector3 r2;*/

    private DoubleVector3 deltaRotTarget = new DoubleVector3(0);

    public override void Init(Particle[] allParticles)
    {
        base.Init(allParticles);
        a1 = DoubleVector3.Normal(new DoubleVector3(_a1));
        /*  b1 = DoubleVector3.Normal(new DoubleVector3(_b1));
          c1 = DoubleVector3.Normal(new DoubleVector3(_c1));*/
        a2 = DoubleVector3.Normal(new DoubleVector3(_a2));
        /* b2 = DoubleVector3.Normal(new DoubleVector3(_b2));
         c2 = DoubleVector3.Normal(new DoubleVector3(_c2));
         r1 = new DoubleVector3(_r1);
         r2 = new DoubleVector3(_r2);*/
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
        //   DoubleVector3 worldB1 = bodies[0].GetOrientation() * b1;
        // DoubleVector3 worldC1 = bodies[0].GetOrientation() * c1;
        DoubleVector3 worldA2 = bodies[1].GetOrientation() * a2;
        //  DoubleVector3 worldB2 = bodies[1].GetOrientation() * b2;
        // DoubleVector3 worldC2 = bodies[1].GetOrientation() * c2;
        // DoubleVector3 worldR     = bodies[0].GetOrientation() * r1;
        //  DoubleVector3 worldR2    = bodies[1].GetOrientation() * r2;

        //Debug.DrawRay(bodies[0].position.ToVector3(), worldA1.ToVector3()*10,Color.black, 0.1f );
        //Debug.DrawRay(bodies[1].position.ToVector3(), worldA2.ToVector3()*10,Color.black, 0.1f );

        deltaRotTarget = DoubleVector3.Cross(worldA2, worldA1);


        // Debug.Log("deltaRot" + deltaRotTarget);


        //todo Asin returns Nan for magnitude greater than 1 which it might be since cross(a,b) == |a||b|sin(angle) and angle is between
        double angle = Math.Asin(Math.Min(DoubleVector3.Magnitude(deltaRotTarget), 1));
        if (angle == 0)
            deltaRotTarget = worldA1.FindPerpendicularVector();
        if (DoubleVector3.Dot(worldA1, worldA2) < 0)
        {
            angle = Math.PI - angle;
        }

        //Debug.Log("ball");
        double boundInRads = bound * Constants.Deg2Rad;
        //  Debug.Log("angle " + angle + " " + boundInRads);
        if (angle < boundInRads)
            return 0;

        return angle - boundInRads;
    }

    protected override DoubleVector3 GetGradient(int i)
    {
        return DoubleVector3.Normal(deltaRotTarget);
    }

    protected override double GetSign(int i)
    {
        if (i == 0)
            return -1;
        return 1;
    }

    public override void Solve(double deltaTime)
    {
        base.Solve(deltaTime);
        double newError = Evaluate();
        if (newError >= 0.1)
        {
            Debug.Log("Ball Joint diverge" + newError);
            // Debug.Break();
        }
    }
}
