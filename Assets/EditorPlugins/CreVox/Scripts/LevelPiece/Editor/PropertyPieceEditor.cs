using UnityEngine;
using UnityEditor;

namespace CreVox
{
    [CustomEditor (typeof(PropertyPiece))]
    public class PropertyPieceEditor : LevelPieceEditor
    {
        PropertyPiece pp;

        void OnEnable ()
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
                    case FocalComponent.Unknown:
                        DrawInsUnknown (i);
                        break;

                    case FocalComponent.Probability:
                        pp.PProperties [i].tObject = pp;
                        EditorGUILayout.HelpBox ("Control Marker's Active Probability", MessageType.Info, true);
                        break;

                    case FocalComponent.DefaultEventRange:
                        pp.PProperties [i].tObject = pp;
                        EditorGUILayout.HelpBox ("Modify each item's Default Event Range", MessageType.Info, true);
                        break;

                    case FocalComponent.Transform:
                        DrawInsTransform (i);
                        break;

                    case FocalComponent.TriggerKeyString:
                        DrawInsTriggerKeyString (i);
                        break;

                    case FocalComponent.ActorKeyString:
                        DrawInsActorKeyString (i);
                        break;

                    case FocalComponent.MessageActor:
                        DrawInsMessageActor (i);
                        break;

                    case FocalComponent.AddLootActor:
                        DrawInsAddLootActor (i);
                        break;

                    case FocalComponent.AreaTipActor:
                        DrawInsAreaTipActor (i);
                        break;

                    case FocalComponent.CounterActor:
                        DrawInsCounterActor (i);
                        break;

                    case FocalComponent.EnemySpawner:
                        DrawInsEnemySpawner (i);
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

        void DrawInsUnknown (int _index, bool isIns = true)
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

        void DrawInsTransform (int _index, bool isIns = true)
        {
            if (isIns) {
                pp.PProperties [_index].tObject = EditorGUILayout.ObjectField (
                    "Target", pp.PProperties [_index].tObject, typeof(GameObject), true);
            }
            if (pp.PProperties [_index].tObject != null) {
                GameObject obj = (GameObject)pp.PProperties [_index].tObject;
                EditorGUILayout.LabelField ("Modifiable Field : ",
                    "Position　(" + obj.transform.localPosition + ")\n" +
                    "Scale   　(" + obj.transform.localScale + ")",
                    EditorStyles.miniLabel,
                    GUILayout.Height (30));
            } else {
                DrawInsDragFirst ();
            }
        }

        void DrawInsTriggerKeyString (int _index, bool isIns = true)
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

        void DrawInsActorKeyString (int _index, bool isIns = true)
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

        void DrawInsMessageActor (int _index, bool isIns = true)
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

        void DrawInsAddLootActor (int _index, bool isIns = true)
        {
            if (isIns) {
                pp.PProperties [_index].tObject = EditorGUILayout.ObjectField (
                    "Target", pp.PProperties [_index].tObject, typeof(AddLootActor), true);
            }
            if (pp.PProperties [_index].tObject != null) {
                AddLootActor obj = (AddLootActor)pp.PProperties [_index].tObject;
                EditorGUILayout.LabelField ("Modifiable Field : ",
                    "Key String　(" + obj.m_keyString + ")\n" +
                    "Loot ID　(" + obj.m_lootID + ")",
                    EditorStyles.miniLabel,
                    GUILayout.Height (12 * 3));
            } else {
                DrawInsDragFirst ();
            }
        }

        void DrawInsAreaTipActor (int _index, bool isIns = true)
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

        void DrawInsCounterActor (int _index, bool isIns = true)
        {
            if (isIns) {
                pp.PProperties [_index].tObject = EditorGUILayout.ObjectField (
                    "Target", pp.PProperties [_index].tObject, typeof(CounterActor), true);
            }
            if (pp.PProperties [_index].tObject != null) {
                CounterActor obj = (CounterActor)pp.PProperties [_index].tObject;
                EditorGUILayout.LabelField ("Modifiable Field : ",
                    "Key String　(" + obj.m_keyString + ")\n" +
                    "Active Count　(" + obj.m_activeCount + ")",
                    EditorStyles.miniLabel,
                    GUILayout.Height (12 * 3));
            } else {
                DrawInsDragFirst ();
            }
        }

