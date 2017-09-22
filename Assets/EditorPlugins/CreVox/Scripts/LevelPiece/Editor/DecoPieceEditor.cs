using UnityEngine;
using UnityEditor;
using System;
using System.Collections.Generic;
using System.Xml.Serialization.Configuration;

namespace CreVox
{
    [CustomEditor (typeof(DecoPiece))]
    public class DecoPieceEditor : LevelPieceEditor
    {
        DecoPiece dp;
        SerializedProperty root;
        SerializedProperty tree;

        void OnEnable ()
        {
            dp = (DecoPiece)target;
            root = serializedObject.FindProperty ("root");
            tree = serializedObject.FindProperty ("tree");
        }

        public override void OnInspectorGUI ()
        {
            EditorGUI.BeginChangeCheck ();
            serializedObject.Update ();
            DrawInspector ();

            using (var h = new EditorGUILayout.HorizontalScope ()) {
                EditorGUILayout.LabelField ("Decoration Object", EditorStyles.boldLabel, GUILayout.Width (Screen.width - 120));
                GUILayout.FlexibleSpace ();
                if (GUILayout.Button ("Test", GUILayout.Width (45))) {
                    dp.SetupPiece (null);
                }
                if (GUILayout.Button ("Clear", GUILayout.Width (45))) {
                    dp.ClearRoot ();
                }
            }
            // Root
            EditorGUILayout.ObjectField (root, typeof(GameObject), new GUIContent ("Root"));

            using (var h = new EditorGUILayout.HorizontalScope ()) {
                if (GUILayout.Button ("Init", "prebutton")) {
                    if (EditorUtility.DisplayDialog ("風蕭蕭兮易水寒", "Remoove All Tree Element !!?", "Yes", "No"))
                        InitTree ();
                }
                if (GUILayout.Button ("AutoGet", "prebutton")) {
                    if (EditorUtility.DisplayDialog ("", "Remoove All Tree Element and find all prefab in childrens?", "Yes", "No"))
                        AutoBuildTree ();
                }
            }

            // trees
            if (dp.tree.Count > 0) {
                DrawTree (dp.tree [0], tree.GetArrayElementAtIndex (0).FindPropertyRelative ("childs"));
            }

            // tools
            DrawSetParentTool ();
            DrawChildObjectButtonList ();

            serializedObject.ApplyModifiedProperties ();
            if (EditorGUI.EndChangeCheck ()) {
                UpdateTreeIndex ();
                EditorUtility.SetDirty (dp);
            }
        }

        private void DrawTree (TreeElement _te, SerializedProperty _childs)
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

                    var childTe = tree.GetArrayElementAtIndex (tIndex);
                    var showNodeProp = childTe.FindPropertyRelative ("showNode");
                    var selfProp = childTe.FindPropertyRelative ("self");
                    var treeIndexProp = childTe.FindPropertyRelative ("self.treeIndex");
                    var probabilityProp = childTe.FindPropertyRelative ("self.probability");
                    var typeProp = childTe.FindPropertyRelative ("self.type");
                    var sourceProp = childTe.FindPropertyRelative ("self.source");
                    var instanceProp = childTe.FindPropertyRelative ("self.instance");
                    var childsProp = childTe.FindPropertyRelative ("childs");
                    var childProbabilityProp = _childs.GetArrayElementAtIndex (i).FindPropertyRelative ("probability");

