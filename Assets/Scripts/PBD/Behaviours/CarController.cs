using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CarController : PBDMonoBehaviour
{
    public double speed;
    public double turnSpeed;
    private PBDRigidbody rb;
    public PBDRigidbody wheelBL, wheelBR, wheelFL, wheelFR;
    private bool moved = false;
    void Awake()
    {
        rb = GetComponent<PBDRigidbody>();
    }

    void Update()
    {
        moved = false;
        SetTorque(new DoubleVector3(0));
        if (Input.GetKey(KeyCode.RightArrow))
            MoveForward(1);
        if (Input.GetKey(KeyCode.LeftArrow))
            MoveForward(-1);

        if (Input.GetKey(KeyCode.UpArrow))
            Rotate(1);
        if (Input.GetKey(KeyCode.DownArrow))
            Rotate(-1);

        if (moved)
            SetVelocity(new DoubleVector3(0));
    }

    private void SetVelocity(DoubleVector3 val)
    {
        wheelBL.angularVelocity = val;
        wheelBR.angularVelocity = val;
        wheelFL.angularVelocity = val;
        wheelFR.angularVelocity = val;
    }

    private void SetTorque(DoubleVector3 val)
    {
        wheelBL.externalTorque = val;
        wheelBR.externalTorque = val;
        wheelFL.externalTorque = val;
        wheelFR.externalTorque = val;
    }

    private void MoveForward(double sign)
    {
        wheelBL.externalTorque += rb.forward * speed * sign;
        wheelBR.externalTorque += rb.forward * speed * sign;
        wheelFL.externalTorque += rb.forward * speed * sign;
        wheelFR.externalTorque += rb.forward * speed * sign;
        moved = true;
    }

    private void Rotate(double sign)
    {
        wheelBL.externalTorque += rb.up * turnSpeed * sign;
        wheelBR.externalTorque += rb.up * turnSpeed * sign;
        wheelFL.externalTorque += rb.up * turnSpeed * -sign;
        wheelFR.externalTorque += rb.up * turnSpeed * -sign;
        moved = true;
    }
}
