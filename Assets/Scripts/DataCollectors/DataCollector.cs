using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
public abstract class DataCollector : MonoBehaviour
{
    protected PhysicsEngine engine;

    public string subFolder;

    void Awake()
    {
        engine = GetComponent<PhysicsEngine>();
    }

    void OnEnable()
    {
        if (engine == null)
            return;
        engine.physicsStepEnd += StepEnd;
        engine.physicsSubstepEnd += SubstepEnd;
    }

    void OnDisable()
    {
        if (engine == null)
            return;
        engine.physicsStepEnd -= StepEnd;
        engine.physicsSubstepEnd -= SubstepEnd;
    }

    protected virtual void StepEnd() {}
    protected virtual void SubstepEnd() {}
}
