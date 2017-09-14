using UnityEngine;
using UnityEditor;
using System;
using System.Collections.Generic;

namespace CreVox
{
    [CustomEditor (typeof(DecoPiece))]
    public class DecoPieceEditor : LevelPieceEditor
    {
        string[] artpacks;
        DecoPiece dp;
        GameObject tempObjold;
        SerializedProperty decos;
        SerializedProperty tree;

        void OnEnable ()
        {
            dp = (DecoPiece)target;
            tree = serializedObject.FindProperty ("tree");
            ClearTree ();
        }

        public override void OnInspectorGUI ()
        {
            EditorGUI.BeginChangeCheck ();
            serializedObject.Update ();
            DrawInspector ();

            using (var h = new EditorGUILayout.HorizontalScope ()) {
                EditorGUILayout.LabelField ("Decoration Object", EditorStyles.boldLabel,GUILayout.Width (Screen.width - 120));
                if (GUILayout.Button ("Test")) {
                    dp.SetupPiece (null);
                }
                if (GUILayout.Button ("Clear")) {
                    ClearTree ();
                }
            }

            // trees
            if (GUILayout.Button ("Init")) {
                InitTree();
            }
            if (dp.tree.Count > 0) {
                DrawTree (dp.tree [0], tree.GetArrayElementAtIndex (0).FindPropertyRelative ("childs"));
            }

            serializedObject.ApplyModifiedProperties ();
            if (EditorGUI.EndChangeCheck ()) {
                UpdateTreeIndex ();
                EditorUtility.SetDirty (dp);
            }
        }

