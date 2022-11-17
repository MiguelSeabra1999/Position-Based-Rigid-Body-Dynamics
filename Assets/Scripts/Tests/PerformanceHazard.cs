using System.Threading;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class PerformanceHazard : MonoBehaviour
{
    public float minFreq = 1;
    public float maxFreq = 2;
    public float minWait = 0.01f;
    public float maxWait = 0.1f;
    // Start is called before the first frame update
    void Start()
    {
        StartCoroutine(LagRoutine());
    }

    private IEnumerator LagRoutine()
    {
        while (true)
        {
            yield return new WaitForSeconds(UnityEngine.Random.Range(minFreq, maxFreq));
            Debug.Log("sleep");
            Thread.Sleep((int)(UnityEngine.Random.Range(minWait, maxWait) * 1000.0));
            Debug.Log("wake");
        }
    }
}
