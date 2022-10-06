using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
public class EnergyCollectorUnity : MonoBehaviour
{
    private List<DataPacket> totalEnergy = new List<DataPacket>();
    private List<DataPacket> kineticEnergy = new List<DataPacket>();
    private List<DataPacket> potentialEnergy = new List<DataPacket>();
    private bool used = false;

    void OnDestroy()
    {
        if (!used)
            return;
        FileWritter.WriteToFile("Unity/Energy", "totalEnergy", totalEnergy);
        FileWritter.WriteToFile("Unity/Energy", "kineticEnergy", kineticEnergy);
        FileWritter.WriteToFile("Unity/Energy", "potentialEnergy", potentialEnergy);
    }

    void FixedUpdate()
    {
        used = true;
        DataPacket data = GetTotalEnergy();
        totalEnergy.Add(data);
        data = GetKineticEnergy();
        kineticEnergy.Add(data);
        data = GetPotentialEnergy();
        potentialEnergy.Add(data);
    }

    private DataPacket GetTotalEnergy()
    {
        double sum = 0;
        int n  = transform.childCount;
        for (int i = 0; i < n; i++)
        {
            Rigidbody rb = transform.GetChild(i).GetComponent<Rigidbody>();
            if (rb != null)
                sum += CalcTotalEnergy(rb);
        }
        return new DataPacket(sum);
    }

    private DataPacket GetPotentialEnergy()
    {
        double sum = 0;
        int n  = transform.childCount;
        for (int i = 0; i < n; i++)
        {
            Rigidbody rb = transform.GetChild(i).GetComponent<Rigidbody>();
            if (rb != null)
                if (!rb.isKinematic)
                    sum += CalcPotentialEnergy(rb);
        }
        return new DataPacket(sum);
    }

    private DataPacket GetKineticEnergy()
    {
        double sum = 0;
        int n  = transform.childCount;
        for (int i = 0; i < n; i++)
        {
            Rigidbody rb = transform.GetChild(i).GetComponent<Rigidbody>();
            if (rb == null)
                continue;
            if (rb.isKinematic)
                continue;

            if (rb != null)
                sum += CalcKineticEnergy(rb);
        }
        return new DataPacket(sum);
    }

    private double CalcKineticEnergy(Rigidbody rb)
    {
        float linearEnergy = rb.velocity.magnitude * rb.velocity.magnitude * rb.mass * .5f;

        Vector3 wSelf = Quaternion.Inverse(rb.transform.rotation) * rb.angularVelocity;
        wSelf.Normalize();
        Vector3 aux = new Vector3(rb.inertiaTensor.x * wSelf.x, rb.inertiaTensor.y * wSelf.y, rb.inertiaTensor.z * wSelf.z);
        float moment = aux.magnitude;
        float w = rb.angularVelocity.magnitude;
        double angularEnergy = 0.5 * moment * w * w;

        return linearEnergy + angularEnergy;
    }

    public double CalcPotentialEnergy(Rigidbody rb)
    {
        if (rb.isKinematic)
            return 0;
        if (!rb.gameObject.activeInHierarchy)
            return 0;
        return rb.mass * 9.8f *  rb.transform.position.y;
    }

    public double CalcTotalEnergy(Rigidbody rb)
    {
        return CalcKineticEnergy(rb) + CalcPotentialEnergy(rb);
    }
}
