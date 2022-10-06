using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Threading;
[Serializable]
public abstract class PBDConstraint
{
    protected List<Particle> bodies = new List<Particle>();
    public double compliance = 0;
    [HideInInspector] public double lagrangeMult;

    protected double accuracy = 0.00001f;
    public double breakForce = 0;
    [HideInInspector] public bool broken = false;
    protected double forceEstimate = 0;
    private Mutex[] mutexes;

    // static private double maxForce = 0;
    /*

    protected Constraint(double _compliance)
    {
        compliance = _compliance;
    }
*/
    public virtual void Init(Particle[] allParticles)
    {
        mutexes = new Mutex[bodies.Count];
        for (int i = 0; i < bodies.Count; i++)
            mutexes[i] = bodies[i].mutex;
    }

    protected abstract double GetGradientMagnitude(int i);
    public abstract double Evaluate();
    protected abstract DoubleVector3 GetGradient(int i);
    protected abstract double GetSign(int i);

    protected virtual bool LagrangeMultConstraint(double h) {return false;}

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
        /*   Solve(deltaTime, (i, correction, lagrangeMult) =>
           {
               DoubleVector3 positional = GetSign(i) * correction;
               DoubleQuaternion rotational = GetOrientationCorrection(GetGradient(i) * lagrangeMult , GetSign(i), i);
               corrections[bodies[i].indexID].Add(new Correction(positional, rotational));
           });*/
        if (broken)
            return;
        double error = Evaluate();
        if (error == 0)
            return;

        lagrangeMult = GetLagrangeMultiplier(error, deltaTime);

        if (breakForce > 0)
        {
            forceEstimate = Math.Abs(lagrangeMult) / (deltaTime);
            if (forceEstimate > breakForce)
            {
                broken = true;
                return;
            }
        }

        if (LagrangeMultConstraint(deltaTime))
            return;

