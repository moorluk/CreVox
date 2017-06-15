using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public delegate void GameObjectCallback(GameObject a_gameObj);

public class EventDriver : MonoBehaviour {

	protected ArrayList m_callback = new ArrayList();
	protected Dictionary< string, List<EventActor> > m_actorMap = new Dictionary< string, List<EventActor> >();

	static float SPACE_SIZE_3D = 0.5f;
	public bool isEventConnected = true;
	public TriggerEvent[] m_TriggerEventList;
	public EventActor[] m_EventActorList;

	public void SceneEventUpdate()
	{
	}

	virtual public void RegisterCallback(GameObjectCallback a_actorCallback)
	{
	}

	virtual public void TriggerCallback(GameObject a_gameObject)
	{
	}

	virtual public void RegisterActor(EventActor a_actor)
	{
		string key = a_actor.m_keyString;
		if ( m_actorMap.ContainsKey(key) )
		{
			m_actorMap[key].Add(a_actor);
		}
		else
		{
			List<EventActor> newList = new List<EventActor>();
			newList.Add(a_actor);
			m_actorMap.Add(key, newList);
		}
		Debug.Log ("[EA]" + a_actor.GetComponentInParent<CreVox.PaletteItem> ().name + " >> [ED]" + this.name);
	}

	virtual public void RemoveActor(EventActor a_actor)
	{
	}

	virtual public void TriggerActor(TriggerEvent a_gameobject)
	{
	}
	virtual public void TriggerActor(List<string> a_keyStrings)
	{
	}
	void DrawSpawners()
	{
	}
	void OnDrawGizmos()
	{
	}

	static Vector3 GetColliderCenter(Collider a_collider)
	{
		return Vector3.zero;
	}

	public void OnValidate()
	{ 
	}
}
