using UnityEngine;
using UnityEditor;
using System;
using System.Reflection;
using System.Collections.Generic;
using System.Collections;

namespace CreVox
{
	[CustomEditor(typeof(PropertyPiece))]
	public class PropertyPieceEditor : LevelPieceEditor
	{
		PropertyPiece pp;

		void OnEnable()
		{
			pp = (PropertyPiece)target;
		}
		#region InspectorGUI
		public override void OnInspectorGUI ()
		{
			pp = (PropertyPiece)target;
			Color def = GUI.color;
			EditorGUI.BeginChangeCheck ();

			EditorGUILayout.LabelField ("Event", EditorStyles.boldLabel);
			using (var h = new EditorGUILayout.HorizontalScope ("Box")) {
				pp.eventRange = (LevelPiece.EventRange)EditorGUILayout.EnumPopup ("Event Range", pp.eventRange);
			}
			EditorGUILayout.Separator ();

			EditorGUILayout.LabelField ("Modified Component", EditorStyles.boldLabel);
			for (int i = 0; i < pp.PProperties.Length; i++) {
				if (pp.PProperties [i].tComponent != FocalComponent.Unused)
					GUI.color = (pp.PProperties [i].tObject == null) ? Color.red : Color.green;
				using (var v = new EditorGUILayout.VerticalScope (EditorStyles.helpBox)) {
					GUI.color = def;
					EditorGUI.BeginChangeCheck ();
					pp.PProperties [i].tComponent = (FocalComponent)EditorGUILayout.EnumPopup (
						"Type", pp.PProperties [i].tComponent);
					if (EditorGUI.EndChangeCheck ()) {
						//清除既有att參數
						pp.PProperties [i].tObject = null;
					}
					//判斷tComponent類型並指定相應處理
					switch (pp.PProperties [i].tComponent) {
					case FocalComponent.ActorKeyString:
						DrawInsActorKeyString (i);
						break;

					case FocalComponent.TriggerKeyString:
						DrawInsTriggerKeyString (i);
						break;

					case FocalComponent.AddLootActor:
						DrawInsAddLootActor (i);
						break;

					case FocalComponent.CounterActor:
						DrawInsCounterActor (i);
						break;

					case FocalComponent.MessageActor:
						DrawInsMessageActor (i);
						break;

					case FocalComponent.AreaTipActor:
						DrawInsAreaTipActor (i);
						break;

					case FocalComponent.EnemySpawner:
						DrawInsEnemySpawner (i);
						break;

					case FocalComponent.Transform:
						DrawInsTransform (i);
						break;

					case FocalComponent.Unknown:
						DrawInsUnknown (i);
						break;

					case FocalComponent.DefaultEventRange:
						pp.PProperties [i].tObject = pp;
						EditorGUILayout.HelpBox ("Modify each item's Default Event Range", MessageType.Info, true);
						break;

					default:
						EditorGUILayout.HelpBox ("↗ Select a componennt Type...", MessageType.None, true);
						break;
					}
				}
			}
			if (EditorGUI.EndChangeCheck ())
				EditorUtility.SetDirty (pp);
		}

