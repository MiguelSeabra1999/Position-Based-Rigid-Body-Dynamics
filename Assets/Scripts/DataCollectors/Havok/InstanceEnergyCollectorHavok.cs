using Unity.Entities;
using Unity.Jobs;
using Unity.Physics;
using Unity.Physics.Systems;
using UnityEngine;
using System.Collections.Generic;
using Unity.Physics.Authoring;
using Unity.Mathematics;
using Unity.Transforms;
[RequireComponent(typeof(PhysicsBodyAuthoring))]
public class InstanceEnergyCollectorHavok : MonoBehaviour, IConvertGameObjectToEntity
{
    private static int idCount;
    public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
    {
        dstManager.AddComponentData(entity, new CollectEnergy
        {
            id = idCount
        });
        //  Debug.Log("havok object " + gameObject.name + " is id " + idCount);
        idCount++;
    }
}
[UpdateInGroup(typeof(FixedStepSimulationSystemGroup))]
[UpdateBefore(typeof(BuildPhysicsWorld))]
public partial class EnergyCollectorHavokSystem : SystemBase
{
    protected override void OnUpdate()
    {
        var deltaTime = Time.DeltaTime;

        Entities
            .WithName("CollectEnergy")
            .WithBurst()
            .ForEach((ref CollectEnergy collectEnergy, ref Translation t, ref Rotation r, ref PhysicsVelocity pv, ref PhysicsMass pm) =>
            {
                EnergyCollectorHavok.SampleVelocity(t, r, pv.Linear, pv.Angular, pm.InverseMass, pm.InverseInertia);
            }).Schedule();
    }
}

public struct CollectEnergy : IComponentData
{
    public int id;
}
