using Unity.Entities;
using Unity.Jobs;
using Unity.Physics;
using Unity.Physics.Systems;
using Unity.Burst;
using Unity.Collections;
using UnityEngine;
using Unity.Physics.Authoring;

public struct CollisionEventCollect : IComponentData {}

[RequireComponent(typeof(PhysicsBodyAuthoring))]
public class CollisionCollectorHavok : MonoBehaviour, IConvertGameObjectToEntity
{
    public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
    {
        dstManager.AddComponentData(entity, new CollisionEventCollect {});
    }
}

[UpdateInGroup(typeof(FixedStepSimulationSystemGroup))]
[UpdateAfter(typeof(ExportPhysicsWorld)), UpdateBefore(typeof(EndFramePhysicsSystem))]
public partial class CollisionCollectorHavokSystem : SystemBase
{
    StepPhysicsWorld m_StepPhysicsWorldSystem;
    EntityQuery m_ImpulseGroup;
    protected override void OnCreate()
    {
        m_StepPhysicsWorldSystem = World.GetOrCreateSystem<StepPhysicsWorld>();
        m_ImpulseGroup = GetEntityQuery(new EntityQueryDesc
        {
            All = new ComponentType[]
            {
                typeof(CollisionEventCollect)
            }
        });
    }

    [BurstCompile]
    struct CollisionEventCollectJob : ICollisionEventsJob
    {
        [ReadOnly] public ComponentDataFromEntity<CollisionEventCollect> ColliderEventCollectGroup;
        public ComponentDataFromEntity<PhysicsVelocity> PhysicsVelocityGroup;

        public void Execute(CollisionEvent collisionEvent)
        {
            //EnergyCollectorHavok.ReportCollision();
        }
    }

    protected override void OnStartRunning()
    {
        base.OnStartRunning();
        this.RegisterPhysicsRuntimeSystemReadOnly();
    }

    protected override void OnUpdate()
    {
        if (m_ImpulseGroup.CalculateEntityCount() == 0)
        {
            return;
        }


        Dependency = new CollisionEventCollectJob
        {
            ColliderEventCollectGroup = GetComponentDataFromEntity<CollisionEventCollect>(true),
            PhysicsVelocityGroup = GetComponentDataFromEntity<PhysicsVelocity>(),
        }.Schedule(m_StepPhysicsWorldSystem.Simulation, Dependency);
    }
}
