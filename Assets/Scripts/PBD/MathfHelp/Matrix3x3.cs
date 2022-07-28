using System;
using UnityEngine;

public class Matrix3x3
{
    public double[] values;
    
    public Matrix3x3()
    {
        this.values = new double[9];
       
        values[0] = 1;
        values[1] = 0;
        values[2] = 0;

        values[3] = 0;
        values[4] = 1;
        values[5] = 0;

        values[6] = 0;
        values[7] = 0;
        values[8] = 1;
    }

   public double this[int i]
   {
      get { return values[i]; }
      set { values[i] = value; }
   }

    public override string ToString()
    {
        return "(" +  
        values[0] + ", " +
        values[1] + ", " +
        values[2] + ", " +
        values[3] + ", " +
        values[4] + ", " +
        values[5] + ", " +
        values[6] + ", " +
        values[7] + ", " +
        values[8] + ")";
    }

    public Matrix3x3 GetInverse()
    {
        Matrix3x3 m = new Matrix3x3();
        double det = values[0] * (values[4]*values[8] - values[7]*values[5]) - values[1] * (values[3]*values[8] - values[6]*values[6]) + values[2] * (values[3]*values[7] - values[6]*values[4]);
        double detInv = 1/det;
        m[0] = values[4]*values[8] - values[5]*values[7];
        m[1] = values[2]*values[7] - values[1]*values[8];
        m[2] = values[1]*values[5] - values[2]*values[4];
        m[3] = values[5]*values[6] - values[1]*values[8];
        m[4] = values[0]*values[8] - values[2]*values[6];
        m[5] = values[2]*values[3] - values[0]*values[5];
        m[6] = values[3]*values[7] - values[4]*values[6];
        m[7] = values[1]*values[6] - values[0]*values[7];
        m[8] = values[0]*values[4] - values[1]*values[3];

        return m * detInv;
    }

    public static DoubleVector3 operator *(Matrix3x3 a, DoubleVector3 b)
    {
        return new DoubleVector3(
            a[0]*b.x + a[1]*b.y + a[2]*b.z,
            a[3]*b.x + a[4]*b.y + a[5]*b.z,
            a[6]*b.x + a[7]*b.y + a[8]*b.z
        );
    }
    public static DoubleVector3 operator *(DoubleVector3 a, Matrix3x3 b)
    {
        return new DoubleVector3(
            DoubleVector3.Dot(a, new DoubleVector3(b[0], b[3], b[6])),
            DoubleVector3.Dot(a, new DoubleVector3(b[1], b[4], b[7])),
            DoubleVector3.Dot(a, new DoubleVector3(b[2], b[5], b[8]))
        );
    }
    public static Matrix3x3 operator *(Matrix3x3 a, double b)
    {
        Matrix3x3 m = new Matrix3x3();

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
    public static Matrix3x3 operator *(double b, Matrix3x3 a)
    {
        Matrix3x3 m = new Matrix3x3();

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
    public static Matrix3x3 operator *(Matrix3x3 a, Matrix3x3 b)
    {
        Matrix3x3 m = new Matrix3x3();

        m[0] = a[0]*b[0] + a[1]*b[3] + a[2]*b[6];
        m[1] = a[0]*b[1] + a[1]*b[4] + a[2]*b[7];
        m[2] = a[0]*b[2] + a[1]*b[5] + a[2]*b[8];
        m[3] = a[3]*b[0] + a[4]*b[3] + a[5]*b[6];
        m[4] = a[3]*b[1] + a[4]*b[4] + a[5]*b[7];
        m[5] = a[3]*b[2] + a[4]*b[5] + a[5]*b[8];
        m[6] = a[6]*b[0] + a[7]*b[3] + a[8]*b[6];
        m[7] = a[6]*b[1] + a[7]*b[4] + a[8]*b[7];
        m[8] = a[6]*b[2] + a[7]*b[5] + a[8]*b[8];

        return m;
    }


}
