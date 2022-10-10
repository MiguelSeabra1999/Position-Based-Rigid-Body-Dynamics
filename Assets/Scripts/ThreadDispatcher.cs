using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Threading;
using System;
public class ThreadDispatcher
{
    [HideInInspector] public static readonly int NTHREADS = 7;
    public CountdownEvent countdownEvent = new CountdownEvent(NTHREADS);

    public void DistributeLoad(Action<int, int, double, int> work, int loadSize, double h)
    {
        countdownEvent.Reset();
        int increment =  loadSize / NTHREADS;
        if (increment == 0 || loadSize < NTHREADS)
        {
            for (int i = 0; i < NTHREADS; i++)
            {
                if (i + 1 > loadSize)
                    countdownEvent.Signal();
                else
                    InitThread(work, i , (i + 1), h, i);
            }

            countdownEvent.Wait();
        }
        else
        {
            for (int i = 0; i < NTHREADS - 1; i++)
            {
                InitThread(work, i * increment, (i + 1) * increment, h, i);
            }
            InitThread(work, (NTHREADS - 1) * increment, loadSize, h, NTHREADS - 1);
            countdownEvent.Wait();
        }
    }

    public void DistributeLoad(int loadSize, double h)
    {
        countdownEvent.Reset();
        int increment =  loadSize / NTHREADS;
        if (increment == 0 || loadSize < NTHREADS)
        {
            for (int i = 0; i < NTHREADS; i++)
            {
                if (i + 1 > loadSize)
                    countdownEvent.Signal();
                else
                    InitThread(i , (i + 1), h, i);
            }

            countdownEvent.Wait();
        }
        else
        {
            for (int i = 0; i < NTHREADS - 1; i++)
            {
                InitThread(i * increment, (i + 1) * increment, h, i);
            }
            InitThread((NTHREADS - 1) * increment, loadSize, h, NTHREADS - 1);
            countdownEvent.Wait();
        }
    }

    public void DistributeLoad(int loadSize, double h, List<List<Correction>> corrections)
    {
        countdownEvent.Reset();
        int increment =  loadSize / NTHREADS;
        if (increment == 0 || loadSize < NTHREADS)
        {
            for (int i = 0; i < NTHREADS; i++)
            {
                if (i + 1 > loadSize)
                    countdownEvent.Signal();
                else
                    InitThread(i , (i + 1), h, i, corrections);
            }

            countdownEvent.Wait();
        }
        else
        {
            for (int i = 0; i < NTHREADS - 1; i++)
            {
                InitThread(i * increment, (i + 1) * increment, h, i, corrections);
            }
            InitThread((NTHREADS - 1) * increment, loadSize, h, NTHREADS - 1, corrections);
            countdownEvent.Wait();
        }
    }

    public void InitThread(int from, int to, double h, int i)
    {
        ThreadPool.QueueUserWorkItem(new WaitCallback(
            (object callback) =>
            {
                DoWork(from, to, h, i);
                countdownEvent.Signal();
            }
        ));
    }

    public void InitThread(int from, int to, double h, int i, List<List<Correction>> corrections)
    {
        ThreadPool.QueueUserWorkItem(new WaitCallback(
            (object callback) =>
            {
                DoWork(from, to, h, i, corrections);
                countdownEvent.Signal();
            }
        ));
    }

    protected virtual void DoWork(int from, int to, double h, int i)
    {
    }

    protected virtual void DoWork(int from, int to, double h, int i, List<List<Correction>> corrections)
    {
    }

    public void InitThread(Action<int, int, double, int> work, int from, int to, double h, int i)
    {
        ThreadPool.QueueUserWorkItem(new WaitCallback(
            (object callback) =>
            {
                work(from, to, h, i);
                countdownEvent.Signal();
            }
        ));
    }

    public void InitThread<T>(Action<T, int> work, CountdownEvent countdownEvent, int depth, T obj)
    {
        ThreadPool.QueueUserWorkItem(new WaitCallback(
            (object callback) =>
            {
                work(obj, depth);
                countdownEvent.Signal();
            }
        ));
    }

    public void Wait()
    {
        countdownEvent.Wait();
    }

    public void Reset()
    {
        countdownEvent.Reset();
    }
}
