using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public abstract class PBDAngularConstraint : PBDConstraint
{
    public override void Init(Particle[] allParticles)
    {
    }

    protected override double GetLagrangeMultiplier(double error, double deltaTime)
    {
        double denominator = 0;
        for (int i = 0; i < bodies.Count; i++)
        {
            double gradientMag = GetGradientMagnitude(i);
            double wi = bodies[i].GetGeneralizedInverseMass(GetGradient(i));
            denominator += wi * gradientMag * gradientMag;
        }
        denominator += compliance / (deltaTime * deltaTime);

        return error  / denominator;
    }

    public override void Solve(double deltaTime)
    {
        double error = Evaluate();

        if (error == 0)
            return;

        lagrangeMult = GetLagrangeMultiplier(error, deltaTime);


        if (LagrangeMultConstraint())
            return;

        for (int i = 0; i < bodies.Count; i++)
        {
            //Debug.DrawLine(bodies[i].position.ToVector3(), (bodies[i].position + correction).ToVector3(), Color.yellow);
            double wi = bodies[i].GetGeneralizedInverseMass(GetGradient(i));
            //  Debug.Log("error " + error + " lag " + lagrangeMult*wi);
            // DoubleQuaternion correction = new DoubleQuaternion(lagrangeMult*wi*Constants.Rad2Deg, GetGradient(i));

            /*  if (wi == 0)
                  continue;
              DoubleVector3 corr = GetGradient(i) * lagrangeMult;
              corr = bodies[i].GetOrientation().Inverse() * corr;
              corr = bodies[i].GetInertiaTensorInverted() * corr;
              corr = bodies[i].GetOrientation() * corr;


              bodies[i].SetRotation(bodies[i].GetOrientation() + GetSign(i) * 0.5 * corr * bodies[i].GetOrientation());*/


            DoubleQuaternion correction = (GetSign(i) * GetGradient(i) * lagrangeMult * wi).ToQuaternion();
            bodies[i].SetRotation(DoubleQuaternion.Normal(correction * bodies[i].GetOrientation()));
            /*if(i == 1)
            {
            Debug.Log("correction " + GetGradient(i)*lagrangeMult*wi*Constants.Rad2Deg);
            Debug.Log("gradient " + GetGradient(i));
            Debug.Log("lagrange " + lagrangeMult*Constants.Rad2Deg);
            Debug.Log("wi " + wi);
            Debug.Log("rotation " + bodies[i].GetOrientation().ToEuler()*Constants.Rad2Deg);

            }*/
            //continue;

            /*
              DoubleVector3 correctionVec = GetGradient(i) * lagrangeMult * wi;
               DoubleVector3 correctionSelfCoords = bodies[i].ProjectToSelfCoordinates(correctionVec);

              correctionSelfCoords = bodies[i].GetInertiaTensorInverted()*correctionSelfCoords;
              correctionVec = bodies[i].GetOrientation()*correctionSelfCoords;*/
        }
    }

//limits the angle between the axes n1 and n2 of two bodies to be in the interval [a,b] using the common rotation axis n.
    public DoubleVector3 LimitAngle(DoubleVector3 n, DoubleVector3 n1, DoubleVector3 n2, double a, double b)
    {
        DoubleVector3 cross = DoubleVector3.Cross(n1, n2);
        double fi = Math.Asin(DoubleVector3.Dot(cross, n));
        if (DoubleVector3.Dot(n1, n2) < 0)
            fi = Math.PI - fi;
        if (fi > Math.PI)
            fi = fi - 2 * Math.PI;
        if (fi < -Math.PI)
            fi = fi + 2 * Math.PI;
        if (fi < a || fi > b)
        {
            //fi = Math.Clamp(fi, a, b);
            n1 = DoubleVector3.Rotate(n1, n, fi);
            return DoubleVector3.Cross(n1, n2);
        }
        return new DoubleVector3(0);
    }
}
