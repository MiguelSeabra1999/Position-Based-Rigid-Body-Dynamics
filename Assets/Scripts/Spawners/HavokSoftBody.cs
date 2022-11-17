using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Physics.Authoring;

public class HavokSoftBody : MonoBehaviour
{
    public bool createOnAwake = true;
    public bool drawParticleMeshes = true;
    public PrefabSO prefabSO;

    public float compliance = 0;
    public float edge = 1;
    public Vector3Int dims = new Vector3Int(1, 1, 1);

    public GameObject[,,] particles = new GameObject[2, 2, 2];
    private bool[,] connected = new bool[8, 8];

    private int id = 0;

    [HideInInspector] public List<Vector3> newVertices = new List<Vector3>();
    [HideInInspector] public List<Vector2> newUV = new List<Vector2>();
    [HideInInspector] public List<int> newTriangles = new List<int>();
    [HideInInspector] public MeshFilter meshFilter;
    [HideInInspector] public Mesh mesh;


    void Awake()
    {
        if (createOnAwake)
            Init();
    }

    [ContextMenu("createSoftBody")]
    private void Init()
    {
        mesh = new Mesh();
        meshFilter = GetComponent<MeshFilter>();

        id = 0;


        int n = (dims.x + 1) * (dims.y + 1) * (dims.z + 1);
        particles = new GameObject[dims.x + 1, dims.y + 1, dims.z + 1];
        connected = new bool[n, n];

        MakeCube(transform.position);
        AddNeighbors();
        //CreateMesh();
    }

    void Update()
    {
        //CreateMesh();
    }

    private void CreateMesh()
    {
        newVertices.Clear();
        newUV.Clear();
        newTriangles.Clear();

        for (int x = 0; x < dims.x; x++)
        {
            for (int y = 0; y < dims.y; y++)
            {
                AddCubeFace(
                    particles[x, y, 0].transform.localPosition,
                    particles[x, y + 1, 0].transform.localPosition,
                    particles[x + 1, y, 0].transform.localPosition,
                    particles[x + 1, y + 1, 0].transform.localPosition
                );
                AddCubeFace(
                    particles[x, y, dims.z].transform.localPosition,
                    particles[x + 1, y, dims.z].transform.localPosition,
                    particles[x, y + 1, dims.z].transform.localPosition,
                    particles[x + 1, y + 1, dims.z].transform.localPosition
                );
            }
        }
        for (int z = 0; z < dims.z; z++)
        {
            for (int y = 0; y < dims.y; y++)
            {
                AddCubeFace(
                    particles[0, y, z].transform.localPosition,
                    particles[0, y, z + 1].transform.localPosition,
                    particles[0, y + 1, z].transform.localPosition,
                    particles[0, y + 1, z + 1].transform.localPosition
                );
                AddCubeFace(
                    particles[dims.x, y, z].transform.localPosition,
                    particles[dims.x, y + 1, z].transform.localPosition,
                    particles[dims.x, y, z + 1].transform.localPosition,
                    particles[dims.x, y + 1, z + 1].transform.localPosition
                );
            }
        }
        for (int x = 0; x < dims.x; x++)
        {
            for (int z = 0; z < dims.z; z++)
            {
                AddCubeFace(
                    particles[x  , 0 , z].transform.localPosition,
                    particles[x + 1, 0 , z].transform.localPosition,
                    particles[x  , 0 , z + 1].transform.localPosition,
                    particles[x + 1, 0 , z + 1].transform.localPosition
                );
                AddCubeFace(
                    particles[x  , dims.y, z].transform.localPosition,
                    particles[x , dims.y, z + 1].transform.localPosition,
                    particles[x + 1 , dims.y, z].transform.localPosition,
                    particles[x + 1, dims.y, z + 1].transform.localPosition
                );
            }
        }


        SetMesh();
    }

