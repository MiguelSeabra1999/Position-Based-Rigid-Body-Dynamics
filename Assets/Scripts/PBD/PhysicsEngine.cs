using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using System.Threading;
public  class PhysicsEngine : MonoBehaviour
{
    public bool parallel = true;
    public bool selfCollisions = true;
    public bool stepByStep = false;
    public bool optimizeCollisionDetection = true;
    public bool performBroadPhaseOncePerSimStep = false;

    public bool storeTotalEnergy;
    public AnimationCurve totalEnergyPlot = new AnimationCurve();
    protected bool instantSeperateContacts = false;
    public static double gravForce = 9.8;
    public  Particle[] allBodies;
    public  PBDParticle[] allParticles;
    public  PBDRigidbody[] allRigidbodies;
    private  PBDMonoBehaviour[] monoBehaviours;

    public int substeps = 1;
    public PBDConstraint[] constraints;
    public List<PBDConstraint> temporaryConstraints = new List<PBDConstraint>();

    public List<DistanceConstraint> distanceConstraints = new List<DistanceConstraint>();
    public List<BallJointConstraint> ballJointConstraint = new List<BallJointConstraint>();
    public List<TwistConstraint> twistConstraint = new List<TwistConstraint>();
    public List<HingeConstraint> hingeConstraint = new List<HingeConstraint>();
    public List<AlignAngleConstraint> alignAngleConstraint = new List<AlignAngleConstraint>();
    public List<VolumeConstraint> volumeConstraint = new List<VolumeConstraint>();

    public CollisionEngine collisionEngine = new CollisionEngine();
    public event UnityAction physicsStepEnd;
    public event UnityAction physicsSubstepEnd;
    /*protected List<PBDCollision> collisions = new List<PBDCollision>();
    protected  PBDCollision  col;*/
    // public double accuracy = 0.00001;
    private double prevTime = 0;
    private Thread[] threads = new Thread[8];


    protected virtual void Start()
    {
        allBodies = GetComponentsInChildren<Particle>();
        allParticles = GetComponentsInChildren<PBDParticle>();
        allRigidbodies = GetComponentsInChildren<PBDRigidbody>();
        monoBehaviours = GetComponentsInChildren<PBDMonoBehaviour>();

        foreach (PBDMonoBehaviour m in monoBehaviours)
        {
            m.engine = this;
        }


        List<PBDConstraint> constraintsList = new List<PBDConstraint>();

        foreach (AlignAngleConstraint c in alignAngleConstraint)
        {
            constraintsList.Add(c);
        }
        foreach (BallJointConstraint c in ballJointConstraint)
        {
            constraintsList.Add(c);
        }
        foreach (HingeConstraint c in hingeConstraint)
        {
            constraintsList.Add(c);
        }
        foreach (TwistConstraint c in twistConstraint)
        {
            constraintsList.Add(c);
        }
        foreach (DistanceConstraint c in distanceConstraints)
        {
            constraintsList.Add(c);
        }
        foreach (VolumeConstraint c in volumeConstraint)
        {
            constraintsList.Add(c);
        }
        constraints = constraintsList.ToArray();

        StoreColliders();

        foreach (PBDConstraint c in constraints)
            c.Init(allBodies);
        //threads = new Thread[constraints.Length];
    }

    protected virtual void Update()
    {
        double deltaTime;
        if (prevTime == 0)
            deltaTime = Time.deltaTime;
        else
            deltaTime = (Time.realtimeSinceStartupAsDouble - prevTime) * Time.timeScale;
        prevTime = Time.realtimeSinceStartupAsDouble;
        /*   Debug.Log("unity " + Time.deltaTime);
           Debug.Log("mine " + deltaTime);
           int fps = (int)(1.0 / deltaTime);

           Debug.Log("fps: " + fps);*/
        double h = deltaTime / substeps;

        if (optimizeCollisionDetection && performBroadPhaseOncePerSimStep && selfCollisions)
            collisionEngine.BroadCollisionDetection();
        for (int substep = 0; substep < substeps; substep++)
        {
            PositionalUpdate(h);
            if (parallel)
                ParallelConstraintSolve(h);
            else
                ConstraintSolve(h);
            RecalcVelocities(h);
            CollisionSolve(h);
            // RecalcVelocities(h);
            VelocitySolve(h);//Collision Handling


            InvokePhysicsUpdate(h);
            collisionEngine.Clear();
        }
        UpdateActualPositions();
        InvokeUpdate(deltaTime);
    }

