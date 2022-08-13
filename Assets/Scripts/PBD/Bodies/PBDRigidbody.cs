using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PBDRigidbody : Particle
{
    [SerializeField] private Vector3 startingAngularVelocity;
    public DoubleQuaternion orientation;
    public DoubleVector3 angularVelocity;
    private Matrix3x3 inertiaTensor = Matrix3x3.Identity();
    private Matrix3x3 inertiaTensorInverted = Matrix3x3.Identity();
    public DoubleQuaternion prevOrientation;
    public DoubleVector3 prevAngularVelocity;
    private DoubleVector3 externalTorque = new DoubleVector3(0, 0, 0);


    protected override void Awake()
    {
        base.Awake();

        orientation = new DoubleQuaternion(transform.rotation);
        angularVelocity = new DoubleVector3(startingAngularVelocity);
        prevAngularVelocity = angularVelocity;
        prevOrientation = orientation;
    }

    protected  void Start()
    {
        if (pbdCollider != null)
        {
            inertiaTensor = pbdCollider.GetInertiaTensor();

            inertiaTensorInverted = pbdCollider.GetInertiaTensorInverted();
        }
    }

    protected override void Update()
    {
        /** /
        Debug.DrawRay(position.ToVector3(), right.ToVector3()*5, Color.red);
        Debug.DrawRay(position.ToVector3(), up.ToVector3()*5, Color.green);
        Debug.DrawRay(position.ToVector3(), forward.ToVector3()*5, Color.blue);
       /*
        Debug.DrawRay(position.ToVector3(), ProjectToWorldCoordinates(right).ToVector3()*5, Color.red);
        Debug.DrawRay(position.ToVector3(), ProjectToWorldCoordinates(up).ToVector3()*5, Color.green);
        Debug.DrawRay(position.ToVector3(), ProjectToWorldCoordinates(forward).ToVector3()*5, Color.blue);
        /**/


        base.Update();
        //Debug.DrawLine(transform.position, transform.position + (angularVelocity).ToVector3()/5 , Color.blue);
        /*  pbdCollider.CalcBoundingBox();
          pbdCollider.aabb.DrawWireframe(Color.black);*/

        Debug.DrawRay(transform.position, angularVelocity.ToVector3(), Color.blue, 0.1f);
    }

    public override void UpdatePrevPosition()
    {
        base.UpdatePrevPosition();
        prevOrientation = orientation;
    }

    public override void UpdatePrevVelocity()
    {
        base.UpdatePrevVelocity();
        prevAngularVelocity = angularVelocity;
    }

    public override void UpdateVelocity(double h)
    {
        base.UpdateVelocity(h);
        //DoubleVector3 angularVelocitySelfCoords = ProjectToSelfCoordinates(angularVelocity);
        angularVelocity +=  h * inertiaTensorInverted * (externalTorque - (DoubleVector3.Cross(angularVelocity , inertiaTensor *  angularVelocity)));
    }

    public override void UpdatePosition(double h)
    {
        base.UpdatePosition(h);

        orientation += h * 0.5 * angularVelocity * orientation;
        orientation = DoubleQuaternion.Normal(orientation);
        orientation = DoubleQuaternion.Clean(orientation);

        UpdateAxes();
    }

    public override void RecalcVelocity(double h)
    {
        base.RecalcVelocity(h);

        DoubleQuaternion deltaOrientation = orientation * prevOrientation.Inverse();
        deltaOrientation = DoubleQuaternion.Normal(deltaOrientation);
        angularVelocity = 2 * deltaOrientation.VectorPart() / h;
        angularVelocity = deltaOrientation.w >= 0 ? angularVelocity : -angularVelocity;
    }

    public override void ApplyRestitution(DoubleVector3 p, double sign, DoubleVector3 r)
    {
        base.ApplyRestitution(p, sign, r);
        ApplyRotation(p,  sign,  r);
        UpdatePrevPosition();
    }

    public override void ApplyCorrectionOrientation(DoubleVector3 correction, double sign, DoubleVector3 offset) //used in constraints
    {
        DoubleVector3 prevPos = position + orientation * offset;

        correction = ProjectToSelfCoordinates(correction);
        DoubleVector3 torque = DoubleVector3.Cross(offset, correction);
        torque =  orientation * (inertiaTensorInverted * torque);
        orientation += sign * 0.5 * torque * orientation;
        orientation =   DoubleQuaternion.Normal(orientation);

        DoubleVector3 newPos = position + orientation * offset;
        // position += (prevPos - newPos);
    }

    public override DoubleQuaternion GetCorrectionOrientation(DoubleVector3 correction, double sign, DoubleVector3 offset)
    {
        correction = ProjectToSelfCoordinates(correction);
        DoubleVector3 torque = DoubleVector3.Cross(offset, correction);
        torque =  orientation * (inertiaTensorInverted * torque);
        return sign * 0.5 * torque * orientation;
    }

    public override void ApplyRotation(DoubleVector3 p, double sign, DoubleVector3 r)//Used in restitution
    {
        if (inverseMass == 0 || Math.Abs(DoubleVector3.Dot(r, p)) == 1)
            return;

        DoubleVector3 torque = DoubleVector3.Cross(r, p);
        torque = ProjectToSelfCoordinates(torque);

        if (DoubleVector3.MagnitudeSqr(torque) == 0)
            return;
        torque = inertiaTensorInverted * torque;
        torque = orientation * torque;

        angularVelocity += sign * torque;
    }

    public override void ApplyCorrectionPrevOrientation(DoubleVector3 correction, DoubleVector3 offset)
    {
    }

    public override void ApplyDynamicFriction(DoubleVector3 deltaV, double sign, DoubleVector3 r)
    {
        base.ApplyDynamicFriction(deltaV, sign, r);
        if (inverseMass == 0)
            return;

        DoubleVector3 rCrossP = DoubleVector3.Cross(r, sign * deltaV);
        if (DoubleVector3.MagnitudeSqr(rCrossP) == 0)
            return;
        //rCrossP = ProjectToSelfCoordinates(rCrossP);

        angularVelocity +=  sign * (inertiaTensorInverted * rCrossP);
    }

    public override DoubleVector3 GetVelocityAtCollisionPoint(DoubleVector3 r)
    {
        return GetVelocityAtPoint(this.angularVelocity, r);
    }

    public override DoubleVector3 GetPrevVelocityAtCollisionPoint(DoubleVector3 r)
    {
        return GetVelocityAtPoint(prevAngularVelocity, r);
    }

    private DoubleVector3 GetVelocityAtPoint(DoubleVector3 angularVelocity, DoubleVector3 r)
    {
        if (DoubleVector3.MagnitudeSqr(angularVelocity) == 0)
            return velocity;


        return velocity + DoubleVector3.Cross(angularVelocity, orientation * r);
    }

    public void UpdateAxes()
    {
        right = orientation * new DoubleVector3(1, 0, 0);
        forward = orientation * new DoubleVector3(0, 0, 1);
        up = orientation * new DoubleVector3(0, 1, 0);
        right = DoubleVector3.Normal(right);
        forward = DoubleVector3.Normal(forward);
        up = DoubleVector3.Normal(up);
    }

    public override DoubleQuaternion GetOrientation()
    {
        return orientation;
    }

    public override void SetRotation(DoubleQuaternion q)
    {
        orientation = DoubleQuaternion.Clean(q);
        UpdateAxes();
    }

    public override void SetRotationRaw(DoubleQuaternion q)
    {
        orientation = q;
        UpdateAxes();
    }

    public override DoubleQuaternion GetPrevOrientation()
    {
        return prevOrientation;
    }

    public override double GetGeneralizedInverseMass(DoubleVector3 correctionDir, DoubleVector3 r)
    {
        if (DoubleVector3.MagnitudeSqr(r) == 0 || DoubleVector3.MagnitudeSqr(correctionDir) == 0 || inverseMass == 0)
            return inverseMass;
        DoubleVector3 rCrossN = DoubleVector3.Cross(r, ProjectToSelfCoordinates(correctionDir));
        // DoubleVector3 rCrossN = DoubleVector3.Cross(r, correctionDir);
//        Debug.Log("normal w = " + inverseMass + " generalized W = " + inverseMass + DoubleVector3.Dot(rCrossN * inertiaTensorInverted, rCrossN));
        double rotationW =  DoubleVector3.Dot(rCrossN * inertiaTensorInverted, rCrossN);
        // Debug.Log(gameObject.name + " wi " + inverseMass + " " + rotationW);
        return inverseMass + rotationW;
    }

    public override double GetGeneralizedInverseMass(DoubleVector3 correctionDir)
    {
        //   return inverseMass;
        if (inverseMass == 0)
            return 0;
        correctionDir = ProjectToSelfCoordinates(correctionDir);

        DoubleVector3 aux = correctionDir * inertiaTensorInverted;
        return DoubleVector3.Dot(aux, correctionDir);
    }

    public override double CalcKineticEnergy()
    {
        return CalcLinearKineticEnergy() + CalcRotationalKineticEnergy();
    }

    private double CalcRotationalKineticEnergy()
    {
        DoubleVector3 wSelf = orientation.Inverse() * angularVelocity;
        DoubleVector3 aux = inertiaTensor * DoubleVector3.Normal(wSelf);
        double moment = DoubleVector3.Magnitude(aux);
        double w =  DoubleVector3.Magnitude(angularVelocity);

        return 0.5 * moment * w * w;
    }
}
