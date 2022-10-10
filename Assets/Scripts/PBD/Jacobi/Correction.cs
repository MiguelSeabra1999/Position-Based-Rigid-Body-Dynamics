using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
public struct Correction
{
    public DoubleVector3 positional;
    public DoubleQuaternion rotational;
    public double lagrangian;

    public Correction(DoubleVector3 positional, DoubleQuaternion rotational, double lagrangian)
    {
        this.positional = positional;
        this.rotational = rotational;
        this.lagrangian = Math.Abs(lagrangian);
    }

    public static Correction operator+(Correction a, Correction b)
    {
        return new Correction(a.positional + b.positional, a.rotational + b.rotational, a.lagrangian + b.lagrangian);
    }

    public static Correction operator/(Correction a, double b)
    {
        double invertedB = 1.0 / b;
        return new Correction(a.positional * invertedB , a.rotational * invertedB, 0);
    }

    public static Correction operator*(Correction a, double b)
    {
        return new Correction(a.positional * b , a.rotational * b, 0);
    }
}
