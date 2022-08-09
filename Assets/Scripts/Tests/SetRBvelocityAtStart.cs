using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SetRBvelocityAtStart : MonoBehaviour
{
    public Vector3 initialVelocity = Vector3.zero;
    // Start is called before the first frame update
    void Start()
    {
        GetComponent<Rigidbody>().velocity = initialVelocity;
    }

    // Update is called once per frame
    void Update()
    {
    }
}
