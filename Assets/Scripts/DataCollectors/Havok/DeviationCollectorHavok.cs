using Unity.Entities;
using Unity.Jobs;
using Unity.Physics;
using Unity.Physics.Systems;
using UnityEngine;
using System.Collections.Generic;
using Unity.Physics.Authoring;
using Unity.Mathematics;
using Unity.Transforms;
using System;
public class DeviationCollectorHavok : MonoBehaviour
{
    private static DeviationCollectorHavok instance;
    private List<DataPacket> xDeviation = new List<DataPacket>();
    private List<DataPacket> yDeviation = new List<DataPacket>();
    private List<double> xDeviationBuffer = new List<double>();
    private List<double> yDeviationBuffer = new List<double>();
    private bool used = false;
    private Vector3 startingPos;
    void Awake()
    {
        instance = this;
    }

    void OnDestroy()
    {
        if (!used)
            return;
        FileWritter.WriteToFile("Havok/deviation", "xDeviation", xDeviation);
        FileWritter.WriteToFile("Havok/deviation", "yDeviation", yDeviation);
    }

    void FixedUpdate()
    {
        foreach (double value in instance.xDeviationBuffer)
        {
            xDeviation.Add(new DataPacket(value));
        }
        foreach (double value in instance.yDeviationBuffer)
        {
            yDeviation.Add(new DataPacket(value));
        }
        instance.xDeviationBuffer.Clear();
        instance.yDeviationBuffer.Clear();
    }

    public static void SamplePosition(float3 pos)
    {
        Vector3 position = new Vector3(pos.x, pos.y, pos.z);
        if (!instance.used)
        {
            instance.used = true;
            instance.startingPos = position;
            return;
        }

        Vector3 deviation = position - instance.startingPos;

        float horizontalDeviation = (new Vector2(deviation.x, deviation.z)).magnitude;
        float verticalDeviation = deviation.y;
        instance.xDeviationBuffer.Add(horizontalDeviation);
        instance.yDeviationBuffer.Add(verticalDeviation);
    }
}
