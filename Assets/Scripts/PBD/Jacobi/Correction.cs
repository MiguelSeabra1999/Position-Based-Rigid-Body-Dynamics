using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public struct Correction
{
    public DoubleVector3 positional;
    public DoubleQuaternion rotational;

    public Correction(DoubleVector3 positional, DoubleQuaternion rotational)
    {
        this.positional = positional;
        this.rotational = rotational;
    }

    public static Correction operator+(Correction a, Correction b)
    {
        return new Correction(a.positional + b.positional, a.rotational + b.rotational);
    }

    public static Correction operator/(Correction a, int b)
    {
        double invertedB = 1.0 / (double)b;
        return new Correction(a.positional * invertedB, a.rotational * invertedB);
    }
}
