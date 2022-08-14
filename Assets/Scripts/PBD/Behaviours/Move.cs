using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Move : PBDMonoBehaviour
{
    public double speed;
    private Particle particle;
    void Awake()
    {
        particle = GetComponent<Particle>();
    }

    void Update()
    {
        particle.externalForce = new DoubleVector3(0);
        if (Input.GetKey(KeyCode.RightArrow))
            particle.externalForce += new DoubleVector3(Vector3.right) * speed;
        if (Input.GetKey(KeyCode.LeftArrow))
            particle.externalForce += new DoubleVector3(Vector3.left) * speed;
        if (Input.GetKey(KeyCode.UpArrow))
            particle.externalForce += new DoubleVector3(Vector3.forward) * speed;
        if (Input.GetKey(KeyCode.DownArrow))
            particle.externalForce += new DoubleVector3(Vector3.back) * speed;
    }
}
