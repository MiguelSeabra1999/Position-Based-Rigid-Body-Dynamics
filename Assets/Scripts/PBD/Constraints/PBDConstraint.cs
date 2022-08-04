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
            denominator += wi * gradientMag * gradientMag;
        }
        denominator += compliance / (deltaTime * deltaTime);

        return -1 * error / denominator;
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
                DoubleVector3 correction = GetGradient(i) * lagrangeMult *  (1 / bodies[i].mass);
                bodies[i].position += correction;
            }

            UpdateOrientation(GetGradient(i) * lagrangeMult , i);

            if (bodies[i].GetOrientation().IsNan())
                Debug.Log("Nan found rotation" + bodies[i].gameObject);
            if (bodies[i].position.IsNan())
                Debug.Log("Nan found position" + bodies[i].gameObject);
            //bodies[i].prevPosition += correction;
        }


        /*    double newError = Evaluate();
             if(newError != 0)
                 Debug.Log("error " + newError);*/
    }

    protected virtual void UpdateOrientation(DoubleVector3 correction, int index)
    {
        return;
    }
}
