using System.Diagnostics;
using Unity.Entities;
using Unity.Physics.Systems;
using UnityEngine;
using System;
// Source: https://forum.unity.com/threads/unity-physics-discussion.646486/page-18#post-5027081

[UpdateBefore(typeof(BuildPhysicsWorld))]
public class PrePhysicsSetDeltaTimeSystem : ComponentSystem
{
    public bool isRealTimeStep = true;
    public float timeScale = 1;
    public float previousDeltaTime = UnityEngine.Time.deltaTime;
    private float prevTime = 0;


    protected override void OnUpdate()
    {
        DateTimeOffset now = (DateTimeOffset)DateTime.UtcNow;  // using System;
        long nowInMilliseconds = now.ToUnixTimeMilliseconds();


        double realDeltaTime = 0.001 * (nowInMilliseconds - prevTime);
        prevTime = nowInMilliseconds;

        previousDeltaTime = UnityEngine.Time.deltaTime;


        UnityEngine.Time.fixedDeltaTime = (float)realDeltaTime * timeScale;
    }
}

[UpdateAfter(typeof(ExportPhysicsWorld))]
public class PostPhysicsResetDeltaTimeSystem : ComponentSystem
{
    public PrePhysicsSetDeltaTimeSystem preSystem;

    protected override void OnCreate()
    {
        preSystem = World.DefaultGameObjectInjectionWorld.GetOrCreateSystem<PrePhysicsSetDeltaTimeSystem>();
    }

    protected override void OnUpdate()
    {
        UnityEngine.Time.fixedDeltaTime = preSystem.previousDeltaTime;
    }
}
