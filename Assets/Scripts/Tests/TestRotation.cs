using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestRotation : PBDMonoBehaviour
{
    private PBDRigidbody rb;


    private void Start()
    {
        rb = GetComponent<PBDRigidbody>();

        DoubleQuaternion q = new DoubleQuaternion(1, 0, 0, 1);
        DoubleQuaternion q1 = new DoubleQuaternion(1, 0, 1, 0);
        DoubleQuaternion q2 = DoubleQuaternion.Power(q1 * q, 0.5);
        rb.SetRotation(DoubleQuaternion.Normal(rb.GetOrientation() * q2));
    }

    /*   public override void PBDUpdate(double h)
       {
           DoubleVector3 up = new DoubleVector3(0, 1, 0);
           rb.ApplyRestitution(up, 1, new DoubleVector3(.5, 0, 0));
           rb.ApplyRestitution(up, 1, new DoubleVector3(-.5, 0, 0));
           Debug.Log("v: " + rb.velocity);
           Debug.Log("w: " + rb.angularVelocity);
       }*/
}
