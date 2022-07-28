using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PBDSpawner : MonoBehaviour
{
    public Vector3Int cubeDims = new Vector3Int();
    public float spacing = 1;
    public float positionalNoise = 0;
    public bool randRotation;
    public GameObject spherePrefab;
    private int count = 0;
    
    [ContextMenu("spawnSphereCube")]
    public void SpawnCubeOfSpheres()
    {
        count = 0;
        Vector3 startingPoint = new Vector3(transform.position.x - cubeDims.x * spacing * 0.5f, 
                                            transform.position.y,
                                            transform.position.z - cubeDims.y * spacing * 0.5f);
        for(int i = 0; i < cubeDims.z ; i++)
        {
            Vector3 point = startingPoint + Vector3.up * spacing * i;
            SpawnPlaneOfSpheres(point, cubeDims.x, cubeDims.y);
        }
    }

    private void SpawnPlaneOfSpheres(Vector3 startingPoint, int x, int y)
    {   
        for(int i = 0; i < x; i++)
        {
            Vector3 point = startingPoint + spacing*i * Vector3.right;
            SpawnLineOfSpheres(Vector3.forward, point ,y);
        }
    }
    private void SpawnLineOfSpheres(Vector3 axis, Vector3 startingPoint, int n)
    {   
        for(int i = 0; i < n; i++)
        {
            Vector3 point = startingPoint + axis.normalized*spacing * i;
            Vector3 noise = new Vector3(Random.Range(-positionalNoise,positionalNoise),Random.Range(-positionalNoise,positionalNoise),Random.Range(-positionalNoise,positionalNoise));
            Quaternion rotation = Quaternion.identity;
            if(randRotation)
                rotation = Random.rotation;
            GameObject newObj = Instantiate(spherePrefab, point + noise, rotation);
            newObj.transform.SetParent(transform.parent);
            newObj.name = newObj.name + count;
            count++;
        }
    }
}
