using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

[Serializable]
public class AlignAngleConstraint : PBDAngularConstraint
{
    public PBDRigidbody body;
    public PBDRigidbody otherBody;


    public Vector3 _a1 = Vector3.up;
    public DoubleVector3 a1;

    public Vector3 _a2 = Vector3.up;
    public DoubleVector3 a2;


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
        DoubleVector3 worldA2 = bodies[1].GetOrientation() * a2;
        DoubleQuaternion a1Q = new DoubleQuaternion(worldA1);
        DoubleQuaternion a2Q = new DoubleQuaternion(worldA1);

        DoubleQuaternion q = bodies[0].GetOrientation() * bodies[1].GetOrientation().Inverse();
        // Debug.Log("diff " + q.ToEuler() * Constants.Rad2Deg);
        deltaRotTarget = 2 * q.VectorPart();
        //  Debug.Log(DoubleVector3.Magnitude(deltaRotTarget) * Constants.Rad2Deg);
        return DoubleVector3.Magnitude(deltaRotTarget);
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
}
