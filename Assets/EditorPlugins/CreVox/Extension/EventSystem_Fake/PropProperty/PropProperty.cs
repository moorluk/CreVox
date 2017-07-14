using UnityEngine;
using UnityEngine.Events;
using System.Collections.Generic;

[System.Serializable]
public class OutputEvent : UnityEvent<GameObject>
{
    public OutputEvent(OnEvent a_event)
    {
        this.m_event = a_event;
    }
    public OnEvent m_event;
}

public class PropProperty : MonoBehaviour 
{
    [HideInInspector]
    public List<OutputEvent> m_events = new List<OutputEvent>();

    public void Start()
    {
        gameObject.layer = LayerMask.NameToLayer("TriggerLayer");
    }

    protected void OnMessage(OnEvent a_event)
    {
        for (int m = 0; m < m_events.Count; ++m)
        {
            if (m_events[m].m_event == a_event)
            {
                m_events[m].Invoke(gameObject);
            }
        }
    }

    public void OnTrigger(OnEvent a_event)
    {
        OnMessage(a_event);
    }

    void OnFinished()
    {
        OnMessage(OnEvent.OnFinished);
    }
}
