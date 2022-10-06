using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ColorChanger : MonoBehaviour
{
    public Color startColor;
    public Color endColor;
    private Material mat;
    // Start is called before the first frame update
    void Awake()
    {
        mat = GetComponent<MeshRenderer>().material;
        mat.color = startColor;
    }

    public void ChangeColor()
    {
        mat.color = endColor;
    }
}
