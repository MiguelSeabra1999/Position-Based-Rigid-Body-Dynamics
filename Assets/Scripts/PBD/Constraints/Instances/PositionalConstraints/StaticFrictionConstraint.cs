using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
public class StaticFrictionConstraint : PBDConstraint
{
    private PBDCollision col;
    private PBDFrictionCollision frictionCol;
    private NonPenetrationConstraint normalConstraint;
    private DoubleVector3 gradient;
    private double error;


    public StaticFrictionConstraint() {}
    public StaticFrictionConstraint(PBDCollision col, NonPenetrationConstraint normalConstraint, PBDFrictionCollision frictionCollision)
    {
        this.col = col;
        bodies.Add(col.a);
        bodies.Add(col.b);
        this.normalConstraint = normalConstraint;
        this.frictionCol = frictionCollision;
    }

    public void SetNewValue(PBDCollision col, NonPenetrationConstraint normalConstraint, PBDFrictionCollision frictionCollision)
    {
        if (bodies.Count == 0)
        {
            bodies.Add(col.a);
            bodies.Add(col.b);
        }
        else
        {
            bodies[0] = col.a;
            bodies[1] = col.b;
        }
        this.col = col;
        this.normalConstraint = normalConstraint;
        this.frictionCol = frictionCollision;
    }

    protected override DoubleVector3 GetGradient(int bodyIndex)
    {
        return gradient;
    }

    protected override double GetSign(int i)
    {
        if (i == 0)
            return 1;
        return -1;
    }

    protected override double GetGradientMagnitude(int bodyIndex)
    {
        return 1;
    }

    public override double Evaluate()
    {
        DoubleVector3 p1 = col.a.position + col.a.GetOrientation() * col.rA;
        DoubleVector3 p1Prev = col.a.prevPosition + col.a.GetPrevOrientation() * col.rA;
        DoubleVector3 p2 = col.b.position + col.b.GetOrientation() * col.rB;
        DoubleVector3 p2Prev = col.b.prevPosition + col.b.GetPrevOrientation() * col.rB;

        DoubleVector3 deltaPos = (p1 - p1Prev) - (p2 - p2Prev);
        DoubleVector3 deltaPosT = deltaPos - DoubleVector3.Dot(deltaPos, col.normal) * col.normal;
        gradient = DoubleVector3.Normal(deltaPosT);
        error = DoubleVector3.Magnitude(deltaPosT);
        /* if (DoubleVector3.Magnitude(col.a.velocity) > 0 || DoubleVector3.Magnitude(col.b.velocity) > 0)
         {
             Debug.Log("applying");
             Debug.Log("error " + error);
         }*/
        if (error == 0)
        {
            col.hasStaticFriction = false;
            col.hasDynamicFriction = false;
            col.hasFriction = false;
        }

        return error;
    }

    protected override bool LagrangeMultConstraint(double h)
    {
        double staticFrictionCoefficient = (col.a.staticFrictionCoefficient + col.b.staticFrictionCoefficient) / 2;

        frictionCol.tangencialDir = gradient;


        if (!col.hasStaticFriction || Math.Abs(normalConstraint.lagrangeMult) * staticFrictionCoefficient <= Math.Abs(lagrangeMult))
        {
            frictionCol.normalForceLargrangeMult = Math.Abs(normalConstraint.lagrangeMult);

            // Debug.Log(col.a.gameObject.name + "No static friction normal force = " + normalConstraint.lagrangeMult);
            return true; // Aborts solve
        }

        //   Debug.Log(col.a.gameObject.name +"Yes static friction");
        frictionCol.appliedStaticFriction = true;
        // Debug.DrawRay((bodies[1].position + bodies[1].GetOrientation()*GetBodyR(1)).ToVector3(), (-gradient).ToVector3()*10, Color.green, 0.1f );
        //   bodies[1].CancelVelocityAlongDirection(-gradient);

        return false;
    }

    protected override DoubleVector3 GetBodyR(int index)
    {
        if (index == 0)
        {
            return col.rA;
            //  return new DoubleVector3(0,0,0);
        }
        else
        {
            return col.rB;
            // return new DoubleVector3(0,0,0);
        }
    }

    /*   protected override void UpdateOrientation(DoubleVector3 correction, double sign , int index)
       {
           DoubleVector3 bodyOffset = GetBodyR(index);
           //Debug.Log(bodies[index].gameObject.name  + bodyOffset);
           if (DoubleVector3.MagnitudeSqr(bodyOffset) > 0)
               bodies[index].ApplyCorrectionOrientation(correction, -sign, bodyOffset);
       }*/
}
