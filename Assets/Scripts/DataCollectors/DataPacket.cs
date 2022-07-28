using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DataPacket
{
    private double timeStamp;
    private double data;

    public DataPacket(double data)
    {
        this.data = data;
        this.timeStamp = Time.timeSinceLevelLoadAsDouble;
    }

    public override string ToString()
    {
        return "[" + timeStamp + "] " + data;
    }
}