		private void DrawInsActorKeyString (int _index, bool isIns = true)
		{
			if (isIns) {
				pp.PProperties [_index].tObject = EditorGUILayout.ObjectField (
					"Target", pp.PProperties [_index].tObject, typeof(EventActor), true);
			}
			if (pp.PProperties [_index].tObject != null) {
				EventActor obj = (EventActor)pp.PProperties [_index].tObject;
				EditorGUILayout.LabelField ("Modifiable Field : ",
					"Key String　(" + obj.m_keyString + ")",
					EditorStyles.miniLabel);
			} else {
				DrawInsDragFirst ();
			}
		}
		private void DrawInsTriggerKeyString (int _index, bool isIns = true)
		{
			if (isIns) {
				pp.PProperties [_index].tObject = EditorGUILayout.ObjectField (
					"Target", pp.PProperties [_index].tObject, typeof(TriggerEvent), true);
			}
			if (pp.PProperties [_index].tObject != null) {
				TriggerEvent obj = (TriggerEvent)pp.PProperties [_index].tObject;
				EditorGUILayout.LabelField ("Modifiable Field : ",
					"Key String　(" + obj.m_keyString + ")",
					EditorStyles.miniLabel);
			} else {
				DrawInsDragFirst ();
			}
		}
		private void DrawInsAddLootActor (int _index, bool isIns = true)
		{
			if (isIns) {
				pp.PProperties [_index].tObject = EditorGUILayout.ObjectField (
					"Target", pp.PProperties [_index].tObject, typeof(AddLootActor), true);
			}
			if (pp.PProperties [_index].tObject != null) {
				AddLootActor obj = (AddLootActor)pp.PProperties [_index].tObject;
				EditorGUILayout.LabelField ("Modifiable Field : ",
					"Key String　(" + obj.m_keyString + ")\n" +
					"Loot ID　(" + obj.m_lootID.ToString () + ")",
					EditorStyles.miniLabel,
					GUILayout.Height (12 * 3));
			} else {
				DrawInsDragFirst ();
			}
		}
		private void DrawInsCounterActor (int _index, bool isIns = true)
		{
			if (isIns) {
				pp.PProperties [_index].tObject = EditorGUILayout.ObjectField (
					"Target", pp.PProperties [_index].tObject, typeof(CounterActor), true);
			}
			if (pp.PProperties [_index].tObject != null) {
				CounterActor obj = (CounterActor)pp.PProperties [_index].tObject;
				EditorGUILayout.LabelField ("Modifiable Field : ",
					"Key String　(" + obj.m_keyString + ")\n" +
					"Active Count　(" + obj.m_activeCount.ToString () + ")",
					EditorStyles.miniLabel,
					GUILayout.Height (12 * 3));
			} else {
				DrawInsDragFirst ();
			}
		}
		private void DrawInsMessageActor (int _index, bool isIns = true)
		{
			if (isIns) {
				pp.PProperties [_index].tObject = EditorGUILayout.ObjectField (
					"Target", pp.PProperties [_index].tObject, typeof(MessageActor), true);
			}
			if (pp.PProperties [_index].tObject != null) {
				MessageActor obj = (MessageActor)pp.PProperties [_index].tObject;
				EditorGUILayout.LabelField ("Modifiable Field : ",
					"Key String　(" + obj.m_keyString + ")\n" +
					"Loot ID　(" + obj.m_message + ")",
					EditorStyles.miniLabel,
					GUILayout.Height (12 * 3));
			} else {
				DrawInsDragFirst ();
			}
		}
		private void DrawInsAreaTipActor (int _index, bool isIns = true)
		{
			if (isIns) {
				pp.PProperties [_index].tObject = EditorGUILayout.ObjectField (
					"Target", pp.PProperties [_index].tObject, typeof(AreaTipActor), true);
			}
			if (pp.PProperties [_index].tObject != null) {
				AreaTipActor obj = (AreaTipActor)pp.PProperties [_index].tObject;
				EditorGUILayout.LabelField ("Modifiable Field : ",
					"Key String　(" + obj.m_keyString + ")\n" +
					"StringKey (" + obj.m_stringKey + ")",
					EditorStyles.miniLabel,
					GUILayout.Height (12 * 3));
			} else {
				DrawInsDragFirst ();
			}
		}
		private void DrawInsEnemySpawner (int _index, bool isIns = true)
		{
			if (isIns) {
				pp.PProperties [_index].tObject = EditorGUILayout.ObjectField (
					"Target", pp.PProperties [_index].tObject, typeof(EnemySpawner), true);
			}
			if (pp.PProperties [_index].tObject != null) {
				EnemySpawner obj = (EnemySpawner)pp.PProperties [_index].tObject;
				EditorGUILayout.LabelField ("Modifiable Field : ",
					"Enemy Type　(" + obj.m_enemyType.ToString () + ")\n" +
					"Spawner Data\n" +
					"　├─ Total Qty　(" + obj.m_spawnerData.m_totalQty.ToString () + ")\n" +
					"　├─ Max Live Qty　(" + obj.m_spawnerData.m_maxLiveQty.ToString () + ")\n" +
					"　├─ Spwn Count Per Time　(" + obj.m_spawnerData.m_spwnCountPerTime.ToString () + ")\n" +
					"　├─ Random Spawn X　(" + obj.m_spawnerData.m_randomSpawn.x.ToString () + ")\n" +
					"　└─ Random Spawn Y　(" + obj.m_spawnerData.m_randomSpawn.y.ToString () + ")",
					EditorStyles.miniLabel,
					GUILayout.Height (12 * 7));
				if(!Application.isPlaying)
				((EnemySpawner)pp.PProperties [_index].tObject).m_isStart = false;
			} else {
				DrawInsDragFirst ();
			}
		}
		private void DrawInsTransform (int _index, bool isIns = true)
		{
			if (isIns) {
				pp.PProperties [_index].tObject = EditorGUILayout.ObjectField (
					"Target", pp.PProperties [_index].tObject, typeof(GameObject), true);
			}
			if (pp.PProperties [_index].tObject != null) {
				GameObject obj = (GameObject)pp.PProperties [_index].tObject;
				EditorGUILayout.LabelField ("Modifiable Field : ",
					"Position　(" + obj.transform.localPosition.ToString () + ")\n" +
					"Scale   　(" + obj.transform.localScale.ToString () + ")",
					EditorStyles.miniLabel,
					GUILayout.Height (30));
			} else {
				DrawInsDragFirst ();
			}
		}
		private void DrawInsUnknown (int _index, bool isIns = true)
		{
			if (isIns) {
				pp.PProperties [_index].tObject = EditorGUILayout.ObjectField (
					"Target", pp.PProperties [_index].tObject, typeof(object), true);
			}
			if (pp.PProperties [_index].tObject != null) {
				EditorGUILayout.HelpBox ("實現夢想請洽二七...", MessageType.Warning, true);
			} else {
				DrawInsDragFirst ();
			}
		}
		private void DrawInsDragFirst()
		{
			EditorGUILayout.HelpBox ("↗ Drag a component into object field...", MessageType.None, true);
		}
		#endregion

