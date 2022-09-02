using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpawnerSoftBody : MonoBehaviour
{
    public bool drawParticleMeshes = true;
    public PrefabSO prefabSO;
    public double compliance = 0;
    public double friction = 0.2;
    public float edge = 1;
    public Vector3Int dims = new Vector3Int(1, 1, 1);
    private PhysicsEngine engine;
    private Particle[,,] particles = new Particle[2, 2, 2];
    private bool[,] connected = new bool[8, 8];

    private int id = 0;

    private List<Vector3> newVertices = new List<Vector3>();
    private List<Vector2> newUV = new List<Vector2>();
    private List<int> newTriangles = new List<int>();
    private MeshFilter meshFilter;
    private Mesh mesh;


    void Awake()
    {
        Init();
    }

    [ContextMenu("createSoftBody")]
    private void Init()
    {
        mesh = new Mesh();
        meshFilter = GetComponent<MeshFilter>();

        id = 0;
        engine = transform.parent.GetComponent<PhysicsEngine>();

        int n = (dims.x + 1) * (dims.y + 1) * (dims.z + 1);
        particles = new Particle[dims.x + 1, dims.y + 1, dims.z + 1];
        connected = new bool[n, n];

        MakeCube(transform.position);
        AddNeighbors();
    }

    void Update()
    {
        CreateMesh();
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


        AddVolumeConstraint(particles[x, y, z],     particles[x, y + 1, z], particles[x, y, z + 1],     particles[x + 1, y, z]);
        AddVolumeConstraint(particles[x, y + 1, z + 1], particles[x, y + 1, z], particles[x + 1, y + 1, z + 1], particles[x, y, z + 1]);
        AddVolumeConstraint(particles[x + 1, y + 1, z], particles[x, y + 1, z], particles[x + 1, y, z],     particles[x + 1, y + 1, z + 1]);
        AddVolumeConstraint(particles[x + 1, y, z + 1], particles[x, y, z + 1], particles[x + 1, y + 1, z + 1], particles[x + 1, y, z]);


        //  AddVolumeConstraint(p[x+1,y+1,z+1], p[x,y+1,z], p[x+1,y,z], p[x,y,z+1]);
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

    private Particle SpawnParticle(Vector3 pos)
    {
        GameObject obj = Instantiate(prefabSO.pbdParticle, pos , Quaternion.identity);
        obj.transform.localScale = new Vector3(0.25f, 0.25f, 0.25f);
        obj.GetComponent<PBDColliderSphere>().radius = 0.125;
        obj.transform.SetParent(transform);
        Particle p = obj.GetComponent<Particle>();
        p.staticFrictionCoefficient = friction;
        p.dynamicFrictionCoefficient = friction;
        obj.name = "" + id++;

        if (!drawParticleMeshes)
        {
            obj.GetComponent<MeshRenderer>().enabled = false;
        }
        return p;
    }

    private void AddDistanceConstraint(Vector3Int a, Vector3Int b)
    {
        if (connected[VecToIndex(a), VecToIndex(b)])
            return;

        DistanceConstraint c = new DistanceConstraint();
        c.compliance = compliance;
        c.body = particles[a.x, a.y, a.z];
        c.otherBody = particles[b.x, b.y, b.z];
        c.goalDistance = (particles[a.x, a.y, a.z].gameObject.transform.position -  particles[b.x, b.y, b.z].gameObject.transform.position).magnitude;
        engine.distanceConstraints.Add(c);

        connected[VecToIndex(a), VecToIndex(b)] = true;
        connected[VecToIndex(b), VecToIndex(a)] = true;
    }

    private int VecToIndex(Vector3Int v)
    {
        return (v.z * (dims.x + 1) * (dims.y + 1)) + (v.y * (dims.x + 1)) + v.x;
        //return v.x + (dims.y + 1) * (v.y + (dims.z + 1) * v.z);
    }

    private void AddVolumeConstraint(Particle a, Particle b, Particle c, Particle d)
    {
        VolumeConstraint constraint = new VolumeConstraint();
        constraint.compliance = 0;
        constraint.body0 = a;
        constraint.body1 = b;
        constraint.body2 = c;
        constraint.body3 = d;
        engine.volumeConstraint.Add(constraint);
    }
}
