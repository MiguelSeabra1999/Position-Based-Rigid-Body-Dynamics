using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class CollectionRoutine : MonoBehaviour
{
    protected  List<DataPacket> values = new List<DataPacket>();
    public abstract void CollectData(GameObject scenario, int step);

    public abstract void WriteData();

    public virtual void InitScenario(GameObject scenario)
    {
    }
}
