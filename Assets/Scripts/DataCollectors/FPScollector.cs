using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FPScollector : DataCollector
{
    private List<DataPacket> fps = new List<DataPacket>();
    void OnDestroy()
    {
        FileWritter.WriteToFile("FPS/", "FPS", fps);
    }

    protected override void StepEnd()
    {
        fps.Add(new DataPacket(1 / engine.deltaTime));
    }
}
