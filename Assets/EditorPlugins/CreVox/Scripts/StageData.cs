using UnityEngine;
using System.Collections.Generic;
using CreVox;

[System.Serializable]
public class DList
{
    public string Name = "";
    public List<Dungeon> Dlist = new List<Dungeon> ();
}

[CreateAssetMenu (menuName = "CreVox/Stage Data")]
public class StageData : ScriptableObject
{
    public List<DList> stageList = new List<DList>();
}

