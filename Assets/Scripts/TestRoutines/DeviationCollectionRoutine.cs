using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class DeviationCollectionRoutine : CollectionRoutine
{
    protected  List<DataPacket> stdX = new List<DataPacket>();
    protected  List<DataPacket> stdY = new List<DataPacket>();
    protected  List<DataPacket> meanX = new List<DataPacket>();
    protected  List<DataPacket> meanY = new List<DataPacket>();
    public override void InitScenario(GameObject scenario)
    {
        base.InitScenario(scenario);
    }

    public override void CollectData(GameObject scenario, int step)
    {
        DeviationCollector deviationCollector = scenario.GetComponentInChildren<DeviationCollector>();
        int n = deviationCollector.xDeviation.Count;
        double xSum = 0, ySum = 0;
        for (int i = 0; i < n; i++)
        {
            xSum += deviationCollector.xDeviation[i].data;
            ySum += deviationCollector.yDeviation[i].data;
        }
        double meanXValue = xSum / n;
        double meanYValue = ySum / n;
        meanX.Add(new DataPacket(step, meanXValue));
        meanY.Add(new DataPacket(step, meanYValue));
        xSum = 0;
        ySum = 0;

        for (int i = 0; i < n; i++)
        {
            double diff = deviationCollector.xDeviation[i].data - meanXValue;
            xSum += diff * diff;
            diff = deviationCollector.yDeviation[i].data - meanYValue;
            ySum += diff * diff;
        }

        double xStdValue = Math.Sqrt(xSum / n);
        double yStdValue = Math.Sqrt(ySum / n);

        stdX.Add(new DataPacket(step, xStdValue));
        stdY.Add(new DataPacket(step, yStdValue));
    }

    public override void WriteData()
    {
        FileWritter.WriteToFile("MeanDeviation", "xMean", meanX);
        FileWritter.WriteToFile("MeanDeviation", "yMean", meanY);
        FileWritter.WriteToFile("MeanDeviation", "xStd", stdX);
        FileWritter.WriteToFile("MeanDeviation", "yStd", stdY);
    }
}
