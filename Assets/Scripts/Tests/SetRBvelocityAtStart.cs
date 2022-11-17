using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SetRBvelocityAtStart : MonoBehaviour
{
    public Vector3 initialVelocity = Vector3.zero;
    public Vector3 initialAngularVelocity = Vector3.zero;
    private Rigidbody rb;
    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.maxAngularVelocity = 1000;
        rb.velocity = initialVelocity;
        rb.angularVelocity = initialAngularVelocity;
    }

    // Update is called once per frame
}
