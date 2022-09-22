using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum EngineTypes
{
    PBD
}

[CreateAssetMenu(menuName = "PBRBD-Comparissons/PrefabSO")]
public class PrefabSO : ScriptableObject
{
    public GameObject pbdCube;
    public GameObject pbdCapsule;
    public GameObject pbdSphere;
    public GameObject pbdScene;
    public GameObject pbdSceneJacobi;
    public GameObject pbdWreckingBall;
    public GameObject pbdParticle;
    public GameObject havokCube;
    public GameObject havokCapsule;
    public GameObject havokCapsuleTrigger;
    public GameObject havokSphere;
    public GameObject havokSphereStatic;
    public GameObject havokScene;
    public GameObject havokWreckingBall;
    public GameObject unityCube;
    public GameObject unityCapsule;
    public GameObject unitySphere;
    public GameObject unityScene;
    public GameObject unityWreckingBall;
}
