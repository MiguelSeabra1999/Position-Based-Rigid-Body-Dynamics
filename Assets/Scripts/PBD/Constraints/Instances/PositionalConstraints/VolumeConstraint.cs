using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
[Serializable]
public class VolumeConstraint : PBDConstraint
{
    public Particle body0 = null;
    public Particle body1 = null;
    public Particle body2 = null;
    public Particle body3 = null;
    public double goalVolume = 0;
    private DoubleVector3[] gradients = new DoubleVector3[4];
    private const double oneSixth = 1.0 / 6.0;


    // private bool invert = false;
    public override void Init(Particle[] allParticles)
    {
        bodies.Add(body0);
        bodies.Add(body1);
        bodies.Add(body2);
        bodies.Add(body3);
        if (goalVolume == 0)
            goalVolume = CalcVolume();
    }

    protected override DoubleVector3 GetGradient(int bodyIndex)
    {
        return gradients[bodyIndex];
    }

    protected  DoubleVector3 CalcGradient(int bodyIndex)
    {
        DoubleVector3 result;
        switch (bodyIndex)
        {
            case 0:
                result = DoubleVector3.Cross((bodies[3].position - bodies[1].position), (bodies[2].position - bodies[1].position));
                break;
            case 1:
                result = DoubleVector3.Cross((bodies[2].position - bodies[0].position), (bodies[3].position - bodies[0].position));
                break;
            case 2:
                result = DoubleVector3.Cross((bodies[3].position - bodies[0].position), (bodies[1].position - bodies[0].position));
                break;
            case 3:
                result = DoubleVector3.Cross((bodies[1].position - bodies[0].position), (bodies[2].position - bodies[0].position));
                break;
            default:
                result =  new DoubleVector3(0);
                break;
        }


        if (result.IsNan())
        {
            result =  new DoubleVector3(0);
        }

        return result;
    }

    protected override double GetSign(int i)
    {
        return 1;
    }

    protected override double GetGradientMagnitude(int bodyIndex)
    {
        // return 1;
        //   Debug.Log("mag " + DoubleVector3.Magnitude(gradients[bodyIndex]));
        return DoubleVector3.Magnitude(gradients[bodyIndex]);
    }

    public override double Evaluate()
    {
        //invert = false;
        double currentVolume = CalcVolume();
        double error =  6 * (currentVolume - goalVolume);


        for (int i = 0; i < 4; i++)
        {
            gradients[i] = DoubleVector3.Normal(CalcGradient(i));
        }

//        Debug.Log(currentVolume);
        return error;
    }

    private double CalcVolume()
    {
        DoubleVector3 cross = DoubleVector3.Cross((bodies[1].position - bodies[0].position), (bodies[2].position - bodies[0].position));


        if (DoubleVector3.Magnitude(cross) == 0)
            Debug.Log("zero mag in eval");
        if (cross.IsNan())
            Debug.Log("Nan gradient in eval");
        double dot = DoubleVector3.Dot(cross, (bodies[3].position - bodies[0].position));


        return oneSixth * dot;
    }
}