                    //Draw First Line of TreeElement
                    using (var h = new EditorGUILayout.HorizontalScope ()) {
                        showNodeProp.boolValue = EditorGUILayout.Toggle (showNodeProp.boolValue, "foldout", GUILayout.Width (10));
                        int indentTemp2 = EditorGUI.indentLevel;
                        EditorGUI.indentLevel = 0;
                        treeIndexProp.intValue = tIndex;
                        GUILayout.Label (treeIndexProp.intValue.ToString (), GUILayout.Width (20));
                        switch (typeProp.intValue) {
                        default:
                            Color _color = GUI.color;
                            if (instanceProp.objectReferenceValue != null)
                                GUI.color = Color.green;
                            using (var ch = new EditorGUI.ChangeCheckScope ()) {
                                GameObject newSource = sourceProp.objectReferenceValue as GameObject;
                                newSource = (GameObject)EditorGUILayout.ObjectField (newSource, typeof(GameObject), true);
                                if (ch.changed) {
                                    if (newSource == null || PrefabUtility.GetPrefabType (newSource) == PrefabType.Prefab)
                                        sourceProp.objectReferenceValue = newSource;
                                    else if (PrefabUtility.GetPrefabType (newSource) == PrefabType.PrefabInstance) {
                                        AssignPrefabInstance (newSource, selfProp);
                                    }
                                }
                            }
                            GUI.color = _color;
                            break;
                        case (int)DecoType.RandomOne:
                        case (int)DecoType.RandomAll:
                            GUILayout.Label ("   ▼  ▼  ▼", EditorStyles.objectField);
                            break;
                        }
                        EditorGUILayout.PropertyField (typeProp, GUIContent.none, GUILayout.Width (80));
                        EditorGUI.indentLevel = indentTemp2;
                    }

