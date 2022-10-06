using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Physics.Authoring;

public class SpawnerComparissons : MonoBehaviour
{
    public bool buildOnAwake = false;
    public Vector3Int dims = new Vector3Int();
    public Vector3 startPos = new Vector3();
    public float spacing = 1;
    public float positionalNoise = 0;
    public bool randRotation;
    public double compliance = 0;

    private int count = 0;
    public PrefabSO prefabSO;
    private GameObject pbd, havok, unity, jacobi;
    private PhysicsEngine pbdEngine;
    private PhysicsEngineJacobi jacobiEngine;

    private List<GameObject> pbdLinks = new List<GameObject>();

    void Start()
    {
        if (!buildOnAwake)
            return;
        ClearHierarchy();
        CreateScenes();

        SpawnRigidCradle();
    }

    [ContextMenu("CreateWrekingBall")]
    private void CreateWrekingBall()
    {
        startPos = new Vector3(0, 11, 0);
        dims = new Vector3Int(8, 0, 0);
        SpawnRope();
        startPos = new Vector3(0, .5f, 0);
        dims = new Vector3Int(3, 2, 5);
        SpawnCube();
    }

    [ContextMenu("CreateScenes")]
    private void CreateScenes()
    {
        pbd   = Instantiate(prefabSO.pbdScene);
        pbd.transform.SetParent(transform);
        pbdEngine = pbd.GetComponent<PhysicsEngine>();

        jacobi   = Instantiate(prefabSO.pbdSceneJacobi);
        jacobi.transform.SetParent(transform);
        jacobiEngine = jacobi.GetComponent<PhysicsEngineJacobi>();

        havok = Instantiate(prefabSO.havokScene);
        havok.transform.SetParent(transform);

        unity = Instantiate(prefabSO.unityScene);
        unity.transform.SetParent(transform);
    }

    [ContextMenu("SpawnCube")]
    private void CreateCube()
    {
        SpawnCube();
    }

    public void SpawnCube()
    {
        count = 0;
        Vector3 startingPoint = new Vector3(transform.position.x - dims.x * spacing * 0.5f,
            transform.position.y + 1,
            transform.position.z - dims.y * spacing * 0.5f);
        for (int i = 0; i < dims.z; i++)
        {
            Vector3 point = startingPoint + Vector3.up * spacing * i;
            SpawnPlaneOfCubes(point, dims.x, dims.y);
        }
    }

    private void SpawnPlaneOfCubes(Vector3 startingPoint, int x, int y)
    {
        for (int i = 0; i < x; i++)
        {
            Vector3 point = startingPoint + spacing * i * Vector3.right;
            SpawnLineOfCubes(Vector3.forward, point , y);
        }
    }

    private void SpawnLineOfCubes(Vector3 axis, Vector3 startingPoint, int n)
    {
        for (int i = 0; i < n; i++)
        {
            Vector3 point = startingPoint + axis.normalized * spacing * i;
            Vector3 noise = new Vector3(Random.Range(-positionalNoise, positionalNoise), Random.Range(-positionalNoise, positionalNoise), Random.Range(-positionalNoise, positionalNoise));
            Quaternion rotation = Quaternion.identity;
            if (randRotation)
                rotation = Random.rotation;
            SpawnAllVersions(point + noise, rotation);
        }
    }

    private GameObject SpawnInstance(GameObject prefab, Vector3 pos, Quaternion rot, Transform parent)
    {
        GameObject newObj = Instantiate(prefab, pos, rot);
        newObj.transform.SetParent(parent);
        newObj.name = newObj.name + count;
        count++;
        return newObj;
    }

    private void SpawnAllVersions(Vector3 pos, Quaternion rot)
    {
        SpawnInstance(prefabSO.pbdCube, pos, rot, pbd.transform);
        SpawnInstance(prefabSO.pbdCube, pos, rot, jacobi.transform);
        SpawnInstance(prefabSO.havokCube, pos, rot, havok.transform);
        SpawnInstance(prefabSO.unityCube, pos, rot, unity.transform);
    }

    private void SpawnAllVersionsCapsule(Vector3 pos, Quaternion rot)
    {
        SpawnInstance(prefabSO.pbdCapsule, pos, rot, pbd.transform);
        SpawnInstance(prefabSO.pbdCapsule, pos, rot, jacobi.transform);
        SpawnInstance(prefabSO.havokCapsule, pos, rot, havok.transform);
        SpawnInstance(prefabSO.unityCapsule, pos, rot, unity.transform);
    }

