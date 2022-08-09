using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class PhysicsEngine : MonoBehaviour
{
    public bool stepByStep = false;
    public bool optimizeCollisionDetection = true;
    public bool performBroadPhaseOncePerSimStep = false;

    public bool storeTotalEnergy;
    public AnimationCurve totalEnergyPlot = new AnimationCurve();
    private bool instantSeperateContacts = false;
    public static double gravForce = 9.8;
    public  Particle[] allBodies;
    public  PBDParticle[] allParticles;
    public  PBDRigidbody[] allRigidbodies;

    public int substeps = 1;
    public List<PBDConstraint> constraints = new List<PBDConstraint>();
    public List<PBDConstraint> temporaryConstraints = new List<PBDConstraint>();

    public List<DistanceConstraint> distanceConstraints = new List<DistanceConstraint>();
    public List<BallJointConstraint> ballJointConstraint = new List<BallJointConstraint>();
    public List<TwistConstraint> twistConstraint = new List<TwistConstraint>();
    public List<HingeConstraint> hingeConstraint = new List<HingeConstraint>();

    public CollisionEngine collisionEngine = new CollisionEngine();
    public event UnityAction physicsStepEnd;
    public event UnityAction physicsSubstepEnd;
    /*private List<PBDCollision> collisions = new List<PBDCollision>();
    private  PBDCollision  col;*/
    // public double accuracy = 0.00001;


    void Start()
    {
        allBodies = GetComponentsInChildren<Particle>();
        allParticles = GetComponentsInChildren<PBDParticle>();
        allRigidbodies = GetComponentsInChildren<PBDRigidbody>();
        foreach (BallJointConstraint c in ballJointConstraint)
        {
            constraints.Add(c);
        }
        foreach (HingeConstraint c in hingeConstraint)
        {
            constraints.Add(c);
        }
        foreach (TwistConstraint c in twistConstraint)
        {
            constraints.Add(c);
        }
        foreach (DistanceConstraint c in distanceConstraints)
        {
            constraints.Add(c);
        }

        StoreColliders();

        foreach (PBDConstraint c in constraints)
            c.Init(allBodies);
    }

    private void Update()
    {
        double h = Time.deltaTime / substeps;
        //Get potential collision should be done here
        if (optimizeCollisionDetection && performBroadPhaseOncePerSimStep)
            collisionEngine.BroadCollisionDetection();
        for (int substep = 0; substep < substeps; substep++)
        {
            PositionalUpdate(h);
            // CheckCollisions();

            ConstraintSolve(h);
            //RecalcVelocities(h);

            if (instantSeperateContacts)
                collisionEngine.SeparateContacts();

            VelocitySolve(h);//Collision Handling


            physicsSubstepEnd?.Invoke();

            collisionEngine.Clear();
            //temporaryConstraints.Clear();

            for (int i = 0; i < allBodies.Length; i++)
                allBodies[i].PBDphysicsUpdate();
            // ClampVelocity(h);
        }
        UpdateActualPositions();


        for (int i = 0; i < allBodies.Length; i++)
            allBodies[i].PBDupdate();
        /*    if (storeTotalEnergy)
                StoreTotalEnergy();*/
        if (stepByStep)
            Pause();
        physicsStepEnd?.Invoke();
        // Debug.Break();
    }

    private void VelocitySolve(double h)
    {
        collisionEngine.LoopCollisions(
            (c) => c.HandleCollision(h)
        );
    }

    private void RecalcVelocities(double h)
    {
        for (int i = 0; i <  allParticles.Length; i++)
        {
            PBDParticle particle = allParticles[i];
            particle.RecalcVelocity(h);
        }
        for (int i = 0; i <  allRigidbodies.Length; i++)
        {
            PBDRigidbody particle = allRigidbodies[i];
            particle.RecalcVelocity(h);
        }
    }

    private void PositionalUpdate(double h)
    {
        for (int i = 0; i < allBodies.Length; i++)
        {
            Particle particle = allBodies[i];
            particle.UpdatePrevPosition();
            particle.UpdatePrevVelocity();
            particle.UpdateVelocity(h);
            particle.UpdatePosition(h);
        }
    }

    private void ConstraintSolve(double h)
    {
        for (int i = 0; i < temporaryConstraints.Count; i++)
        {
            PBDConstraint constraint = temporaryConstraints[i];
            constraint.Solve(h);
        }
        for (int i = 0; i < constraints.Count; i++)
        {
            PBDConstraint constraint = constraints[i];
            constraint.Solve(h);
        }
        RecalcVelocities(h);

        if (optimizeCollisionDetection)
        {
            if (!performBroadPhaseOncePerSimStep)
                collisionEngine.BroadCollisionDetection();
            collisionEngine.NarrowCollisionDetection(!instantSeperateContacts, h);
        }
        else
        {
            collisionEngine.count = 0;
            collisionEngine.FullCollisionDetection(collisionEngine.allColliders , h, !instantSeperateContacts);
        }
    }

    private void UpdateActualPositions()
    {
        for (int i = 0; i < allParticles.Length; i++)
        {
            allParticles[i].transform.position = allParticles[i].position.ToVector3();
        }

        for (int i = 0; i < allRigidbodies.Length; i++)
        {
            allRigidbodies[i].transform.position = allRigidbodies[i].position.ToVector3();
            allRigidbodies[i].transform.rotation = allRigidbodies[i].orientation.ToQuaternion().normalized;
        }
    }

    private void StoreColliders()
    {
        List<PBDCollider> cols = new List<PBDCollider>();

        for (int i = 0; i < allParticles.Length; i++)
        {
            if (allParticles[i].pbdCollider != null)
                cols.Add(allParticles[i].pbdCollider);
        }

        for (int i = 0; i < allRigidbodies.Length; i++)
        {
            if (allRigidbodies[i].pbdCollider != null)
                cols.Add(allRigidbodies[i].pbdCollider);
        }

        collisionEngine.StoreColliders(cols);
    }

    /* public DoubleRayHit[] RayCastAll(DoubleRay ray)
     {
                 //todo
         DoubleRayHit[] hits = new DoubleRayHit[2];
         return hits;
     }*/

    public bool RayCast(DoubleRay ray, ref DoubleRayHit hit)
    {
        //  Debug.Log("Raycast");
        double minDist = Double.MaxValue;
        bool hasHit = false;
        foreach (PBDCollider col in collisionEngine.allColliders)
        {
            DoubleRayHit thisHit = new DoubleRayHit();
            Debug.DrawRay(ray.point.ToVector3(), ray.direction.ToVector3() * 100, Color.yellow, 1f);
            if (col.IntersectRay(ray, ref thisHit))
            {
                hasHit = true;
                if (thisHit.hitDistance < minDist)
                {
                    minDist = thisHit.hitDistance;
                    hit = thisHit;
                }
            }
        }
        return hasHit;
    }

    private void StoreTotalEnergy()
    {
        double sum = 0;
        foreach (Particle p in allBodies)
        {
            sum += p.CalcTotalEnergy(gravForce);
        }
        totalEnergyPlot.AddKey(Time.realtimeSinceStartup, (float)sum);
    }

    public void Pause()
    {
        UpdateActualPositions();
        Debug.Break();
    }
}
