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
		DefaultEventRange = 4,
		Transform = 5,
//		TriggerKeyStringOld = 6,
//		AddLootActorOld = 2,
//		MessageActorOld = 7,
//		AreaTipActorOld = 9,
//		CounterActorOld = 8,
//		EnemySpawnerOld = 3,
		TriggerKeyString = 100,
		ActorKeyString = 200,
		MessageActor = 201,
		AddLootActor = 202,
		AreaTipActor = 203,
		CounterActor = 204,
		EnemySpawner = 301,
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
					switch (t.Length) {
					case 1:
						t = new string[]{ "true", t [0], PProperties [0].tRange.ToString () };
						break;

					case 2:
						t = new string[]{ "true", t [0], t [1] };
						break;
					}
					if (t [0] == "true") {
						PProperties [i].tActive = true;
						switch (t [1]) {
						case "DefaultEventRange":
							eventRange = (LevelPiece.EventRange)Enum.Parse (typeof(LevelPiece.EventRange), t [2]);
							break;

						case "ActorKeyString":
							if (obj != null && obj is EventActor && _code.Length == 2) {
								EventActor ea = (EventActor)obj;
								PProperties [i].tRange = (LevelPiece.EventRange)Enum.Parse (typeof(LevelPiece.EventRange), t [2]);
								ea.m_keyString = _code [1];
							}
							break;

						case "TriggerKeyString":
							if (obj != null && obj is TriggerEvent && _code.Length == 2) {
								TriggerEvent te = (TriggerEvent)obj;
								PProperties [i].tRange = (LevelPiece.EventRange)Enum.Parse (typeof(LevelPiece.EventRange), t [2]);
								te.m_keyString = _code [1];
							}
							break;

						case "AddLootActor":
							if (obj != null && obj is AddLootActor && _code.Length == 3) {
								AddLootActor ala = (AddLootActor)obj;
								PProperties [i].tRange = (LevelPiece.EventRange)Enum.Parse (typeof(LevelPiece.EventRange), t [2]);
								ala.m_keyString = _code [1];
								ala.m_lootID = int.Parse (_code [2]);
							}
							break;

						case "CounterActor":
							if (obj != null && obj is CounterActor && _code.Length == 3) {
								CounterActor ca = (CounterActor)obj;
								PProperties [i].tRange = (LevelPiece.EventRange)Enum.Parse (typeof(LevelPiece.EventRange), t [2]);
								ca.m_keyString = _code [1];
								ca.m_activeCount = int.Parse (_code [2]);
							}
							break;

						case "MessageActor":
							if (obj != null && obj is MessageActor && _code.Length == 3) {
								MessageActor ma = (MessageActor)obj;
								PProperties [i].tRange = (LevelPiece.EventRange)Enum.Parse (typeof(LevelPiece.EventRange), t [2]);
								ma.m_keyString = _code [1];
								ma.m_message = _code [2];
							}
							break;

						case "AreaTipActor":
							if (obj != null && obj is AreaTipActor && _code.Length == 3) {
								AreaTipActor ma = (AreaTipActor)obj;
								PProperties [i].tRange = (LevelPiece.EventRange)Enum.Parse (typeof(LevelPiece.EventRange), t [2]);
								ma.m_keyString = _code [1];
								ma.m_stringKey = _code [2];
							}
							break;

						case "EnemySpawner":
							if (obj != null && obj is EnemySpawner && _code.Length > 5) {
								EnemySpawner es = (EnemySpawner)obj;
								PProperties [i].tRange = (LevelPiece.EventRange)Enum.Parse (typeof(LevelPiece.EventRange), t [2]);
								es.m_enemyType = (EnemyType)Enum.Parse (typeof(EnemyType), _code [1]);
								es.m_spawnerData.m_totalQty = int.Parse (_code [2]);
								es.m_spawnerData.m_maxLiveQty = int.Parse (_code [3]);
								es.m_spawnerData.m_spwnCountPerTime = int.Parse (_code [4]);
								string[] _r5 = _code [5].Split (new string[1]{ "," }, StringSplitOptions.None);
								es.m_spawnerData.m_randomSpawn = new Vector2 (float.Parse (_r5 [0]), float.Parse (_r5 [1]));
								if (_code.Length > 6) {
									string[] _r6 = _code [6].Split (new string[1]{ "," }, StringSplitOptions.None);
									es.m_AiData = ScriptableObject.CreateInstance (typeof(AiData)) as AiData;
									es.m_AiData.eye = int.Parse (_r6 [0]);
									es.m_AiData.ear = int.Parse (_r6 [1]);
								}
								if (_code.Length > 7) {
									string[] _r7 = _code [7].Split (new string[1]{ "," }, StringSplitOptions.None);
									string[] v3 = _r7[0].Split (new string[1]{ "_" }, StringSplitOptions.None);
									es.m_AiData.toggleOffset = new Vector3 (
										float.Parse (v3 [0]),
										float.Parse (v3 [1]),
										float.Parse (v3 [2])
									);
									es.m_AiData.toggle = float.Parse (v3 [3]);
									es.m_AiData.toggleOffsets = new Vector4[_r7.Length-1];
									for (int o = 0; o < es.m_AiData.toggleOffsets.Length; o++) {
										string[] v4 = _r7 [o + 1].Split (new string[1]{ "_" }, StringSplitOptions.None);
										es.m_AiData.toggleOffsets [o] = new Vector4 (
											float.Parse (v4 [0]),
											float.Parse (v4 [1]),
											float.Parse (v4 [2]),
											float.Parse (v4 [3])
										);	
									}
								}
								if (es.m_isStart == false)
									es.m_isStart = true;
							}
							break;

						case "Transform":
							if (obj != null && obj is GameObject && _code.Length == 3) {
								GameObject es = (GameObject)obj;
								PProperties [i].tRange = (LevelPiece.EventRange)Enum.Parse (typeof(LevelPiece.EventRange), t [2]);
								string[] _p = _code [1].Split (new string[1]{ "," }, StringSplitOptions.None);
								string[] _s = _code [2].Split (new string[1]{ "," }, StringSplitOptions.None);
								es.transform.localPosition = new Vector3 (float.Parse( _p[0]),float.Parse( _p[1]),float.Parse( _p[2]));
								es.transform.localScale = new Vector3 (float.Parse( _s[0]),float.Parse( _s[1]),float.Parse( _s[2]));
							}
							break;

						case "Unknown":
							if (obj != null) {

							}
							break;

						default:
							if (item.attributes [i].Length > 0)
								item.attributes [i] = "";
							break;
						}
					} else {
						PProperties [i].tActive = false;
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
						Debug.Log ("<b>" + this.name + "</b> send <b>[" + i + "] " + PProperties [i].tObject.name + "</b>" +
						"to <b>range(" + PProperties [i].tRange + ")</b>");
						SendActorUpward (a, PProperties [i].tRange);
					}
				}
				if (notPP) {
					Debug.Log ("<b>" + this.name + "</b> send <b> " + a.name + "</b>" +
					"to <b>range(" + eventRange + ")</b>");
					SendActorUpward (a, eventRange);
				}
			}
		}
		public void CheckAiData (EnemySpawner obj)
		{
			if (obj.m_AiData == null) {
				obj.m_AiData = ScriptableObject.CreateInstance (typeof(AiData)) as AiData;
				obj.m_AiData.name = this.gameObject.GetInstanceID ().ToString ();
				obj.m_AiData.toggle = 10;
				obj.m_AiData.eye = 10;
				obj.m_AiData.ear = 10;
			}
			if (obj.m_AiData.toggleOffsets == null) {
				obj.m_AiData.toggleOffsets = new Vector4[0];
			}
		}

		void OnDrawGizmos ()
		{
			if (!Application.isPlaying) {
				Matrix4x4 oldMatrix = Gizmos.matrix;
				Gizmos.color = Color.yellow;
				Gizmos.matrix = transform.localToWorldMatrix;
				for (int i = 0; i < 5; i++) {
					if (PProperties [i].tObject is EnemySpawner) {
						AiData data = ((EnemySpawner)PProperties [i].tObject).m_AiData;
						if (data != null) {
							Vector3 center = data.toggleOffset;
							float range = data.toggle;
							Gizmos.DrawWireSphere (center, range);
							if (data.toggleOffsets != null) {
								for (int o = 0; o < data.toggleOffsets.Length; o++) {
									center = new Vector3 (
										data.toggleOffsets [o].x, 
										data.toggleOffsets [o].y, 
										data.toggleOffsets [o].z
									);
									range = data.toggleOffsets [o].w;
									Gizmos.DrawWireSphere (center, range);
								}
							}
						}
					}
				}
				Gizmos.matrix = oldMatrix;
			}
		}
	}
}