    private void AddCubeFace(Vector3 pos0, Vector3 pos1, Vector3 pos2, Vector3 pos3)
    {
        int index = newVertices.Count;
        newVertices.Add(pos0);
        newVertices.Add(pos1);
        newVertices.Add(pos2);
        newVertices.Add(pos3);
        newTriangles.Add(index);
        newTriangles.Add(index + 1);
        newTriangles.Add(index + 2);
        newTriangles.Add(index + 3);
        newTriangles.Add(index + 2);
        newTriangles.Add(index + 1);
        newUV.Add(new Vector2(0, 0));
        newUV.Add(new Vector2(0, 1));
        newUV.Add(new Vector2(1, 0));
        newUV.Add(new Vector2(0, 0));
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

    private void AddNeighbors()
    {
        for (int i = 1; i < dims.x; i++)
            AddNeighborCubeRight(i, 0, 0);

        for (int z = 1; z < dims.z; z++)
        {
            AddNeighborCubeForward(0, 0, z);
            for (int x = 1; x < dims.x; x++)
            {
                //  AddNeighborCubeRight(j, i, 0);

                particles[x + 1, 0, z + 1]    = SpawnParticle(particles[x, 0, z + 1].transform.position + Vector3.right * edge);
                particles[x + 1, 0 + 1, z + 1]  = SpawnParticle(particles[x, 0 + 1, z + 1].transform.position + Vector3.right * edge);
                ConnectParticles(x, 0, z);
            }
        }

        for (int y = 1; y < dims.y; y++)
            AddNeighborCubeUp(0, y, 0);

        for (int x = 1; x < dims.x; x++)
            for (int y = 1; y < dims.y; y++)
            {
                particles[x + 1, y + 1, 0] = SpawnParticle(particles[x + 1, y, 0].transform.position + Vector3.up * edge);
                particles[x + 1, y + 1, 0 + 1] = SpawnParticle(particles[x + 1, y, 0 + 1].transform.position + Vector3.up * edge);
                ConnectParticles(x, y, 0);
            }

        for (int z = 1; z < dims.z; z++)
        {
            for (int y = 1; y < dims.y; y++)
            {
                particles[0, y + 1, z + 1] = SpawnParticle(particles[0, y, z + 1].transform.position + Vector3.up * edge);
                particles[0 + 1, y + 1, z + 1] = SpawnParticle(particles[0 + 1, y, z + 1].transform.position + Vector3.up * edge);
                ConnectParticles(0, y, z);
            }
            for (int x = 1; x < dims.x; x++)
            {
                for (int y = 1; y < dims.y; y++)
                {
                    particles[x + 1, y + 1, z + 1] = SpawnParticle(particles[x + 1, y, z + 1].transform.position + Vector3.up * edge);
                    ConnectParticles(x, y, z);
                }
            }
        }
    }

    private void AddNeighborCubeRight(int x, int y, int z)
    {
        particles[x + 1, y, z]      = SpawnParticle(particles[x, y, z].transform.position + Vector3.right * edge);
        particles[x + 1, y + 1, z]    = SpawnParticle(particles[x, y + 1, z].transform.position + Vector3.right * edge);
        particles[x + 1, y, z + 1]    = SpawnParticle(particles[x, y, z + 1].transform.position + Vector3.right * edge);
        particles[x + 1, y + 1, z + 1]  = SpawnParticle(particles[x, y + 1, z + 1].transform.position + Vector3.right * edge);
        ConnectParticles(x, y, z);
    }

    private void AddNeighborCubeUp(int x, int y, int z)
    {
        particles[x, y + 1, z] = SpawnParticle(particles[x, y, z].transform.position + Vector3.up * edge);
        particles[x + 1, y + 1, z] = SpawnParticle(particles[x + 1, y, z].transform.position + Vector3.up * edge);
        particles[x, y + 1, z + 1] = SpawnParticle(particles[x, y, z + 1].transform.position + Vector3.up * edge);
        particles[x + 1, y + 1, z + 1] = SpawnParticle(particles[x + 1, y, z + 1].transform.position + Vector3.up * edge);
        ConnectParticles(x, y, z);
    }

    private void AddNeighborCubeForward(int x, int y, int z)
    {
        particles[x, y, z + 1] = SpawnParticle(particles[x, y, z].transform.position + Vector3.forward * edge);
        particles[x + 1, y , z + 1] = SpawnParticle(particles[x + 1, y, z].transform.position + Vector3.forward * edge);
        particles[x, y + 1, z + 1] = SpawnParticle(particles[x, y + 1, z].transform.position + Vector3.forward * edge);
        particles[x + 1, y + 1, z + 1] = SpawnParticle(particles[x + 1, y + 1, z].transform.position + Vector3.forward * edge);
        ConnectParticles(x, y, z);
    }

    private void ConnectParticles(int x, int y, int z)
    {/*
        particles[0] - > particles[x,y,z]
        particles[1] - > particles[x,y+1,z]
        particles[2] - > particles[x,y,z+1]
        particles[3] - > particles[x+1,y,z]
        particles[4] - > particles[x+1,y+1,z]
        particles[5] - > particles[x+1,y+1,z+1]
        particles[6] - > particles[x,y+1,z+1]
        particles[7] - > particles[x+1,y,z+1]
*/
        AddDistanceConstraint(new Vector3Int(x, y, z),   new Vector3Int(x, y, z + 1));
        AddDistanceConstraint(new Vector3Int(x, y, z),   new Vector3Int(x + 1, y, z));
        AddDistanceConstraint(new Vector3Int(x, y, z),   new Vector3Int(x, y + 1, z));
        AddDistanceConstraint(new Vector3Int(x, y + 1, z), new Vector3Int(x, y, z + 1));
        AddDistanceConstraint(new Vector3Int(x, y + 1, z), new Vector3Int(x + 1, y, z));
        AddDistanceConstraint(new Vector3Int(x, y, z + 1), new Vector3Int(x + 1, y, z));


        AddTetrahedronConstrains(new Vector3Int(x + 1, y + 1, z + 1), new Vector3Int(x + 1, y + 1, z), new Vector3Int(x, y + 1, z + 1), new Vector3Int(x + 1, y, z + 1));
        AddTetrahedronConstrains(new Vector3Int(x + 1, y + 1, z + 1), new Vector3Int(x, y + 1, z), new Vector3Int(x, y, z + 1), new Vector3Int(x + 1, y, z));
        AddTetrahedronConstrains(new Vector3Int(x + 1, y + 1, z), new Vector3Int(x, y + 1, z), new Vector3Int(x + 1, y, z));
        AddTetrahedronConstrains(new Vector3Int(x, y + 1, z + 1), new Vector3Int(x, y + 1, z), new Vector3Int(x, y, z + 1));
        AddTetrahedronConstrains(new Vector3Int(x + 1, y, z + 1), new Vector3Int(x, y, z + 1), new Vector3Int(x + 1, y, z));
    }

    private void MakeCube(Vector3 pos)
    {
        particles[0, 0, 0] = SpawnParticle(pos);
        particles[0, 0, 1] = SpawnParticle(pos + new Vector3(0, 0, 1) * edge);
        particles[0, 1, 0] = SpawnParticle(pos + new Vector3(0, 1, 0) * edge);
        particles[0, 1, 1] = SpawnParticle(pos + new Vector3(0, 1, 1) * edge);
        particles[1, 0, 0] = SpawnParticle(pos + new Vector3(1, 0, 0) * edge);
        particles[1, 0, 1] = SpawnParticle(pos + new Vector3(1, 0, 1) * edge);
        particles[1, 1, 0] = SpawnParticle(pos + new Vector3(1, 1, 0) * edge);
        particles[1, 1, 1] = SpawnParticle(pos + new Vector3(1, 1, 1) * edge);

        ConnectParticles(0, 0, 0);
    }

    /*private void MakeTetrahedron(Vector3 pos, Vector3 up, Vector3 right)
    {
        particles[0] = SpawnParticle(pos);
        particles[1] = SpawnParticle(pos + up * edge);
        particles[2] = SpawnParticle(pos + Vector3.Cross(right, up) * edge);
        particles[3] = SpawnParticle(pos + right * edge);

        AddDistanceConstraint(0, 1);
        AddDistanceConstraint(0, 2);
        AddDistanceConstraint(0, 3);
        AddDistanceConstraint(1, 2);
        AddDistanceConstraint(1, 3);
        AddDistanceConstraint(2, 3);

        AddVolumeConstraint(0, 1, 2, 3);
    }*/

    private void AddTetrahedronConstrains(Vector3Int a, Vector3Int b, Vector3Int c, Vector3Int d)
    {
        AddDistanceConstraint(a, b);
        AddDistanceConstraint(a, c);
        AddDistanceConstraint(a, d);
    }

    private void AddTetrahedronConstrains(Vector3Int a, Vector3Int b, Vector3Int c)
    {
        AddDistanceConstraint(a, b);
        AddDistanceConstraint(a, c);
    }

    private GameObject SpawnParticle(Vector3 pos)
    {
        GameObject obj = Instantiate(prefabSO.havokParticle, pos , Quaternion.identity);
        obj.transform.SetParent(transform.parent);
        obj.transform.localScale = new Vector3(0.25f, 0.25f, 0.25f);

        obj.transform.SetParent(transform);

        obj.name = "" + id++;

        if (!drawParticleMeshes)
        {
            obj.GetComponent<MeshRenderer>().enabled = false;
        }
        return obj;
    }

    private void AddDistanceConstraint(Vector3Int a, Vector3Int b)
    {
        if (connected[VecToIndex(a), VecToIndex(b)])
            return;

        LimitedDistanceJoint c = particles[a.x, a.y, a.z].gameObject.AddComponent<LimitedDistanceJoint>();

        c.ConnectedBody = particles[b.x, b.y, b.z].GetComponent<PhysicsBodyAuthoring>();
        c.EnableCollision = false;
        c.PositionLocal = new Vector3(0, 0, 0);
        c.PositionInConnectedEntity = new Vector3(0, 0, 0);
        c.AutoSetConnected = false;

        float dist = (particles[a.x, a.y, a.z].transform.position - particles[b.x, b.y, b.z].transform.position).magnitude;
        c.MinDistance = c.MaxDistance = dist;


        connected[VecToIndex(a), VecToIndex(b)] = true;
        connected[VecToIndex(b), VecToIndex(a)] = true;
    }

    private int VecToIndex(Vector3Int v)
    {
        return (v.z * (dims.x + 1) * (dims.y + 1)) + (v.y * (dims.x + 1)) + v.x;
        //return v.x + (dims.y + 1) * (v.y + (dims.z + 1) * v.z);
    }
}
