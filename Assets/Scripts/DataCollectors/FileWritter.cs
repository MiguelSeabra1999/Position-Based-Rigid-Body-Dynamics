using System.IO;
using System.Collections.Generic;
using UnityEngine;
public static class FileWritter
{
    private static string baseFolder = "C:/Users/legom/PBRBD/EntityComponentSystemSamples-master/PBRBD-Comparissons/Assets/Data";
    public static void WriteToFile(string subsubFolder, string fileName, List<DataPacket> data)
    {
        string dirPath = baseFolder;

        if (subsubFolder != "" && subsubFolder != null)
            dirPath += "/" + subsubFolder;
        string filePath = "/" + fileName + ".txt";

        if (!Directory.Exists(dirPath))
        {
            Directory.CreateDirectory(dirPath);
        }
        Debug.Log("Writting" + dirPath + filePath);
        StreamWriter writer = new StreamWriter(dirPath + filePath, false);

        foreach (DataPacket packet in data)
        {
            writer.WriteLine(packet.ToString());
        }
        writer.Close();
    }
}
