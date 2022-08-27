using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TetrahedronMesh : MonoBehaviour
{
    private List<Vector3> newVertices = new List<Vector3>();
    private List<Vector2> newUV = new List<Vector2>();
    private List<int> newTriangles = new List<int>();
    private MeshFilter meshFilter;
    private Mesh mesh;

    private GameObject anchor0, anchor1, anchor2, anchor3;
    void Start()
    {
        Init();
    }

    [ContextMenu("createMesh")]
    private void Init()
    {
        mesh = new Mesh();
        meshFilter = GetComponent<MeshFilter>();

        anchor0 = transform.GetChild(0).gameObject;
        anchor1 = transform.GetChild(1).gameObject;
        anchor2 = transform.GetChild(2).gameObject;
        anchor3 = transform.GetChild(3).gameObject;

        CreateMesh(anchor0.transform.position, anchor1.transform.position, anchor2.transform.position, anchor3.transform.position);
    }

    void Update()
    {
        CreateMesh(anchor0.transform.position, anchor1.transform.position, anchor2.transform.position, anchor3.transform.position);
    }

    private void CreateMesh(Vector3 pos0, Vector3 pos1, Vector3 pos2, Vector3 pos3)
    {
        newVertices.Clear();
        newUV.Clear();
        newTriangles.Clear();

        AddTriangle(pos2 ,   pos1, pos0);
        AddTriangle(pos1, pos3 , pos0);
        AddTriangle(pos3, pos2, pos0);
        AddTriangle(pos2, pos3, pos1);

        int index = newUV.Count;
        newUV[index - 3] = new Vector2(1, 0);
        newUV[index - 2] = new Vector2(0, 0);
        newUV[index - 1] = new Vector2(.5f, 1);


        SetMesh();
    }

    private void SetMesh()
    {
        mesh.Clear();
        mesh.vertices = newVertices.ToArray();
        mesh.uv = newUV.ToArray();
        mesh.triangles = newTriangles.ToArray();

        mesh.RecalculateNormals();
        meshFilter.mesh = mesh;
    }

    private void AddTriangle(Vector3 a, Vector3 b, Vector3 c)
    {
        int startIndex = newVertices.Count;
        newVertices.Add(a);
        newVertices.Add(b);
        newVertices.Add(c);

        newTriangles.Add(startIndex);
        newUV.Add(new Vector2(1, 0));
        newTriangles.Add(startIndex + 1);
        newUV.Add(new Vector2(0, 1));
        newTriangles.Add(startIndex + 2);
        newUV.Add(new Vector2(0, 0));
    }
}
