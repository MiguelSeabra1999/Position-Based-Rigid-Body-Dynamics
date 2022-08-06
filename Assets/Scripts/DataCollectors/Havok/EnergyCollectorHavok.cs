using Unity.Entities;
using Unity.Jobs;
using Unity.Physics;
using Unity.Physics.Systems;
using UnityEngine;
using System.Collections.Generic;
using Unity.Physics.Authoring;
using Unity.Mathematics;
using Unity.Transforms;


public class EnergyCollectorHavok : MonoBehaviour
{
    private  List<DataPacket> totalEnergy = new List<DataPacket>();
    private  List<DataPacket> kineticEnergy = new List<DataPacket>();
    private  List<DataPacket> potentialEnergy = new List<DataPacket>();
    private  List<DataPacket> collisionTimeStamps = new List<DataPacket>();
    private  List<double> totalEnergyBuffer = new List<double>();
    private  List<double> kineticEnergyBuffer = new List<double>();
    private  List<double> potentialEnergyBuffer = new List<double>();
    private static EnergyCollectorHavok instance;
    private bool collided = false;
    void Awake()
    {
        instance = this;
    }

    void Start()
    {
        potentialEnergyBuffer.Clear();
        kineticEnergyBuffer.Clear();
        totalEnergyBuffer.Clear();
    }

    void OnDestroy()
    {
        FileWritter.WriteToFile("Havok/Energy", "totalEnergy", totalEnergy);
        FileWritter.WriteToFile("Havok/Energy", "kineticEnergy", kineticEnergy);
        FileWritter.WriteToFile("Havok/Energy", "potentialEnergy", potentialEnergy);
        FileWritter.WriteToFile("Havok/Collisions", "collisionTimes", collisionTimeStamps);
    }

    void FixedUpdate()
    {
        SendBuffer(potentialEnergyBuffer, potentialEnergy);
        SendBuffer(kineticEnergyBuffer, kineticEnergy);
        SendBuffer(totalEnergyBuffer, totalEnergy);
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
        data.Add(new DataPacket(sum));
        buffer.Clear();
    }

    public static void ReportCollision()
    {
        instance.collided = true;
    }

    public static void SampleVelocity(Translation pos, Rotation rot, float3 linear, float3 angular, float mass, float3 inverseInertia)
    {
        double kinetic = (double)CalcKineticEnergy(linear, angular, mass, inverseInertia, rot);
        double potential = (double)CalcPotentialEnergy(pos.Value, mass);
        instance.kineticEnergyBuffer.Add(kinetic);
        instance.potentialEnergyBuffer.Add(potential);
        instance.totalEnergyBuffer.Add(kinetic + potential);
    }

    private static double CalcKineticEnergy(float3 linear, float3 angular, float mass, float3 inverseInertiaTensor, Rotation rotation)
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
    }

    public static double CalcPotentialEnergy(float3 pos, float mass)
    {
        return mass * 9.8f *  pos.y;
    }
}
