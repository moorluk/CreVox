﻿using UnityEngine;
using UnityEditor;
using System;
using System.Collections.Generic;

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

        #region InspectorGUI

        float screenW;

        public override void OnInspectorGUI ()
        {
            screenW = Screen.width - 30;

            EditorGUI.BeginChangeCheck ();
            serializedObject.Update ();
            DrawInspector ();

            using (var h = new EditorGUILayout.HorizontalScope ()) {
                EditorGUILayout.LabelField ("Decoration Tree", EditorStyles.boldLabel, GUILayout.Width (screenW - 120));
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
                    if (EditorUtility.DisplayDialog ("", "Remoove All Tree Element !!?", "Yes", "No"))
                        InitTree ();
                }
                if (GUILayout.Button ("AutoGet", "prebutton")) {
                    if (EditorUtility.DisplayDialog ("", "Remoove All Tree Element and find all prefab in childrens ?", "Yes", "No"))
                        AutoBuildTree ();
                }
                if (GUILayout.Button ("Sort", "prebutton")) {
                    if (EditorUtility.DisplayDialog ("", "Sort All Tree Element's Index ?", "Yes", "No"))
                        Sort ();
                }
                if (GUILayout.Button("Update", "prebutton")) {
                    if (EditorUtility.DisplayDialog("", "Update All Tree Element's Transform ?", "Yes", "No"))
                        UpdateNode();
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

        void DrawTree (TreeElement _te, SerializedProperty _childs)
        {
            int indentTemp = EditorGUI.indentLevel;
            using (var v = new EditorGUILayout.VerticalScope ("RL Background", GUILayout.Height (16))) {
                for (int i = 0; i < _te.childs.Count; i++) {
                    //search Tree to get TreeElement & index
                    int tIndex = _te.childs [i].FindListByNode (dp.tree);
                    if (tIndex < 0) {
                        Debug.LogError (i + " : " + _te.childs [i].treeIndex);
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
                                    PrefabType _type = PrefabUtility.GetPrefabType (newSource);
                                    if (newSource == null || _type == PrefabType.Prefab)
                                        sourceProp.objectReferenceValue = newSource;
                                    else if (_type == PrefabType.PrefabInstance) {
                                        AssignPrefabInstance (newSource, selfProp);
                                        UnityEngine.Object.DestroyImmediate (newSource);
                                    }
                                }
                            }
                                if (instanceProp.objectReferenceValue != null)
                                {
                                    EditorGUILayout.ObjectField(instanceProp.objectReferenceValue, typeof(GameObject), true);
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

        #endregion

        #region TreeFunction

        void InitTree ()
        {
            dp.tree.Clear ();
            dp.tree.Add (new TreeElement ());
            dp.tree [0].self.type = DecoType.Tree;
            serializedObject.Update ();
        }

        void AutoBuildTree ()
        {
            InitTree ();

            //clean other tree
            var _trees = dp.Root.GetComponentsInChildren<DecoPiece> (false);
            foreach (var t in _trees) {
                if (t.Equals (dp))
                    continue;
                t.ClearRoot ();
            }

            //search prefab
            Transform[] _all = dp.Root.GetComponentsInChildren<Transform> (false);
            List<GameObject> _allPrefab = new List<GameObject> ();
            foreach (Transform t in _all) {
                if (t == null)
                    continue;
                GameObject g = t.gameObject;
                PrefabType pt = PrefabUtility.GetPrefabType (g);
                GameObject pr = PrefabUtility.FindPrefabRoot (g);
                if (g != dp.gameObject && pt == PrefabType.PrefabInstance && pr == g)
                    _allPrefab.Add (g);
            }
            //add TreeElement
            for (int i = 0; i < _allPrefab.Count; i++)
                AddElement (dp.tree [0]);
            serializedObject.Update ();
            //assign prefab
            for (int i = 1; i < dp.tree.Count; i++) {
                GameObject _pInstance = _allPrefab [i - 1];
                SerializedProperty _nodeProp = tree.GetArrayElementAtIndex (i).FindPropertyRelative ("self");
                AssignPrefabInstance (_pInstance, _nodeProp);
            }
            foreach (GameObject g in _allPrefab) {
                if (g)
                    GameObject.DestroyImmediate (g, false);
            }
            serializedObject.ApplyModifiedProperties ();
            serializedObject.Update ();
        }

        void Sort ()
        {
            List<TreeElement> newTree = new List<TreeElement> ();
            newTree.Add (dp.tree [0]);
            SortChild (dp.tree [0], newTree);
            if (newTree.Count == dp.tree.Count)
                dp.tree = newTree;
            else {
                string log = "";
                for (int i = 0; i < newTree.Count; i++)
                    log += "[" + i + "]" + newTree [i].self.treeIndex + "\n";
                Debug.Log (log);
            }
        }

        void SortChild (TreeElement _te, List<TreeElement> _newTree)
        {
            for (int i = 0; i < _te.childs.Count; i++)
                _newTree.Add (dp.tree [_te.childs [i].FindListByNode (dp.tree)]);
            for (int i = 0; i < _te.childs.Count; i++)
                SortChild (dp.tree [_te.childs [i].FindListByNode (dp.tree)], _newTree);
        }

        void UpdateNode ()
        {
            for (int i = 0; i < dp.tree.Count; i++) {
                if (dp.tree[i].self.instance != null) {
                    Transform n = dp.tree[i].self.instance.transform;
                    dp.tree[i].self.pos = n.localPosition;
                    dp.tree[i].self.rot = n.localEulerAngles;
                    dp.tree[i].self.scl = n.localScale;
                }
            }
        }

        void AddElement (TreeElement _parent)
        {
            TreeElement newT = new TreeElement ();
            newT.parent.id = _parent.self.id;
            newT.parent.treeIndex = _parent.self.treeIndex;
            newT.self.id = GetNewID ();
            dp.tree.Add (newT);
            _parent.childs.Add (newT.self);
        }

        void RemoveElement (IList<NIndex> _childs)
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

        #endregion

        int GetNewID ()
        {
            int newID = Mathf.Abs (Guid.NewGuid ().GetHashCode ());
            Predicate<TreeElement> checkNode = obj => obj.self.id.Equals (newID);
            while (dp.tree.Exists (checkNode)) {
                newID = Mathf.Abs (Guid.NewGuid ().GetHashCode ());
            }
            return newID;
        }

        static void AssignPrefabInstance (GameObject _prefabInstance, SerializedProperty _nodeProp)
        {
            GameObject newSourceAsset = (GameObject)PrefabUtility.GetPrefabParent (_prefabInstance);
            _nodeProp.FindPropertyRelative ("pos").vector3Value = _prefabInstance.transform.localPosition;
            _nodeProp.FindPropertyRelative ("rot").vector3Value = _prefabInstance.transform.localEulerAngles;
            _nodeProp.FindPropertyRelative ("scl").vector3Value = _prefabInstance.transform.localScale;
            _nodeProp.FindPropertyRelative ("source").objectReferenceValue = newSourceAsset;
        }

        void UpdateTreeIndex ()
        {
            for (int i = 0; i < dp.tree.Count; i++) {
                dp.tree [i].self.treeIndex = i;
            }
            for (int i = 0; i < dp.tree.Count; i++) {
                dp.tree [i].parent.treeIndex = dp.tree [i].parent.FindListByNode (dp.tree);
                foreach (NIndex n in dp.tree[i].childs)
                    n.treeIndex = n.FindListByNode (dp.tree);
            }
        }

        #region SetParentTool

        static bool showSetParentTool = true;
        int _parentIndex;

        int ParentIndex {
            get{ return _parentIndex; }
            set{ _parentIndex = Mathf.Clamp (value, 0, dp.tree.Count - 1); }
        }

        int _childIndex;

        int ChildIndex {
            get{ return _childIndex; }
            set{ _childIndex = Mathf.Clamp (value, 0, dp.tree.Count - 1); }
        }

        void DrawSetParentTool ()
        {
            showSetParentTool = EditorGUILayout.Foldout (showSetParentTool, "Set Parent Tool");
            if (showSetParentTool) {
                using (var h = new GUILayout.HorizontalScope (EditorStyles.helpBox)) {
                    EditorGUILayout.LabelField ("Child", GUILayout.Width (35));
                    ChildIndex = EditorGUILayout.IntField (ChildIndex, GUILayout.Width (25));
                    EditorGUILayout.LabelField (": Parent (" + dp.tree [ChildIndex].parent.treeIndex + ") →", GUILayout.Width (90));
                    ParentIndex = EditorGUILayout.IntField (ParentIndex, GUILayout.Width (25));
                    EditorGUILayout.Space ();
                    if (GUILayout.Button ("Set", GUILayout.Width (50))) {
                        SetParent ();
                    }
                }
            }
        }

        void SetParent ()
        {
            TreeElement c = dp.tree [_childIndex];
            TreeElement oldP = dp.tree [c.parent.treeIndex];
            TreeElement newP = dp.tree [_parentIndex];
            Predicate<NIndex> sameID = obj => obj.id == c.self.id;

            if (oldP.self.id == newP.self.id)
                return;

            oldP.childs.RemoveAll (sameID);
            newP.childs.RemoveAll (sameID);
            newP.childs.Add (c.self);
            if (newP.self.type == DecoType.Node)
                newP.self.type = DecoType.Tree;
            c.parent = newP.self;
        }

        #endregion

        #region ChildObjectButtonList

        static bool showChildObjectButtonList;

        void DrawChildObjectButtonList ()
        {
            showChildObjectButtonList = EditorGUILayout.Foldout (showChildObjectButtonList, "Object List");
            Rect r = EditorGUI.IndentedRect (EditorGUILayout.GetControlRect ());
            r.height = 12;
            r.width = screenW - 100;
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

        #endregion
    }

}