		#region EditorGUI
        public override void OnEditorGUI(ref BlockItem item)
        {
			PropertyPiece pp = (PropertyPiece)target;
			Color def = GUI.contentColor;
			EditorGUI.BeginChangeCheck ();
			for (int i = 0; i < pp.PProperties.Length; i++) {
				EditorGUI.BeginDisabledGroup (pp.PProperties [i].tComponent == FocalComponent.Unused);
				using (var v = new EditorGUILayout.VerticalScope (EditorStyles.helpBox)) {
					pp.PProperties [i].tActive = EditorGUILayout.ToggleLeft (pp.PProperties [i].tComponent.ToString (),pp.PProperties [i].tActive, EditorStyles.boldLabel);
					if (pp.PProperties [i].tActive) {
						pp.PProperties [i].tRange = (LevelPiece.EventRange)EditorGUILayout.EnumPopup ("Event Range", pp.PProperties [i].tRange);
						switch (pp.PProperties [i].tComponent) {
						case FocalComponent.ActorKeyString:
							if (pp.PProperties [i].tObject != null) {
								EventActor obj = (EventActor)pp.PProperties [i].tObject;
								obj.m_keyString = EditorGUILayout.TextField ("Key String", obj.m_keyString);

								string _code = "true," + pp.PProperties [i].tComponent + "," + pp.PProperties [i].tRange + ";" +
								               obj.m_keyString;
								item.attributes [i] = _code;
							}
							break;

						case FocalComponent.TriggerKeyString:
							if (pp.PProperties [i].tObject != null) {
								TriggerEvent obj = (TriggerEvent)pp.PProperties [i].tObject;
								obj.m_keyString = EditorGUILayout.TextField ("Key String", obj.m_keyString);

								string _code = "true," + pp.PProperties [i].tComponent + "," + pp.PProperties [i].tRange + ";" +
								               obj.m_keyString;
								item.attributes [i] = _code;
							}
							break;

						case FocalComponent.AddLootActor:
							if (pp.PProperties [i].tObject != null) {
								AddLootActor obj = (AddLootActor)pp.PProperties [i].tObject;
								obj.m_keyString = EditorGUILayout.TextField ("Key String", obj.m_keyString);
								obj.m_lootID = EditorGUILayout.IntField ("Loot ID", obj.m_lootID);

								string _code = "true," + pp.PProperties [i].tComponent + "," + pp.PProperties [i].tRange + ";" +
								               obj.m_keyString + ";" +
								               obj.m_lootID.ToString ();
								item.attributes [i] = _code;
							}
							break;

						case FocalComponent.CounterActor:
							if (pp.PProperties [i].tObject != null) {
								CounterActor obj = (CounterActor)pp.PProperties [i].tObject;
								obj.m_keyString = EditorGUILayout.TextField ("Key String", obj.m_keyString);
								obj.m_activeCount = EditorGUILayout.IntField ("Loot ID", obj.m_activeCount);

								string _code = "true," + pp.PProperties [i].tComponent + "," + pp.PProperties [i].tRange + ";" +
									obj.m_keyString + ";" +
									obj.m_activeCount.ToString ();
								item.attributes [i] = _code;
							}
							break;

						case FocalComponent.MessageActor:
							if (pp.PProperties [i].tObject != null) {
								MessageActor obj = (MessageActor)pp.PProperties [i].tObject;
								obj.m_keyString = EditorGUILayout.TextField ("Key String", obj.m_keyString);
								obj.m_message = EditorGUILayout.TextField ("Message", obj.m_message);

								string _code = "true," + pp.PProperties [i].tComponent + "," + pp.PProperties [i].tRange + ";" +
									obj.m_keyString + ";" +
									obj.m_message;
								item.attributes [i] = _code;
							}
							break;

						case FocalComponent.AreaTipActor:
							if (pp.PProperties [i].tObject != null) {
								AreaTipActor obj = (AreaTipActor)pp.PProperties [i].tObject;
								obj.m_keyString = EditorGUILayout.TextField ("Key String", obj.m_keyString);
								obj.m_stringKey = EditorGUILayout.TextField ("String Key(Tip ID)", obj.m_stringKey);

								string _code = "true," + pp.PProperties [i].tComponent + "," + pp.PProperties [i].tRange + ";" +
									obj.m_keyString + ";" +
									obj.m_stringKey;
								item.attributes [i] = _code;
							}
							break;

						case FocalComponent.EnemySpawner:
							if (pp.PProperties [i].tObject != null) {
								EnemySpawner obj = (EnemySpawner)pp.PProperties [i].tObject;
								obj.m_enemyType = (EnemyType)EditorGUILayout.EnumPopup ("Enemy Type", obj.m_enemyType);
								EditorGUILayout.LabelField ("Spawner Data");
								EditorGUI.indentLevel++;
								obj.m_spawnerData.m_totalQty = EditorGUILayout.IntField ("Total Qty", obj.m_spawnerData.m_totalQty);
								obj.m_spawnerData.m_maxLiveQty = EditorGUILayout.IntField ("Max Live Qty", obj.m_spawnerData.m_maxLiveQty);
								obj.m_spawnerData.m_spwnCountPerTime = EditorGUILayout.IntField ("Spwn Count Per Time", obj.m_spawnerData.m_spwnCountPerTime);
								obj.m_spawnerData.m_randomSpawn = EditorGUILayout.Vector2Field ("Random Spawn", obj.m_spawnerData.m_randomSpawn);
								EditorGUI.indentLevel--;

								string _code = "true," + pp.PProperties [i].tComponent + "," + pp.PProperties [i].tRange + ";" +
								               obj.m_enemyType.ToString () + ";" +
								               obj.m_spawnerData.m_totalQty.ToString () + ";" +
								               obj.m_spawnerData.m_maxLiveQty.ToString () + ";" +
								               obj.m_spawnerData.m_spwnCountPerTime.ToString () + ";" +
								               obj.m_spawnerData.m_randomSpawn.x.ToString () + "," + obj.m_spawnerData.m_randomSpawn.y.ToString ();
								item.attributes [i] = _code;
							}
							break;

						case FocalComponent.Transform:
							if (pp.PProperties [i].tObject != null) {
								GameObject obj = (GameObject)pp.PProperties [i].tObject;
								obj.transform.localPosition = EditorGUILayout.Vector3Field ("Position", obj.transform.localPosition);
								obj.transform.localScale = EditorGUILayout.Vector3Field ("Scale", obj.transform.localScale);

								string _code = "true," + pp.PProperties [i].tComponent + "," + pp.PProperties [i].tRange + ";" +
									obj.transform.localPosition.x + "," + obj.transform.localPosition.y  + "," + obj.transform.localPosition.z  + ";" +
									obj.transform.localScale.x + ","  + obj.transform.localScale.y + "," + obj.transform.localScale.z ;
								item.attributes [i] = _code;
							}
							break;

						case FocalComponent.Unknown:
							if (pp.PProperties [i].tObject != null) {

							}
							break;

						case FocalComponent.DefaultEventRange:
							item.attributes [i] = "true," + pp.PProperties [i].tComponent + "," + pp.PProperties [i].tRange;
							break;

						default:
							if (item.attributes [i].Length > 0)
								item.attributes [i] = "";
							break;
						}
						EditorGUILayout.LabelField (item.attributes [i], EditorStyles.miniTextField);
					} else {
						item.attributes [i] = "false," + pp.PProperties [i].tComponent + "," + pp.PProperties [i].tRange;
						switch (pp.PProperties [i].tComponent) {
						case FocalComponent.ActorKeyString:
							DrawInsActorKeyString (i,false);
							break;

						case FocalComponent.TriggerKeyString:
							DrawInsTriggerKeyString (i,false);
							break;

						case FocalComponent.AddLootActor:
							DrawInsAddLootActor (i,false);
							break;

						case FocalComponent.CounterActor:
							DrawInsCounterActor (i,false);
							break;

						case FocalComponent.MessageActor:
							DrawInsMessageActor (i,false);
							break;

						case FocalComponent.AreaTipActor:
							DrawInsAreaTipActor (i,false);
							break;

						case FocalComponent.EnemySpawner:
							DrawInsEnemySpawner (i,false);
							break;

						case FocalComponent.Unknown:
							DrawInsUnknown (i,false);
							break;

						case FocalComponent.DefaultEventRange:
							pp.PProperties [i].tObject = pp;
							EditorGUILayout.HelpBox ("Modify each item's Default Event Range", MessageType.Info, true);
							break;

						default:
							break;
						}
					}
				}
				EditorGUI.EndDisabledGroup ();
			}
			if (EditorGUI.EndChangeCheck ())
				EditorUtility.SetDirty (pp);
        }
		#endregion
    }
}
