using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestRotation : PBDMonoBehaviour
{
    private PBDRigidbody rb;


    private void Start()
    {
        rb = GetComponent<PBDRigidbody>();
    }

    public override void PBDUpdate(double h)
    {
        DoubleVector3 up = new DoubleVector3(0, 1, 0);
        rb.ApplyRestitution(up, 1, new DoubleVector3(.5, 0, 0));
        rb.ApplyRestitution(up, 1, new DoubleVector3(-.5, 0, 0));
        Debug.Log("v: " + rb.velocity);
        Debug.Log("w: " + rb.angularVelocity);
    }
}
