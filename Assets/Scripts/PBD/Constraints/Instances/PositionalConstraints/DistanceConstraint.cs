using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class DistanceConstraint : PBDConstraint
{
    public double goalDistance;
    public int firstBodyIndex;
    public int secondBodyIndex;
    public Vector3 firstBodyOffsetFloat;
    public Vector3 secondBodyOffsetFloat;
    public DoubleVector3 firstBodyOffset;
    public DoubleVector3 secondBodyOffset;
    protected DoubleVector3 bodyDirection;
    
/*
    DistanceConstraint(int _firstBodyIndex, int _secondBodyIndex, float, _goalDistance, double _compliance) : base(_compliance)
    {
        firstBodyIndex = _firstBodyIndex;
        secondBodyIndex = _secondBodyIndex;
        goalDistance = _goalDistance;
        Init();
      
    }
*/
    public override void Init(Particle[] allParticles)
    {
        bodies.Add(allParticles[firstBodyIndex]);
        bodies.Add(allParticles[secondBodyIndex]);
        firstBodyOffset = new DoubleVector3(firstBodyOffsetFloat);
        secondBodyOffset = new DoubleVector3(secondBodyOffsetFloat);
    }
    protected override DoubleVector3 GetGradient(int bodyIndex)
    {
        if (bodyIndex == 0 )
        {
            return bodyDirection * -1;
        }
        return bodyDirection;
    }

    protected override double GetGradientMagnitude(int bodyIndex)
    {
        return 1;
    }

    protected override DoubleVector3 GetBodyR(int index)
    {
        if(index == 0)
            return firstBodyOffset;
        else
            return secondBodyOffset;
    }

    protected override double Evaluate()
    {

        DoubleVector3 distanceVec = (bodies[1].position + bodies[1].GetOrientation() * secondBodyOffset) -  (bodies[0].position + bodies[0].GetOrientation() * firstBodyOffset);
        Debug.DrawLine((bodies[1].position + bodies[1].GetOrientation() * secondBodyOffset).ToVector3(),(bodies[0].position + bodies[0].GetOrientation() * firstBodyOffset).ToVector3());
        bodyDirection = DoubleVector3.Normal(distanceVec);

        double distance = DoubleVector3.Magnitude(distanceVec);
        double error = distance  - goalDistance;

        if (Math.Abs(error) < accuracy)
            return 0;
        return error;
    }
    protected override void UpdateOrientation(DoubleVector3 correction, int index)
    {

        DoubleVector3 offset = firstBodyOffset;
        if(index == 1)
            offset = secondBodyOffset;
        if(DoubleVector3.MagnitudeSqr(offset) > 0)
        {
            bodies[index].ApplyCorrectionOrientation(correction,offset);
  
        }
    }
}