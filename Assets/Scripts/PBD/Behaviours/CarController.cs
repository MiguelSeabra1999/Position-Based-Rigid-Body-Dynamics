using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CarController : PBDMonoBehaviour
{
    public bool addHingeConstraints = true;
    public double hingeCompliance = 0.001;
    public double turnAngle = 45;
    public double speed;
    public double turnSpeed;
    private PBDRigidbody rb;
    public PBDRigidbody wheelBL, wheelBR, wheelFL, wheelFR;
    private bool moved = false;
    private HingeConstraint FRhinge, FLhinge;
    private DoubleVector3 turnAxisRight = new DoubleVector3(0);
    private DoubleVector3 turnAxisLeft = new DoubleVector3(0);

    private PhysicsEngine physicsEngine;

    private int rotation = 0;
    void Awake()
    {
        physicsEngine = transform.parent.GetComponent<PhysicsEngine>();
        rb = GetComponent<PBDRigidbody>();
        if (addHingeConstraints)
            AddHingeConstraints();
    }

    private void AddHingeConstraints()
    {
        FLhinge = AddHinge(wheelFL, -1);
        FRhinge = AddHinge(wheelFR, 1);
        AddHinge(wheelBL, -1);
        AddHinge(wheelBR, 1);

        turnAxisRight = new DoubleQuaternion(turnAngle, new DoubleVector3(Vector3.forward)) * new DoubleVector3(Vector3.right);
        turnAxisLeft = new DoubleQuaternion(-turnAngle, new DoubleVector3(Vector3.forward)) * new DoubleVector3(Vector3.right);
    }

    private HingeConstraint AddHinge(PBDRigidbody wheel, float sign)
    {
        HingeConstraint c = new HingeConstraint();
        c.body = rb;
        c.otherBody = wheel;
        c._a1 = Vector3.right;
        c._a2 = sign * Vector3.forward;
        c.compliance = hingeCompliance;
        physicsEngine.hingeConstraint.Add(c);
        return c;
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
            {
                rotation = -1;
            }
            else if (Input.GetKey(KeyCode.LeftArrow) || Input.GetKey(KeyCode.A))
            {
                rotation = 1;
            }
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

        if (addHingeConstraints)
        {
            if (rotation == 0)
            {
                FRhinge.a1 = new DoubleVector3(Vector3.right);
                FLhinge.a1 = new DoubleVector3(Vector3.right);
            }
            else if (rotation == 1)
            {
                FRhinge.a1 = turnAxisRight;
                FLhinge.a1 = turnAxisRight;
            }
            else if (rotation == -1)
            {
                FRhinge.a1 = turnAxisLeft;
                FLhinge.a1 = turnAxisLeft;
            }
        }

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
