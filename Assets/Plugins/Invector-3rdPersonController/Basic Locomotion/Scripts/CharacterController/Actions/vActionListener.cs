using UnityEngine;
using System.Collections;
using Invector;

public abstract class vActionListener : vMonoBehaviour
{
    public bool actionEnter;
    public bool actionStay;
    public bool actionExit;
    [HideInInspector]
    public vOnActionHandle OnDoAction;

    public virtual void OnActionEnter(Collider other)
    {

    }

    public virtual void OnActionStay(Collider other)
    {

    }

    public virtual void OnActionExit(Collider other)
    {

    }

    [System.Serializable]
    public class vOnActionHandle : UnityEngine.Events.UnityEvent<vTriggerGenericAction>
    {

    }
}