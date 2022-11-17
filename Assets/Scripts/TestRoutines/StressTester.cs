using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;


public  class StressTester : MonoBehaviour
{
    public EngineTypes engineType;
    public string functionName;
    public GameObject spawner;
    public int step = 10;
    public int startStep = 1;
    public int maxSubsteps = 100;
    public float simulationTime = 10;
    protected int currentSteps;
    private AudioSource completeSound;

    protected SpawnerComparissons currentSpawner;
    private CollectionRoutine[] collectionRoutines;

    void Awake()
    {
        collectionRoutines = GetComponents<CollectionRoutine>();
        completeSound = GetComponent<AudioSource>();
    }

    void Start()
    {
        currentSteps = startStep;
        StartCoroutine(TestScenario(currentSteps));
    }

    private IEnumerator TestScenario(int substeps)
    {
        currentSpawner = Instantiate(spawner, Vector3.zero, Quaternion.identity).GetComponent<SpawnerComparissons>();

        currentSpawner.dims = new Vector3Int(currentSteps, currentSteps, currentSteps);
        currentSpawner.SendMessage(functionName);
        DestroyOthers();

        yield return null;
        InitAllScenarios(currentSpawner.gameObject);
        yield return new WaitForSeconds(simulationTime);

        CollectAllData(currentSpawner.gameObject, currentSteps);

        Destroy(currentSpawner.gameObject);
        if (engineType == EngineTypes.Havok)
            DestroyEntities();
        yield return null;
        GC.Collect();
        yield return new WaitForSeconds(1);
        currentSteps += step;
        if (currentSteps <= maxSubsteps)
            StartCoroutine(TestScenario(currentSteps));
        else
        {
            WriteAllData();
            if (completeSound)
            {
                completeSound.Play();
                yield return new WaitForSeconds(5);
            }
        }
    }

    private void DestroyOthers()
    {
        switch (engineType)
        {
            case EngineTypes.PBD:
                Destroy(currentSpawner.jacobi);
                Destroy(currentSpawner.havok);
                Destroy(currentSpawner.unity);
                return;
            case EngineTypes.Jacobi:
                Destroy(currentSpawner.pbd);
                Destroy(currentSpawner.havok);
                Destroy(currentSpawner.unity);
                return;
            case EngineTypes.Havok:
                Destroy(currentSpawner.pbd);
                Destroy(currentSpawner.jacobi);
                Destroy(currentSpawner.unity);
                return;
            case EngineTypes.Unity:
                Destroy(currentSpawner.pbd);
                Destroy(currentSpawner.jacobi);
                Destroy(currentSpawner.havok);
                return;
        }
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

    private void DestroyEntities()
    {
    }
}


[Serializable]
public enum EngineTypes
{
    PBD,
    Jacobi,
    Unity,
    Havok
}
