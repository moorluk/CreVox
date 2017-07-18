using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using System.Collections.Generic;

public enum EnemyType
{
    Origin = -1,
    Asassin = 0,
    Warrior = 1,
    Guardian = 2,
    Ranger = 3,
	Boss1f = 4,
	Boss2f = 5,
	Boss3f = 6,
	Boss4f = 7,
	Boss5f = 8,
	Boss6f = 9,
	Bomber = 10,
    Count = 11
}

[System.Serializable]
public class SpawnerData
{
    public int m_totalQty;
    public int m_maxLiveQty;
    [HideInInspector]
    public float m_spwnTime;
    [HideInInspector]
    public float m_StartTime;
    public int m_spwnCountPerTime;
    public Vector2 m_randomSpawn = Vector2.zero;

    SpawnerData()
    {
        m_totalQty = 0;
        m_maxLiveQty = 0;
        m_spwnTime = 0f;
        m_StartTime = 0f;
        m_spwnCountPerTime = 1;
    }
    public SpawnerData(int a_totalQty, int a_maxLiveQty, float a_spwnTime, float a_StartTime, int a_spwnCountPerTime)
    {
        m_totalQty = a_totalQty;
        m_maxLiveQty = a_maxLiveQty;
        m_spwnTime = a_spwnTime;
        m_StartTime = a_StartTime;
        m_spwnCountPerTime = a_spwnCountPerTime;
    }
}

public class EnemySpawner : SpawnerBase 
{
    public EnemyType m_enemyType = EnemyType.Origin;
    public int m_enemyId = 0;

    public SpawnerData m_spawnerData;
    public bool testClear = false;
    public bool m_isStart = false;
	public AiData m_AiData;
    
    private float m_nextSpawnTime = 0f;
    private bool m_boss = false;
    private bool isClear = false;
}
