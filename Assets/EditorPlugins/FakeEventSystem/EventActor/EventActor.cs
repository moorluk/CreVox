using UnityEngine;
using System.Collections;

public class EventActor : MonoBehaviour {

    public string m_keyString = "";
	public int m_activeCount = -1;

	virtual public void RegisterEvent()
	{
		if (m_keyString != null)
		{
			SendMessageUpwards("RegisterActor", this);
		}
	}

	public void Excute()
	{
		if (m_activeCount != 0)
		{
			Action();
		}
		if (m_activeCount > 0)
		{
			--m_activeCount;
		}
	}

	virtual public void Action(){}
}