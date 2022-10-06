using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[RequireComponent(typeof(ColorChanger))]
public class ChangeColorOnMove : MonoBehaviour
{
    public static int changeCount = 0;
    private ColorChanger colorChanger;
    private Vector3 prevPos;
    void Awake()
    {
        colorChanger = GetComponent<ColorChanger>();
        prevPos = transform.position;
    }

    void Update()
    {
        if (prevPos != transform.position)
            colorChanger.ChangeColor();
        prevPos = transform.position;
    }
}
