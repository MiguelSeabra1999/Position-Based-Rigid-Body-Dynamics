using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpawnerCloth : MonoBehaviour
{
    public bool createOnAwake = true;
    public bool drawParticleMeshes = true;
    public PrefabSO prefabSO;
    public double compliance = 0;
    public double friction = 0.2;
    public float edge = 1;
    public Vector3Int dims = new Vector3Int(1, 1, 1);
    private PhysicsEngine engine;
    public Particle[,] particles = new Particle[2, 2];
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

    [ContextMenu("createCloth")]
    private void Init()
    {
        mesh = new Mesh();
        meshFilter = GetComponent<MeshFilter>();

        id = 0;
        engine = transform.parent.GetComponent<PhysicsEngine>();

        int n = (dims.x + 1) * (dims.y + 1);
        particles = new Particle[dims.x + 1, dims.y + 1];
        connected = new bool[n, n];

        CreateCloth();
        CreateMesh();
    }

    void Update()
    {
        CreateMesh();
        if (Input.GetKey(KeyCode.P))
            foreach (Particle p in particles)
            {
                p.position = new DoubleVector3(p.position.x, 0.5f, p.position.z);
                p.prevPosition = p.position;
            }
    }

    private void CreateMesh()
    {
        newVertices.Clear();
        newUV.Clear();
        newTriangles.Clear();

        for (int x = 0; x < dims.x - 1; x++)
        {
            for (int y = 0; y < dims.y - 1; y++)
            {
                AddCubeFace(
                    particles[x, y].transform.localPosition,
                    particles[x, y + 1].transform.localPosition,
                    particles[x + 1, y].transform.localPosition,
                    particles[x + 1, y + 1].transform.localPosition
                );
            }
        }


        SetMesh();
    }

    private void CreateCloth()
    {
        for (int y = 0; y < dims.y; y++)
        {
            for (int x = 0; x < dims.x; x++)
            {
                particles[x, y] = SpawnParticle(transform.position + new Vector3(x, y, 0) * edge);
            }
        }
        for (int y = 0; y < dims.y - 1; y++)
        {
            for (int x = 0; x < dims.x - 1; x++)
            {
                ConnectParticles(x, y);
            }
        }
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

        index = newVertices.Count;
        newVertices.Add(pos0);
        newVertices.Add(pos1);
        newVertices.Add(pos2);
        newVertices.Add(pos3);
        newTriangles.Add(index + 2);
        newTriangles.Add(index + 1);
        newTriangles.Add(index);
        newTriangles.Add(index + 1);
        newTriangles.Add(index + 2);
        newTriangles.Add(index + 3);
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

    private void ConnectParticles(int x, int y)
    {
        AddDistanceConstraint(new Vector2Int(x, y),   new Vector2Int(x, y + 1));
        AddDistanceConstraint(new Vector2Int(x, y),   new Vector2Int(x + 1, y));
        AddDistanceConstraint(new Vector2Int(x + 1, y), new Vector2Int(x + 1, y + 1));
        AddDistanceConstraint(new Vector2Int(x, y + 1), new Vector2Int(x + 1, y + 1));
/*
        AddDistanceConstraint(new Vector2Int(x, y),   new Vector2Int(x + 1, y + 1));
        AddDistanceConstraint(new Vector2Int(x + 1, y), new Vector2Int(x , y + 1));
        */
    }

    private Particle SpawnParticle(Vector3 pos)
    {
        GameObject obj = Instantiate(prefabSO.pbdParticle, pos , Quaternion.identity);
        obj.transform.localScale = new Vector3(0.25f, 0.25f, 0.25f);
        obj.GetComponent<PBDColliderSphere>().radius = 0.125;
        obj.transform.SetParent(transform);
        Particle p = obj.GetComponent<Particle>();
        p.staticFrictionCoefficient = 0;
        p.dynamicFrictionCoefficient = friction;
        obj.name = "" + id++;

        if (!drawParticleMeshes)
        {
            obj.GetComponent<MeshRenderer>().enabled = false;
        }
        return p;
    }

    private void AddDistanceConstraint(Vector2Int a, Vector2Int b)
    {
        if (connected[VecToIndex(a), VecToIndex(b)])
            return;

        DistanceConstraint c = new DistanceConstraint();
        c.compliance = compliance;
        c.body = particles[a.x, a.y];
        c.otherBody = particles[b.x, b.y];
        c.goalDistance = (particles[a.x, a.y].gameObject.transform.position -  particles[b.x, b.y].gameObject.transform.position).magnitude;
        engine.distanceConstraints.Add(c);

        connected[VecToIndex(a), VecToIndex(b)] = true;
        connected[VecToIndex(b), VecToIndex(a)] = true;
    }

    private int VecToIndex(Vector2Int v)
    {
        return v.y * (dims.x + 1) + v.x;
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
