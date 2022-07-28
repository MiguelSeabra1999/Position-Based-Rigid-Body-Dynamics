
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
        base.Init(allParticles);
        a1 = DoubleVector3.Normal(new DoubleVector3(_a1));
        a2 = DoubleVector3.Normal(new DoubleVector3(_a2));
        rotAxis = DoubleVector3.Normal(new DoubleVector3(_rotAxis));
     

        bodies.Add(body);
        bodies.Add(otherBody);
    }
    protected override double GetGradientMagnitude(int i)
    {
        return 1;
    }
    protected override double Evaluate()
    {
        DoubleVector3 worldA1 = bodies[0].GetOrientation() * a1;


        DoubleVector3 worldA2 = bodies[1].GetOrientation() * a2;


        deltaRotTarget = LimitAngle(rotAxis,worldA1,worldA2,0,0);


        DoubleVector3 parallelComponent = DoubleVector3.Dot(deltaRotTarget,rotAxis)*rotAxis;
        DoubleVector3 perpComponent = deltaRotTarget - parallelComponent;

        deltaRotTarget = parallelComponent;

        double angle = Math.Asin(Math.Min(DoubleVector3.Magnitude(deltaRotTarget),1));
        
        Debug.Log("angle " + angle*Constants.Rad2Deg);
        Debug.Log(deltaRotTarget);
        return angle;
    }
    protected override DoubleVector3 GetGradient(int i)
    {
        if(i ==0)
            return DoubleVector3.Normal(deltaRotTarget);
        return -1*DoubleVector3.Normal(deltaRotTarget);
    }
    protected override DoubleQuaternion GetGradientAngle(int i)
    {
        DoubleQuaternion rot = new DoubleQuaternion(DoubleVector3.Magnitude(deltaRotTarget), DoubleVector3.Normal(deltaRotTarget));
        if(i == 1)
            return rot.Inverse();
        return rot;
    }


}
