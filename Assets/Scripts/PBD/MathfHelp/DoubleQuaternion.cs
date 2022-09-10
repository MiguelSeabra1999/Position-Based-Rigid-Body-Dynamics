using System;
using UnityEngine;

public readonly struct DoubleQuaternion
{
    public readonly double w, x, y, z;
    private static readonly double EPSILON = 0.00001;

    public DoubleQuaternion(double w, double x, double y, double z)
    {
        this.w = w;
        this.x = x;
        this.y = y;
        this.z = z;
    }

    public DoubleQuaternion(Quaternion q)
    {
        this.w = q.w;
        this.x = q.x;
        this.y = q.y;
        this.z = q.z;
    }

    public DoubleQuaternion(double angle, DoubleVector3 axis)
    {
        /*DoubleVector3 axisNormal = DoubleVector3.Normal(axis);
        double a = angle * Constants.Deg2Rad;
        w = Math.Cos(a / 2);
        double s = Math.Sin(a / 2);
        x = axisNormal.x * s;
        y = axisNormal.y * s;
        z = axisNormal.z * s;*/
        DoubleQuaternion neutral = new DoubleQuaternion(1, 0, 0, 0);
        DoubleVector3 rotVec = angle * axis;
        DoubleQuaternion q = neutral + 0.5 * new DoubleQuaternion(0, rotVec.x, rotVec.y, rotVec.z) * neutral;
        x = q.x;
        y = q.y;
        z = q.z;
        w = q.w;
    }

    public DoubleQuaternion(DoubleVector3 vector)
    {
        double angle = DoubleVector3.Magnitude(vector);
        DoubleVector3 axis = DoubleVector3.Normal(vector);

        double a = angle * Constants.Deg2Rad;
        w = Math.Cos(a / 2);
        double s = Math.Sin(a / 2);
        x = axis.x * s;
        y = axis.y * s;
        z = axis.z * s;
    }

    public DoubleQuaternion(double x, double y, double z)
    {
        double halfX = x / 2;
        double halfY = y / 2;
        double halfZ = z / 2;
        this.w = Math.Cos(halfX) * Math.Cos(halfY) * Math.Cos(halfZ) + Math.Sin(halfX) * Math.Sin(halfY) * Math.Sin(halfZ);
        this.x = Math.Sin(halfX) * Math.Cos(halfY) * Math.Cos(halfZ) - Math.Cos(halfX) * Math.Sin(halfY) * Math.Sin(halfZ);
        this.y = Math.Cos(halfX) * Math.Sin(halfY) * Math.Cos(halfZ) - Math.Sin(halfX) * Math.Cos(halfY) * Math.Sin(halfZ);
        this.z = Math.Cos(halfX) * Math.Cos(halfY) * Math.Sin(halfZ) - Math.Sin(halfX) * Math.Sin(halfY) * Math.Cos(halfZ);
    }

    public DoubleQuaternion(DoubleVector3 from, DoubleVector3 to)
    {
        DoubleVector3 cross = DoubleVector3.Cross(from, to);
        this.x = cross.x;
        this.y = cross.y;
        this.z = cross.z;
        this.w = Math.Sqrt(DoubleVector3.MagnitudeSqr(from) * DoubleVector3.MagnitudeSqr(to)) + DoubleVector3.Dot(from, to);

        //Normalize
        double magnitude = Math.Sqrt(this.w * this.w + this.x * this.x + this.y * this.y + this.z * this.z);
        double s = 1 / magnitude;
        this.x *= s;
        this.y *= s;
        this.z *= s;
        this.w *= s;
    }

    public Quaternion ToQuaternion()
    {
        return new Quaternion((float)x, (float)y, (float)z, (float)w);
    }

    /* public DoubleVector3 ToEuler()
     {
         double roll,pitch,yaw;
         roll = Math.Atan2(2 *(w*x + y*z), w*w - x*x - y*y + z*z);
         pitch = Math.Asin(2 * (w*y - x*z));
         yaw = Math.Atan2(2 * (w*z + x*y), w*w + x*x - y*y - z*z);
         roll *= (double)Mathf.Rad2Deg;
         pitch *= (double)Mathf.Rad2Deg;
         yaw *= (double)Mathf.Rad2Deg;


         return new DoubleVector3(roll, pitch, yaw);
     }*/

    public DoubleVector3 ToEuler()
    {
        double heading, attitude, bank;
        double sqw = w * w;
        double sqx = x * x;
        double sqy = y * y;
        double sqz = z * z;
        double unit = sqx + sqy + sqz + sqw; // if normalised is one, otherwise is correction factor
        double test = x * y + z * w;
        if (test > 0.499 * unit) // singularity at north pole
        {
            heading = 2 * Math.Atan2(x, w);
            attitude = Math.PI / 2;
            bank = 0;
        }
        if (test < -0.499 * unit) // singularity at south pole
        {
            heading = -2 *  Math.Atan2(x, w);
            attitude = -Math.PI / 2;
            bank = 0;
        }
        heading = Math.Atan2(2 * y * w - 2 * x * z , sqx - sqy - sqz + sqw);
        attitude = Math.Asin(2 * test / unit);
        bank = Math.Atan2(2 * x * w - 2 * y * z , -sqx + sqy - sqz + sqw);
        return new DoubleVector3(bank, heading, attitude) * Constants.Rad2Deg;
    }

    public DoubleVector3 VectorPart()
    {
        return new DoubleVector3(x, y, z);
    }

    public static DoubleQuaternion operator+(DoubleQuaternion q, DoubleQuaternion s)
    {
        return new DoubleQuaternion(q.w + s.w, q.x + s.x, q.y + s.y, q.z + s.z);
    }

    public static DoubleQuaternion operator-(DoubleQuaternion q, DoubleQuaternion s)
    {
        return new DoubleQuaternion(q.w - s.w, q.x - s.x, q.y - s.y, q.z - s.z);
        //return q + s.Inverse();
    }

    public static DoubleQuaternion operator*(DoubleQuaternion q, double s)
    {
        return new DoubleQuaternion(q.w * s, q.x * s, q.y * s, q.z * s);
    }

    public static DoubleQuaternion operator*(double s, DoubleQuaternion q)
    {
        return new DoubleQuaternion(q.w * s, q.x * s, q.y * s, q.z * s);
    }

    public static DoubleVector3 operator*(DoubleQuaternion q, DoubleVector3 v)
    {
        DoubleQuaternion a = new DoubleQuaternion(0, v.x, v.y, v.z);
        DoubleVector3 qv = new DoubleVector3(q.x, q.y, q.z);
        DoubleVector3 cross = DoubleVector3.Cross(qv, v);
        return v + cross * (2 * q.w) + DoubleVector3.Cross(qv, cross) * 2;
    }

    public static DoubleQuaternion operator*(DoubleVector3 v, DoubleQuaternion q)
    {
        DoubleQuaternion a = new DoubleQuaternion(0, v.x, v.y, v.z);
        return a * q;
    }

    public static DoubleQuaternion operator*(DoubleQuaternion q, DoubleQuaternion s)
    {
        return new DoubleQuaternion(
            q.w * s.w - q.x * s.x - q.y * s.y - q.z * s.z,
            q.w * s.x + q.x * s.w + q.y * s.z - q.z * s.y,
            q.w * s.y - q.x * s.z + q.y * s.w + q.z * s.x,
            q.w * s.z + q.x * s.y - q.y * s.x + q.z * s.w
        );
    }

    public override string ToString()
    {
        return "(" + this.w + ", " + this.x + ", " + this.y + ", " + this.z + ")";
    }

    public  DoubleQuaternion Inverse()
    {
        return new DoubleQuaternion(w, -x, -y, -z);
    }

    public static double Quadrance(DoubleQuaternion q)
    {
        return q.w * q.w + q.x * q.x + q.y * q.y + q.z * q.z;
    }

    public static double Magnitude(DoubleQuaternion q)
    {
        return Math.Sqrt(DoubleQuaternion.Quadrance(q));
    }

    public static DoubleQuaternion Normal(DoubleQuaternion q)
    {
        double s = 1 / DoubleQuaternion.Magnitude(q);

        return q * s;
    }

/*
    public static DoubleQuaternion Inverse(DoubleQuaternion q)
    {
        return DoubleQuaternion.Conjugate(q) * (1/DoubleQuaternion.Quadrance(q));
    }
*/
    public static DoubleQuaternion Lerp(DoubleQuaternion from, DoubleQuaternion to, double percent)
    {
        double cos_angle = from.x * to.x + from.y * to.y + from.z * to.z + from.w * to.w;
        double percentInv = 1 - percent;
        double percentFinal = (cos_angle > 0) ? percent : -percent;
        DoubleQuaternion qi = from * percentInv + to * percentFinal;
        return DoubleQuaternion.Normal(qi);
    }

    public static DoubleQuaternion Slerp(DoubleQuaternion from, DoubleQuaternion to, double percent)
    {
        double angle = Math.Acos(from.x * to.x + from.y * to.y + from.z * to.z + from.w * to.w);
        double percentInv = Math.Sin((1 - percent) * angle) / Math.Sin(angle);
        double percentFinal = Math.Sin(percent * angle) / Math.Sin(angle);
        DoubleQuaternion qi = from * percentInv + to * percentFinal;
        return DoubleQuaternion.Normal(qi);
    }

    public static DoubleQuaternion Euler(double x, double y, double z) // yaw (Z), pitch (Y), roll (X)
    {
        // Abbreviations for the various angular functions
        double cy = Math.Cos(x * 0.5);
        double sy = Math.Sin(x * 0.5);
        double cp = Math.Cos(y * 0.5);
        double sp = Math.Sin(y * 0.5);
        double cr = Math.Cos(z * 0.5);
        double sr = Math.Sin(z * 0.5);

        return new DoubleQuaternion(
            cr * cp * cy + sr * sp * sy,
            sr * cp * cy - cr * sp * sy,
            cr * sp * cy + sr * cp * sy,
            cr * cp * sy - sr * sp * cy
        );
    }

    public static DoubleVector3 ToEulerAngles(DoubleQuaternion q)
    {
        double x, y, z;
        double sinr_cosp = 2 * (q.w * q.x + q.y * q.z);
        double cosr_cosp = 1 - 2 * (q.x * q.x + q.y * q.y);
        x = Math.Atan2(sinr_cosp, cosr_cosp);

        double sinp = 2 * (q.w * q.y - q.z * q.x);

        if (Math.Abs(sinp) >= 1)
            y = Math.PI * Math.Sign(sinp);
        else
            y = Math.Asin(sinp);

        double siny_cosp = 2 * (q.w * q.z + q.x * q.y);
        double cosy_cosp = 1 - 2 * (q.y * q.y + q.z * q.z);
        z = Math.Atan2(siny_cosp, cosy_cosp);

        return new DoubleVector3(x, y, z);
    }

    public static DoubleQuaternion Clean(DoubleQuaternion q)
    {
        double w, x, y, z;
        w = q.w;
        x = q.x;
        y = q.y;
        z = q.z;
        if (Math.Abs(q.w) < EPSILON)
            w = 0.0f;
        if (Math.Abs(q.x) < EPSILON)
            x = 0.0f;
        if (Math.Abs(q.y) < EPSILON)
            y = 0.0f;
        if (Math.Abs(q.z) < EPSILON)
            z = 0.0f;
        return new DoubleQuaternion(w, x, y, z);
    }

    public static DoubleQuaternion Power(DoubleQuaternion q, double exp)
    {
        q = DoubleQuaternion.Normal(q);

        if (exp == 1)
            return q;
        DoubleVector3 v = q.VectorPart();
        DoubleVector3 n = DoubleVector3.Normal(v);

        double angle = Math.Asin(DoubleVector3.Magnitude(v) / DoubleQuaternion.Magnitude(q));
        DoubleVector3 aux = n * Math.Sin(exp * angle);
        DoubleQuaternion result = Math.Pow(DoubleQuaternion.Magnitude(q), exp) * new DoubleQuaternion(Math.Cos(exp * angle) , aux.x, aux.y, aux.z);

        return DoubleQuaternion.Normal(result);
    }

    public static DoubleQuaternion PowerElementWise(DoubleQuaternion q, double exp)
    {
        return new DoubleQuaternion(Math.Pow(q.w, exp), Math.Pow(q.x, exp), Math.Pow(q.y, exp), Math.Pow(q.z, exp));
    }

    public static DoubleQuaternion RandomQuaternion()
    {
        return DoubleQuaternion.Normal(new DoubleQuaternion((double)UnityEngine.Random.Range(0.0f, 10.0f), (double)UnityEngine.Random.Range(0.0f, 10.0f), (double)UnityEngine.Random.Range(0.0f, 10.0f), (double)UnityEngine.Random.Range(0.0f, 10.0f)));
    }

    public static DoubleQuaternion RotationBetween(DoubleVector3 v1, DoubleVector3 v2)
    {
        DoubleVector3 cross = DoubleVector3.Cross(v1, v2);
        double angle = DoubleVector3.Magnitude(cross);
        DoubleVector3 dir = DoubleVector3.Normal(cross);
        DoubleQuaternion q = new DoubleQuaternion(Math.Asin(angle) * Constants.Rad2Deg, dir);
        return DoubleQuaternion.Normal(q);
    }

    public static DoubleQuaternion RotationVect(DoubleVector3 v1, DoubleVector3 v2)
    {
        DoubleVector3 cross = DoubleVector3.Cross(v1, v2);
        double angle = DoubleVector3.Magnitude(cross);
        DoubleVector3 dir = DoubleVector3.Normal(cross);
        DoubleQuaternion q = new DoubleQuaternion(Math.Asin(angle) * Constants.Rad2Deg, dir);
        return DoubleQuaternion.Normal(q);
    }

    public bool IsNan()
    {
        return double.IsNaN(w) || double.IsNaN(x) || double.IsNaN(y) || double.IsNaN(z);
    }
}
