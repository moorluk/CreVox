using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class AddLootActor : EventActor 
{
    public int m_lootID;
    public int m_lootCount = 1;
    private List<LootResult> m_lrList = new List<LootResult>();
}
public struct LootResult
{
}
