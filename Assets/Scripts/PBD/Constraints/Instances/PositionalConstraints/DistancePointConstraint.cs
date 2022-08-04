using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DistancePointConstraint : PBDConstraint
{
    public double goalDistance = 0;


    public DoubleVector3 anchorPoint;
    protected DoubleVector3 bodyOffset;
    protected DoubleVector3 bodyDirection;


    private DoubleVector3 prevAnchor;
    private PhysicsEngine engine;

    private bool moving = false;
    private int substepCount = 0;
    private int currSubstep = 0;

    public DistancePointConstraint(Particle body, DoubleVector3 offset, double compliance, PhysicsEngine engine)
    {
        this.compliance = compliance;
        bodies.Add(body);
        bodyOffset = offset;
        anchorPoint = body.position + bodies[0].GetOrientation() * bodyOffset;
        this.engine = engine;
    }

    public void SetNewAnchor(DoubleVector3 anchor)
    {
        prevAnchor = anchorPoint;
        anchorPoint = anchor;
        moving = true;

        substepCount = engine.substeps;
        currSubstep = 1;
    }

    protected override DoubleVector3 GetGradient(int bodyIndex)
    {
        return bodyDirection * -1;
    }

    protected override double GetGradientMagnitude(int bodyIndex)
    {
        return 1;
    }

    public override double Evaluate()
    {
        if (!moving)
            return EvaluatePoint(anchorPoint);

        double percent = (double)currSubstep / (double)substepCount;
        //        Debug.Log(percent);
        if (percent >= 1)
        {
            moving = false;
            percent = 1;
            return EvaluatePoint(anchorPoint);
        }
        DoubleVector3 realAnchor = DoubleVector3.Lerp(prevAnchor, anchorPoint, percent);
        currSubstep++;
        return EvaluatePoint(realAnchor);
    }

    private double EvaluatePoint(DoubleVector3 point)
    {
        DoubleVector3 distanceVec = point - (bodies[0].position + bodies[0].GetOrientation() * bodyOffset);
        Debug.DrawLine((bodies[0].position + bodies[0].GetOrientation() * bodyOffset).ToVector3(), bodies[0].position.ToVector3(), Color.cyan, 0.1f);

        bodyDirection = DoubleVector3.Normal(distanceVec);

        double distance = DoubleVector3.Magnitude(distanceVec);
        double error = distance - goalDistance;

        if (Math.Abs(error) < accuracy)
            return 0;
        return error;
    }

    protected override void UpdateOrientation(DoubleVector3 correction, int index)
    {
        if (DoubleVector3.MagnitudeSqr(bodyOffset) > 0)
            bodies[index].ApplyCorrectionOrientation(correction, bodyOffset);
    }

    protected override DoubleVector3 GetBodyR(int index)
    {
        return bodyOffset;
    }

    public override void Solve(double deltaTime)
    {
        base.Solve(deltaTime);
        /* if(Evaluate() != 0)

             Debug.Log(Evaluate());*/
    }
}
