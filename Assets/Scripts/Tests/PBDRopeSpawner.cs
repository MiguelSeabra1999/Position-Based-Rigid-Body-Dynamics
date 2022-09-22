using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PBDRopeSpawner : MonoBehaviour
{
    public bool addAngularConstraints;
    public int numberOfLinks = 5;
    public double linkMass = 10;
    public double linkCompliance = 0;
    public double angleCompliance = 0;
    public double angleBound = 0;
    public double twistCompliance = 0;
    public double breakForce = 0;

    public GameObject capsulePrefab;
    public GameObject particlePrefab;
    public PhysicsEngine engine;

    private List<PBDRigidbody> links = new List<PBDRigidbody>();


    [ContextMenu("Generate Chain")]
    private void GenerateChain()
    {
        ClearHierarchy();
        GameObject pivot = Instantiate(particlePrefab, transform.position, Quaternion.identity);
        pivot.transform.SetParent(transform);

        if (addAngularConstraints)
        {
            PBDRigidbody pivotRb = pivot.GetComponent<PBDRigidbody>();
            pivotRb.inverseMass = 0;
            pivotRb.gravityScale = 0;
            links.Add(pivotRb);
        }
        else
        {
            PBDParticle pivotParticle = pivot.GetComponent<PBDParticle>();
            pivotParticle.inverseMass = 0;
            pivotParticle.gravityScale = 0;
        }

        for (int i = 0; i < numberOfLinks; i++)
        {
            Vector3 pos = transform.position + Vector3.right * (2 * i + 1);
            AddLink(pos);
        }

        ClearConstraints();

        for (int i = 1; i <= numberOfLinks; i++)
        {
            AddConstraints(i - 1, i);
        }

        engine.distanceConstraints[0].firstBodyOffsetFloat = new Vector3(0, 0, 0);
    }

    [ContextMenu("Generate particle chain")]
    private void GenerateParticleChain()
    {
        ClearHierarchy();
        GameObject pivot = Instantiate(particlePrefab, transform.position, Quaternion.identity);
        pivot.transform.SetParent(transform);

        if (addAngularConstraints)
        {
            PBDRigidbody pivotRb = pivot.GetComponent<PBDRigidbody>();
            pivotRb.inverseMass = 0;
            pivotRb.gravityScale = 0;
            links.Add(pivotRb);
        }
        else
        {
            PBDParticle pivotParticle = pivot.GetComponent<PBDParticle>();
            pivotParticle.inverseMass = 0;
            pivotParticle.gravityScale = 0;
        }

        for (int i = 0; i < numberOfLinks; i++)
        {
            Vector3 pos = transform.position + Vector3.right * (2 * (i + 1));
            AddLinkParticle(pos);
        }

        ClearConstraints();

        for (int i = 1; i <= numberOfLinks; i++)
        {
            AddConstraintsParticle(i - 1, i);
        }

        engine.distanceConstraints[0].firstBodyOffsetFloat = new Vector3(0, 0, 0);
    }

    private void AddLink(Vector3 pos)
    {
        GameObject newLink = Instantiate(capsulePrefab, pos, Quaternion.Euler(0, 0, 90));
        newLink.transform.SetParent(transform);
        newLink.transform.localScale = new Vector3(2, 2, 2);
        PBDColliderCapsule col = newLink.GetComponent<PBDColliderCapsule>();
        PBDRigidbody rb = newLink.GetComponent<PBDRigidbody>();
        rb.pbdCollider = null;
        //DestroyImmediate(col);
        col.isTrigger = true;
        col.length = 4;
        col.radius = 1;
        rb.mass = linkMass;
        rb.inverseMass = 1.0 / linkMass;
        if (addAngularConstraints)
            links.Add(newLink.GetComponent<PBDRigidbody>());
    }

    private void AddLinkParticle(Vector3 pos)
    {
        GameObject newLink = Instantiate(particlePrefab, pos, Quaternion.Euler(0, 0, 90));
        newLink.transform.SetParent(transform);

        PBDColliderSphere col = newLink.GetComponent<PBDColliderSphere>();
        PBDParticle rb = newLink.GetComponent<PBDParticle>();


        rb.mass = linkMass;
        rb.inverseMass = 1.0 / linkMass;
        if (addAngularConstraints)
            links.Add(newLink.GetComponent<PBDRigidbody>());
    }

    private void ClearConstraints()
    {
        engine.distanceConstraints.Clear();
        engine.ballJointConstraint.Clear();
        engine.twistConstraint.Clear();
    }

    private void AddConstraints(int a, int b)
    {
        AddDistanceConstraint(a, b);
        if (addAngularConstraints && a != 0)
        {
            AddBallJointConstraint(a, b);
            AddTwistConstraint(a, b);
        }
    }

    private void AddConstraintsParticle(int a, int b)
    {
        AddDistanceConstraintParticle(a, b);
    }

    private void AddDistanceConstraint(int a, int b)
    {
        DistanceConstraint newConstraint = new DistanceConstraint();
        newConstraint.compliance = linkCompliance;
        newConstraint.goalDistance = 0;
        newConstraint.firstBodyIndex = a;
        newConstraint.secondBodyIndex = b;
        newConstraint.firstBodyOffsetFloat = new Vector3(0, -1, 0);
        newConstraint.secondBodyOffsetFloat = new Vector3(0, 1, 0);
        newConstraint.breakForce = breakForce;
        engine.distanceConstraints.Add(newConstraint);
    }

    private void AddDistanceConstraintParticle(int a, int b)
    {
        DistanceConstraint newConstraint = new DistanceConstraint();
        newConstraint.breakForce = breakForce;
        newConstraint.compliance = linkCompliance;
        newConstraint.goalDistance = 2;
        newConstraint.firstBodyIndex = a;
        newConstraint.secondBodyIndex = b;

        engine.distanceConstraints.Add(newConstraint);
    }

    private void AddBallJointConstraint(int a, int b)
    {
        BallJointConstraint newConstraint = new BallJointConstraint();
        newConstraint.compliance = angleCompliance;
        newConstraint._a1 = Vector3.up;
        newConstraint._a2 = Vector3.up;
        //Debug.Log(links[a]);
        newConstraint.body = links[a];
        newConstraint.otherBody = links[b];
        newConstraint.bound = angleBound;
        engine.ballJointConstraint.Add(newConstraint);
    }

    private void AddTwistConstraint(int a, int b)
    {
        TwistConstraint newConstraint = new TwistConstraint();
        newConstraint.compliance = twistCompliance;
        newConstraint._a1 = Vector3.up;
        newConstraint._a2 = Vector3.up;
        newConstraint._b1 = Vector3.right;
        newConstraint._b2 = Vector3.right;
        newConstraint.body = links[a];
        newConstraint.otherBody = links[b];
        engine.twistConstraint.Add(newConstraint);
    }

    private void ClearHierarchy()
    {
        int n  = transform.childCount;
        for (int i = 0; i < n; i++)
        {
            DestroyImmediate(transform.GetChild(0).gameObject);
        }
    }
}
