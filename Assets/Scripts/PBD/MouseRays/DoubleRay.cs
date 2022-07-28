using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DoubleRay
{
    public DoubleVector3 point;
    public DoubleVector3 direction;
    public DoubleRay(DoubleVector3 point, DoubleVector3 direction)
    {
        this.point = point;
        this.direction = DoubleVector3.Normal(direction);
    }
    public DoubleRay(Ray ray)
    {
        this.point = new DoubleVector3(ray.origin);
        this.direction = new DoubleVector3(ray.direction.normalized);
    }

    public void DebugDraw(float dist)
    {
        Debug.DrawRay(point.ToVector3(), direction.ToVector3() * dist);
    }
    public void DebugDraw(float dist, Color color)
    {
        Debug.DrawRay(point.ToVector3(), direction.ToVector3() * dist, color);
    }
    public void DebugDraw(float dist, Color color, float time)
    {
        Debug.DrawRay(point.ToVector3(), direction.ToVector3() * dist, color, time);
    }

}
