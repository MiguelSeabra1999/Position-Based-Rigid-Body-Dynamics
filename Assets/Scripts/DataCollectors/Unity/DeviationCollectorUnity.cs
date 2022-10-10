using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DeviationCollectorUnity : MonoBehaviour
{
    private List<DataPacket> xDeviation = new List<DataPacket>();
    private List<DataPacket> yDeviation = new List<DataPacket>();
    private bool used = false;

    private Vector3 startingPos;


    void OnDestroy()
    {
        if (!used)
            return;
        FileWritter.WriteToFile("Unity/deviation", "xDeviation", xDeviation);
        FileWritter.WriteToFile("Unity/deviation", "yDeviation", yDeviation);
    }

    void Update()
    {
        if (!used)
        {
            used = true;
            startingPos = transform.position;
            return;
        }
        Vector3 deviation = transform.position - startingPos;
        float horizontalDeviation = (new Vector2(deviation.x, deviation.z)).magnitude;
        float verticalDeviation = deviation.y;
        xDeviation.Add(new DataPacket(horizontalDeviation));
        yDeviation.Add(new DataPacket(verticalDeviation));
    }
}
