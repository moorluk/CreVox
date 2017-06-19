using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

namespace CreVox
{
	public enum FocalComponent
	{
		Unused = 0,
		Unknown = 1,
		AddLootActor = 2,
		EnemySpawner = 3,
		DefaultEventRange = 4,
		ActorKeyString = 5,
		TriggerKeyString = 6
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
					case "ActorKeyString":
						if (obj != null && obj is EventActor && _code.Length == 2) {
							EventActor ea = (EventActor)obj;
							PProperties[i].tRange = (LevelPiece.EventRange)Enum.Parse (typeof(LevelPiece.EventRange), t [1]);
							ea.m_keyString = _code [1];
						}
						break;

					case "TriggerKeyString":
						if (obj != null && obj is TriggerEvent && _code.Length == 2) {
							TriggerEvent te = (TriggerEvent)obj;
							PProperties[i].tRange = (LevelPiece.EventRange)Enum.Parse (typeof(LevelPiece.EventRange), t [1]);
							te.m_keyString = _code [1];
						}
						break;

					case "AddLootActor":
						if (obj != null && obj is AddLootActor && _code.Length == 3) {
							AddLootActor ala = (AddLootActor)obj;
							PProperties[i].tRange = (LevelPiece.EventRange)Enum.Parse (typeof(LevelPiece.EventRange), t [1]);
							ala.m_keyString = _code [1];
							ala.m_lootID = int.Parse (_code [2]);
						}
						break;

					case "EnemySpawner":
						if (obj != null && obj is EnemySpawner && _code.Length == 6) {
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
			EventActor[] acs = GetComponentsInChildren<EventActor> ();
			foreach (EventActor a in acs) {
				bool notPP = true;
				for (int i = 0; i < PProperties.Length; i++) {
					if (PProperties [i].tObject is EventActor && Equals (a, PProperties [i].tObject) && PProperties [i].tRange != EventRange.Free) {
						notPP = false;
						Debug.Log ("<b>" + this.name + "</b> send <b>[" + i + "] " + PProperties [i].tObject.name + "</b>\n" +
						"to <b>range(" + PProperties [i].tRange + ")</b>");
						SendActorUpward (a, PProperties [i].tRange);
					}
				}
				if (notPP) {
					Debug.Log ("<b>" + this.name + "</b> send <b> " + a.name + "</b>\n" +
					"to <b>range(" + eventRange + ")</b>");
					SendActorUpward (a, eventRange);
				}
			}
		}
	}
}