using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
public  class CollectDataIncremental : MonoBehaviour
{
    public GameObject scenario;
    public int step = 10;
    public int maxSubsteps = 100;
    public float simulationTime = 10;
    protected int currentSteps;

    protected GameObject currentScenario;
    private CollectionRoutine[] collectionRoutines;

    void Awake()
    {
        collectionRoutines = GetComponents<CollectionRoutine>();
    }

    void Start()
    {
        StartCoroutine(TestScenario(1));
    }

    private IEnumerator TestScenario(int substeps)
    {
        currentScenario = Instantiate(scenario, Vector3.zero, Quaternion.identity);
        InitAllScenarios(currentScenario);
        PhysicsEngine engine = currentScenario.GetComponent<PhysicsEngine>();
        engine.substeps = substeps;
        yield return null;
        InitAllScenarios(currentScenario);
        yield return new WaitForSeconds(simulationTime);

        CollectAllData(currentScenario, currentSteps);

        Destroy(currentScenario);
        GC.Collect();

        currentSteps += step;
        if (currentSteps <= maxSubsteps)
            StartCoroutine(TestScenario(currentSteps));
        else
            WriteAllData();
    }

    protected  void CollectAllData(GameObject scenario, int step)
    {
        foreach (CollectionRoutine collectionRoutine in collectionRoutines)
            collectionRoutine.CollectData(scenario, step);
    }

    protected  void WriteAllData()
    {
        foreach (CollectionRoutine collectionRoutine in collectionRoutines)
            collectionRoutine.WriteData();
    }

    protected  void InitAllScenarios(GameObject scenario)
    {
        foreach (CollectionRoutine collectionRoutine in collectionRoutines)
            collectionRoutine.InitScenario(scenario);
    }
}