        for (int i = 0; i < bodies.Count; i++)
        {
            double wi = bodies[i].GetGeneralizedInverseMass(GetGradient(i), GetBodyR(i));

            if (bodies[i].inverseMass != 0)
            {
                double gradientMag = GetGradientMagnitude(i);
                DoubleVector3 correction = GetGradient(i) * gradientMag *   lagrangeMult *  (1 / bodies[i].mass);

                DoubleVector3 positional = GetSign(i) * correction;
                DoubleQuaternion rotational = GetOrientationCorrection(GetGradient(i) * lagrangeMult , GetSign(i), i);
                corrections[bodies[i].indexID].Add(new Correction(positional, rotational));

                //   Debug.DrawRay(bodies[i].position.ToVector3(), GetGradient(i).ToVector3(), Color.white, 0.01f);
            }

            bodies[i].Validate();
        }
    }

    public virtual void ParallelGetContribution(double deltaTime, List<List<Correction>> corrections)
    {
        /* Solve(deltaTime, (i, correction, lagrangeMult) =>
         {
             DoubleVector3 positional = GetSign(i) * correction;
             DoubleQuaternion rotational = GetOrientationCorrection(GetGradient(i) * lagrangeMult , GetSign(i), i);
             lock (corrections)
             {
                 corrections[bodies[i].indexID].Add(new Correction(positional, rotational));
             }
         });*/

        if (broken)
            return;
        double error = Evaluate();
        if (error == 0)
            return;

        lagrangeMult = GetLagrangeMultiplier(error, deltaTime);

        if (breakForce > 0)
        {
            forceEstimate = Math.Abs(lagrangeMult) / (deltaTime);
            if (forceEstimate > breakForce)
            {
                broken = true;
                return;
            }
        }

        if (LagrangeMultConstraint(deltaTime))
            return;

        for (int i = 0; i < bodies.Count; i++)
        {
            double wi = bodies[i].GetGeneralizedInverseMass(GetGradient(i), GetBodyR(i));

            if (bodies[i].inverseMass != 0)
            {
                double gradientMag = GetGradientMagnitude(i);
                DoubleVector3 correction = GetGradient(i) * gradientMag *   lagrangeMult *  (1 / bodies[i].mass);

                DoubleVector3 positional = GetSign(i) * correction;
                DoubleQuaternion rotational = GetOrientationCorrection(GetGradient(i) * lagrangeMult , GetSign(i), i);
                lock (corrections)
                {
                    corrections[bodies[i].indexID].Add(new Correction(positional, rotational));
                }

                //   Debug.DrawRay(bodies[i].position.ToVector3(), GetGradient(i).ToVector3(), Color.white, 0.01f);
            }

            bodies[i].Validate();
        }
    }

    public virtual void Solve(double deltaTime)
    {
        /* Solve(deltaTime, (i, correction, lagrangeMult) =>
         {
             bodies[i].position += GetSign(i) * correction;
             UpdateOrientation(GetGradient(i) * lagrangeMult , GetSign(i), i);
         });*/

        if (broken)
            return;
        double error = Evaluate();
        if (error == 0)
            return;

        lagrangeMult = GetLagrangeMultiplier(error, deltaTime);

        if (breakForce > 0)
        {
            forceEstimate = Math.Abs(lagrangeMult) / (deltaTime);
            if (forceEstimate > breakForce)
            {
                broken = true;
                return;
            }
        }

        if (LagrangeMultConstraint(deltaTime))
            return;

        for (int i = 0; i < bodies.Count; i++)
        {
            double wi = bodies[i].GetGeneralizedInverseMass(GetGradient(i), GetBodyR(i));

            if (bodies[i].inverseMass != 0)
            {
                double gradientMag = GetGradientMagnitude(i);
                DoubleVector3 correction = GetGradient(i) * gradientMag *   lagrangeMult *  (1 / bodies[i].mass);

                bodies[i].position += GetSign(i) * correction;
                UpdateOrientation(GetGradient(i) * lagrangeMult , GetSign(i), i);

                //   Debug.DrawRay(bodies[i].position.ToVector3(), GetGradient(i).ToVector3(), Color.white, 0.01f);
            }

            bodies[i].Validate();
        }
    }

    public  void Solve(double deltaTime, Action<int, DoubleVector3, double> updateFunc)
    {
        if (broken)
            return;
        double error = Evaluate();
        if (error == 0)
            return;

        lagrangeMult = GetLagrangeMultiplier(error, deltaTime);

        if (breakForce > 0)
        {
            forceEstimate = Math.Abs(lagrangeMult) / (deltaTime);
            if (forceEstimate > breakForce)
            {
                broken = true;
                return;
            }
        }

        if (LagrangeMultConstraint(deltaTime))
            return;

        for (int i = 0; i < bodies.Count; i++)
        {
            double wi = bodies[i].GetGeneralizedInverseMass(GetGradient(i), GetBodyR(i));

            if (bodies[i].inverseMass != 0)
            {
                double gradientMag = GetGradientMagnitude(i);
                DoubleVector3 correction = GetGradient(i) * gradientMag *   lagrangeMult *  (1 / bodies[i].mass);

                updateFunc(i, correction, lagrangeMult);

                //   Debug.DrawRay(bodies[i].position.ToVector3(), GetGradient(i).ToVector3(), Color.white, 0.01f);
            }

            bodies[i].Validate();
        }
    }

    protected virtual void UpdateOrientation(DoubleVector3 correction, double sign, int index)
    {
        return;
    }

    protected virtual DoubleQuaternion GetOrientationCorrection(DoubleVector3 correction, double sign, int index)
    {
        return new DoubleQuaternion(0, 0, 0, 0);
    }

    public void ParallelSolve(double h)
    {
        WaitHandle.WaitAll(mutexes);

        Solve(h);

        for (int i = 0; i < mutexes.Length; i++)
            mutexes[i].ReleaseMutex();
    }

    public void ParallelSolve(double h, List<List<Correction>> corrections)
    {
        WaitHandle.WaitAll(mutexes);

        ParallelGetContribution(h, corrections);

        for (int i = 0; i < mutexes.Length; i++)
            mutexes[i].ReleaseMutex();
    }
}