                    //Draw Content of TreeElement
                    bool sub = false, useNode = false;
                    if (showNodeProp.boolValue) {
                        switch (typeProp.intValue) {
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
                            GUILayout.Space (3);
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
                                using (var ch = new EditorGUI.ChangeCheckScope ()) {
                                    EditorGUILayout.PropertyField (selfProp);
                                    if (ch.changed) {
                                        childProbabilityProp.floatValue = probabilityProp.floatValue;
                                    }
                                }
                            }
                            if (sub) {
                                TreeElement t = dp.tree [tIndex];
                                DrawTree (t, childsProp);
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

        private void UpdateTreeIndex ()
        {
            for (int i = 0; i < dp.tree.Count; i++) {
                dp.tree [i].self.treeIndex = i;
            }
        }

        private void InitTree ()
        {
            dp.tree.Clear ();
            dp.tree.Add (new TreeElement ());
            dp.tree [0].self.type = DecoType.Tree;
            serializedObject.Update ();
        }

        private void AutoBuildTree ()
        {
            //init Tree
            dp.tree.Clear ();
            dp.tree.Add (new TreeElement ());
            TreeElement _parent = dp.tree [0];
            _parent.self.type = DecoType.Tree;

            //search prefab
            Transform[] _all = dp.root.GetComponentsInChildren<Transform> (false);
            List<GameObject> _allPrefab = new List<GameObject> ();
            foreach (Transform t in _all) {
                GameObject g = t.gameObject;
                PrefabType pt = PrefabUtility.GetPrefabType (g);
                GameObject pr = PrefabUtility.FindPrefabRoot (g);
                if (g != dp.gameObject && pt == PrefabType.PrefabInstance && pr == g)
                    _allPrefab.Add (g);
            }
            //add TreeElement
            for (int i = 0; i < _allPrefab.Count; i++)
                AddElement (_parent);
            serializedObject.Update ();
            //assign prefab
            for (int i = 1; i < dp.tree.Count; i++) {
                GameObject _pInstance = _allPrefab [i - 1];
                SerializedProperty _nodeProp = tree.GetArrayElementAtIndex (i).FindPropertyRelative ("self");
                AssignPrefabInstance (_pInstance, _nodeProp);
            }
            serializedObject.ApplyModifiedProperties ();
            serializedObject.Update ();
        }

        private void AssignPrefabInstance (GameObject _prefabInstance, SerializedProperty _nodeProp)
        {
            GameObject newSourceAsset = (GameObject)PrefabUtility.GetPrefabParent (_prefabInstance);
            _nodeProp.FindPropertyRelative ("pos").vector3Value = _prefabInstance.transform.localPosition;
            _nodeProp.FindPropertyRelative ("rot").vector3Value = _prefabInstance.transform.localEulerAngles;
            _nodeProp.FindPropertyRelative ("scl").vector3Value = _prefabInstance.transform.localScale;
            _nodeProp.FindPropertyRelative ("source").objectReferenceValue = newSourceAsset;
            GameObject.DestroyImmediate (_prefabInstance);
        }

        private void AddElement (TreeElement _parent)
        {
            TreeElement newT = new TreeElement ();
            newT.parent.id = _parent.self.id;
            newT.parent.treeIndex = _parent.self.treeIndex;
            newT.self.id = Mathf.Abs (System.Guid.NewGuid ().GetHashCode ());
            dp.tree.Add (newT);
            _parent.childs.Add (newT.self as NIndex);
        }

        private void RemoveElement (List<NIndex> _childs)
        {
            if (_childs.Count == 0)
                return;
            int workingTIndex = _childs [_childs.Count - 1].FindListByNode (dp.tree);
            List<NIndex> _eChilds = dp.tree [workingTIndex].childs;
            while (_eChilds.Count > 0) {
                RemoveElement (_eChilds);
            }
            dp.tree.RemoveAt (workingTIndex);
            _childs.RemoveAt (_childs.Count - 1);
            serializedObject.ApplyModifiedProperties ();
        }

        static bool showSetParentTool = true;
        private int _parentIndex = 0; 
        int ParentIndex
        {
            get{return _parentIndex; }
            set{_parentIndex = Mathf.Clamp (value, 0, dp.tree.Count - 1);}
        }
        private int _childIndex = 0;
        int ChildIndex
        {
            get{return _childIndex; }
            set{_childIndex = Mathf.Clamp (value, 0, dp.tree.Count - 1);}
        }
        private void DrawSetParentTool ()
        {
            showSetParentTool = EditorGUILayout.Foldout (showSetParentTool, "Set Parent Tool");
            if (showSetParentTool) {
                using (var h = new GUILayout.HorizontalScope (EditorStyles.helpBox)) {
                    EditorGUILayout.LabelField ("Child", GUILayout.Width (35));
                    ChildIndex = EditorGUILayout.IntField (ChildIndex, GUILayout.Width (25));
                    EditorGUILayout.LabelField (": Parent(" + dp.tree[ChildIndex].parent.treeIndex + ")-->", GUILayout.Width (85));
                    ParentIndex = EditorGUILayout.IntField (ParentIndex, GUILayout.Width (25));
                    EditorGUILayout.Space ();
                    if (GUILayout.Button ("Set", GUILayout.Width (50))){
                        SetParent ();
                    }
                }
            }
        }

        private void SetParent ()
        {
            TreeElement c = dp.tree [_childIndex];
            TreeElement oldP = dp.tree [c.parent.treeIndex];
            TreeElement newP = dp.tree [_parentIndex];
            Predicate<NIndex> sameID = delegate(NIndex obj) {return obj.id == c.self.id;};

            if (oldP.self.id == newP.self.id)
                return;

            oldP.childs.RemoveAll (sameID);
            newP.childs.Add (c.self);
            if (newP.self.type == DecoType.Node)
                newP.self.type = DecoType.Tree;
            c.parent = newP.self;
        }

        static bool showChildObjectButtonList = false;
        private void DrawChildObjectButtonList ()
        {
            showChildObjectButtonList = EditorGUILayout.Foldout (showChildObjectButtonList, "Object List");
            Rect r = EditorGUI.IndentedRect (EditorGUILayout.GetControlRect ());
            r.height = 12;
            r.width = Screen.width - 100;
            if (showChildObjectButtonList) {
                var transforms = dp.transform.GetComponentsInChildren<Transform> ();
                foreach (var t in transforms) {
                    GameObject obj = t.gameObject;
                    if (obj != dp.gameObject && PrefabUtility.GetPrefabType (obj) == PrefabType.PrefabInstance && PrefabUtility.FindPrefabRoot (obj) == obj) {
                        if (GUI.Button (r, obj.name, "minibutton")) {
                            Selection.activeGameObject = obj;
                        }
                        r.y += r.height;
                    }
                }
                GUILayout.Space (r.y);
            }
        }

    }

}
