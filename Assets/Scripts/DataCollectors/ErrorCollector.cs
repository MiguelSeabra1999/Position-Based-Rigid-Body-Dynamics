using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ErrorCollector : DataCollector
{
    private  List<DataPacket> collisionsError = new List<DataPacket>();
    private  List<DataPacket> constraintsError = new List<DataPacket>();
    private  List<DataPacket> totalError = new List<DataPacket>();
    private  List<double> error = new List<double>();
    void OnDestroy()
    {
        WriteToFile("Error", "meanErrorConstraints", constraintsError);
        /*  WriteToFile("Error", "meanErrorCollisions", collisionsError);
          WriteToFile("Error", "meanError", totalError);*/
    }

    protected override void StepEnd()
    {
        double totalError = 0;
        int n = 0;
        foreach (PBDConstraint constraint in engine.constraints)
        {
            totalError += constraint.Evaluate();
            n++;
        }
        constraintsError.Add(new DataPacket(totalError / n));
        error.Add(totalError / n);
        /*  double totalErrorCol = 0;
          int nCol = 0;
          foreach(PBDCollision collision in engine.collisionEngine.collisions)
          {
              totalErrorCol += collision.
              nCol++;
          }*/
    }

    public double GetMeanError()
    {
        double sum = 0;
        double n = 0;
        foreach (double e in error)
        {
            n++;
            sum += e;
        }
        return sum / n;
    }
}