        void DrawInsEnemySpawner (int _index, bool isIns = true)
        {
            if (isIns) {
                pp.PProperties [_index].tObject = EditorGUILayout.ObjectField (
                    "Target", pp.PProperties [_index].tObject, typeof(EnemySpawner), true);
            }
            if (pp.PProperties [_index].tObject != null) {
                EnemySpawner obj = (EnemySpawner)pp.PProperties [_index].tObject;
                pp.CheckAiData (obj);
                EditorGUILayout.LabelField ("Modifiable Field : ",
                    "Enemy ID  (" + obj.m_enemyId + ")\n" +
                    "Enemy Type　(" + obj.m_enemyType + ")\n" +
                    "Spawner Data\n" +
                    "　├─ Total Qty　(" + obj.m_spawnerData.m_totalQty + ")\n" +
                    "　├─ Max Live Qty　(" + obj.m_spawnerData.m_maxLiveQty + ")\n" +
                    "　├─ Spwn Count Per Time　(" + obj.m_spawnerData.m_spwnCountPerTime + ")\n" +
                    "　├─ Random Spawn X　(" + obj.m_spawnerData.m_randomSpawn.x + ")\n" +
                    "　└─ Random Spawn Y　(" + obj.m_spawnerData.m_randomSpawn.y + ")\n" +
                    "AI Data\n" +
                    "　├─ Toggle　(" + obj.m_AiData.toggle + ")\n" +
                    "　├─ Eye　(" + obj.m_AiData.eye + ")\n" +
                    "　├─ Ear　(" + obj.m_AiData.ear + ")\n" +
                    "　└─ Toggle Offsets　(" + obj.m_AiData.toggleOffsets.Length + ")\n" +
                    "　　　└─ Offsets　(x,y,z,w(range))\n" +
                    "Patrol Points\u3000(x,y,z)",
                    EditorStyles.miniLabel,
                    GUILayout.Height (170));
                if (!Application.isPlaying)
                    ((EnemySpawner)pp.PProperties [_index].tObject).m_isStart = false;
            } else {
                DrawInsDragFirst ();
            }
        }

        static void DrawInsDragFirst ()
        {
            EditorGUILayout.HelpBox ("↗ Drag a component into object field...", MessageType.None, true);
        }

        #endregion

        #region EditorGUI