    private (GameObject, GameObject,  GameObject, GameObject)  SpawnAllVersionsSphere(Vector3 pos, Quaternion rot)
    {
        GameObject obj = SpawnInstance(prefabSO.pbdSphere, pos, rot, pbd.transform);
        GameObject objJacobi = SpawnInstance(prefabSO.pbdSphere, pos, rot, jacobi.transform);
        GameObject objHavok = SpawnInstance(prefabSO.havokSphere, pos, rot, havok.transform);
        GameObject objUnity = SpawnInstance(prefabSO.unitySphere, pos, rot, unity.transform);
        return (obj, objJacobi, objHavok, objUnity);
    }

    private (GameObject, GameObject, GameObject, GameObject)  SpawnAllVersionsWrekingBall(Vector3 pos, Quaternion rot)
    {
        GameObject obj = SpawnInstance(prefabSO.pbdWreckingBall, pos, rot, pbd.transform);
        GameObject objJacobi = SpawnInstance(prefabSO.pbdWreckingBall, pos, rot, jacobi.transform);
        GameObject objHavok = SpawnInstance(prefabSO.havokWreckingBall, pos, rot, havok.transform);
        GameObject objUnity = SpawnInstance(prefabSO.unityWreckingBall, pos, rot, unity.transform);
        return (obj, objJacobi, objHavok, objUnity);
    }

    private (GameObject, GameObject, GameObject, GameObject) SpawnAllVersionsCapsuleTrigger(Vector3 pos, Quaternion rot)
    {
        GameObject obj = SpawnInstance(prefabSO.pbdCapsule, pos, rot, pbd.transform);
        MakeTriggerPBD(obj);
        GameObject objJacobi = SpawnInstance(prefabSO.pbdCapsule, pos, rot, jacobi.transform);
        MakeTriggerPBD(objJacobi);

        GameObject objHavok = SpawnInstance(prefabSO.havokCapsuleTrigger, pos, rot, havok.transform);

        GameObject objUnity = SpawnInstance(prefabSO.unityCapsule, pos, rot, unity.transform);
        MakeTriggerUnity(objUnity);

        return (obj, objJacobi, objHavok, objUnity);
    }

    private (GameObject, GameObject, GameObject, GameObject) SpawnAllVersionsStaticSphere(Vector3 pos, Quaternion rot)
    {
        GameObject obj = SpawnInstance(prefabSO.pbdSphere, pos, rot, pbd.transform);
        MakeStaticPBD(obj);
        GameObject jacobiObj = SpawnInstance(prefabSO.pbdSphere, pos, rot, jacobi.transform);
        MakeStaticPBD(jacobiObj);

        GameObject objHavok = SpawnInstance(prefabSO.havokSphereStatic, pos, rot, havok.transform);

        GameObject objUnity = SpawnInstance(prefabSO.unitySphere, pos, rot, unity.transform);
        MakeStaticUnity(objUnity);

        return (obj, jacobiObj, objHavok, objUnity);
    }

    private void MakeTriggerPBD(GameObject obj)
    {
        PBDCollider col = obj.GetComponent<PBDCollider>();
        col.isTrigger = true;
    }

    private void MakeTriggerUnity(GameObject obj)
    {
        Collider col = obj.GetComponent<Collider>();
        col.isTrigger = true;
    }

    private void MakeStaticPBD(GameObject obj)
    {
        PBDRigidbody rb = obj.GetComponent<PBDRigidbody>();
        rb.inverseMass = 0;
        rb.gravityScale = 0;
    }

    private void MakeStaticUnity(GameObject obj)
    {
        Rigidbody rb = obj.GetComponent<Rigidbody>();
        rb.isKinematic = true;
    }

    private DistanceConstraint MakePBDConstraint(PhysicsEngine engine, GameObject a, GameObject b)
    {
        DistanceConstraint newConstraint = new DistanceConstraint();
        newConstraint.compliance = compliance;
        newConstraint.goalDistance = 0;
        newConstraint.body = a.GetComponent<PBDRigidbody>();
        newConstraint.otherBody = b.GetComponent<PBDRigidbody>();
        newConstraint.firstBodyOffsetFloat = new Vector3(0, -0.5f, 0);
        newConstraint.secondBodyOffsetFloat = new Vector3(0, 0.5f, 0);
        engine.distanceConstraints.Add(newConstraint);

        return newConstraint;
    }

