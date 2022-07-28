using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PBDMouseInteraction : MonoBehaviour
{
    public PhysicsEngine engine;
    public Camera cam;
    private double basePushForce = 5;
    private double pushForceChargeSpeed = 50;

    public double compliance = 0;
    public float sensitivity = 1;
    private bool grabbing = false;
    private DistancePointConstraint constraint;
    private int constraintIndex;
    private Color ogColor = Color.black;
    private Renderer rend;
   //private Vector3 prevPos = Vector3.zero;
   private Vector3 xAxis;
   private Vector3 yAxis;

   private double pushForce;
   private float startPushTime;
    void Awake()
    {
        if(engine == null)
        {
            engine = GetComponent<PhysicsEngine>();
            if(engine == null)
                Destroy(this);
        }
        if(cam == null)
        {
            GameObject camObj = GameObject.FindGameObjectWithTag("MainCamera");
            if(camObj == null)
                Destroy(this);
            else
                cam = camObj.GetComponent<Camera>();
            if(cam == null)
                Destroy(this);
        }

    }
    void Start()
    {
       // constraint = new DistancePointConstraint(engine.allRigidbodies[0], new DoubleVector3(10,0,0) , compliance);
       // engine.constraints.Add(constraint);
        
    }

    void OnDrawGizmos()
    {
        if(constraint != null)
            Gizmos.DrawWireSphere(constraint.anchorPoint.ToVector3(), 1);
    }
    [ExecuteInEditMode]
    void Update()
    {
        if(Input.GetKeyDown(KeyCode.Mouse0) && !grabbing)
            Grab(false);
        if(Input.GetKeyDown(KeyCode.Mouse1) && !grabbing)
            Grab(true);
        if((Input.GetKeyUp(KeyCode.Mouse0) || Input.GetKeyUp(KeyCode.Mouse1)) && grabbing)
            LetGo();

        if(Input.GetKeyDown(KeyCode.Mouse2))
            BeginPush();
        if(Input.GetKeyUp(KeyCode.Mouse2))
            Push();

        if(grabbing)
            HandleMouseMovement();
        
    }

    private void BeginPush()
    {
        startPushTime = Time.time;
    }

    private void Push()
    {
        pushForce = ((double)(Time.time - startPushTime)) * pushForceChargeSpeed + basePushForce;
        Ray floatRay = GetCurrentRay();
        DoubleRay ray = new DoubleRay(floatRay);
        DoubleRayHit hit = new DoubleRayHit();
        if(!engine.RayCast(ray, ref hit))
            return;
        grabbing = true;

        hit.collider.particle.ApplyRestitution(ray.direction*pushForce, 1, hit.point - hit.collider.particle.position);
     //   hit.collider.particle.ApplyRestitution(new DoubleVector3(Vector3.forward)*5, 1,(hit.collider.particle.position - new DoubleVector3(0.5,0.5,0.5) - hit.collider.particle.position) );


    }

    private void Grab(bool grabAtCenter)
    {
       Ray floatRay = GetCurrentRay();
        DoubleRay ray = new DoubleRay(floatRay);
        DoubleRayHit hit = new DoubleRayHit();
        if(!engine.RayCast(ray, ref hit))
            return;

        if(hit.collider.particle.inverseMass == 0)
            return;
        grabbing = true;

        GameObject target = hit.collider.gameObject;
        rend = target.GetComponent<Renderer>();
        ogColor = rend.material.color;
        rend.material.color = Random.ColorHSV();

        constraintIndex = engine.constraints.Count;
        Debug.DrawLine(hit.point.ToVector3(), hit.collider.particle.position.ToVector3(), Color.cyan, 10);
        //Debug.DrawRay(hit.point.ToVector3(), Vector3.up*10, Color.blue, 10);

        if(grabAtCenter)
            constraint = new DistancePointConstraint(hit.collider.particle, new DoubleVector3(0,0,0), compliance, engine);
            //constraint = new DistancePointConstraint(hit.collider.particle, new DoubleVector3(0,0.5,0) , compliance);
        else
           constraint = new DistancePointConstraint(hit.collider.particle, hit.collider.particle.ProjectToSelfCoordinates((hit.point - hit.collider.particle.position) ), compliance, engine);
           // constraint = new DistancePointConstraint(hit.collider.particle, hit.point - hit.collider.particle.position , compliance);

        engine.temporaryConstraints.Add(constraint);

        xAxis = cam.transform.right;
        yAxis = cam.transform.up;

    }

    private void LetGo()
    {

        //Debug.Log("Letting Go");
        grabbing = false;

        if(rend != null && rend.material != null)
            rend.material.color = ogColor;

        engine.temporaryConstraints.Remove(constraint);
    }

    private void HandleMouseMovement()
    {
       // Ray ray = GetCurrentRay();
        //Vector3 deltaPos = ray.origin - prevPos;

       //constraint.anchorPoint += new DoubleVector3(deltaPos*sensitivity);
       Vector3 movement = xAxis * Input.GetAxisRaw("Mouse X") + yAxis * Input.GetAxisRaw("Mouse Y");
       DoubleVector3 doubleMovement = new DoubleVector3(movement*sensitivity);
       if(DoubleVector3.MagnitudeSqr(doubleMovement) > 0)
       //constraint.anchorPoint += new DoubleVector3(movement*sensitivity);
       constraint.SetNewAnchor(constraint.anchorPoint + doubleMovement);
       

        //prevPos = ray.origin;
    }

    private Ray GetCurrentRay()
    {
        return cam.ScreenPointToRay(Input.mousePosition, cam.stereoActiveEye);
    }
}
