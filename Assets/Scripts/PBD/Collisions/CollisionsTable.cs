using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CollisionsTable 
{
    private PBDCollider[] allColliders;
    private Dictionary<(PBDCollider, PBDCollider), bool> collisions;
    public CollisionsTable(PBDCollider[] allColliders)
    {
        this.allColliders = allColliders;
        Init();
    }
    public void Init()
    {

    }

    
}
