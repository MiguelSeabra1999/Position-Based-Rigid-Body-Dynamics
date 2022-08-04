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
    private GameObject pbd, havok, unity;
    private PhysicsEngine pbdEngine;

    private List<GameObject> pbdLinks = new List<GameObject>();

    void Start()
    {
        if (!buildOnAwake)
            return;
        ClearHierarchy();
        CreateScenes();
        SpawnRope();
    }

    [ContextMenu("CreateScenes")]
    private void CreateScenes()
    {
        pbd   = Instantiate(prefabSO.pbdScene);
        pbd.transform.SetParent(transform);
        pbdEngine = pbd.GetComponent<PhysicsEngine>();

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
            SpawnPlaneOfSpheres(point, dims.x, dims.y);
        }
    }

    private void SpawnPlaneOfSpheres(Vector3 startingPoint, int x, int y)
    {
        for (int i = 0; i < x; i++)
        {
            Vector3 point = startingPoint + spacing * i * Vector3.right;
            SpawnLineOfSpheres(Vector3.forward, point , y);
        }
    }

    private void SpawnLineOfSpheres(Vector3 axis, Vector3 startingPoint, int n)
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
        SpawnInstance(prefabSO.havokCube, pos, rot, havok.transform);
        SpawnInstance(prefabSO.unityCube, pos, rot, unity.transform);
    }

    private void SpawnAllVersionsCapsule(Vector3 pos, Quaternion rot)
    {
        SpawnInstance(prefabSO.pbdCapsule, pos, rot, pbd.transform);
        SpawnInstance(prefabSO.havokCapsule, pos, rot, havok.transform);
        SpawnInstance(prefabSO.unityCapsule, pos, rot, unity.transform);
    }

    private (GameObject, GameObject, GameObject) SpawnAllVersionsCapsuleTrigger(Vector3 pos, Quaternion rot)
    {
        GameObject obj = SpawnInstance(prefabSO.pbdCapsule, pos, rot, pbd.transform);
        MakeTriggerPBD(obj);

        GameObject objHavok = SpawnInstance(prefabSO.havokCapsuleTrigger, pos, rot, havok.transform);

        GameObject objUnity = SpawnInstance(prefabSO.unityCapsule, pos, rot, unity.transform);
        MakeTriggerUnity(objUnity);

        return (obj, objHavok, objUnity);
    }

    private (GameObject, GameObject, GameObject) SpawnAllVersionsStaticSphere(Vector3 pos, Quaternion rot)
    {
        GameObject obj = SpawnInstance(prefabSO.pbdSphere, pos, rot, pbd.transform);
        MakeStaticPBD(obj);

        GameObject objHavok = SpawnInstance(prefabSO.havokSphereStatic, pos, rot, havok.transform);

        GameObject objUnity = SpawnInstance(prefabSO.unitySphere, pos, rot, unity.transform);
        MakeStaticUnity(objUnity);

        return (obj, objHavok, objUnity);
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

    private DistanceConstraint MakePBDConstraint(GameObject a, GameObject b)
    {
        DistanceConstraint newConstraint = new DistanceConstraint();
        newConstraint.compliance = compliance;
        newConstraint.goalDistance = 0;
        newConstraint.body = a.GetComponent<PBDRigidbody>();
        newConstraint.otherBody = b.GetComponent<PBDRigidbody>();
        newConstraint.firstBodyOffsetFloat = new Vector3(0, -0.5f, 0);
        newConstraint.secondBodyOffsetFloat = new Vector3(0, 0.5f, 0);
        pbdEngine.distanceConstraints.Add(newConstraint);

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

    private void MakeFirstConstraint((GameObject, GameObject, GameObject) a, (GameObject, GameObject, GameObject) b)
    {
        DistanceConstraint pbdConstraint = MakePBDConstraint(a.Item1, b.Item1);
        pbdConstraint.firstBodyOffsetFloat = Vector3.zero;
        BallAndSocketJoint havokConstraint = MakeHavokConstraint(a.Item2, b.Item2);
        havokConstraint.PositionLocal = Vector3.zero;
        ConfigurableJoint unityConstraint = MakeUnityConstraint(a.Item3, b.Item3);
        unityConstraint.anchor = Vector3.zero;
    }

    private void MakeConstraint((GameObject, GameObject, GameObject) a, (GameObject, GameObject, GameObject) b)
    {
        MakePBDConstraint(a.Item1, b.Item1);
        MakeHavokConstraint(a.Item2, b.Item2);
        MakeUnityConstraint(a.Item3, b.Item3);
    }

    [ContextMenu("SpawnRope")]
    private void SpawnRope()
    {
        (GameObject, GameObject, GameObject)previousObjects;
        (GameObject, GameObject, GameObject)objects;
        objects = SpawnAllVersionsStaticSphere(startPos - Vector3.right * 0.5f , Quaternion.identity);
        for (int i = 0; i < dims.x; i++)
        {
            previousObjects = objects;
            objects = SpawnAllVersionsCapsuleTrigger(startPos + Vector3.right * i, Quaternion.Euler(0, 0, 90));
            if (i == 0)
                MakeFirstConstraint(previousObjects, objects);
            else
                MakeConstraint(previousObjects, objects);
        }
    }

    private void ClearHierarchy()
    {
        int n  = transform.childCount;
        for (int i = 0; i < n; i++)
        {
            DestroyImmediate(transform.GetChild(0).gameObject);
        }
    }
}