    private void InvokePhysicsUpdate(double h)
    {
        for (int i = 0; i < allBodies.Length; i++)
            allBodies[i].PBDphysicsUpdate();

        for (int i = 0; i < monoBehaviours.Length; i++)
            monoBehaviours[i].PBDphysicsUpdate(h);
        physicsSubstepEnd?.Invoke();
    }

    private void InvokeUpdate(double deltaTime)
    {
        for (int i = 0; i < allBodies.Length; i++)
            allBodies[i].PBDupdate();

        for (int i = 0; i < monoBehaviours.Length; i++)
            monoBehaviours[i].PBDUpdate(deltaTime);

        if (stepByStep)
            Pause();
        physicsStepEnd?.Invoke();
    }

    protected void VelocitySolve(double h)
    {
        collisionEngine.LoopCollisions(
            (c) => c.HandleCollision(h)
        );
        /* collisionEngine.LoopCollisions(
             (c) => c.HandleFriction(h)
         );*/
    }

    protected void RecalcVelocities(double h)
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

    protected void PositionalUpdate(double h)
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

    protected virtual void ConstraintSolve(double h)
    {
        for (int i = 0; i < temporaryConstraints.Count; i++)
        {
            PBDConstraint constraint = temporaryConstraints[i];
            constraint.Solve(h);
        }
        int n = constraints.Length;
        for (int i = 0; i < n; i++)
        {
            PBDConstraint constraint = constraints[i];
            if (!constraint.broken)
                constraint.Solve(h);
        }
    }

    protected virtual void ParallelConstraintSolve(double h)
    {
        for (int i = 0; i < temporaryConstraints.Count; i++)
        {
            PBDConstraint constraint = temporaryConstraints[i];
            constraint.Solve(h);
        }

        int n = constraints.Length;
        int increment = n / threads.Length;
        for(int i = 0; i < threads.Length-1; i++)
        {
            InitConstraintThread(i*increment, (i+1)*increment, i, h);
        }
        InitConstraintThread((threads.Length-1)*increment, threads.Length, threads.Length-1, h);


        for (int i = 0; i < 4; i++)
        {
            threads[i].Join();
        }
    }

    protected void InitConstraintThread(int from, int to, int index, double h)
    {
        threads[index] = new Thread(() => 
        {
            for (int i = from; i < to; i++)
            {
                PBDConstraint constraint = constraints[i];
                constraint.ParallelSolve(h);
            }
        }); threads[index].Start();
    }

    protected virtual void CollisionSolve(double h)
    {
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

    protected void UpdateActualPositions()
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

    protected void StoreColliders()
    {
        collisionEngine.selfCollisions = selfCollisions;
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

    protected void StoreTotalEnergy()
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

    public PBDRigidbody PBDInstantiate(GameObject obj, Vector3 p, Quaternion r)
    {
        GameObject newObj = Instantiate(obj, p, r, transform);
        PBDRigidbody newBody = newObj.GetComponent<PBDRigidbody>();
        PBDCollider newCol = newObj.GetComponent<PBDCollider>();

        Particle[] allBodiesNew = new Particle[allBodies.Length + 1];
        for (int i = 0; i < allBodies.Length; i++)
        {
            allBodiesNew[i] = allBodies[i];
        }
        allBodiesNew[allBodies.Length] = newBody;
        allBodies = allBodiesNew;

        PBDRigidbody[] allRBNew = new PBDRigidbody[allRigidbodies.Length + 1];
        for (int i = 0; i < allRigidbodies.Length; i++)
        {
            allRBNew[i] = allRigidbodies[i];
        }
        allRBNew[allRigidbodies.Length] = newBody;
        allRigidbodies = allRBNew;

        PBDCollider[] allcolNew = new PBDCollider[collisionEngine.allColliders.Length + 1];
        for (int i = 0; i < collisionEngine.allColliders.Length; i++)
        {
            allcolNew[i] = collisionEngine.allColliders[i];
        }
        allcolNew[collisionEngine.allColliders.Length] = newCol;
        collisionEngine.allColliders = allcolNew;

        collisionEngine.IncreaseColCount();

        return newBody;
    }
}
