using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public abstract class Particle : MonoBehaviour
{
    public int indexID = 0;
    public bool wasInCollision = false;
    public Vector3 startingVelocity = Vector3.zero;
    public DoubleVector3 position = new DoubleVector3(0, 0, 0);
    public DoubleVector3 velocity = new DoubleVector3(0, 0, 0);
    public DoubleVector3 prevPosition = new DoubleVector3(0, 0, 0);
    public DoubleVector3 prevVelocity = new DoubleVector3(0, 0, 0);
    private DoubleVector3 allForces = new DoubleVector3(0, -1, 0) * 9.8;
    public double mass = 1;
    public double inverseMass = 1;
    public double gravityScale = 1;
    public double restitution = 1;
    public double staticFrictionCoefficient = 1;
    public double dynamicFrictionCoefficient = 1;
    public PBDCollider pbdCollider = null;
    public DoubleVector3 forward = new DoubleVector3(0, 0, 1);
    public DoubleVector3 up = new DoubleVector3(0, 1, 0);
    public DoubleVector3 right = new DoubleVector3(1, 0, 0);
    private DoubleVector3[] axes = new DoubleVector3[3];

    /*     public double normalForceLargrangeMult;
     public bool appliedStaticFriction;*/

    public abstract DoubleQuaternion GetOrientation();
    public abstract void ApplyCorrectionOrientation(DoubleVector3 correction, double sign, DoubleVector3 offset);
    public abstract DoubleQuaternion GetCorrectionOrientation(DoubleVector3 correction, double sign, DoubleVector3 offset);
    public abstract void ApplyCorrectionPrevOrientation(DoubleVector3 correction, DoubleVector3 offset);
    public virtual DoubleQuaternion GetPrevOrientation()
    {
        return GetOrientation();
    }

    public virtual DoubleVector3 GetVelocityAtCollisionPoint(DoubleVector3 r)
    {
        return velocity;
    }

    public virtual DoubleVector3 GetPrevVelocityAtCollisionPoint(DoubleVector3 r)
    {
        return prevVelocity;
    }

    protected virtual void Awake()
    {
        if (pbdCollider == null)
        {
            pbdCollider = GetComponent<PBDCollider>();
        }
        position = new DoubleVector3(transform.position);
        velocity = new DoubleVector3(startingVelocity);

        if (pbdCollider != null)
            pbdCollider.particle = this;

        prevVelocity = velocity;
        prevPosition = position;
    }

    protected virtual void Update()
    {
    }

    void OnDrawGizmos()
    {
        //Handles.Label(transform.position, "v:" + DoubleVector3.Magnitude(velocity));
    }

    public virtual void PBDupdate()
    {
        DrawLine();
    }

    public virtual void PBDphysicsUpdate()
    {
        //DrawLine();
        if (pbdCollider != null)
            pbdCollider.ColliderUpdate();
    }

    public virtual void ApplyRestitution(DoubleVector3 p, double sign, DoubleVector3 r)
    {
        if (inverseMass == 0)
            return;
        velocity += sign * (p);

        //UpdatePrevVelocity();
    }

    public virtual void ApplyRotation(DoubleVector3 p, double sign, DoubleVector3 r)
    {
        return;
    }

    public virtual void SetRotation(DoubleQuaternion q)
    {
        return;
    }

    public virtual void SetRotationRaw(DoubleQuaternion q)
    {
        return;
    }

    public void CancelVelocityAlongDirection(DoubleVector3 dir)
    {
        DoubleVector3 velocityNormal = DoubleVector3.Normal(velocity);
        double dot = DoubleVector3.Dot(velocityNormal, dir);
        if (dot > 0)
            velocity -= dir * (dot * DoubleVector3.Magnitude(velocity));
    }

    public DoubleVector3 GetVelocityAlongDirection(DoubleVector3 dir)
    {
        DoubleVector3 velocityNormal = DoubleVector3.Normal(velocity);
        double dot = DoubleVector3.Dot(velocityNormal, dir);
        if (dot > 0)
            return dir * (dot * DoubleVector3.Magnitude(velocity));
        return new DoubleVector3(0, 0, 0);
    }

    public virtual void ApplyDynamicFriction(DoubleVector3 deltaV,  double sign, DoubleVector3 r)
    {
        velocity += sign * deltaV;
    }

    public virtual void UpdatePrevPosition()
    {
        prevPosition = position;
    }

    public virtual void UpdatePrevVelocity()
    {
        prevVelocity = velocity;
    }

    public virtual void UpdateVelocity(double h)
    {
        velocity += new DoubleVector3(0, -1, 0) * PhysicsEngine.gravForce * h * gravityScale;
    }

    public virtual void UpdatePosition(double h)
    {
        position += velocity * h;
    }

    public virtual void RecalcVelocity(double h)
    {
        //Debug.Log(gameObject.name + " pos " + position + " prevPos " + prevPosition);
        DoubleVector3 deltaPos = position - prevPosition;
        velocity = deltaPos / h;
    }

    public DoubleVector3[] GetAxes()
    {
        /**/
        axes[0] = right;
        axes[1] = up;
        axes[2] = forward;


        return axes;
    }

    public DoubleVector3 ProjectToSelfCoordinates(DoubleVector3 vec)
    {
        return new DoubleVector3(
            DoubleVector3.Dot(vec, right),
            DoubleVector3.Dot(vec, up),
            DoubleVector3.Dot(vec, forward)
        );
    }

    public DoubleVector3 ProjectToWorldCoordinates(DoubleVector3 vec)
    {
        return right * vec.x + up * vec.y + forward * vec.z;
    }

    public virtual double GetGeneralizedInverseMass(DoubleVector3 correctionDir, DoubleVector3 r)
    {
        return inverseMass;
    }

    public virtual double GetGeneralizedInverseMass(DoubleVector3 correctionDir)
    {
        return inverseMass;
    }

    public virtual Matrix3x3 GetInertiaTensorInverted()
    {
        if (pbdCollider != null)
            return pbdCollider.GetInertiaTensorInverted();
        return Matrix3x3.Identity();
    }

    public virtual Matrix3x3 GetInertiaTensor()
    {
        if (pbdCollider != null)
            return pbdCollider.GetInertiaTensor();
        return Matrix3x3.Identity();
    }

    public double CalcLinearMomentum()
    {
        return mass * DoubleVector3.Magnitude(velocity);
    }

    public  double CalcLinearKineticEnergy()
    {
        double v  = DoubleVector3.Magnitude(velocity);
        return 0.5 * mass * v * v;
    }

    public virtual double CalcKineticEnergy()
    {
        return CalcLinearKineticEnergy();
    }

    public double CalcPotentialEnergy(double gravityForce)
    {
        return mass * gravityForce * position.y;
    }

    public double CalcTotalEnergy(double gravityForce)
    {
        return CalcKineticEnergy() + CalcPotentialEnergy(gravityForce);
    }

    public void DrawLine()
    {
        Debug.DrawLine(transform.position, transform.position + (velocity).ToVector3() , GetComponent<Renderer>().material.color);
    }
}
