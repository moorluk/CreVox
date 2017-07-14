using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class SpawnerBase : MonoBehaviour 
{
	public GameObject m_prefab;
	public List<Transform> m_spwnPoints;
    protected int m_liveCount = 0;
    public int LiveCount
    {
        get
        {
            return m_liveCount;
        }
        set
        {
            m_liveCount = value;
        }
    }
    protected int m_spawnCount = 0;
    [HideInInspector]
	public Vector3 m_lastDiePos;
}
