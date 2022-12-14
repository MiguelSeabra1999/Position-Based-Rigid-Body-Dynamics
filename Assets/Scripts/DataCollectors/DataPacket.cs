using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public struct DataPacket
{
    private double timeStamp;
    public double data;

    public DataPacket(double data)
    {
        this.data = data;
        this.timeStamp = Time.timeSinceLevelLoadAsDouble;
    }

    public DataPacket(double stamp, double data)
    {
        this.data = data;
        this.timeStamp = stamp;
    }

    public override string ToString()
    {
        return "[" + timeStamp + "] " + data;
    }
}
