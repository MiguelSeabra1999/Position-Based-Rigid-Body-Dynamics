using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Threading;

public class CollisionThread : ThreadDispatcher
{
    private CollisionEngine collisionEngine;
    public CollisionThread(CollisionEngine engine)
    {
        collisionEngine = engine;
    }

    protected override void DoWork(int from, int to, double h, int index)
    {
        for (int i = from; i < to; i++)
        {
            int a = collisionEngine.collisionPairs[i].Item1;
            int b = collisionEngine.collisionPairs[i].Item2;

            lock (collisionEngine.checkedCols)
            {
                if (collisionEngine.IsAlreadyChecked(a, b))
                    continue;
            }

            PBDCollider iCol = collisionEngine.allColliders[a];
            PBDCollider jCol = collisionEngine.allColliders[b];

            if (iCol.aabb.CollidesWith(jCol.aabb))
            {
                Mutex[] mutexes = {iCol.mutex, jCol.mutex};
                Mutex.WaitAll(mutexes);

                collisionEngine.ParallelCheckCollision(iCol, jCol, h, index);
                //ParallelCheckCollision(iCol, jCol,  h, index);
                mutexes[0].ReleaseMutex();
                mutexes[1].ReleaseMutex();
            }
        }
    }

    protected override void DoWork(int from, int to, double h, int index, List<List<Correction>> corrections)
    {
        for (int i = from; i < to; i++)
        {
            int a = collisionEngine.collisionPairs[i].Item1;
            int b = collisionEngine.collisionPairs[i].Item2;

            lock (collisionEngine.checkedCols)
            {
                if (collisionEngine.IsAlreadyChecked(a, b))
                    continue;
            }

            PBDCollider iCol = collisionEngine.allColliders[a];
            PBDCollider jCol = collisionEngine.allColliders[b];

            if (iCol.aabb.CollidesWith(jCol.aabb))
            {
                Mutex[] mutexes = {iCol.mutex, jCol.mutex};
                Mutex.WaitAll(mutexes);

                collisionEngine.ParallelCheckCollision(iCol, jCol, h, index, corrections);
                //ParallelCheckCollision(iCol, jCol,  h, index);
                mutexes[0].ReleaseMutex();
                mutexes[1].ReleaseMutex();
            }
        }
    }
}
