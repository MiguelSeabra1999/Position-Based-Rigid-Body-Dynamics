using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TimeScaleManager : MonoBehaviour
{
    [Range(0.01f, 5.0f)] public float timeScale = 1;
    public int targetFPS = 60;

    // Start is called before the first frame update
    void Start()
    {
        Time.timeScale = timeScale;
    }

    // Update is called once per frame
    void Update()
    {
        Time.timeScale = timeScale;

        Application.targetFrameRate = targetFPS;
    }
}