    private BallAndSocketJoint MakeHavokConstraint(GameObject a, GameObject b)
    {
        BallAndSocketJoint constraint = a.AddComponent<BallAndSocketJoint>();
        constraint.ConnectedBody = b.GetComponent<PhysicsBodyAuthoring>();
        constraint.EnableCollision = false;
        constraint.PositionLocal = new Vector3(0, -0.5f, 0);
        constraint.PositionInConnectedEntity = new Vector3(0, 0.5f, 0);

        return constraint;
    }

    private ConfigurableJoint MakeUnityConstraint(GameObject a, GameObject b)
    {
        ConfigurableJoint constraint = a.AddComponent<ConfigurableJoint>();

        constraint.connectedBody = b.GetComponent<Rigidbody>();
        constraint.autoConfigureConnectedAnchor = false;
        constraint.anchor = new Vector3(0, -0.5f, 0);
        constraint.connectedAnchor = new Vector3(0, 0.5f, 0);
        constraint.xMotion = ConfigurableJointMotion.Locked;
        constraint.yMotion = ConfigurableJointMotion.Locked;
        constraint.zMotion = ConfigurableJointMotion.Locked;


        return constraint;
    }

    private void MakeFirstConstraint((GameObject, GameObject, GameObject, GameObject) a, (GameObject, GameObject, GameObject, GameObject) b)
    {
        DistanceConstraint pbdConstraint = MakePBDConstraint(pbdEngine, a.Item1, b.Item1);
        pbdConstraint.firstBodyOffsetFloat = Vector3.zero;
        DistanceConstraint jacobiConstraint = MakePBDConstraint(jacobiEngine, a.Item2, b.Item2);
        jacobiConstraint.firstBodyOffsetFloat = Vector3.zero;
        BallAndSocketJoint havokConstraint = MakeHavokConstraint(a.Item3, b.Item3);
        havokConstraint.PositionLocal = Vector3.zero;
        ConfigurableJoint unityConstraint = MakeUnityConstraint(a.Item4, b.Item4);
        unityConstraint.anchor = Vector3.zero;
    }

    private void MakeLastConstraint((GameObject, GameObject, GameObject, GameObject) a, (GameObject, GameObject, GameObject, GameObject) b)
    {
        DistanceConstraint pbdConstraint = MakePBDConstraint(pbdEngine, a.Item1, b.Item1);
        pbdConstraint.secondBodyOffsetFloat = Vector3.zero;
        DistanceConstraint jacobiConstraint = MakePBDConstraint(jacobiEngine, a.Item2, b.Item2);
        jacobiConstraint.secondBodyOffsetFloat = Vector3.zero;
        BallAndSocketJoint havokConstraint = MakeHavokConstraint(a.Item3, b.Item3);
        havokConstraint.PositionInConnectedEntity = Vector3.zero;
        ConfigurableJoint unityConstraint = MakeUnityConstraint(a.Item4, b.Item4);
        unityConstraint.connectedAnchor = Vector3.zero;
    }

    private void MakeConstraint((GameObject, GameObject, GameObject, GameObject) a, (GameObject, GameObject, GameObject, GameObject) b)
    {
        MakePBDConstraint(pbdEngine, a.Item1, b.Item1);
        MakePBDConstraint(jacobiEngine, a.Item2, b.Item2);
        MakeHavokConstraint(a.Item3, b.Item3);
        MakeUnityConstraint(a.Item4, b.Item4);
    }

    [ContextMenu("SpawnRope")]
    private void SpawnRope()
    {
        SpawnRopeHorizontal(startPos, dims.x);
    }

