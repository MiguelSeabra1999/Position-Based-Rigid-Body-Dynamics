using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoftBodyDebugger : PhysicsEngine
{
    // Start is called before the first frame update
    protected override void Start()
    {
        allBodies = GetComponentsInChildren<Particle>();
        allParticles = GetComponentsInChildren<PBDParticle>();
        allRigidbodies = GetComponentsInChildren<PBDRigidbody>();
        foreach (VolumeConstraint c in volumeConstraint)
        {
            volumeConstraint.Add(c);
        }

        foreach (PBDConstraint c in constraints)
            c.Init(allBodies);
    }

    protected override void Update()
    {
        GetEditorPositions();
        double h = Time.deltaTime / substeps;

        if (Input.GetKey(KeyCode.Space))
            for (int substep = 0; substep < substeps; substep++)
            {
                ConstraintSolve(h);
            }
        UpdateActualPositions();
    }

    protected override void ConstraintSolve(double h)
    {
        for (int i = 0; i < constraints.Length; i++)
        {
            PBDConstraint constraint = constraints[i];
            constraint.Solve(h);
        }
    }

    protected  void GetEditorPositions()
    {
        for (int i = 0; i < allParticles.Length; i++)
        {
            allParticles[i].position = new DoubleVector3(allParticles[i].transform.position);
        }

        for (int i = 0; i < allRigidbodies.Length; i++)
        {
            allRigidbodies[i].position = new DoubleVector3(allRigidbodies[i].transform.position);
            allRigidbodies[i].orientation = new DoubleQuaternion(allRigidbodies[i].transform.rotation);
        }
    }
}
