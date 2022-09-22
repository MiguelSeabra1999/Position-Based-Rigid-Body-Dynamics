using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FPScontrollerPBD : PBDMonoBehaviour
{
    public float speed = 1;
    public float xSensitivity = 1;
    public float ySensitivity = 1;
    public float jumpForce = 1;
    public float shootForce = 1;
    private PBDRigidbody rb;
    public GameObject cameraObject;
    public GameObject bulletPrefab;
    public Transform gunTransform;
    private void Awake()
    {
        rb = GetComponent<PBDRigidbody>();
    }

    public override void PBDUpdate(double deltaTime)
    {
        rb.velocity = new DoubleVector3(0, rb.velocity.y, 0);
        rb.angularVelocity = new DoubleVector3(0);
        if (Input.GetKey(KeyCode.D))
            rb.velocity += new DoubleVector3(transform.right) * speed;
        if (Input.GetKey(KeyCode.A))
            rb.velocity += new DoubleVector3(-1 * transform.right) * speed;
        if (Input.GetKey(KeyCode.W))
            rb.velocity += new DoubleVector3(transform.forward) * speed;
        if (Input.GetKey(KeyCode.S))
            rb.velocity += new DoubleVector3(-1 * transform.forward) * speed;
        if (Input.GetKeyDown(KeyCode.Space))
            rb.velocity += new DoubleVector3(Vector3.up) * jumpForce;
        if (Input.GetKeyDown(KeyCode.Mouse0))
            SpawnBullet();


        float xAxis = Input.GetAxisRaw("Mouse X");
        float yAxis = Input.GetAxisRaw("Mouse Y");
        if (xAxis != 0)
            rb.angularVelocity = new DoubleVector3(Vector3.up) * xSensitivity * xAxis * (float)deltaTime;
        if (yAxis != 0)
            cameraObject.transform.Rotate(Vector3.right * yAxis * ySensitivity * (float)deltaTime, Space.Self);
    }

    private void SpawnBullet()
    {
        PBDRigidbody bullet = engine.PBDInstantiate(bulletPrefab, gunTransform.position - gunTransform.forward, gunTransform.rotation);
        bullet.velocity += new DoubleVector3(cameraObject.transform.forward * shootForce);
    }
}
