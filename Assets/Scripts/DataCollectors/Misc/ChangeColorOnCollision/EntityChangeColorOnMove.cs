using Unity.Entities;
using Unity.Jobs;
using Unity.Physics;
using Unity.Physics.Systems;
using UnityEngine;
using System.Collections.Generic;
using Unity.Physics.Authoring;
using Unity.Mathematics;
using Unity.Transforms;
using Unity.Rendering;
[RequireComponent(typeof(PhysicsBodyAuthoring))]
public class EntityChangeColorOnMove : MonoBehaviour, IConvertGameObjectToEntity
{
    private static int idCount;
    private ChangeColorOnMoveComponent data;
    private bool changedColor = false;
    public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
    {
        data = new ChangeColorOnMoveComponent
        {
            id = idCount ,
            changeColor = false,
            prevPos = Vector3.zero,
        };

        dstManager.AddComponentData(entity, data);
        //  Debug.Log("havok object " + gameObject.name + " is id " + idCount);
        idCount++;
    }

    void Update()
    {
        if (data.changeColor && !changedColor)
        {
            GetComponent<ColorChanger>().ChangeColor();
            changedColor = true;
        }
    }
}
[UpdateInGroup(typeof(FixedStepSimulationSystemGroup))]
[UpdateBefore(typeof(BuildPhysicsWorld))]
public partial class ChangeColorOnMoveBase : SystemBase
{
    protected override void OnUpdate()
    {
        Entities
            .WithName("ChangeColorOnMoveComponent")
            .WithStructuralChanges()
            .ForEach((Entity e, ref ChangeColorOnMoveComponent colorChanger, ref Translation t) =>
            {
                Vector3 current = new Vector3(t.Value.x, t.Value.y, t.Value.z);
                EntityManager.AddComponentData(e, new URPMaterialPropertyBaseColor {Value = new float4(0, 0, 1, 1)});
                if (current != colorChanger.prevPos && colorChanger.prevPos != Vector3.zero)
                    //colorChanger.changeColor = true;
                    EntityManager.AddComponentData(e, new URPMaterialPropertyBaseColor {Value = new float4(1, 0, 0, 1)});
                colorChanger.prevPos = new Vector3(t.Value.x, t.Value.y, t.Value.z);
            }).Run();
    }
}

public struct ChangeColorOnMoveComponent : IComponentData
{
    public int id;
    public bool changeColor;
    public Vector3 prevPos;
}
