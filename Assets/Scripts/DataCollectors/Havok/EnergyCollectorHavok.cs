using Unity.Entities;
using Unity.Jobs;
using Unity.Physics;
using Unity.Physics.Systems;
using UnityEngine;
using System.Collections.Generic;
using Unity.Physics.Authoring;
using Unity.Mathematics;
using Unity.Transforms;
using System;

public class EnergyCollectorHavok : MonoBehaviour
{
    private static bool used = false;
    private  List<DataPacket> totalEnergy = new List<DataPacket>();
    private  List<DataPacket> kineticEnergy = new List<DataPacket>();
    private  List<DataPacket> potentialEnergy = new List<DataPacket>();
    private  List<DataPacket> linearEnergy = new List<DataPacket>();
    private  List<DataPacket> rotationalEnergy = new List<DataPacket>();
    private  List<DataPacket> collisionTimeStamps = new List<DataPacket>();
    private  List<double> totalEnergyBuffer = new List<double>();
    private  List<double> kineticEnergyBuffer = new List<double>();
    private  List<double> potentialEnergyBuffer = new List<double>();
    private  List<double> linearEnergyBuffer = new List<double>();
    private  List<double> rotationalEnergyBuffer = new List<double>();
    private static EnergyCollectorHavok instance;
    private bool collided = false;

    //private long prevTime = 0;

    void Awake()
    {
        instance = this;
    }

    void Start()
    {
        potentialEnergyBuffer.Clear();
        kineticEnergyBuffer.Clear();
        totalEnergyBuffer.Clear();
        linearEnergyBuffer.Clear();
        rotationalEnergyBuffer.Clear();
    }

    void OnDestroy()
    {
        if (!used)
            return;
        FileWritter.WriteToFile("Havok/Energy", "totalEnergy", totalEnergy);
        FileWritter.WriteToFile("Havok/Energy", "kineticEnergy", kineticEnergy);
        FileWritter.WriteToFile("Havok/Energy", "potentialEnergy", potentialEnergy);
        FileWritter.WriteToFile("Havok/Energy", "linearEnergy", linearEnergy);
        FileWritter.WriteToFile("Havok/Energy", "rotationalEnergy", rotationalEnergy);
        FileWritter.WriteToFile("Havok/Collisions", "collisionTimes", collisionTimeStamps);
    }

    void FixedUpdate()
    {
        SendBuffer(potentialEnergyBuffer, potentialEnergy);
        SendBuffer(kineticEnergyBuffer, kineticEnergy);
        SendBuffer(totalEnergyBuffer, totalEnergy);
        SendBuffer(linearEnergyBuffer, linearEnergy);
        SendBuffer(rotationalEnergyBuffer, rotationalEnergy);
        if (collided)
        {
            instance.collisionTimeStamps.Add(new DataPacket(0));
            collided = false;
        }
    }

    private void SendBuffer(List<double> buffer, List<DataPacket> data)
    {
        double sum = 0;
        foreach (double val in buffer)
        {
            sum += val;
        }
        if (sum == 0)
            return;
        //  Debug.Log(buffer.Count);
        data.Add(new DataPacket(sum));
        buffer.Clear();
    }

    public static void ReportCollision()
    {
        instance.collided = true;
    }

    public static void SampleVelocity(Translation pos, Rotation rot, float3 linear, float3 angular, float inverseMass, float3 inverseInertia)
    {
        used = true;
        if (inverseMass == 0)
            return;
        float mass = 1 / inverseMass;
        double linearEnergy = (double)CalcLinearEnergy(linear, angular, mass, inverseInertia, rot);
        double rotationalEnergy = (double)CalcRotationalEnergy(linear, angular, mass, inverseInertia, rot);

        double potential = (double)CalcPotentialEnergy(pos.Value, mass);

        lock (instance.linearEnergyBuffer)
            instance.linearEnergyBuffer.Add(linearEnergy);
        lock (instance.rotationalEnergyBuffer)
            instance.rotationalEnergyBuffer.Add(rotationalEnergy);
        lock (instance.kineticEnergyBuffer)
            instance.kineticEnergyBuffer.Add(linearEnergy + rotationalEnergy);
        lock (instance.potentialEnergyBuffer)
            instance.potentialEnergyBuffer.Add(potential);
        lock (instance.totalEnergyBuffer)
            instance.totalEnergyBuffer.Add(linearEnergy + rotationalEnergy + potential);


        /*   DateTimeOffset now = (DateTimeOffset)DateTime.UtcNow;  // using System;
           long nowInMilliseconds = now.ToUnixTimeMilliseconds();

           double realDeltaTime = 0.001 * (nowInMilliseconds - instance.prevTime);
           instance.prevTime = nowInMilliseconds;

           Debug.Log("real fps" + 1.0 / realDeltaTime);*/
    }

    /*  private static double CalcKineticEnergy(float3 linear, float3 angular, float mass, float3 inverseInertiaTensor, Rotation rotation)
      {
          Vector3 linearVelocity = new Vector3(linear.x, linear.y, linear.z);
          Vector3 angularVelocity = new Vector3(angular.x, angular.y, angular.z);
          Vector3 inertiaTensor = new Vector3(1 / inverseInertiaTensor.x, 1 / inverseInertiaTensor.y, 1 / inverseInertiaTensor.z);
          Quaternion q = rotation.Value;
          float linearEnergy = linearVelocity.magnitude * linearVelocity.magnitude * mass * .5f;

          Vector3 wSelf = Quaternion.Inverse(q) * angularVelocity;
          wSelf.Normalize();
          Vector3 aux = new Vector3(inertiaTensor.x * wSelf.x, inertiaTensor.y * wSelf.y, inertiaTensor.z * wSelf.z);
          float moment = aux.magnitude;
          float w = angularVelocity.magnitude;
          double angularEnergy = 0.5 * moment * w * w;

          return linearEnergy + angularEnergy;
      }*/
    private static double CalcRotationalEnergy(float3 linear, float3 angular, float mass, float3 inverseInertiaTensor, Rotation rotation)
    {
        Vector3 angularVelocity = new Vector3(angular.x, angular.y, angular.z);
        Vector3 inertiaTensor = new Vector3(1 / inverseInertiaTensor.x, 1 / inverseInertiaTensor.y, 1 / inverseInertiaTensor.z);
        Quaternion q = rotation.Value;

        Vector3 wSelf = Quaternion.Inverse(q) * angularVelocity;
        wSelf.Normalize();
        Vector3 aux = new Vector3(inertiaTensor.x * wSelf.x, inertiaTensor.y * wSelf.y, inertiaTensor.z * wSelf.z);
        float moment = aux.magnitude;
        float w = angularVelocity.magnitude;
        return 0.5 * moment * w * w;
    }

    private static double CalcLinearEnergy(float3 linear, float3 angular, float mass, float3 inverseInertiaTensor, Rotation rotation)
    {
        Vector3 linearVelocity = new Vector3(linear.x, linear.y, linear.z);
        return linearVelocity.magnitude * linearVelocity.magnitude * mass * .5f;
    }

    public static double CalcPotentialEnergy(float3 pos, float mass)
    {
        return mass * 9.8f *  pos.y;
    }
}
