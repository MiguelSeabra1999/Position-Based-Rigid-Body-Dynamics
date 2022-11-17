using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;
using Unity.Jobs;
using Unity.Physics;
using Unity.Physics.Systems;
using Unity.Burst;
using Unity.Collections;
using Unity.Physics.Authoring;

public class DestroyEntitiesOnDestroy : MonoBehaviour
{
    public static List<Entity> entities = new List<Entity>();
    private void OnDestroy()
    {
        var entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;

        if (entityManager == null)
            return;
        foreach (Entity e in entities)
        {
            entityManager.DestroyEntity(e);
        }
    }
}
