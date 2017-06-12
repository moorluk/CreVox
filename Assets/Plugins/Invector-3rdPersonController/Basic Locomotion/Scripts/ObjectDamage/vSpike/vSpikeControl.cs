using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class vSpikeControl : MonoBehaviour
{
    [HideInInspector]
    public List<Transform> attachColliders;
   
    void Start()
    {
        attachColliders = new List<Transform>();
        var objs = GetComponentsInChildren<vSpike>();
        foreach (vSpike obj in objs)
            obj.control = this;
    }
}
