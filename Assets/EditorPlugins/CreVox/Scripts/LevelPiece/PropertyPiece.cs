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
		EnemySpawner = 3
	}

	public class PropertyPiece : LevelPiece 
	{
		[Serializable]
		public struct PProperty
		{
			public FocalComponent tComponent;
			public UnityEngine.Object tObject;
		}
		public PProperty[] PProperties = new PProperty[5];

		public override void SetupPiece(BlockItem item)
		{
			//解析從blockitem的attritube,進行相應的動作.
			for (int i = 0; i < 5; i++) {
				string[] _code = UpdateValue (ref item, i);
				if (_code.Length != 0) {
					switch (_code [0]) {
					case "AddLootActor":
						if (PProperties [i].tObject != null && PProperties [i].tObject is AddLootActor) {
							AddLootActor obj = (AddLootActor)PProperties [i].tObject;
							obj.m_lootID = int.Parse (_code [1]);
						}
						break;

					case "EnemySpawner":
						if (PProperties [i].tObject != null && PProperties [i].tObject is EnemySpawner) {
							EnemySpawner obj = (EnemySpawner)PProperties [i].tObject;
							obj.m_enemyType = (EnemyType)Enum.Parse (typeof(EnemyType), _code [1]);
							obj.m_spawnerData.m_totalQty = int.Parse (_code [2]);
							obj.m_spawnerData.m_maxLiveQty = int.Parse (_code [3]);
							obj.m_spawnerData.m_spwnCountPerTime = int.Parse (_code [4]);
							string[] _r = _code [5].Split (new string[1]{ "," }, StringSplitOptions.None);
							obj.m_spawnerData.m_randomSpawn = new Vector2 (float.Parse (_r [0]), float.Parse (_r [1]));
							if (obj.m_isStart == false)
								obj.m_isStart = true;
						}
						break;

					case "Unknown":
						if (PProperties [i].tObject != null) {

						}
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

		public string[] UpdateValue(ref BlockItem item, int id)
		{
			if (item.attributes [id] != null && item.attributes [id].Length > 0) {
				//將item中紀錄的字串取代pProperty中Component特定欄位的值
				return item.attributes [id].Split (new string[1]{ ";" }, StringSplitOptions.None);
			} else {
				return new string[0];
			}
		}
	}
}