using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RandomTests : MonoBehaviour
{
    private Particle particle;
    private PBDCollider col;
    private PBDRigidbody rb;
    public Vector3 vec;

    public Vector3 from;
    public Vector3 to;
    public Vector3 r;
    
    void Awake()
    {
        particle = GetComponent<Particle>();
        rb = GetComponent<PBDRigidbody>();
        col = GetComponent<PBDCollider>();

    }
    void Start()
    {
        TestVelocityAtPoint();
    }

    // Update is called once per frame
    void Update()
    {

          /*  rb.position = new DoubleVector3(rb.transform.position);
            rb.orientation = new DoubleQuaternion(rb.transform.rotation);
            rb.UpdateAxes();*/

          ProjectionTest();
        
    }
 [ContextMenu("TestVelocityAtPoint")]
    private void TestVelocityAtPoint()
    {
        Debug.Log("v: " + DoubleVector3.Magnitude(rb.GetVelocityAtCollisionPoint(new DoubleVector3(r))));
    }
    [ContextMenu("Test Projection")]
    private void ProjectionTest()
    {



        /*DoubleVector3 v1 = DoubleVector3.RandomVector();
        DoubleVector3 v2 = DoubleVector3.RandomVector();*/
        DoubleVector3 v1 = DoubleVector3.Normal(new DoubleVector3(from));
        DoubleVector3 v2 = DoubleVector3.Normal(new DoubleVector3(to));
        DoubleVector3 cross = DoubleVector3.Cross(v1, v2);

        double angle = DoubleVector3.Magnitude(cross);
        DoubleVector3 dir = DoubleVector3.Normal(cross);
        DoubleQuaternion q = new DoubleQuaternion(Math.Asin(angle)*Constants.Rad2Deg, dir);
        q = DoubleQuaternion.Normal(q);
        DoubleVector3 rotatedV1 = q*v1;
      /* Debug.Log("v1 " + v1 + " v2 " + v2);
        Debug.Log("result: " + (rotatedV1));
        Debug.Log("error: " + (v2 - rotatedV1));*/
        if(DoubleVector3.Magnitude(v2 - rotatedV1) > 0.01)
            Debug.Log("erro");

    }

    private DoubleVector3 RecalVelocityTest(DoubleVector3    _from, DoubleVector3 _to)
    {
       // Debug.Log(DoubleVector3.Magnitude(_to)+ " " +DoubleVector3.Normal(_to));
        DoubleQuaternion from = new DoubleQuaternion(DoubleVector3.Magnitude(_from), DoubleVector3.Normal(_from));
        DoubleQuaternion to = new DoubleQuaternion(DoubleVector3.Magnitude(_to), DoubleVector3.Normal(_to));
   
       /* Debug.Log("from " + from);

        Debug.Log("to " + to.ToEuler()*Constants.Rad2Deg);*/
        DoubleQuaternion deltaOrientation = to * from.Inverse();
        DoubleVector3 angularVelocity;
        angularVelocity = 2*deltaOrientation.VectorPart() ;
        angularVelocity = deltaOrientation.w >= 0 ? angularVelocity : -angularVelocity; 
        return angularVelocity;
    }
    
    
}
