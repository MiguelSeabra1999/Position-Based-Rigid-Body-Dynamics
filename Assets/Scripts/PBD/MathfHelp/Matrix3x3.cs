using System;
using UnityEngine;

public struct Matrix3x3
{
    //public double[] values;

    private double a, b, c, d, e, f, g, h, i;


    public static Matrix3x3 Identity()
    {
        return new Matrix3x3(1, 0, 0, 0, 1, 0, 0, 0, 1);
    }

    public Matrix3x3(int a, int b, int c, int d, int e, int f, int g, int h, int i)
    {
        //this.values = new double[9];

        this.a = a;
        this.b = b;
        this.c = c;
        this.d = d;
        this.e = e;
        this.f = f;
        this.g = g;
        this.h = h;
        this.i = i;
    }

    public double this[int index]
    {
        get
        {
            switch (index)
            {
                case 0:
                    return a;
                case 1:
                    return b;
                case 2:
                    return c;
                case 3:
                    return d;
                case 4:
                    return e;
                case 5:
                    return f;
                case 6:
                    return g;
                case 7:
                    return h;
                case 8:
                    return i;
                default:
                    return 0;
            }
        }
        set
        {
            switch (index)
            {
                case 0:
                    a = value;
                    return;
                case 1:
                    b = value;
                    return;
                case 2:
                    c = value;
                    return;
                case 3:
                    d = value;
                    return;
                case 4:
                    e = value;
                    return;
                case 5:
                    f = value;
                    return;
                case 6:
                    g = value;
                    return;
                case 7:
                    h = value;
                    return;
                case 8:
                    i = value;
                    return;
                default:
                    return;
            }
        }
    }

    public override string ToString()
    {
        return "(" +
            a + ", " +
            b + ", " +
            c + ", " +
            d + ", " +
            e + ", " +
            f + ", " +
            g + ", " +
            h + ", " +
            i + ")";
    }

    public Matrix3x3 GetInverse()
    {
        Matrix3x3 m = Matrix3x3.Identity();
        double det = a * (e * i - h * f) - b * (d * i - g * g) + c * (d * h - g * e);
        double detInv = 1 / det;
        m[0] = e * i - f * h;
        m[1] = c * h - b * i;
        m[2] = b * f - c * e;
        m[3] = f * g - b * i;
        m[4] = a * i - c * g;
        m[5] = c * d - a * f;
        m[6] = d * h - e * g;
        m[7] = b * g - a * h;
        m[8] = a * e - b * d;

        return m * detInv;
    }

    public static DoubleVector3 operator*(Matrix3x3 a, DoubleVector3 b)
    {
        return new DoubleVector3(
            a[0] * b.x + a[1] * b.y + a[2] * b.z,
            a[3] * b.x + a[4] * b.y + a[5] * b.z,
            a[6] * b.x + a[7] * b.y + a[8] * b.z
        );
    }

    public static DoubleVector3 operator*(DoubleVector3 a, Matrix3x3 b)
    {
        return new DoubleVector3(
            DoubleVector3.Dot(a, new DoubleVector3(b[0], b[3], b[6])),
            DoubleVector3.Dot(a, new DoubleVector3(b[1], b[4], b[7])),
            DoubleVector3.Dot(a, new DoubleVector3(b[2], b[5], b[8]))
        );
    }

    public static Matrix3x3 operator*(Matrix3x3 a, double b)
    {
        Matrix3x3 m = Matrix3x3.Identity();

        m[0] = a[0] * b;
        m[1] = a[1] * b;
        m[2] = a[2] * b;
        m[3] = a[3] * b;
        m[4] = a[4] * b;
        m[5] = a[5] * b;
        m[6] = a[6] * b;
        m[7] = a[7] * b;
        m[8] = a[8] * b;

        return m;
    }

    public static Matrix3x3 operator*(double b, Matrix3x3 a)
    {
        Matrix3x3 m = Matrix3x3.Identity();

        m[0] = a[0] * b;
        m[1] = a[1] * b;
        m[2] = a[2] * b;
        m[3] = a[3] * b;
        m[4] = a[4] * b;
        m[5] = a[5] * b;
        m[6] = a[6] * b;
        m[7] = a[7] * b;
        m[8] = a[8] * b;

        return m;
    }

    public static Matrix3x3 operator*(Matrix3x3 a, Matrix3x3 b)
    {
        Matrix3x3 m = Matrix3x3.Identity();

        m[0] = a[0] * b[0] + a[1] * b[3] + a[2] * b[6];
        m[1] = a[0] * b[1] + a[1] * b[4] + a[2] * b[7];
        m[2] = a[0] * b[2] + a[1] * b[5] + a[2] * b[8];
        m[3] = a[3] * b[0] + a[4] * b[3] + a[5] * b[6];
        m[4] = a[3] * b[1] + a[4] * b[4] + a[5] * b[7];
        m[5] = a[3] * b[2] + a[4] * b[5] + a[5] * b[8];
        m[6] = a[6] * b[0] + a[7] * b[3] + a[8] * b[6];
        m[7] = a[6] * b[1] + a[7] * b[4] + a[8] * b[7];
        m[8] = a[6] * b[2] + a[7] * b[5] + a[8] * b[8];

        return m;
    }
}
