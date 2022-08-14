using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class PBDMonoBehaviour : MonoBehaviour
{
    public virtual  void PBDphysicsUpdate(double h) {}
    public virtual  void PBDUpdate() {}
}