        private void DrawTree(TreeElement _te,SerializedProperty _childs)
        {
            int indentTemp = EditorGUI.indentLevel;
            using (var v = new EditorGUILayout.VerticalScope ("RL Background", GUILayout.Height (16))) {
                for (int i = 0; i < _te.childs.Count; i++) {
                    //search Tree to get TreeElement & index
                    int tIndex = _te.childs [i].FindListByNode (dp.tree);
                    if (tIndex < 0) {
                        Debug.LogError (i.ToString () + " : " + _te.childs [i].treeIndex);
                        return;
                    }

                    var c = tree.GetArrayElementAtIndex (tIndex);
                    var showNode = c.FindPropertyRelative ("showNode");
                    var treeIndex = c.FindPropertyRelative ("self.treeIndex");
                    var type = c.FindPropertyRelative ("self.type");
                    var source = c.FindPropertyRelative ("self.source");
                    var instance = c.FindPropertyRelative ("self.instance");

                    //Draw First Line of TreeElement
                    using (var h = new EditorGUILayout.HorizontalScope ()) {
                        showNode.boolValue = EditorGUILayout.Toggle (showNode.boolValue, "foldout", GUILayout.Width (12));
                        int indentTemp2 = EditorGUI.indentLevel;
                        EditorGUI.indentLevel = 0;
                        treeIndex.intValue = tIndex;
                        GUILayout.Label (treeIndex.intValue.ToString (), GUILayout.Width (12));
                        switch (type.intValue) {
                        default:
                            Color _color = GUI.color;
                            if (instance.objectReferenceValue != null)
                                GUI.color = Color.green;
                            EditorGUI.BeginChangeCheck ();
                            GameObject newSource = source.objectReferenceValue as GameObject;
                            newSource = (GameObject)EditorGUILayout.ObjectField (newSource, typeof(GameObject), false);
                            if (EditorGUI.EndChangeCheck () && PrefabUtility.GetPrefabType (newSource) == PrefabType.Prefab)
                                source.objectReferenceValue = newSource;
                            GUI.color = _color;
                            break;
                        case (int)DecoType.RandomOne:
                        case (int)DecoType.RandomAll:
                            GUILayout.Label ("   ▼  ▼  ▼", EditorStyles.objectField);
                            break;
                        }
                        type.intValue = EditorGUILayout.Popup (type.intValue, Enum.GetNames (typeof(DecoType)), GUILayout.Width (80));
                        EditorGUI.indentLevel = indentTemp2;
                    }

                    //Draw Content of TreeElement
                    bool sub = false, useNode = false;
                    if (showNode.boolValue) {
                        switch (type.intValue) {
                        case (int)DecoType.Node:
                            useNode = true;
                            break;
                        case (int)DecoType.Tree:
                            useNode = true;
                            sub = true;
                            break;
                        case (int)DecoType.RandomOne:
                        case (int)DecoType.RandomAll:
                            sub = true;
                            break;
                        default:
                            break;
                        }

                        EditorGUILayout.BeginHorizontal ();
                        float indentW = (EditorGUI.indentLevel + 1) * 15 + 5;
                        using (var v2_1 = new EditorGUILayout.VerticalScope (GUILayout.Width (indentW))) {
                            GUILayout.Space(3);
                            EditorGUI.BeginDisabledGroup (i == 0);
                            if (GUILayout.Button ("▲", "ButtonMid", GUILayout.Width (indentW))) {
                                _te.childs.Reverse (i - 1, 2);
                            }
                            EditorGUI.EndDisabledGroup ();
                            EditorGUI.BeginDisabledGroup (i == _te.childs.Count - 1);
                            if (GUILayout.Button ("▼", "ButtonMid", GUILayout.Width (indentW))) {
                                _te.childs.Reverse (i, 2);
                            }
                            EditorGUI.EndDisabledGroup ();
                        }
                        using (var v2_2 = new EditorGUILayout.VerticalScope (GUILayout.Height (5))) {
                            if (useNode) {
                                EditorGUILayout.PropertyField (c.FindPropertyRelative ("self"));
                            }
                            if (sub) {
                                TreeElement t = dp.tree [tIndex];
                                DrawTree (t, c.FindPropertyRelative ("childs"));
                            }
                        }
                        EditorGUILayout.EndHorizontal ();
                    }

                    //Draw End of ListMember
                    EditorGUILayout.LabelField (GUIContent.none, "WindowBottomResize", GUILayout.Height (10));
                }
                GUILayout.Space (5);
            }

            //Draw List Footer
            using (var v = new EditorGUILayout.VerticalScope (GUILayout.Height (0))) {
                Rect r = EditorGUI.IndentedRect (EditorGUILayout.GetControlRect ());
                r = new Rect (r.x - 4, r.y - 2, 60, 8);
                GUI.Label (r, GUIContent.none, "RL Footer");
                r = new Rect (r.x + 7, r.y - 3, 16, 16);
                if (GUI.Button (r, GUIContent.none, "OL Plus")) {
                    AddElement (_te);
                    return;
                }
                r.x += 30;
                if (GUI.Button (r, GUIContent.none, "OL Minus") && _childs.arraySize > 0) {
                    RemoveElement (_te.childs);
                    return;
                }
            }
            EditorGUI.indentLevel = indentTemp;
        }

        private void UpdateTreeIndex()
        {
            for (int i = 0; i < dp.tree.Count; i++) {
                dp.tree [i].self.treeIndex = i;
            }
        }

        private void InitTree()
        {
            dp.tree.Clear ();
            dp.tree.Add (new TreeElement ());
            dp.tree [0].self.Init ();
            serializedObject.Update ();
        }

        private void ClearTree()
        {
            if (dp.tree [0].self.instance)
                GameObject.DestroyImmediate (dp.tree [0].self.instance);
            foreach (TreeElement te in dp.tree) {
                te.self.instance = null;
            }
        }

        private void AddElement(TreeElement _te)
        {
            TreeElement newT = new TreeElement ();
            newT.parent.id = _te.self.id;
            newT.self.id = Mathf.Abs (System.Guid.NewGuid ().GetHashCode ());
            dp.tree.Add (newT);
            _te.childs.Add (newT.self as NIndex);
        }

        private void RemoveElement(List<NIndex> _childs)
        {
            if (_childs.Count == 0)
                return;
            int workingTIndex = _childs [_childs.Count - 1].FindListByNode (dp.tree);
            List<NIndex> _eChilds = dp.tree [workingTIndex].childs;
            while (_eChilds.Count > 0) {
                RemoveElement (_eChilds);
            }
//            Debug.Log ("Remove tree : " + workingtreeIndex);
            dp.tree.RemoveAt(workingTIndex);
            _childs.RemoveAt (_childs.Count - 1);
            serializedObject.ApplyModifiedProperties ();
        }


    }

}
