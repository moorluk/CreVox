﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

namespace CreVox
{
    public enum FocalComponent
    {
        Unused = 0,
        Unknown = 1,
        Probability = 2,
        DefaultEventRange = 4,
        Transform = 5,
        //AddLootActorOld = 2,
        //EnemySpawnerOld = 3,
        //TriggerKeyStringOld = 6,
        //MessageActorOld = 7,
        //AreaTipActorOld = 9,
        //CounterActorOld = 8,
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
        public float probability;

        public override void SetupPiece (BlockItem item)
        {
            //解析從blockitem的attribute,進行相應的動作.
            for (int i = 0; i < 5; i++) {
                string[] _code = UpdateValue (ref item, i);
                UnityEngine.Object obj = PProperties [i].tObject;
                if (_code.Length != 0) {
                    string[] t = _code [0].Split (new string[1]{ "," }, StringSplitOptions.None);
                    //fix old saved attrittube
                    switch (t.Length) {
                    case 1:
                        t = new string[] {
                            "true",
                            t [0],
                            PProperties [0].tRange.ToString ()
                        };
                        break;

                    case 2:
                        t = new string[]{ "true", t [0], t [1] };
                        break;
                    }

                    if (t [0] == "true") {
                        PProperties [i].tActive = true;
                        switch (t [1]) {
                        case "Unknown":
                            if (obj != null) {

                            }
                            break;

                        case "Probability":
                            probability = float.Parse (t [2]);
                            if (!(UnityEngine.Random.value > probability))
                                this.gameObject.SetActive (false);
                            break;

                        case "DefaultEventRange":
                            eventRange = (LevelPiece.EventRange)Enum.Parse (typeof(LevelPiece.EventRange), t [2]);
                            break;

                        case "Transform":
                            if (obj != null && obj is GameObject && _code.Length == 3) {
                                GameObject es = (GameObject)obj;
                                PProperties [i].tRange = (LevelPiece.EventRange)Enum.Parse (typeof(LevelPiece.EventRange), t [2]);
                                string[] _p = _code [1].Split (new string[1]{ "," }, StringSplitOptions.None);
                                string[] _s = _code [2].Split (new string[1]{ "," }, StringSplitOptions.None);
                                es.transform.localPosition = new Vector3 (float.Parse (_p [0]), float.Parse (_p [1]), float.Parse (_p [2]));
                                es.transform.localScale = new Vector3 (float.Parse (_s [0]), float.Parse (_s [1]), float.Parse (_s [2]));
                            }
                            break;

                        case "TriggerKeyString":
                            if (obj != null && obj is TriggerEvent && _code.Length == 2) {
                                TriggerEvent te = (TriggerEvent)obj;
                                PProperties [i].tRange = (LevelPiece.EventRange)Enum.Parse (typeof(LevelPiece.EventRange), t [2]);
                                te.m_keyString = _code [1];
                            }
                            break;

                        case "ActorKeyString":
                            if (obj != null && obj is EventActor && _code.Length == 2) {
                                EventActor ea = (EventActor)obj;
                                PProperties [i].tRange = (LevelPiece.EventRange)Enum.Parse (typeof(LevelPiece.EventRange), t [2]);
                                ea.m_keyString = _code [1];
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

                        case "AddLootActor":
                            if (obj != null && obj is AddLootActor && _code.Length == 3) {
                                AddLootActor ala = (AddLootActor)obj;
                                PProperties [i].tRange = (LevelPiece.EventRange)Enum.Parse (typeof(LevelPiece.EventRange), t [2]);
                                ala.m_keyString = _code [1];
                                ala.m_lootID = int.Parse (_code [2]);
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

                        case "CounterActor":
                            if (obj != null && obj is CounterActor && _code.Length == 3) {
                                CounterActor ca = (CounterActor)obj;
                                PProperties [i].tRange = (LevelPiece.EventRange)Enum.Parse (typeof(LevelPiece.EventRange), t [2]);
                                ca.m_keyString = _code [1];
                                ca.m_activeCount = int.Parse (_code [2]);
                            }
                            break;

                        case "EnemySpawner":
                            if (obj != null && obj is EnemySpawner) {
                                EnemySpawner es = (EnemySpawner)obj;
                                PProperties [i].tRange = (LevelPiece.EventRange)Enum.Parse (typeof(LevelPiece.EventRange), t [2]);
                                es.m_enemyType = (EnemyType)Enum.Parse (typeof(EnemyType), _code [1]);
                                // support spawnerData
                                if (_code.Length > 5) {
                                    es.m_spawnerData.m_totalQty = int.Parse (_code [2]);
                                    es.m_spawnerData.m_maxLiveQty = int.Parse (_code [3]);
                                    es.m_spawnerData.m_spwnCountPerTime = int.Parse (_code [4]);
                                    string[] _r5 = _code [5].Split (new string[1]{ "," }, StringSplitOptions.None);
                                    es.m_spawnerData.m_randomSpawn = new Vector2 (float.Parse (_r5 [0]), float.Parse (_r5 [1]));
                                }
                                // support AiData.basicSetting
                                if (_code.Length > 6) {
                                    string[] _r6 = _code [6].Split (new string[1]{ "," }, StringSplitOptions.None);
                                    es.m_AiData = ScriptableObject.CreateInstance (typeof(AiData)) as AiData;
                                    es.m_AiData.eye = int.Parse (_r6 [0]);
                                    es.m_AiData.ear = int.Parse (_r6 [1]);
                                }
                                // support AiData.ActiveRange[]
                                if (_code.Length > 7) {
                                    string[] _r7 = _code [7].Split (new string[1]{ "," }, StringSplitOptions.None);
                                    string[] v3 = _r7 [0].Split (new string[1]{ "_" }, StringSplitOptions.None);
                                    es.m_AiData.toggleOffset = new Vector3 (float.Parse (v3 [0]), float.Parse (v3 [1]), float.Parse (v3 [2]));
                                    es.m_AiData.toggle = float.Parse (v3 [3]);
                                    es.m_AiData.toggleOffsets = new Vector4[_r7.Length - 1];
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
                                // support patrolPoints[]
                                if (_code.Length > 8) {
                                    string[] _r8 = _code [8].Split (new string[1]{ "," }, StringSplitOptions.None);
                                    es.m_patrolPoints = new Vector3[_r8.Length];
                                    for (int p = 0; p < es.m_patrolPoints.Length; p++) {
                                        string[] v3 = _r8 [p].Split (new string[1]{ "_" }, StringSplitOptions.None);
                                        if (v3.Length == 3)
                                            es.m_patrolPoints [p] = new Vector3 (
                                                float.Parse (v3 [0]),
                                                float.Parse (v3 [1]),
                                                float.Parse (v3 [2])
                                            );
                                    }
                                }
                                if (es.m_isStart == false)
                                    es.m_isStart = true;
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

        #if UNITY_EDITOR
        public void DrawPatrolPoints ()
        {
            Matrix4x4 defMatrix = UnityEditor.Handles.matrix;
            UnityEditor.Handles.matrix = transform.localToWorldMatrix;
            UnityEditor.Handles.color = Color.green;
            for (int i = 0; i < 5; i++) {
                if (PProperties [i].tObject is EnemySpawner) {
                    Vector3[] pPoints = ((EnemySpawner)PProperties [i].tObject).m_patrolPoints;
                    UnityEditor.Handles.DrawAAPolyLine (6f, pPoints);
                    if (pPoints.Length < 2 || Event.current.alt)
                        return;
                    using (var ch = new UnityEditor.EditorGUI.ChangeCheckScope ()) {
                        for (int p = pPoints.Length - 1; p > 0; p--) {
                            UnityEditor.Handles.Label (pPoints [p], p.ToString (), "OL box");
                            pPoints [p] = UnityEditor.Handles.DoPositionHandle (pPoints [p], Quaternion.Euler (Vector3.zero));
                        }
                        if (ch.changed)
                            UnityEditor.EditorUtility.SetDirty (this);
                    }
                }
            }
            UnityEditor.Handles.matrix = defMatrix;
        }
        #endif

        void OnDrawGizmos ()
        {
            // check item is select
            if (Application.isPlaying) return;
            #if UNITY_EDITOR
            Transform t = UnityEditor.Selection.activeTransform;
            if (t == null) return;
            if (!transform.IsChildOf (t)) return;
            Volume v = t.GetComponent<Volume> ();
            if (v == null) return;
            PaletteItem p = v._itemInspected;
            if (p == null) return; 
            LevelPiece l = p.inspectedScript;
            if (l == null) return; 
            if (!l.Equals (this)) return;
            #endif

            Gizmos.color = Color.yellow;
            Matrix4x4 oldMatrix = Gizmos.matrix;
            Gizmos.matrix = transform.localToWorldMatrix;
            for (int i = 0; i < 5; i++) {
                if (!(PProperties [i].tObject is EnemySpawner)) return;
                //Draw EnemySpawner.ToggleOffsets
                AiData data = ((EnemySpawner)PProperties [i].tObject).m_AiData;
                if (data == null) return;
                Gizmos.DrawWireSphere (data.toggleOffset, data.toggle);
                if (data.toggleOffsets == null) return;
                for (int o = 0; o < data.toggleOffsets.Length; o++) {
                    Vector4 vo = data.toggleOffsets [o];
                    Gizmos.DrawWireSphere (new Vector3 (vo.x, vo.y, vo.z), vo.w);
                }
            }
            Gizmos.matrix = oldMatrix;
        }

    }
}