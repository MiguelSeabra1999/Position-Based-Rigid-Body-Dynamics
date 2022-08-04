using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
public abstract class DataCollector : MonoBehaviour
{
    protected PhysicsEngine engine;
    private string baseFolder = "C:/Users/legom/PBRBD/EntityComponentSystemSamples-master/PBRBD-Comparissons/Assets/Data";
    public string subFolder;

    void Awake()
    {
        engine = GetComponent<PhysicsEngine>();
    }

    void OnEnable()
    {
        engine.physicsStepEnd += StepEnd;
        engine.physicsSubstepEnd += SubstepEnd;
    }

    void OnDisable()
    {
        engine.physicsStepEnd += StepEnd;
        engine.physicsSubstepEnd += SubstepEnd;
    }

    protected virtual void StepEnd() {}
    protected virtual void SubstepEnd() {}

    protected void WriteToFile(string subsubFolder, string fileName, List<DataPacket> data)
    {
        string dirPath = baseFolder;
        if (subFolder != "" && subFolder != null)
            dirPath += "/" + subFolder;
        if (subsubFolder != "" && subsubFolder != null)
            dirPath += "/" + subsubFolder;
        string filePath = "/" + fileName + ".txt";

        if (!Directory.Exists(dirPath))
        {
            Directory.CreateDirectory(dirPath);
        }
        Debug.Log(dirPath + filePath);
        StreamWriter writer = new StreamWriter(dirPath + filePath, false);

        foreach (DataPacket packet in data)
        {
            writer.WriteLine(packet.ToString());
        }
        writer.Close();
    }
}
