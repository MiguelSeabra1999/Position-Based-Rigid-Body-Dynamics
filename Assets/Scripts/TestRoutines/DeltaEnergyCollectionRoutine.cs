using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DeltaEnergyCollectionRoutine : CollectionRoutine
{
    private double startEnergy = 0;

    public override void InitScenario(GameObject scenario)
    {
        base.InitScenario(scenario);
        startEnergy = scenario.GetComponent<EnergyCollectorPBD>().GetTotalEnergy().data;
    }

    public override void CollectData(GameObject scenario, int step)
    {
        double endEnergy = scenario.GetComponent<EnergyCollectorPBD>().GetTotalEnergy().data;

        values.Add(new DataPacket(step, startEnergy - endEnergy));
    }

    public override void WriteData()
    {
        FileWritter.WriteToFile("DeltaEnergy", "DeltaEnergy", values);
    }
}
