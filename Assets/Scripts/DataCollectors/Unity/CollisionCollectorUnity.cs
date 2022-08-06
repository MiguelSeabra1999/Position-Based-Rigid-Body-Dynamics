using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CollisionCollectorUnity : MonoBehaviour
{
    private  List<DataPacket> collisionTimeStamps = new List<DataPacket>();

    void OnDestroy()
    {
        FileWritter.WriteToFile("Unity/Collisions", "collisionTimes", collisionTimeStamps);
    }

    public void ReportCollision()
    {
        collisionTimeStamps.Add(new DataPacket(0));
    }
}
