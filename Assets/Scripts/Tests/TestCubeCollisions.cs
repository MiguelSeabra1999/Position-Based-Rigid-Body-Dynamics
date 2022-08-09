using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestCubeCollisions : MonoBehaviour
{
    public Color normalColor;
    public Color collisionColor;
    private PBDColliderBox[] cols;
    private PBDRigidbody[] rbs;
    private List<Renderer> rends = new List<Renderer>();
    private PBDCollision col;
    private CollisionEngine collisionEngine = new CollisionEngine();
    // Start is called before the first frame update
    void Awake()
    {
        cols = GetComponentsInChildren<PBDColliderBox>();
        rbs = GetComponentsInChildren<PBDRigidbody>();
        for (int i = 0; i < cols.Length; i++)
        {
            rends.Add(cols[i].gameObject.GetComponent<Renderer>());
        }
        ResetColor();
    }

    // Update is called once per frame
    void Update()
    {
        foreach (PBDRigidbody rb in rbs)
        {
            rb.position = new DoubleVector3(rb.transform.position);
            rb.orientation = new DoubleQuaternion(rb.transform.rotation);
            rb.UpdateAxes();
        }
        for (int i = 0; i < cols.Length; i++)
        {
            //   ((PBDRigidbody)cols[i].particle).orientation = new DoubleQuaternion(transform.rotation);
            /*((PBDRigidbody)cols[i].particle).forward = new DoubleVector3(transform.forward);
            ((PBDRigidbody)cols[i].particle).right = new DoubleVector3(transform.right);
            ((PBDRigidbody)cols[i].particle).up = new DoubleVector3(transform.up);*/
            // ((PBDRigidbody)cols[i].particle).UpdateAxes();
        }


        ResetColor();
        CheckCollisions();
    }

    private void CheckCollisions()
    {
        for (int i = 0; i < cols.Length; i++)
        {
            for (int j = i + 1; j < cols.Length; j++)
            {
                bool collided = cols[i].CheckCollision(cols[j],  col);
                //Debug.Log(collided);
                if (collided)
                {
                    rends[i].material.color = collisionColor;
                    rends[j].material.color = collisionColor;
                }
            }
        }
    }

    private void ResetColor()
    {
        foreach (Renderer r in rends)
        {
            r.material.color = normalColor;
        }
    }

    [ContextMenu("Seperate")]
    private void Seperate()//Deprecated
    {
        collisionEngine.CreateConstraints(0.0001, col);
        col.a.transform.position = col.a.position.ToVector3();
        col.b.transform.position = col.b.position.ToVector3();
    }

    /*   private void Seperate()//Deprecated
       {
           col.Separate();
           col.a.transform.position = col.a.position.ToVector3();
           col.b.transform.position = col.b.position.ToVector3();
       }*/
}