        public override void OnEditorGUI (ref BlockItem item)
        {
            PropertyPiece _pp = (PropertyPiece)target;
            Color def = GUI.contentColor;
            EditorGUI.BeginChangeCheck ();
            for (int i = 0; i < _pp.PProperties.Length; i++) {
                EditorGUI.BeginDisabledGroup (_pp.PProperties [i].tComponent == FocalComponent.Unused);
                using (var v = new EditorGUILayout.VerticalScope (EditorStyles.helpBox)) {
                    _pp.PProperties [i].tActive = EditorGUILayout.ToggleLeft (_pp.PProperties [i].tComponent.ToString (), _pp.PProperties [i].tActive, EditorStyles.boldLabel);
                    if (_pp.PProperties [i].tActive) {
                        _pp.PProperties [i].tRange = (LevelPiece.EventRange)EditorGUILayout.EnumPopup ("Event Range", _pp.PProperties [i].tRange);
                        switch (_pp.PProperties [i].tComponent) {
                        case FocalComponent.Unknown:
                            if (_pp.PProperties [i].tObject != null) {

                            }
                            break;

                        case FocalComponent.Probability:
                            _pp.probability = EditorGUILayout.Slider (_pp.probability, 0, 1.0f);
                            item.attributes [i] = "true," + _pp.PProperties [i].tComponent + "," + _pp.probability;
                            break;

                        case FocalComponent.DefaultEventRange:
                            item.attributes [i] = "true," + _pp.PProperties [i].tComponent + "," + _pp.PProperties [i].tRange;
                            break;

                        case FocalComponent.Transform:
                            if (_pp.PProperties [i].tObject != null) {
                                GameObject obj = (GameObject)_pp.PProperties [i].tObject;
                                obj.transform.localPosition = EditorGUILayout.Vector3Field ("Position", obj.transform.localPosition);
                                obj.transform.localScale = EditorGUILayout.Vector3Field ("Scale", obj.transform.localScale);

                                string _code = "true," + _pp.PProperties [i].tComponent + "," + _pp.PProperties [i].tRange + ";" +
                                               obj.transform.localPosition.x + "," + obj.transform.localPosition.y + "," + obj.transform.localPosition.z + ";" +
                                               obj.transform.localScale.x + "," + obj.transform.localScale.y + "," + obj.transform.localScale.z;
                                item.attributes [i] = _code;
                            }
                            break;

                        case FocalComponent.TriggerKeyString:
                            if (_pp.PProperties [i].tObject != null) {
                                TriggerEvent obj = (TriggerEvent)_pp.PProperties [i].tObject;
                                obj.m_keyString = EditorGUILayout.TextField ("Key String", obj.m_keyString);

                                string _code = "true," + _pp.PProperties [i].tComponent + "," + _pp.PProperties [i].tRange + ";" +
                                               obj.m_keyString;
                                item.attributes [i] = _code;
                            }
                            break;

                        case FocalComponent.ActorKeyString:
                            if (_pp.PProperties [i].tObject != null) {
                                EventActor obj = (EventActor)_pp.PProperties [i].tObject;
                                obj.m_keyString = EditorGUILayout.TextField ("Key String", obj.m_keyString);

                                string _code = "true," + _pp.PProperties [i].tComponent + "," + _pp.PProperties [i].tRange + ";" +
                                               obj.m_keyString;
                                item.attributes [i] = _code;
                            }
                            break;

                        case FocalComponent.MessageActor:
                            if (_pp.PProperties [i].tObject != null) {
                                MessageActor obj = (MessageActor)_pp.PProperties [i].tObject;
                                obj.m_keyString = EditorGUILayout.TextField ("Key String", obj.m_keyString);
                                obj.m_message = EditorGUILayout.TextField ("Message", obj.m_message);

                                string _code = "true," + _pp.PProperties [i].tComponent + "," + _pp.PProperties [i].tRange + ";" +
                                               obj.m_keyString + ";" +
                                               obj.m_message;
                                item.attributes [i] = _code;
                            }
                            break;

                        case FocalComponent.AddLootActor:
                            if (_pp.PProperties [i].tObject != null) {
                                AddLootActor obj = (AddLootActor)_pp.PProperties [i].tObject;
                                obj.m_keyString = EditorGUILayout.TextField ("Key String", obj.m_keyString);
                                obj.m_lootID = EditorGUILayout.DelayedIntField ("Loot ID", obj.m_lootID);

                                string _code = "true," + _pp.PProperties [i].tComponent + "," + _pp.PProperties [i].tRange + ";" +
                                               obj.m_keyString + ";" +
                                               obj.m_lootID;
                                item.attributes [i] = _code;
                            }
                            break;

                        case FocalComponent.AreaTipActor:
                            if (_pp.PProperties [i].tObject != null) {
                                AreaTipActor obj = (AreaTipActor)_pp.PProperties [i].tObject;
                                obj.m_keyString = EditorGUILayout.TextField ("Key String", obj.m_keyString);
                                obj.m_stringKey = EditorGUILayout.TextField ("String Key(Tip ID)", obj.m_stringKey);

                                string _code = "true," + _pp.PProperties [i].tComponent + "," + _pp.PProperties [i].tRange + ";" +
                                               obj.m_keyString + ";" +
                                               obj.m_stringKey;
                                item.attributes [i] = _code;
                            }
                            break;

                        case FocalComponent.CounterActor:
                            if (_pp.PProperties [i].tObject != null) {
                                CounterActor obj = (CounterActor)_pp.PProperties [i].tObject;
                                obj.m_keyString = EditorGUILayout.TextField ("Key String", obj.m_keyString);
                                obj.m_activeCount = EditorGUILayout.DelayedIntField ("Count", obj.m_activeCount);

                                string _code = "true," + _pp.PProperties [i].tComponent + "," + _pp.PProperties [i].tRange + ";" +
                                               obj.m_keyString + ";" +
                                               obj.m_activeCount;
                                item.attributes [i] = _code;
                            }
                            break;

                        case FocalComponent.EnemySpawner:
                            if (_pp.PProperties [i].tObject != null) {
                                EnemySpawner obj = (EnemySpawner)_pp.PProperties [i].tObject;
                                _pp.CheckAiData (obj);
                                AiData _ai = obj.m_AiData;
                                //_code 0
                                string _code = "true," + _pp.PProperties [i].tComponent + "," + _pp.PProperties [i].tRange + ";"; 

                                //_code 1
                                obj.m_enemyId = EditorGUILayout.DelayedIntField ("Enemy Id", obj.m_enemyId);
                                obj.m_enemyType = (EnemyType)EditorGUILayout.EnumPopup ("Enemy Type", obj.m_enemyType);
                                _code += obj.m_enemyId + "," + obj.m_enemyType + ";";

                                //_code (2 ~ 5) spawnerData
                                EditorGUILayout.LabelField ("Spawner Data");
                                EditorGUI.indentLevel++;

                                //_code 2
                                obj.m_spawnerData.m_totalQty = EditorGUILayout.DelayedIntField ("Total Qty", obj.m_spawnerData.m_totalQty);
                                _code += obj.m_spawnerData.m_totalQty + ";";

                                //_code 3
                                obj.m_spawnerData.m_maxLiveQty = EditorGUILayout.DelayedIntField ("Max Live Qty", obj.m_spawnerData.m_maxLiveQty);
                                _code += obj.m_spawnerData.m_maxLiveQty + ";";

                                //_code 4
                                obj.m_spawnerData.m_spwnCountPerTime = EditorGUILayout.DelayedIntField ("Spawn Count", obj.m_spawnerData.m_spwnCountPerTime);
                                _code += obj.m_spawnerData.m_spwnCountPerTime + ";";

                                //_code 5
                                obj.m_spawnerData.m_randomSpawn = EditorGUILayout.Vector2Field ("Random Spawn", obj.m_spawnerData.m_randomSpawn);
                                _code += obj.m_spawnerData.m_randomSpawn.x + "," + obj.m_spawnerData.m_randomSpawn.y + ";";

                                EditorGUI.indentLevel--;

                                //_code (6 ~ 7) AiData
                                EditorGUILayout.LabelField ("AI Data");
                                EditorGUI.indentLevel++;

                                //_code 6
                                _ai.eye = EditorGUILayout.DelayedFloatField ("Eye Range", _ai.eye);
                                _ai.ear = EditorGUILayout.DelayedFloatField ("Ear Range", _ai.ear);
                                _code += _ai.eye + "," + _ai.ear + ";";

                                //_code 7
                                using (var ch = new EditorGUI.ChangeCheckScope ()) {
                                    Vector4 t = new Vector4 (_ai.toggleOffset.x, _ai.toggleOffset.y, _ai.toggleOffset.z, _ai.toggle);
                                    EditorGUILayout.LabelField ("Toggle");
                                    using (var h = new GUILayout.HorizontalScope ()) {
                                        EditorGUILayout.LabelField ("", GUILayout.Width (30));
                                        t = EditorGUILayout.Vector4Field ("", t);
                                    }
                                    if (ch.changed) {
                                        _ai.toggleOffset = new Vector3 (t.x, t.y, t.z);
                                        _ai.toggle = t.w;
                                    }
                                }
                                _code += _ai.toggleOffset.x + "_" + _ai.toggleOffset.y + "_" + _ai.toggleOffset.z + "_" + _ai.toggle;

                                using (var ch = new EditorGUI.ChangeCheckScope ()) {
                                    int oCount = _ai.toggleOffsets.Length;
                                    oCount = EditorGUILayout.IntSlider ("Extra toggle", oCount, 0, 10);
                                    if (ch.changed && oCount != _ai.toggleOffsets.Length) {
                                        Vector4[] newOffsets = new Vector4[oCount];
                                        for (int o = 0; o < newOffsets.Length; o++)
                                            newOffsets [o] = (o < _ai.toggleOffsets.Length) ? _ai.toggleOffsets [o] : new Vector4 (0.0f, o + 1, 0.0f, 1.0f);
                                        _ai.toggleOffsets = newOffsets;
                                    }
                                }
                                for (int o = 0; o < _ai.toggleOffsets.Length; o++) {
                                    using (var h = new GUILayout.HorizontalScope ()) {
                                        EditorGUILayout.LabelField (o.ToString (), GUILayout.Width (30));
                                        _ai.toggleOffsets [o] = EditorGUILayout.Vector4Field ("", _ai.toggleOffsets [o]);
                                    }
                                    Vector4 v4 = _ai.toggleOffsets [o];
                                    _code += "," + v4.x + "_" + v4.y + "_" + v4.z + "_" + v4.w;
                                }
                                _code += ";";

                                EditorGUI.indentLevel--;

                                //_code 8 Patrol Points
                                using (var ch = new EditorGUI.ChangeCheckScope ()) {
                                    int pCount = obj.m_patrolPoints.Length;
                                    pCount = EditorGUILayout.IntSlider ("Patrol Points", pCount, 1, 20);
                                    if (ch.changed) {
                                        Vector3[] newOffsets = new Vector3[pCount];
                                        for (int p = 0; p < newOffsets.Length; p++)
                                            newOffsets [p] = (p < obj.m_patrolPoints.Length) ? obj.m_patrolPoints [p] : new Vector3 (0.0f, 0.0f, p * 2);
                                        obj.m_patrolPoints = newOffsets;
                                    }
                                }
                                for (int p = 0; p < obj.m_patrolPoints.Length; p++) {
                                    using (var h = new GUILayout.HorizontalScope ()) {
                                        using (var d = new EditorGUI.DisabledGroupScope (p < 1)) {
                                            EditorGUILayout.LabelField (p.ToString (), GUILayout.Width (45));
                                            obj.m_patrolPoints [p] = EditorGUILayout.Vector3Field (GUIContent.none, obj.m_patrolPoints [p]);
                                        }
                                    }
                                    Vector3 v3 = obj.m_patrolPoints [p];
                                    if (p > 0)
                                        _code += ",";
                                    _code += v3.x + "_" + v3.y + "_" + v3.z;
                                }
                                _code += ";";

                                item.attributes [i] = _code;
                            }
                            break;

                        default:
                            if (item.attributes [i].Length > 0)
                                item.attributes [i] = "";
                            break;
                        }
                        EditorGUILayout.LabelField (item.attributes [i], EditorStyles.miniTextField);
                    } else {
                        item.attributes [i] = "false," + _pp.PProperties [i].tComponent + "," + _pp.PProperties [i].tRange;
                        switch (_pp.PProperties [i].tComponent) {
                        case FocalComponent.Unknown:
                            DrawInsUnknown (i, false);
                            break;

                        case FocalComponent.Probability:
                            _pp.PProperties [i].tObject = _pp;
                            EditorGUILayout.HelpBox ("Control Marker's Active Probability", MessageType.Info, true);
                            break;

                        case FocalComponent.DefaultEventRange:
                            _pp.PProperties [i].tObject = _pp;
                            EditorGUILayout.HelpBox ("Modify each item's Default Event Range", MessageType.Info, true);
                            break;

                        case FocalComponent.Transform:
                            DrawInsTransform (i, false);
                            break;

                        case FocalComponent.TriggerKeyString:
                            DrawInsTriggerKeyString (i, false);
                            break;

                        case FocalComponent.ActorKeyString:
                            DrawInsActorKeyString (i, false);
                            break;

                        case FocalComponent.MessageActor:
                            DrawInsMessageActor (i, false);
                            break;

                        case FocalComponent.AddLootActor:
                            DrawInsAddLootActor (i, false);
                            break;

                        case FocalComponent.AreaTipActor:
                            DrawInsAreaTipActor (i, false);
                            break;

                        case FocalComponent.CounterActor:
                            DrawInsCounterActor (i, false);
                            break;

                        case FocalComponent.EnemySpawner:
                            DrawInsEnemySpawner (i, false);
                            break;

                        default:
                            break;
                        }
                    }
                }
                EditorGUI.EndDisabledGroup ();
            }
            if (EditorGUI.EndChangeCheck ())
                EditorUtility.SetDirty (_pp);
        }

        #endregion
    }
}
