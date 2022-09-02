using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class DistanceConstraint : PBDConstraint
{
    public double minDistanceThreshold = 0;
    public double maxDistanceThreshold = 0;
    private double baseCompliance = 0;
    public bool worldSpace1 = false;
    public bool worldSpace2 = false;
    public double goalDistance;
    public int firstBodyIndex;
    public int secondBodyIndex;
    public Particle body = null;
    public Particle otherBody = null;


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
        baseCompliance = compliance;
        if (body == null || otherBody == null)
        {
            bodies.Add(allParticles[firstBodyIndex]);
            bodies.Add(allParticles[secondBodyIndex]);
        }
        else
        {
            bodies.Add(body);
            bodies.Add(otherBody);
        }
        firstBodyOffset = new DoubleVector3(firstBodyOffsetFloat);
        secondBodyOffset = new DoubleVector3(secondBodyOffsetFloat);
    }

    protected override DoubleVector3 GetGradient(int bodyIndex)
    {
        return bodyDirection;
    }

    protected override double GetSign(int i)
    {
        if (i == 0)
            return -1;
        return 1;
    }

    protected override double GetGradientMagnitude(int bodyIndex)
    {
        return 1;
    }

    protected override DoubleVector3 GetBodyR(int index)
    {
        if (index == 0)
            return firstBodyOffset;
        else
            return secondBodyOffset;
    }

    public override double Evaluate()
    {
        DoubleVector3 realOffset1 = firstBodyOffset;
        if (!worldSpace1)
            realOffset1 = bodies[0].GetOrientation() * firstBodyOffset;
        DoubleVector3 realOffset2 = secondBodyOffset;
        if (!worldSpace2)
            realOffset2 = bodies[1].GetOrientation() * secondBodyOffset;
        DoubleVector3 distanceVec = (bodies[1].position + realOffset2) -  (bodies[0].position + realOffset1);
        Debug.DrawLine((bodies[1].position + realOffset2).ToVector3(), (bodies[0].position + realOffset1).ToVector3());
        bodyDirection = DoubleVector3.Normal(distanceVec);

        double distance = DoubleVector3.Magnitude(distanceVec);
        double error = distance  - goalDistance;

        if (Math.Abs(error) < accuracy)
            return 0;

        if ((distance > maxDistanceThreshold && maxDistanceThreshold != 0) || (distance < minDistanceThreshold && minDistanceThreshold != 0))
            compliance = 0;
        else
            compliance = baseCompliance;

        return error;
    }

    protected override void UpdateOrientation(DoubleVector3 correction, double sign, int index)
    {
        if (index == 0 && worldSpace1)
            return;
        if (index == 1 && worldSpace2)
            return;
        DoubleVector3 offset = firstBodyOffset;
        if (index == 1)
            offset = secondBodyOffset;
        if (DoubleVector3.MagnitudeSqr(offset) > 0)
        {
            bodies[index].ApplyCorrectionOrientation(correction, sign, offset);
        }
    }

    protected override DoubleQuaternion GetOrientationCorrection(DoubleVector3 correction, double sign, int index)
    {
        if (index == 0 && worldSpace1)
            return new DoubleQuaternion(0, 0, 0, 0);
        if (index == 1 && worldSpace2)
            return new DoubleQuaternion(0, 0, 0, 0);
        DoubleVector3 offset = firstBodyOffset;
        if (index == 1)
            offset = secondBodyOffset;
        if (DoubleVector3.MagnitudeSqr(offset) > 0)
        {
            return bodies[index].GetCorrectionOrientation(correction, sign, offset);
        }
        return new DoubleQuaternion(0, 0, 0, 0);
    }
}
