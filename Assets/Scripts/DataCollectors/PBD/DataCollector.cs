using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
public abstract class DataCollector : MonoBehaviour
{
    protected PhysicsEngine engine;

    public string subFolder;

    protected virtual void Awake()
    {
        engine = GetComponent<PhysicsEngine>();
        if (engine == null)
            if (transform.parent != null)
                engine = transform.parent.GetComponent<PhysicsEngine>();
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
