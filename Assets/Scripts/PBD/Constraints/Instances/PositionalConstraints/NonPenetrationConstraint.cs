using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NonPenetrationConstraint : PBDConstraint
{
    public PBDCollision col;

    public NonPenetrationConstraint(PBDCollision col)
    {
        this.col = col;
        bodies.Add(col.a);
        bodies.Add(col.b);
    }

    protected override DoubleVector3 GetGradient(int bodyIndex)
    {
        return DoubleVector3.Normal(col.correction);
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
        return DoubleVector3.Magnitude(col.correction);
    }

    protected override DoubleVector3 GetBodyR(int index)
    {
        if (index == 0)
        {
            return col.collisionPoint - col.a.position;
        }
        else
        {
            return col.otherCollisionPoint - col.b.position;
        }
    }

    /*  protected override void UpdateOrientation(DoubleVector3 correction, int index)
      {

          DoubleVector3 offset = col.rA;
          if(index == 1)
              offset = col.rB;
          if(DoubleVector3.MagnitudeSqr(offset) > 0)
              bodies[index].ApplyCorrectionOrientation(correction,offset);
      }*/
}
