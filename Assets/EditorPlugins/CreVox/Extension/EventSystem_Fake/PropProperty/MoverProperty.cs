using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public enum MoverState
{
    Position_Now,
    Moving,
    Position_Goal,
	Count
}
public enum MoverInterpolator
{
    Linear,
    AccDec
}

public enum MoverRoute
{
    Once,
    Loop,
    Inverse
}

public class MoverProperty : PropProperty 
{
    public MoverState m_state = MoverState.Position_Now;
    public Transform[] m_nodes;
    private int m_positiveIndex = 0;
    private int m_nodeIndex = 0;
    public float m_maxSpeed = 1f;
    public MoverInterpolator m_speedType;
    private float m_exitNodeTime = 0;
    public MoverRoute m_route = MoverRoute.Once;
    bool m_doInverse = false;

    void Enable()
    {
        m_state = MoverState.Moving;
    }

    void Disable()
    {
        m_state = MoverState.Position_Now;
    }

    public void OnGoal()
    {
        OnMessage(OnEvent.OnGoal);
    }

	void Update () 
    {
        if (m_state != MoverState.Moving)
        {
            return;
        }
        else
        {
            m_exitNodeTime += Time.deltaTime;
            if (m_nodeIndex < m_nodes.Length && m_nodes[m_nodeIndex] != null)
            {
                if ( (transform.position - m_nodes[m_nodeIndex].position).magnitude < 0.01f )
                {
                    m_positiveIndex += 1;
                    //Debug.LogWarning("Elevator index: " + m_nodeIndex);
                    m_exitNodeTime = 0;
                    if (m_positiveIndex == m_nodes.Length)
                    {
                        if (m_route == MoverRoute.Once || m_route == MoverRoute.Inverse)
                        {
                            m_doInverse = (m_route == MoverRoute.Inverse) ? !m_doInverse : m_doInverse;
                            m_state = MoverState.Position_Goal;
                            OnGoal();
                        }
                        m_positiveIndex = 0;
                    }

                    if (m_doInverse)
                    {
                        m_nodeIndex = GetInverseIndex(m_positiveIndex);
                    }
                    else
                    {
                        m_nodeIndex = m_positiveIndex;
                    }
                }
                if (m_state != MoverState.Position_Goal)
                {
                    UpdatePos(m_nodes[m_nodeIndex].position);
                }
            }
        }
	}

    int GetInverseIndex(int a_index)
    {
        int index = m_nodes.Length - a_index - 1;
        return index;
    }

    void UpdatePos(Vector3 a_targetPos)
    {
        if (m_speedType == MoverInterpolator.Linear)
        {
            Vector3 dir = a_targetPos - transform.position;
            float speed = m_maxSpeed;

            Vector3 offset = dir.normalized * speed * Time.deltaTime;
            if (offset.magnitude > dir.magnitude)
            {
                offset = dir;
            }
            transform.position += offset;
        }
        else if (m_speedType == MoverInterpolator.AccDec)
        {
            transform.position = GetAccDecPos();
        }
    }

    float GetSpeedRate()
    {
        float x = 0.5f;
        float maxSpeed = -Mathf.PI * Mathf.Sin( Mathf.PI * (x+1) );
        return m_maxSpeed / maxSpeed;
    }

    Vector3 GetAccDecPos()
    {
        int previous = (m_positiveIndex - 1) < 0 ? m_nodes.Length - 1 : m_positiveIndex - 1;
        if (m_doInverse)
        {
            previous = GetInverseIndex(previous);
        }

        Vector3 dis = m_nodes[m_nodeIndex].position - m_nodes[previous].position;
        //Debug.LogWarning("Elevator dis: " + dis.ToString());
        float totalTime = dis.magnitude / GetSpeedRate();
        float timeRate = totalTime == 0 ? 0 : m_exitNodeTime / totalTime;

        float x = Mathf.Clamp(timeRate, 0, 1);
        float offset = 0.5f + Mathf.Cos( Mathf.PI * (x + 1) ) / 2;
        //Debug.LogWarning("Elevator offset: " + offset.ToString());
        return m_nodes[previous].position + dis * offset;
    }
}
