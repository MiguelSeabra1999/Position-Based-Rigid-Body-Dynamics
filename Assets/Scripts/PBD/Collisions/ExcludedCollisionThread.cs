using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Threading;

public class ExcludedCollisionThread : ThreadDispatcher
{
    private CollisionEngine collisionEngine;
    public ExcludedCollisionThread(CollisionEngine engine)
    {
        collisionEngine = engine;
    }

    protected override void DoWork(int from, int to, double h, int index)
    {
        for (int i = 0; i < collisionEngine.excludedColliders.Length; i++)
            for (int j = from; j < to; j++)
                collisionEngine.ParallelCheckCollision(collisionEngine.excludedColliders[i], collisionEngine.allColliders[j],  h, index);
    }

    protected override void DoWork(int from, int to, double h, int index, List<List<Correction>> corrections)
    {
        for (int i = 0; i < collisionEngine.excludedColliders.Length; i++)
            for (int j = from; j < to; j++)
                collisionEngine.ParallelCheckCollision(collisionEngine.excludedColliders[i], collisionEngine.allColliders[j],  h, index, corrections);
    }
}
