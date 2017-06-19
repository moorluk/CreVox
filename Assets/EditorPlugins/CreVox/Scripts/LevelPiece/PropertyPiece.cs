using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using System.Runtime.InteropServices;
using System.Reflection;

namespace CreVox
{
	public enum FocalComponent
	{
		Unused = 0,
		Unknown = 1,
		AddLootActor = 2,
		EnemySpawner = 3,
		DefaultEventRange = 4
	}

	public class PropertyPiece : LevelPiece 
	{
		public override void SetupPiece(BlockItem item)
		{
			//解析從blockitem的attritube,進行相應的動作.
			for (int i = 0; i < 5; i++) {
				string[] _code = UpdateValue (ref item, i);
				UnityEngine.Object obj = PProperties [i].tObject;
				if (_code.Length != 0) {
					string[] t = _code [0].Split (new string[1]{ "," }, StringSplitOptions.None);
					if (t.Length < 2)
						t = new string[]{ t [0], PProperties [i].tRange.ToString() };
					switch (t [0]) {
					case "AddLootActor":
						if (obj != null && obj is AddLootActor) {
							AddLootActor ala = (AddLootActor)obj;
							PProperties[i].tRange = (LevelPiece.EventRange)Enum.Parse (typeof(LevelPiece.EventRange), t [1]);
							ala.m_lootID = int.Parse (_code [1]);
						}
						break;

					case "EnemySpawner":
						if (obj != null && obj is EnemySpawner) {
							EnemySpawner es = (EnemySpawner)obj;
							PProperties[i].tRange = (LevelPiece.EventRange)Enum.Parse (typeof(LevelPiece.EventRange), t [1]);
							es.m_enemyType = (EnemyType)Enum.Parse (typeof(EnemyType), _code [1]);
							es.m_spawnerData.m_totalQty = int.Parse (_code [2]);
							es.m_spawnerData.m_maxLiveQty = int.Parse (_code [3]);
							es.m_spawnerData.m_spwnCountPerTime = int.Parse (_code [4]);
							string[] _r = _code [5].Split (new string[1]{ "," }, StringSplitOptions.None);
							es.m_spawnerData.m_randomSpawn = new Vector2 (float.Parse (_r [0]), float.Parse (_r [1]));
							if (es.m_isStart == false)
								es.m_isStart = true;
						}
						break;

					case "Unknown":
						if (obj != null) {

						}
						break;

					case "DefaultEventRange":
						eventRange = (LevelPiece.EventRange)Enum.Parse (typeof(LevelPiece.EventRange), t [1]);
						break;

					default:
						if (item.attributes [i].Length > 0)
							item.attributes [i] = "";
						break;
					}
				}
			}
			//Actor register
			SendActorUpward ();
		}

		public override void SendActorUpward (EventGroup e = EventGroup.Default)
		{
			EventActor[] acs = GetComponentsInChildren<EventActor>();
			foreach (EventActor a in acs) {
				bool notPP = true;
				for (int i = 0; i < PProperties.Length; i++) {
					if (PProperties [i].tObject is EventActor && Equals (a, PProperties [i].tObject) && PProperties[i].tRange != EventRange.Free) {
						notPP = false;
						Debug.Log (this.name + ".[" + i + "]" + PProperties [i].tObject.name + ": " + eventRange + " >> " + PProperties [i].tRange);
						SendActorUpward (a, PProperties [i].tRange);
					}
				}
					if (notPP)
						SendActorUpward (a, eventRange);
			}
		}
	}
}