using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ErrorCollectionRoutine : CollectionRoutine
{
    public override void CollectData(GameObject scenario, int step)
    {
        ErrorCollector eCol = scenario.GetComponent<ErrorCollector>();
        values.Add(new DataPacket(step, eCol.GetMeanError()));
    }

    public override void WriteData()
    {
        FileWritter.WriteToFile("MeanError", "MeanError", values);
    }
}
