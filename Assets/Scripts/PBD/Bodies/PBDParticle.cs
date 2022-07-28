using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PBDParticle : Particle
{
    
    public override DoubleQuaternion GetOrientation()
    {
        return DoubleQuaternion.Euler(0,0,0);
    }
    public override void ApplyCorrectionOrientation(DoubleVector3 correction, DoubleVector3 offset)
    {
        return;
    }
    public override void ApplyCorrectionPrevOrientation(DoubleVector3 correction, DoubleVector3 offset)
    {
        return;
    }

    public override void ApplyRestitution(DoubleVector3 p, double sign, DoubleVector3 r)
    {
        base.ApplyRestitution(p, sign, r);
        UpdatePrevPosition();
    }

}
