using System.Security.Cryptography.X509Certificates;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using System;
using System.Runtime.InteropServices;

public class CppBackend : MonoBehaviour
{
    private const string pluginName = "PBDplugin";
    [DllImport(pluginName)]
    private static extern IntPtr getEventFunction();

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate void DebugDelegate(string str);
    static void CallbackFunction(string str){Debug.Log(str);}

    [DllImport(pluginName)]
    public static extern void SetDebugFunction(IntPtr fp);

        [DllImport(pluginName)]
    public static extern int TestFunc();

    private CommandBuffer cmd;

    void Start()
    {
         Debug.Log("HII");
        DebugDelegate callback_delegate = new DebugDelegate(CallbackFunction);

        IntPtr intptr_delegate = Marshal.GetFunctionPointerForDelegate(callback_delegate);
        SetDebugFunction(intptr_delegate);

        cmd = new CommandBuffer();
        cmd.name = pluginName;
        var camera = Camera.main;
        camera.AddCommandBuffer(CameraEvent.AfterGBuffer, cmd);
    }

    void Update()
    {
        cmd.IssuePluginEvent(getEventFunction(),0);
        Debug.Log(TestFunc());
        

    }

}
