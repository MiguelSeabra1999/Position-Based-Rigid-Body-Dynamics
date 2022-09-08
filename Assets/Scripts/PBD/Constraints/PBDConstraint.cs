using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public abstract class PBDConstraint
{
    protected List<Particle> bodies = new List<Particle>();
    public double compliance = 0;
    [HideInInspector] public double lagrangeMult;

    protected double accuracy = 0.00001f;


    /*

    protected Constraint(double _compliance)
    {
        compliance = _compliance;
    }
*/
    public virtual void Init(Particle[] allParticles) {}
    protected abstract double GetGradientMagnitude(int i);
    public abstract double Evaluate();
    protected abstract DoubleVector3 GetGradient(int i);
    protected abstract double GetSign(int i);

    protected virtual bool LagrangeMultConstraint() {return false;}
    protected virtual DoubleVector3 GetBodyR(int index)
    {
        return new DoubleVector3(0, 0, 0);
    }

    protected virtual double GetLagrangeMultiplier(double error, double deltaTime)
    {
        double denominator = 0;
        for (int i = 0; i < bodies.Count; i++)
        {
            double gradientMag = GetGradientMagnitude(i);
            double wi = bodies[i].GetGeneralizedInverseMass(GetGradient(i), GetBodyR(i));
            denominator += wi  * gradientMag * gradientMag;
        }
        denominator += compliance / (deltaTime * deltaTime);

        if (denominator == 0)
            Debug.Log("Nan lagrange Multiplier");

        return -1 * error / denominator;
    }

    public virtual void GetContribution(double deltaTime, List<List<Correction>> corrections)
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


            double wi = bodies[i].GetGeneralizedInverseMass(GetGradient(i), GetBodyR(i));

            if (bodies[i].inverseMass != 0)
            {
                double gradientMag = GetGradientMagnitude(i);
                DoubleVector3 correction = GetGradient(i) * lagrangeMult *  (1 / bodies[i].mass);
                DoubleVector3 positional = GetSign(i) * correction;
                DoubleQuaternion rotational = GetOrientationCorrection(GetGradient(i) * lagrangeMult , GetSign(i), i);

                //  Debug.Log("Attempting to add " + positional + " to body " + bodies[i].indexID);
                corrections[bodies[i].indexID].Add(new Correction(positional, rotational));
            }


            if (bodies[i].GetOrientation().IsNan())
                Debug.Log("Nan found rotation" + bodies[i].gameObject);
            if (bodies[i].position.IsNan())
                Debug.Log("Nan found position" + bodies[i].gameObject);
            if (bodies[i].position.IsInfinite())
                Debug.Log("Infinite found position" + bodies[i].gameObject);
            //bodies[i].prevPosition += correction;
        }


        /*    double newError = Evaluate();
             if(newError != 0)
                 Debug.Log("error " + newError);*/
    }

    public virtual void Solve(double deltaTime)
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


            double wi = bodies[i].GetGeneralizedInverseMass(GetGradient(i), GetBodyR(i));

            if (bodies[i].inverseMass != 0)
            {
                double gradientMag = GetGradientMagnitude(i);
                DoubleVector3 correction = GetGradient(i) * gradientMag *   lagrangeMult *  (1 / bodies[i].mass);
                bodies[i].position += GetSign(i) * correction;
                UpdateOrientation(GetGradient(i) * lagrangeMult , GetSign(i), i);

                //   Debug.DrawRay(bodies[i].position.ToVector3(), GetGradient(i).ToVector3(), Color.white, 0.01f);
            }


            if (bodies[i].GetOrientation().IsNan())
                Debug.Log("Nan found rotation" + bodies[i].gameObject);
            if (bodies[i].position.IsNan())
                Debug.Log("Nan found position" + bodies[i].gameObject);
            if (bodies[i].position.IsInfinite())
            {
                Debug.Log("Infinite found position" + bodies[i].gameObject);
                Debug.Break();
            }
            //bodies[i].prevPosition += correction;
        }


        /*  double newError = Evaluate();
          if (newError != 0)
              Debug.Log("error " + newError);*/
    }

    protected virtual void UpdateOrientation(DoubleVector3 correction, double sign, int index)
    {
        return;
    }

    protected virtual DoubleQuaternion GetOrientationCorrection(DoubleVector3 correction, double sign, int index)
    {
        return new DoubleQuaternion(0, 0, 0, 0);
    }
}
