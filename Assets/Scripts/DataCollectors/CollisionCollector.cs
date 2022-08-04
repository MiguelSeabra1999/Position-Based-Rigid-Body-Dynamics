using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CollisionCollector : DataCollector
{
    private  List<DataPacket> collisionTimeStamps = new List<DataPacket>();
    private CollisionEngine collisionEngine;
    void OnDestroy()
    {
        WriteToFile("Collisions", "collisionTimes", collisionTimeStamps);
    }

    void Start()
    {
        collisionEngine = engine.collisionEngine;
    }

    protected override void SubstepEnd()
    {
        foreach (PBDCollision col in collisionEngine.collisions)
        {
            collisionTimeStamps.Add(new DataPacket(0));
        }
    }
}
