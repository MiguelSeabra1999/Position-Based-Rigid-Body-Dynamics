using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[RequireComponent(typeof(Rigidbody))]
public class SetInitialVelocity : MonoBehaviour
{
    public Vector3 startingVelocity = Vector3.zero;
    private Rigidbody rb;
    void Awake()
    {
        rb = GetComponent<Rigidbody>();
    }

    // Start is called before the first frame update
    void Start()
    {
        rb.velocity = startingVelocity;
    }
}
