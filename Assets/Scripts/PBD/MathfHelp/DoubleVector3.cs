using System;
using UnityEngine;

public readonly struct DoubleVector3
{
    public readonly double x;
    public readonly double y;
    public readonly double z;
    public  const  double EPSILON = 0.1;

    public DoubleVector3(double x, double y, double z)
    {
        this.x = x;
        this.y = y;
        this.z = z;
    }

    public DoubleVector3(Vector3 vec3)
    {
        this.x = (double)vec3.x;
        this.y = (double)vec3.y;
        this.z = (double)vec3.z;
    }

    public DoubleVector3(double n)
    {
        this.x = n;
        this.y = n;
        this.z = n;
    }

    public Vector3 ToVector3()
    {
        return new Vector3((float)x, (float)y, (float)z);
    }

    public static DoubleVector3 operator+(DoubleVector3 a, DoubleVector3 b)
    {
        return new DoubleVector3(a.x + b.x, a.y + b.y, a.z + b.z);
    }

    public static bool operator==(DoubleVector3 a, DoubleVector3 b)
    {
        return (a.x == b.x && a.y == b.y && a.z == b.z);
    }

    public override bool Equals(object o)
    {
        if (o == null)
            return false;
        return (DoubleVector3)o == this;
    }

    public override int GetHashCode()
    {
        return (this.ToString()).GetHashCode();
    }

    public static bool operator!=(DoubleVector3 a, DoubleVector3 b)
    {
        return !(a.x == b.x && a.y == b.y && a.z == b.z);
    }

    public static DoubleVector3 operator-(DoubleVector3 a)
    {
        return new DoubleVector3(-a.x, -a.y, -a.z);
    }

    public static DoubleVector3 operator-(DoubleVector3 a, DoubleVector3 b)
    {
        return new DoubleVector3(a.x - b.x, a.y - b.y, a.z - b.z);
    }

    public static DoubleVector3 operator*(DoubleVector3 a, DoubleVector3 b)
    {
        return new DoubleVector3(a.x * b.x, a.y * b.y, a.z * b.z);
    }

    public static DoubleVector3 operator*(DoubleVector3 a, double b)
    {
        return new DoubleVector3(a.x * b, a.y * b, a.z * b);
    }

    public static DoubleVector3 operator*(double b, DoubleVector3 a)
    {
        return a * b;
    }

    public static DoubleVector3 operator/(DoubleVector3 a, DoubleVector3 b)
    {
        return new DoubleVector3(a.x / b.x, a.y / b.y, a.z / b.z);
    }

    public static DoubleVector3 operator/(DoubleVector3 a, double b)
    {
        return new DoubleVector3(a.x / b, a.y / b, a.z / b);
    }

    public override string ToString()
    {
        return "(" + this.x + ", " + this.y + ", " + this.z + ")";
    }

    public static double Dot(DoubleVector3 a, DoubleVector3 b)
    {
        return a.x * b.x + a.y * b.y + a.z * b.z;
    }

    public static double Magnitude(DoubleVector3 a)
    {
        return Math.Sqrt(DoubleVector3.Dot(a, a));
    }

    public static double MagnitudeSqr(DoubleVector3 a)
    {
        return DoubleVector3.Dot(a, a);
    }

    public static DoubleVector3 Normal(DoubleVector3 a)
    {
        double mag = Magnitude(a);
        if (mag == 0)
            return new DoubleVector3(0, 0, 0);

        return new DoubleVector3(a.x / mag, a.y / mag, a.z / mag);
    }

    public static DoubleVector3 Cross(DoubleVector3 a, DoubleVector3 b)
    {
        return new DoubleVector3(
            a.y * b.z - a.z * b.y,
            a.z * b.x - a.x * b.z,
            a.x * b.y - a.y * b.x
        );
    }

    public static DoubleVector3 Reflect(DoubleVector3 vector, DoubleVector3 normal)
    {
        double mag = DoubleVector3.Magnitude(vector);
        DoubleVector3 reflected = vector - 2 * normal  * DoubleVector3.Dot(vector, normal);
        reflected = DoubleVector3.Normal(reflected) * mag;
        return reflected;
    }

    private static double GetClosestRelativeDist(DoubleVector3 pointA, DoubleVector3 pointB, DoubleVector3 point)
    {
        DoubleVector3 aToP = point - pointA;
        DoubleVector3 aToB = pointB - pointA;

        double aToBmagSqr = DoubleVector3.MagnitudeSqr(aToB);

        double aToP_dot_aToB = DoubleVector3.Dot(aToP, aToB);
        return aToP_dot_aToB / aToBmagSqr;
    }

    public static DoubleVector3 ClosestPointOnLine(DoubleVector3 pointA, DoubleVector3 pointB, DoubleVector3 point)
    {
        DoubleVector3 aToB = pointB - pointA;
        double aToClosestDist = GetClosestRelativeDist(pointA, pointB, point);
        return pointA + aToB * aToClosestDist;
    }

    public static DoubleVector3 ClosestPointOnSegment(DoubleVector3 pointA, DoubleVector3 pointB, DoubleVector3 point)
    {
        DoubleVector3 aToB = pointB - pointA;
        double aToClosestDist = GetClosestRelativeDist(pointA, pointB, point);
        double mod = aToClosestDist;
        if (mod < 0)
            return pointA;
        if (mod > 1)
            return pointB;

        return pointA + aToB * mod;
    }

    public static DoubleVector3 Lerp(DoubleVector3 from, DoubleVector3 to, double percent)
    {
        if (percent > 1)
            return to;
        if (percent < 0)
            return from;
        DoubleVector3 fromTo = to - from;
        return from + fromTo * percent;
    }

    public bool IsParallel(DoubleVector3 other)
    {
        return IsParallel(other, 0);
    }

    public bool IsParallel(DoubleVector3 other, double epsilon)
    {
        if (Math.Abs(DoubleVector3.Dot(this, other)) >= 1 - epsilon)
            return true;
        return false;
    }

    public bool IsPerpendicular(DoubleVector3 other, double epsilon)
    {
        if (Math.Abs(DoubleVector3.Dot(this, other)) <= epsilon)
            return true;
        return false;
    }

    public bool IsPerpendicular(DoubleVector3 other)
    {
        return IsPerpendicular(other, 0);
    }

    public static DoubleVector3 MinValues(DoubleVector3 a, DoubleVector3 b)
    {
        return new DoubleVector3(Math.Min(a.x, b.x), Math.Min(a.y, b.y), Math.Min(a.z, b.z));
    }

    public static DoubleVector3 MaxValues(DoubleVector3 a, DoubleVector3 b)
    {
        return new DoubleVector3(Math.Max(a.x, b.x), Math.Max(a.y, b.y), Math.Max(a.z, b.z));
    }

    public int GetLongestDirection()
    {
        int i = 0;
        if (y > x)
            i = 1;
        if (this[i] < z)
            i = 2;
        return i;
    }

    public DoubleVector3 CleanNormalVector()
    {
        double x = this.x;
        double y = this.y;
        double z = this.z;
        double xAbs = Math.Abs(x);
        double yAbs = Math.Abs(y);
        double zAbs = Math.Abs(z);
        double xSign = Math.Sign(x);
        double ySign = Math.Sign(y);
        double zSign = Math.Sign(z);
        if (xAbs < EPSILON)
            x = xSign * 0;
        if (1 - xAbs < EPSILON)
            x = xSign * 1;
        if (yAbs < EPSILON)
            y = ySign * 0;
        if (1 - yAbs < EPSILON)
            y = ySign * 1;
        if (zAbs < EPSILON)
            z = zSign * 0;
        if (1 - zAbs < EPSILON)
            z = zSign * 1;
        return new DoubleVector3(x, y, z);
    }

    public static double AngleBetween(DoubleVector3 a, DoubleVector3 b)
    {
        double cos = DoubleVector3.Dot(a, b) / (DoubleVector3.Magnitude(a) * DoubleVector3.Magnitude(b));

        return Math.Acos(cos);
    }

    public static double SignedAngleBetween(DoubleVector3 a, DoubleVector3 b, DoubleVector3 normal)
    {
        return Math.Atan2(DoubleVector3.Dot(DoubleVector3.Cross(a, b), normal), DoubleVector3.Dot(a, b));
    }

    public double this[int i]
    {
        get
        {
            switch (i)
            {
                case 0:
                    return x;
                case 1:
                    return y;
                case 2:
                    return z;
                default:
                    return 0;
            }
        }
    }
    public static DoubleVector3 Power(DoubleVector3 v, double exp)
    {
        return new DoubleVector3(Math.Pow(v.x, exp), Math.Pow(v.y, exp), Math.Pow(v.z, exp));
    }

    public static DoubleVector3 Rotate(DoubleVector3 v, DoubleVector3 around, double angle)
    {
        DoubleQuaternion q = new DoubleQuaternion(angle, around);
        return q * v;
    }

    public static DoubleVector3 RandomVector()
    {
        double x = (double)UnityEngine.Random.Range(0.0f, 100.0f);
        double y = (double)UnityEngine.Random.Range(0.0f, 100.0f);
        double z = (double)UnityEngine.Random.Range(0.0f, 100.0f);
        return DoubleVector3.Normal(new DoubleVector3(x, y, z));
    }

    public DoubleQuaternion ToQuaternion()
    {
        double angle = DoubleVector3.Magnitude(this) * Constants.Rad2Deg;
        return new DoubleQuaternion(angle, DoubleVector3.Normal(this));
    }

    public bool IsNan()
    {
        return double.IsNaN(x) || double.IsNaN(y) || double.IsNaN(z);
    }

    public bool IsInfinite()
    {
        return !(double.IsFinite(x) && double.IsFinite(y) && double.IsFinite(z));
    }

    public DoubleVector3 FindPerpendicularVector()
    {
        double _x = 0, _y = 0, _z = 0;
        if (x == 0)
            _x = 1;
        if (y == 0)
            _y = 1;
        if (z == 0)
            _z = 1;
        return DoubleVector3.Normal(new DoubleVector3(_x, _y, _z));
    }
}
