using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Physics.Authoring;
using Unity.Entities;
using Unity.Jobs;
using Unity.Physics;
using Unity.Physics.Systems;
using Unity.Transforms;
[RequireComponent(typeof(PhysicsBodyAuthoring))]
public class InstanceDeviationCollectorHavok : MonoBehaviour, IConvertGameObjectToEntity
{
    private static int idCount;
    public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
    {
        dstManager.AddComponentData(entity, new CollectDeviation
        {
            id = idCount
        });
        //  Debug.Log("havok object " + gameObject.name + " is id " + idCount);
        idCount++;
    }
}
public struct CollectDeviation : IComponentData
{
    public int id;
}

[UpdateInGroup(typeof(FixedStepSimulationSystemGroup))]
[UpdateBefore(typeof(BuildPhysicsWorld))]
public partial class DeviationCollectorHavokSystem : SystemBase
{
    protected override void OnUpdate()
    {
        Entities
            .WithName("CollectDeviation")
            .WithBurst()
            .ForEach((ref CollectDeviation collectDeviation, ref Translation t) =>
            {
                DeviationCollectorHavok.SamplePosition(t.Value);
            }).Schedule();
    }
}
