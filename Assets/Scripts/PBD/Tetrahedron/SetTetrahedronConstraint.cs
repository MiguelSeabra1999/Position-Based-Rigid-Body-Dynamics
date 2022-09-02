using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SetTetrahedronConstraint : MonoBehaviour
{
    public double compliance = 0;
    private Particle[] p = new Particle[4];
    private PhysicsEngine engine;
    void Awake()
    {
        Init();
    }

    [ContextMenu("createConstraints")]
    private void Init()
    {
        engine = transform.parent.GetComponent<PhysicsEngine>();
        p[0] = transform.GetChild(0).gameObject.GetComponent<Particle>();
        p[1] = transform.GetChild(1).gameObject.GetComponent<Particle>();
        p[2] = transform.GetChild(2).gameObject.GetComponent<Particle>();
        p[3] = transform.GetChild(3).gameObject.GetComponent<Particle>();

        for (int i = 0; i < 4; i++)
        {
            for (int j = i + 1; j < 4; j++)
            {
                AddDistanceConstraint(i, j);
            }
        }
        AddVolumeConstraint();
    }

    private void AddDistanceConstraint(int i, int j)
    {
        DistanceConstraint c = new DistanceConstraint();
        c.compliance = compliance;
        c.body = p[i];
        c.otherBody = p[j];
        c.goalDistance = (p[i].gameObject.transform.position - p[j].gameObject.transform.position).magnitude;
        c.minDistanceThreshold = c.goalDistance * 0.2f;
        c.maxDistanceThreshold = c.goalDistance * 2f;
        engine.distanceConstraints.Add(c);
    }

    private void AddVolumeConstraint()
    {
        VolumeConstraint c = new VolumeConstraint();
        c.compliance = 0;
        c.body0 = p[0];
        c.body1 = p[1];
        c.body2 = p[2];
        c.body3 = p[3];
        engine.volumeConstraint.Add(c);
    }
}
