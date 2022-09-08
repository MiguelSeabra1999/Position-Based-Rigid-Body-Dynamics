using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PhysicsEngineJacobi : PhysicsEngine
{
    private List<List<Correction>> corrections = new List<List<Correction>>();


    protected override void Start()
    {
        base.Start();
        for (int i = 0; i < allBodies.Length; i++)
        {
            allBodies[i].indexID = i;
            corrections.Add(new List<Correction>());
        }
    }

    protected override void ConstraintSolve(double h)
    {
        for (int i = 0; i < temporaryConstraints.Count; i++)
        {
            PBDConstraint constraint = temporaryConstraints[i];
            constraint.GetContribution(h, corrections);
        }
        for (int i = 0; i < constraints.Count; i++)
        {
            PBDConstraint constraint = constraints[i];
            constraint.GetContribution(h, corrections);
        }
        ApplyCorrections();
        ClearCorrections();
    }

    protected override void CollisionSolve(double h)
    {
        if (optimizeCollisionDetection)
        {
            if (!performBroadPhaseOncePerSimStep)
                collisionEngine.BroadCollisionDetection();
            collisionEngine.NarrowCollisionDetection(!instantSeperateContacts, h, corrections);
        }
        else
        {
            collisionEngine.count = 0;
            collisionEngine.FullCollisionDetection(collisionEngine.allColliders , h, !instantSeperateContacts, corrections);
        }
        ApplyCorrections();
        ClearCorrections();
    }

    private void ClearCorrections()
    {
        for (int i = 0; i < corrections.Count; i++)
        {
            corrections[i].Clear();
        }
    }

    private void ApplyCorrections()
    {
        for (int i = 0; i < corrections.Count; i++)
        {
            Correction correction = GetAverageCorrection(corrections[i]);
            allBodies[i].position += correction.positional;
            //    Debug.Log("Adding " + correction.positional + " to body " + allBodies[i].indexID);
            allBodies[i].SetRotation(DoubleQuaternion.Normal(allBodies[i].GetOrientation() + correction.rotational));
        }
    }

    private Correction GetAverageCorrection(List<Correction> corrections)
    {
        Correction sum = new Correction(new DoubleVector3(0), new DoubleQuaternion(0, 0, 0, 0));
        int count = 0;
        if (corrections.Count == 0)
            return sum;
        for (int i = 0; i < corrections.Count; i++)
        {
            sum += corrections[i];
            if (corrections[i].positional != new DoubleVector3(0))
                count++;
        }
        if (count == 0)
            return sum;
        return sum / count;
    }
}
