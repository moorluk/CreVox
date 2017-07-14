using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class TriggerEvent : MonoBehaviour 
{
    public string m_keyString = "";//old
    public List<string> m_keyStrings = new List<string>();
//    public LayerMask m_triggerLayer = (1 << (int)LayerType.PlayerHit);
    public int m_activeCount = -1;

    public void SetMessage(EventActor a_actorId)
    {
    }

    public void Happen()
    {
    }

	public void Record()
	{
	}
    
    void OnTriggerEnter(Collider c)
    {
    }
    void OnTriggerExit(Collider c)
    {
    }
    
    public virtual void TriggerEnter(Collider c){}
    public virtual void TriggerExit(Collider c){}
    public virtual void RemoveObject(GameObject a_go){}
}
