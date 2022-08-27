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
    private const double oneSixth = 1.0 / 6.0;
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
        switch (bodyIndex)
        {
            case 0:
                return DoubleVector3.Normal(DoubleVector3.Cross((bodies[3].position - bodies[1].position), (bodies[2].position - bodies[1].position)));
            case 1:
                return DoubleVector3.Normal(DoubleVector3.Cross((bodies[2].position - bodies[0].position), (bodies[3].position - bodies[0].position)));
            case 2:
                return DoubleVector3.Normal(DoubleVector3.Cross((bodies[3].position - bodies[0].position), (bodies[1].position - bodies[0].position)));
            case 3:
                return DoubleVector3.Normal(DoubleVector3.Cross((bodies[1].position - bodies[0].position), (bodies[2].position - bodies[0].position)));
            default:
                return new DoubleVector3(0);
        }
    }

    protected override double GetSign(int i)
    {
        return 1;
    }

    protected override double GetGradientMagnitude(int bodyIndex)
    {
        return 1;
        /* switch (bodyIndex)
         {
             case 0:
                 return DoubleVector3.Magnitude(DoubleVector3.Cross((bodies[3].position - bodies[1].position), (bodies[2].position - bodies[1].position)));
             case 1:
                 return DoubleVector3.Magnitude(DoubleVector3.Cross((bodies[2].position - bodies[0].position), (bodies[3].position - bodies[0].position)));
             case 2:
                 return DoubleVector3.Magnitude(DoubleVector3.Cross((bodies[3].position - bodies[0].position), (bodies[1].position - bodies[0].position)));
             case 3:
                 return DoubleVector3.Magnitude(DoubleVector3.Cross((bodies[1].position - bodies[0].position), (bodies[2].position - bodies[0].position)));
             default:
                 return 0;
         }*/
    }

    public override double Evaluate()
    {
        return 6 * (CalcVolume() - goalVolume);
    }

    private double CalcVolume()
    {
        DoubleVector3 cross = DoubleVector3.Cross((bodies[1].position - bodies[0].position), (bodies[2].position - bodies[0].position));
        double dot = DoubleVector3.Dot(cross, (bodies[3].position - bodies[0].position));

        return oneSixth * dot;
    }
}
