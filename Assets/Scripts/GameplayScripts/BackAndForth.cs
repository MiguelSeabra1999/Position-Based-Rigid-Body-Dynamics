using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BackAndForth : PBDMonoBehaviour
{
    public double length = 0;
    public double speed = 1;
    public Vector3 _axis = Vector3.right;
    public DoubleVector3 axis = new DoubleVector3(0);
    private DoubleVector3 startingPos = new DoubleVector3(0);
    private Particle particle;
    void Start()
    {
        particle = GetComponent<Particle>();
        axis = new DoubleVector3(_axis);
        startingPos = particle.position;
    }

    public override void PBDphysicsUpdate(double h)
    {
        particle.velocity = new DoubleVector3(0);
        particle.position = startingPos + (length * Math.Cos(Time.realtimeSinceStartupAsDouble * speed)) * axis;
    }
}
