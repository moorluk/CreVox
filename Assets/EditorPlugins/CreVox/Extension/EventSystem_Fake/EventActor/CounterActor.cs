using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class CounterActor : EventActor 
{
	public List<EventActor> m_actors;

	void Update()
	{
		if (m_activeCount == 0) {
			foreach (EventActor e in m_actors) {
				e.Action ();
				e.m_activeCount = 0;
			}
			--m_activeCount;
		}
	}
}
