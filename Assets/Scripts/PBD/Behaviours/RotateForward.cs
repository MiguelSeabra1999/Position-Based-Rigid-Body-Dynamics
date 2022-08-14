using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RotateForward : PBDMonoBehaviour
{
    public double speed;
    private PBDRigidbody rb;
    void Awake()
    {
        rb = GetComponent<PBDRigidbody>();
    }

    void Update()
    {
        rb.externalTorque = new DoubleVector3(0);
        if (Input.GetKey(KeyCode.RightArrow))
            rb.externalTorque += new DoubleVector3(Vector3.back) * speed;
        if (Input.GetKey(KeyCode.LeftArrow))
            rb.externalTorque += new DoubleVector3(Vector3.forward) * speed;
        if (Input.GetKey(KeyCode.UpArrow))
            rb.externalTorque -= new DoubleVector3(Vector3.right) * speed;
        if (Input.GetKey(KeyCode.DownArrow))
            rb.externalTorque -= new DoubleVector3(Vector3.left) * speed;
        if (DoubleVector3.MagnitudeSqr(rb.externalTorque) == 0)
            rb.angularVelocity = new DoubleVector3(0);
    }
}
