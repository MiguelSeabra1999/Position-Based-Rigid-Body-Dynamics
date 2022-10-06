using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StopRunningTimeout : MonoBehaviour
{
    public float timeout = 0;

    void Start()
    {
        if (timeout > 0)
            Invoke("Timeout", timeout);
    }

    private void Timeout()
    {
        UnityEditor.EditorApplication.isPlaying = false;
    }
}
