
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CubeCollisionVisualliser : MonoBehaviour
{
    public List<PBDColliderBox> boxes;
    private List<Mesh> boxMeshes = new List<Mesh>();
    private List<GameObject> clones = new List<GameObject>();
    private List<Mesh> cloneMeshes = new List<Mesh>();
    

    void Start()
    {
        
/*
        foreach(PBDColliderBox box in boxes)
        {
            clones.Add(Instantiate(box.gameObject,  Vector3.zero, Quaternion.identity));
            boxMeshes.Add(box.gameObject.GetComponent<MeshFilter>().mesh);
        }
        foreach(GameObject box in clones)
        {
            cloneMeshes.Add(box.GetComponent<MeshFilter>().mesh);
        }*/
    
    }

    void Update()
    {
        /*
        float[] projViewMatrix = PBDCollider.GenProjViewMatrix(transform.position, Vector3.zero, 10);
        for(int i = 0; i < boxes.Count; i++)
        {
            Vector3[] vertices = boxMeshes[i].vertices;
            Vector3[] cloneVertices = new Vector3[vertices.Length];
            for(int j = 0; j < vertices.Length; j++)
            {
                float[] point = PBDCollider.ProjectPoint(projViewMatrix, vertices[j] + boxes[i].transform.rotation * boxes[i].transform.position);
                cloneVertices[j] = new Vector3(point[0],point[1],10);

            }
            cloneMeshes[i].vertices = cloneVertices;
            cloneMeshes[i].RecalculateNormals();
        }*/
    }
    /*
    public static float[] GenProjViewMatrix(Vector3 from, Vector3 to,  float size)
    {
        Vector3 dir = (to - from).normalized;
        Vector3 right = Vector3.Cross(dir, Vector3.up);
        Vector3 up = Vector3.Cross(right, dir);
        Vector3 t = -from;

        float rightSize = size/2;
        float leftSize = -rightSize;
        float topSize = rightSize;
        float bottomSize = -rightSize;
        float near = 0;
        float far = size;

        float[] result = new float[16];

        float rPlusL = rightSize + leftSize;
        float rMinusL_inv = 1/(rightSize - leftSize);
        result[0] = (2*right.x - t.x*rPlusL) * rMinusL_inv;
        result[1] = (2*right.y - t.y*rPlusL) * rMinusL_inv;
        result[2] = (2*right.z - t.z*rPlusL) * rMinusL_inv;
        result[3] = -rPlusL * rMinusL_inv;

        float tPlusB = topSize + bottomSize;
        float tMinusB_inv = 1/(topSize - bottomSize);
        result[4] = (2*up.x - t.x*tPlusB) * tMinusB_inv;
        result[5] = (2*up.y - t.y*tPlusB) * tMinusB_inv;
        result[6] = (2*up.z - t.z*tPlusB) * tMinusB_inv;        
        result[7] = -tPlusB * tMinusB_inv;

        float fPlusN = far + near;
        float fMinusN_inv = 1/(far - near);
        result[8]  = (2*dir.x - t.x*fPlusN) * fMinusN_inv;
        result[9]  = (2*dir.y - t.y*fPlusN) * fMinusN_inv;
        result[10] = (2*dir.z - t.z*fPlusN) * fMinusN_inv;        
        result[11] = -fPlusN * fMinusN_inv;

        result[12] = t.x;
        result[13] = t.y;
        result[14] = t.z;        
        result[15] = 1;

        return result;
    }

    public static  float[] ProjectPoint(float[] projViewMatrix, Vector3 point)
    {
        float[] result = new float[3];

        result[0] = point.x * projViewMatrix[0]  + point.y * projViewMatrix[1]  + point.z * projViewMatrix[2]  + projViewMatrix[3];
        result[1] = point.x * projViewMatrix[4]  + point.y * projViewMatrix[5]  + point.z * projViewMatrix[6]  + projViewMatrix[7];
        result[2] = point.x * projViewMatrix[8]  + point.y * projViewMatrix[9]  + point.z * projViewMatrix[10] + projViewMatrix[11];
        float w   = point.x * projViewMatrix[12] + point.y * projViewMatrix[13] + point.z * projViewMatrix[14] + projViewMatrix[15];

        result[0] /= w;
        result[1] /= w;
        result[2] /= w;

        return result;
    }


 */
}
