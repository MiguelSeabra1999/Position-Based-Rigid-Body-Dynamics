using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CarController : PBDMonoBehaviour
{
    public double turnAngle = 45;
    public double speed;
    public double turnSpeed;
    private PBDRigidbody rb;
    public PBDRigidbody wheelBL, wheelBR, wheelFL, wheelFR;
    private bool moved = false;

    private int rotation = 0;
    void Awake()
    {
        rb = GetComponent<PBDRigidbody>();
    }

    void Update()
    {
        moved = false;

        SetTorque(new DoubleVector3(0));

        if (Input.GetKey(KeyCode.UpArrow) || Input.GetKey(KeyCode.W))
            MoveForward(-1);
        if (Input.GetKey(KeyCode.DownArrow) || Input.GetKey(KeyCode.S))
            MoveForward(1);

        if (!moved)
        {
            if (Input.GetKey(KeyCode.RightArrow) || Input.GetKey(KeyCode.D))
                Rotate(-1);
            if (Input.GetKey(KeyCode.LeftArrow) || Input.GetKey(KeyCode.A))
                Rotate(1);
        }
        else
        {
            if (Input.GetKey(KeyCode.RightArrow) || Input.GetKey(KeyCode.D))
                rotation = -1;
            else if (Input.GetKey(KeyCode.LeftArrow) || Input.GetKey(KeyCode.A))
                rotation = 1;
            else
                rotation = 0;
        }

        if (/*!moved ||*/ Input.GetKey(KeyCode.Space))
            SetVelocity(new DoubleVector3(0));
        if (moved)
            return;
    }

    private void SetVelocity(DoubleVector3 val)
    {
        wheelBL.angularVelocity = val;
        wheelBR.angularVelocity = val;
        wheelFL.angularVelocity = val;
        wheelFR.angularVelocity = val;
        wheelBL.prevOrientation = wheelBL.orientation;
        wheelBR.prevOrientation = wheelBR.orientation;
        wheelFL.prevOrientation = wheelFL.orientation;
        wheelFR.prevOrientation = wheelFR.orientation;
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
        wheelBL.externalTorque += rb.right * speed * sign;
        wheelBR.externalTorque += rb.right * speed * sign;

        DoubleVector3 rotationAxis = rb.right;
        if (rotation == 1)
            rotationAxis =  new DoubleQuaternion(turnAngle, rb.forward) * rb.right;
        if (rotation == -1)
            rotationAxis =  new DoubleQuaternion(-turnAngle, rb.forward) * rb.right;


        wheelFL.externalTorque += rotationAxis * speed * sign;
        wheelFR.externalTorque += rotationAxis * speed * sign;

        moved = true;
    }

    private void Rotate(double sign)
    {
        wheelBL.externalTorque += rb.right * turnSpeed * sign;
        wheelFL.externalTorque += rb.right * turnSpeed * sign;
        wheelBR.externalTorque += rb.right * turnSpeed * -sign;
        wheelFR.externalTorque += rb.right * turnSpeed * -sign;
        moved = true;
    }
}
