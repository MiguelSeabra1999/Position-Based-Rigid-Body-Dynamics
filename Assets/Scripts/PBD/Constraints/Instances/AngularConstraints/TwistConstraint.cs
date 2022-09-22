using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

[Serializable]
public class TwistConstraint : PBDAngularConstraint
{
    public PBDRigidbody body;
    public PBDRigidbody otherBody;


    public Vector3 _a1 = Vector3.up;
    public DoubleVector3 a1;
    public Vector3 _b1 = Vector3.right;
    public DoubleVector3 b1;
    /*public Vector3 _c1;
    public DoubleVector3 c1;*/
    public Vector3 _a2 = Vector3.up;
    public DoubleVector3 a2;
    public Vector3 _b2 = Vector3.right;
    public DoubleVector3 b2;
    /*   public Vector3 _c2;
       public DoubleVector3 c2;
       public Vector3 _r1;
       public DoubleVector3 r1;
       public Vector3 _r2;
       public DoubleVector3 r2;*/

    private DoubleVector3 deltaRotTarget = new DoubleVector3(0);

    public override void Init(Particle[] allParticles)
    {
        a1 = DoubleVector3.Normal(new DoubleVector3(_a1));
        b1 = DoubleVector3.Normal(new DoubleVector3(_b1));
        //c1 = DoubleVector3.Normal(new DoubleVector3(_c1));
        a2 = DoubleVector3.Normal(new DoubleVector3(_a2));
        b2 = DoubleVector3.Normal(new DoubleVector3(_b2));
        /*c2 = DoubleVector3.Normal(new DoubleVector3(_c2));
        r1 = new DoubleVector3(_r1);
        r2 = new DoubleVector3(_r2);*/
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
        DoubleVector3 up = new DoubleVector3(Vector3.up);

        double angle0 =  DoubleVector3.AngleBetween(worldA1, up);
        DoubleVector3 dir0 = DoubleVector3.Cross(worldA1, up);
        DoubleQuaternion body0rot = new DoubleQuaternion(angle0, dir0);

        double angle1 =  DoubleVector3.AngleBetween(worldA2, up);
        DoubleVector3 dir1 = DoubleVector3.Cross(worldA2, up);
        DoubleQuaternion body1rot = new DoubleQuaternion(angle1, dir1);

        DoubleQuaternion body0oriented = body0rot * bodies[0].GetOrientation();
        DoubleQuaternion body1oriented = body1rot * bodies[1].GetOrientation();


        DoubleVector3 orientedB1 = body0oriented * b1;
        DoubleVector3 orientedB2 = body1oriented * b2;


        double angle = DoubleVector3.SignedAngleBetween(orientedB1, orientedB2, a1);


        return angle;
    }

/*  public override double Evaluate()
    {
        DoubleVector3 worldA1 = bodies[0].GetOrientation() * a1;
        DoubleVector3 worldB1 = bodies[0].GetOrientation() * b1;
        // DoubleVector3 worldC1 = bodies[0].GetOrientation() * c1;
        DoubleVector3 worldA2 = bodies[1].GetOrientation() * a2;
        DoubleVector3 worldB2 = bodies[1].GetOrientation() * b2;
        // DoubleVector3 worldC2 = bodies[1].GetOrientation() * c2;
        // DoubleVector3 worldR     = bodies[0].GetOrientation() * r1;
        //  DoubleVector3 worldR2    = bodies[1].GetOrientation() * r2;

        DoubleVector3 sum = (worldA1 + worldA2);
        DoubleVector3 n = sum / DoubleVector3.Normal(sum);
        DoubleVector3 n1 = worldB1 - DoubleVector3.Dot(n, worldB1) * n;
        n1 = DoubleVector3.Normal(n1);
        DoubleVector3 n2 = worldB2 - DoubleVector3.Dot(n, worldB2) * n;
        n2 = DoubleVector3.Normal(n2);
        deltaRotTarget = LimitAngle(n, n1, n2, 0, 0);
        double angle = Math.Asin(Math.Min(DoubleVector3.Magnitude(deltaRotTarget), 1));
        // Debug.Log("twist");

        return angle;
    }*/

    protected override DoubleVector3 GetGradient(int i)
    {
        DoubleVector3 axis;
        if (i == 0)
            axis = a1;
        else
            axis = a2;
        return bodies[i].GetOrientation() * axis;
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
         if (newError >= 0.1)
         {
             Debug.Log("Twist diverge" + newError);
             // Debug.Break();
         }
     }*/
}
