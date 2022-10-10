using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DeviationCollector : DataCollector
{
    private Particle body;
    [HideInInspector] public List<DataPacket> xDeviation = new List<DataPacket>();
    [HideInInspector] public List<DataPacket> yDeviation = new List<DataPacket>();
    private bool used = false;

    private DoubleVector3 startingPos;
    protected override void Awake()
    {
        base.Awake();
        body = GetComponent<Particle>();
    }

    void OnDestroy()
    {
        if (!used)
            return;

        FileWritter.WriteToFile(subFolder + "/deviation", "xDeviation", xDeviation);
        FileWritter.WriteToFile(subFolder + "/deviation", "yDeviation", yDeviation);
    }

    protected override void StepEnd()
    {
        if (!used)
        {
            used = true;
            startingPos = body.position;
            return;
        }
        DoubleVector3 deviation = body.position - startingPos;
        double horizontalDeviation = (new Vector2((float)deviation.x, (float)deviation.z)).magnitude;
        double verticalDeviation = deviation.y;
        xDeviation.Add(new DataPacket(horizontalDeviation));
        yDeviation.Add(new DataPacket(verticalDeviation));
    }
}