    private void SpawnRopeHorizontal(Vector3 pos, int len)
    {
        (GameObject, GameObject, GameObject, GameObject)previousObjects;
        (GameObject, GameObject, GameObject, GameObject)objects;
        pos += Vector3.up * 0.5f;
        objects = SpawnAllVersionsStaticSphere(pos , Quaternion.identity);
        MakeTriggerPBD(objects.Item1);
        MakeTriggerPBD(objects.Item2);
        MakeTriggerUnity(objects.Item4);

        pos += Vector3.right * 0.5f;
        for (int i = 0; i < len; i++)
        {
            previousObjects = objects;
            objects = SpawnAllVersionsCapsuleTrigger(pos , Quaternion.Euler(0, 0, 90));
            pos += Vector3.right;
            if (i == 0)
                MakeFirstConstraint(previousObjects, objects);
            else
                MakeConstraint(previousObjects, objects);
        }

        (GameObject, GameObject, GameObject, GameObject)spheres = SpawnAllVersionsWrekingBall(pos - Vector3.right * 0.5f, Quaternion.identity);
        MakeLastConstraint(objects, spheres);
    }

    [ContextMenu("SpawnRigidCradle")]
    private void SpawnRigidCradle()
    {
        Vector3 pos = startPos;
        for (int x = 0; x < dims.x - 1; x++)
        {
            pos += Vector3.right * spacing;
            SpawnRopeVertical(pos);
        }
        pos += Vector3.right * spacing;
        SpawnRopeHorizontal(pos, dims.y);
    }

    [ContextMenu("SpawnRopeVertical")]
    private void SpawnRopeVertical()
    {
        SpawnRopeVertical(startPos);
    }

    private void SpawnRopeVertical(Vector3 pos)
    {
        (GameObject, GameObject, GameObject, GameObject)previousObjects;
        (GameObject, GameObject, GameObject, GameObject)objects;
        objects = SpawnAllVersionsStaticSphere(pos + Vector3.up * 0.5f , Quaternion.identity);
        MakeTriggerPBD(objects.Item1);
        MakeTriggerPBD(objects.Item2);
        MakeTriggerUnity(objects.Item4);
        for (int i = 0; i < dims.y; i++)
        {
            previousObjects = objects;
            objects = SpawnAllVersionsCapsuleTrigger(pos + Vector3.down * i, Quaternion.Euler(0, 0, 0));
            if (i == 0)
                MakeFirstConstraint(previousObjects, objects);
            else
                MakeConstraint(previousObjects, objects);
        }

        (GameObject, GameObject, GameObject, GameObject)spheres = SpawnAllVersionsWrekingBall(pos + Vector3.down * (dims.y - 0.5f), Quaternion.identity);
        MakeLastConstraint(objects, spheres);
    }

    [ContextMenu("Clear")]
    private void ClearHierarchy()
    {
        int n  = transform.childCount;
        for (int i = 0; i < n; i++)
        {
            DestroyImmediate(transform.GetChild(0).gameObject);
        }
    }

    [ContextMenu("Spawn Pyramid")]
    private void SpawnPyramid()
    {
        Vector3 point = startPos;


        for (int i = 0; i < dims.y; i++)
        {
            if (dims.x <= i)
                return;
            Vector3 midpoint  = point + ((Vector3.right + Vector3.forward) * (i + i * spacing)) * 0.25f;
            SpawnPlaneOfCubes(midpoint, dims.x - i, dims.x - i);
            point += Vector3.up * spacing;
        }
    }

    [ContextMenu("Spawn8BallSetup")]
    private void Spawn8BallSetup()
    {
        Vector3 basePoint = startPos;
        for (int i = 0; i < dims.x; i++)
        {
            Vector3 point = basePoint + Vector3.forward * (i + i * spacing) * 0.5f;
            for (int j = 0; j < i + 1; j++)
            {
                SpawnAllVersionsSphere(point, Quaternion.identity);
                point += Vector3.back * (1 + spacing);
            }
            basePoint += Vector3.left * (1 + spacing);
        }

        (GameObject, GameObject, GameObject, GameObject)objects = SpawnAllVersionsSphere(startPos + Vector3.right * 2, Quaternion.identity);
        objects.Item1.GetComponent<Particle>().startingVelocity = Vector3.left * 5;
        objects.Item3.GetComponent<Rigidbody>().velocity = Vector3.left * 5;
    }

    [ContextMenu("Spawn Line of spheres")]
    private void SpawnLineofSpheres()
    {
        Vector3 point = startPos;
        for (int i = 0; i < dims.x; i++)
        {
            SpawnAllVersionsSphere(point, Quaternion.identity);
            point += Vector3.right * spacing;
        }
    }
}
