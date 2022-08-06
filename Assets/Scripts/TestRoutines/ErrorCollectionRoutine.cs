using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ErrorCollectionRoutine : DataCollector
{
    public GameObject scenario;
    public int step = 10;
    public int maxSubsteps = 100;
    public float simulationTime = 10;
    private int currentSteps;
    private  List<DataPacket> meanError = new List<DataPacket>();
    // Start is called before the first frame update
    void Start()
    {
        StartCoroutine(TestScenario(1));
    }

    // Update is called once per frame
    void Update()
    {
    }

    private IEnumerator TestScenario(int substeps)
    {
        GameObject obj = Instantiate(scenario, Vector3.zero, Quaternion.identity);
        PhysicsEngine engine = obj.GetComponent<PhysicsEngine>();
        engine.substeps = substeps;
        yield return new WaitForSeconds(simulationTime);

        ErrorCollector eCol = obj.GetComponent<ErrorCollector>();
        meanError.Add(new DataPacket(substeps, eCol.GetMeanError()));

        Destroy(obj);

        currentSteps += step;
        if (currentSteps <= maxSubsteps)
            StartCoroutine(TestScenario(currentSteps));
        else
            FileWritter.WriteToFile("Error", "errorEvolution", meanError);
    }
}
