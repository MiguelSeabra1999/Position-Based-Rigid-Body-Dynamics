using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InstanceCollisionCollectorUnity : MonoBehaviour
{
    private  List<DataPacket> collisionTimeStamps = new List<DataPacket>();
    private CollisionCollectorUnity parentCollector;
    void Start()
    {
        parentCollector = transform.parent.GetComponent<CollisionCollectorUnity>();
    }

    void OnCollisionEnter(Collision collision)
    {
        parentCollector.ReportCollision();
    }
}
