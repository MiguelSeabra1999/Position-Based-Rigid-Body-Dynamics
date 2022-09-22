using System.Data;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpawnerCloth : MonoBehaviour
{
    public double breakForce = 0;
    public bool createOnAwake = true;
    public bool drawParticleMeshes = true;
    public PrefabSO prefabSO;
    public double compliance = 0;
    public double friction = 0.2;
    public float edge = 1;
    public float size = 0.125f;
    public Vector3Int dims = new Vector3Int(1, 1, 1);
    private PhysicsEngine engine;
    public Particle[,] particles = new Particle[2, 2];
    private bool[,] connected = new bool[8, 8];
    private PBDConstraint[,] connectedConstraint = new PBDConstraint[4, 4];
    public Particle attachTo;

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
        connectedConstraint = new PBDConstraint[n, n];

        CreateCloth();
        CreateMesh();
        particles[0, 0].inverseMass = 0;
        particles[0, 0].gravityScale = 0;
        particles[0, dims.y - 1].inverseMass = 0;
        particles[0, dims.y - 1].gravityScale = 0;
        particles[dims.x - 1, 0].inverseMass = 0;
        particles[dims.x - 1, 0].gravityScale = 0;
        particles[dims.x - 1, dims.y - 1].inverseMass = 0;
        particles[dims.x - 1, dims.y - 1].gravityScale = 0;
/** /
        if (attachTo != null)
        {
            DistanceConstraint c = new DistanceConstraint();
            c.body = attachTo;
            c.otherBody = particles[dims.x - 1, 0];
            c.firstBodyOffsetFloat = new Vector3(-.5f, .5f, -.5f);
            engine.distanceConstraints.Add(c);

            c = new DistanceConstraint();
            c.body = attachTo;
            c.otherBody = particles[0, 0];
            c.firstBodyOffsetFloat = new Vector3(.5f, .5f, -.5f);
            engine.distanceConstraints.Add(c);
        }
        /**/
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
                AddCubeFace(x, y);
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
                particles[x, y] = SpawnParticle(transform.position + new Vector3(x,  y, transform.position.z) * edge);
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

    private bool IsLineConnected(int i0, int i1)
    {
        PBDConstraint c = connectedConstraint[i0, i1];
        if (c == null)
            return true;
        if (connectedConstraint[i0, i1].broken)
            return false;
        return true;
    }

    private void BreakConstraint(int i0, int i1)
    {
        PBDConstraint c = connectedConstraint[i0, i1];
        if (c == null)
            return;
        connectedConstraint[i0, i1].broken = true;
    }

    private bool IsTriConnected(int i0, int i1, int i2)
    {
        int count = 0;
        if (IsLineConnected(i0, i1))
            count++;
        if (IsLineConnected(i1, i2))
            count++;
        if (IsLineConnected(i2, i0))
            count++;


        if (count <= 1)
        {
            BreakConstraint(i0, i1);
            BreakConstraint(i1, i2);
            BreakConstraint(i2, i0);
        }
        if (count == 3)
            return true;
        return false;
    }

    private void AddCubeFace(int x, int y)
    {
        bool connected0 = false;
        bool connected1 = false;
        Vector3 pos0 = particles[x, y].transform.localPosition;
        Vector3 pos1 = particles[x, y + 1].transform.localPosition;
        Vector3 pos2 = particles[x + 1, y].transform.localPosition;
        Vector3 pos3 = particles[x + 1, y + 1].transform.localPosition;

        int index0 = VecToIndex(new Vector2Int(x, y));
        int index1 = VecToIndex(new Vector2Int(x, y + 1));
        int index2 = VecToIndex(new Vector2Int(x + 1, y));
        int index3 = VecToIndex(new Vector2Int(x + 1, y + 1));

        int index = newVertices.Count;
        newVertices.Add(pos0);
        newVertices.Add(pos1);
        newVertices.Add(pos2);
        newVertices.Add(pos3);

        if (IsTriConnected(index0, index1, index2))
        {
            newTriangles.Add(index);
            newTriangles.Add(index + 1);
            newTriangles.Add(index + 2);
            connected0 = true;
        }
        if (IsTriConnected(index3, index2, index1))
        {
            newTriangles.Add(index + 3);
            newTriangles.Add(index + 2);
            newTriangles.Add(index + 1);
            connected1 = true;
        }


        newUV.Add(new Vector2(0, 0));
        newUV.Add(new Vector2(0, 1));
        newUV.Add(new Vector2(1, 0));
        newUV.Add(new Vector2(0, 0));

        index = newVertices.Count;
        newVertices.Add(pos0);
        newVertices.Add(pos1);
        newVertices.Add(pos2);
        newVertices.Add(pos3);
        if (connected0)
        {
            newTriangles.Add(index + 2);
            newTriangles.Add(index + 1);
            newTriangles.Add(index);
        }
        if (connected1)
        {
            newTriangles.Add(index + 1);
            newTriangles.Add(index + 2);
            newTriangles.Add(index + 3);
        }
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

        AddDistanceConstraint(new Vector2Int(x, y),   new Vector2Int(x + 1, y + 1));
        AddDistanceConstraint(new Vector2Int(x + 1, y), new Vector2Int(x , y + 1));
    }

    private Particle SpawnParticle(Vector3 pos)
    {
        GameObject obj = Instantiate(prefabSO.pbdParticle, pos , Quaternion.identity);
        obj.transform.localScale = new Vector3(size, size, size);
        obj.GetComponent<PBDColliderSphere>().radius = size / 2f;
        obj.transform.SetParent(transform);
        Particle p = obj.GetComponent<Particle>();
        p.staticFrictionCoefficient = friction;
        p.dynamicFrictionCoefficient = friction;
        p.restitution = 0;
        p.mass = 0.01;
        p.inverseMass = 100;
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
        c.breakForce = breakForce;
        engine.distanceConstraints.Add(c);

        connected[VecToIndex(a), VecToIndex(b)] = true;
        connected[VecToIndex(b), VecToIndex(a)] = true;
        connectedConstraint[VecToIndex(a), VecToIndex(b)] = c;
        connectedConstraint[VecToIndex(b), VecToIndex(a)] = c;
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
