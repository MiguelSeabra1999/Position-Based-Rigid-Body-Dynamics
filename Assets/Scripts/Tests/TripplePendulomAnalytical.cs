using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
public class TripplePendulomAnalytical : MonoBehaviour
{
    private double gravity  = -9.81;
    public double[] masses = new double[3];
    public double[] lengths = new double[3];
    private double[] theta = {Math.PI / 2.0, Math.PI, Math.PI};
    private double[] omega = new double[3];
    public GameObject[] beads = new GameObject[3];
    private double prevTime = 0;
    private List<DataPacket> totalEnergy = new List<DataPacket>();
    private Vector3[] prevPos  = new Vector3[3];

    public static TripplePendulomAnalytical instance;
    void OnDestroy()
    {
        FileWritter.WriteToFile("Analytical/Energy", "totalEnergy", totalEnergy);
    }

    private void Start()
    {
        instance = this;
        double x = 0;
        double y = 0;
        for (int i = 0; i < beads.Length; i++)
        {
            x += lengths[i] * Math.Sin(theta[i]);
            y += lengths[i] * -Math.Cos(theta[i]);
            beads[i].transform.position = new Vector3((float)x, (float)y, 0);
            prevPos[i] = beads[i].transform.position;
        }
        prevTime = Time.realtimeSinceStartupAsDouble;
    }

    /* private void Update()
     {
         //SimulateAnalytic(1.0 / 60.0);
         SimulateAnalytic(Time.deltaTime);
         prevTime = Time.realtimeSinceStartupAsDouble;
     }*/

    public static void Simulate(double dt)
    {
        for (int i = 0; i < instance.beads.Length; i++)
            instance.prevPos[i] = instance.beads[i].transform.position;
        instance.SimulateAnalytic(dt);

        instance.StoreTotalEnergy(dt);
    }

    private void StoreTotalEnergy(double dt)
    {
        double sum = 0;
        for (int i = 0; i < instance.beads.Length; i++)
        {
            Vector3 v = beads[i].transform.position - prevPos[i];
            double velocity = v.magnitude / dt;

            sum += 9.8f *  beads[i].transform.position.y;
            sum += velocity * velocity  * 0.5f;
        }
        totalEnergy.Add(new DataPacket(sum));
    }

    private void SimulateAnalytic(double dt)
    {
        double g = -gravity;
        double m1 = this.masses[0];
        double m2 = this.masses[1];
        double m3 = this.masses[2];
        double l1 = this.lengths[0];
        double l2 = this.lengths[1];
        double l3 = this.lengths[2];
        double t1 = this.theta[0];
        double t2 = this.theta[1];
        double t3 = this.theta[2];
        double w1 = this.omega[0];
        double w2 = this.omega[1];
        double w3 = this.omega[2];

        double b1 =
            g * l1 * m1 * Math.Sin(t1) + g * l1 * m2 * Math.Sin(t1) + g * l1 * m3 * Math.Sin(t1) + m2 * l1 * l2 * Math.Sin(t1 - t2) * w1 * w2 +
            m3 * l1 * l3 * Math.Sin(t1 - t3) * w1 * w3       +   m3 * l1 * l2 * Math.Sin(t1 - t2) * w1 * w2  +
            m2 * l1 * l2 * Math.Sin(t2 - t1) * (w1 - w2) * w2  +
            m3 * l1 * l2 * Math.Sin(t2 - t1) * (w1 - w2) * w2  +
            m3 * l1 * l3 * Math.Sin(t3 - t1) * (w1 - w3) * w3;

        double a11 = l1 * l1 * (m1 + m2 + m3);
        double a12 = m2 * l1 * l2 * Math.Cos(t1 - t2) + m3 * l1 * l2 * Math.Cos(t1 - t2);
        double a13 = m3 * l1 * l3 * Math.Cos(t1 - t3);

        double b2 =
            g * l2 * m2 * Math.Sin(t2) + g * l2 * m3 * Math.Sin(t2) + w1 * w2 * l1 * l2 * Math.Sin(t2 - t1) * (m2 + m3) +
            m3 * l2 * l3 * Math.Sin(t2 - t3) * w2 * w3               +
            (m2 + m3) * l1 * l2 * Math.Sin(t2 - t1) * (w1 - w2) * w1   +
            m3 * l2 * l3 * Math.Sin(t3 - t2) * (w2 - w3) * w3;

        double a21 = (m2 + m3) * l1 * l2 * Math.Cos(t2 - t1);
        double a22 = l2 * l2 * (m2 + m3);
        double a23 = m3 * l2 * l3 * Math.Cos(t2 - t3);

        double b3 =
            m3 * g * l3 * Math.Sin(t3) - m3 * l2 * l3 * Math.Sin(t2 - t3) * w2 * w3 - m3 * l1 * l3 * Math.Sin(t1 - t3) * w1 * w3 +
            m3 * l1 * l3 * Math.Sin(t3 - t1) * (w1 - w3) * w1    +
            m3 * l2 * l3 * Math.Sin(t3 - t2) * (w2 - w3) * w2;

        double a31 = m3 * l1 * l3 * Math.Cos(t1 - t3);
        double a32 = m3 * l2 * l3 * Math.Cos(t2 - t3);
        double a33 = m3 * l3 * l3;

        b1 = -b1;
        b2 = -b2;
        b3 = -b3;

        double det = a11 * (a22 * a33 - a23 * a32) + a21 * (a32 * a13 - a33 * a12) + a31 * (a12 * a23 - a13 * a22);
        if (det == 0.0)
            return;

        double a1 = b1 * (a22 * a33 - a23 * a32) + b2 * (a32 * a13 - a33 * a12) + b3 * (a12 * a23 - a13 * a22);
        double a2 = b1 * (a23 * a31 - a21 * a33) + b2 * (a33 * a11 - a31 * a13) + b3 * (a13 * a21 - a11 * a23);
        double a3 = b1 * (a21 * a32 - a22 * a31) + b2 * (a31 * a12 - a32 * a11) + b3 * (a11 * a22 - a12 * a21);

        a1 /= det;
        a2 /= det;
        a3 /= det;

        this.omega[0] += a1 * dt;
        this.omega[1] += a2 * dt;
        this.omega[2] += a3 * dt;
        this.theta[0] += this.omega[0] * dt;
        this.theta[1] += this.omega[1] * dt;
        this.theta[2] += this.omega[2] * dt;

        double x = 0.0, y = 0.0;
        for (int i = 0; i < this.masses.Length; i++)
        {
            x += this.lengths[i] * Math.Sin(this.theta[i]);
            y += this.lengths[i] * -Math.Cos(this.theta[i]);

            this.beads[i].transform.position = new Vector3((float)x, (float)y, 0);
        }
    }
}
