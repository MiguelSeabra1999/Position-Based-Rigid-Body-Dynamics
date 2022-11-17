using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MeanFpsCollectorRoutine : CollectionRoutine
{
    double prevTime = 0;
    private List<double> fps = new List<double>();


    void Update()
    {
        fps.Add(CalcFps());
    }

    private double CalcFps()
    {
        double deltaTime;

        deltaTime = (Time.realtimeSinceStartupAsDouble - prevTime) * Time.timeScale;
        prevTime = Time.realtimeSinceStartupAsDouble;
        return 1 / deltaTime;
    }

    public override void InitScenario(GameObject scenario)
    {
        fps.Clear();
        prevTime = Time.realtimeSinceStartupAsDouble;
    }

    public override void CollectData(GameObject scenario, int step)
    {
        double sum = 0;
        foreach (double f in fps)
        {
            sum += f;
        }
        double meanFps = sum / fps.Count;
        fps.Clear();
        values.Add(new DataPacket(step, meanFps));
    }

    public override void WriteData()
    {
        FileWritter.WriteToFile("MeanFPS", "MeanFPS", values);
    }
}
