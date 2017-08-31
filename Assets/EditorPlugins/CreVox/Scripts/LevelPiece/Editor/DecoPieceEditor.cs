using UnityEngine;
using UnityEditor;
using System.IO;
using UnityEditorInternal;
using System.ComponentModel;
using System;
using System.Runtime.Remoting;
using BehaviorDesigner.Runtime.Tasks.Basic.UnityCapsuleCollider;
using BehaviorDesigner.Runtime.Tasks.Basic.UnityDebug;
using TreeEditor;
using EditorExtend;
using UnityEditor.Graphs;

namespace CreVox
{
    [CustomEditor (typeof(DecoPiece))]
    public class DecoPieceEditor : LevelPieceEditor
    {
        string[] artpacks;
        DecoPiece dp;
        GameObject tempObjold;
        SerializedProperty decos;
        //ReorderableList
        private ReorderableList list_Nodes;

        void OnEnable ()
        {
            dp = (DecoPiece)target;
            decos = serializedObject.FindProperty ("decos");
            //ReorderableList
            list_Nodes = GenerateList (serializedObject, decos, "Decoration");
        }

        public override void OnInspectorGUI ()
        {
            serializedObject.Update ();
            EditorGUI.BeginChangeCheck ();
            DrawInspector ();

            EditorGUILayout.Separator ();
            EditorGUILayout.LabelField ("Decoration Object", EditorStyles.boldLabel);

            DrawList (decos);

            //ReorderableList
//            EditorGUILayout.Space();
//            list_Nodes.DoLayoutList();

            if (EditorGUI.EndChangeCheck ()) {
                EditorUtility.SetDirty (dp);
            }
            serializedObject.ApplyModifiedProperties ();
        }

        private void DrawList(SerializedProperty list)
        {
            EditorGUI.indentLevel++;
            int indentTemp = EditorGUI.indentLevel;
            using (var v = new EditorGUILayout.VerticalScope ("RL Background", GUILayout.Height(16))) {
                for (int i = 0; i < list.arraySize; i++) {
                    var d = list.GetArrayElementAtIndex (i); 
                    var type = d.FindPropertyRelative ("type");
                    var showNode = d.FindPropertyRelative ("showNode");
                    var source = d.FindPropertyRelative ("node.source");

                    //Draw First Line of ListMember
                    EditorGUI.indentLevel = 0;
                    using (var h = new EditorGUILayout.HorizontalScope ()) {
                        showNode.boolValue = EditorGUILayout.Toggle (showNode.boolValue, "foldout", GUILayout.Width (12));
                        EditorGUILayout.PropertyField (source, GUIContent.none);
                        type.intValue = EditorGUILayout.Popup (type.intValue, Enum.GetNames (typeof(DecoType)), GUILayout.Width (80));
                    }

                    //Draw Content of ListMember
                    EditorGUI.indentLevel ++;
                    bool useNode = false, useTree = false, useROne = false;
                    if (showNode.boolValue) {
                        switch (type.intValue) {
                        case (int)DecoType.Node:
                            useNode = true;
                            break;

                        case (int)DecoType.Tree:
                            useNode = true;
                            useTree = true;
                            break;

                        case (int)DecoType.RandomOne:
                            useROne = true;
                            break;

                        default:
                            break;
                        }

                        if (useNode)
                            EditorGUILayout.PropertyField (d.FindPropertyRelative ("node"));
                        EditorGUILayout.BeginHorizontal ();
                        if (useTree || useROne)
                            EditorGUILayout.LabelField (GUIContent.none, GUILayout.Width (EditorGUI.indentLevel * 12));
                        EditorGUILayout.BeginVertical ();
                        if (useTree)
                            DrawList (d.FindPropertyRelative ("treeNodes"));
                        if (useROne)
                            DrawList (d.FindPropertyRelative ("selectNodes"));
                        EditorGUILayout.EndVertical ();
                        EditorGUILayout.EndHorizontal ();
                    }
                    EditorGUI.indentLevel = 1;

                    //Draw End of ListMember
                    EditorGUILayout.LabelField (GUIContent.none, "WindowBottomResize", GUILayout.Height (12));
                }
                EditorGUILayout.Space ();
            }

            //Draw List Footer
            int _arrayEnd = (list.arraySize < 1) ? 0 : list.arraySize - 1;
            EditorGUI.indentLevel = 0;
            EditorGUILayout.LabelField (GUIContent.none, GUILayout.Height (-8), GUILayout.Width(60));
            Rect r = EditorGUILayout.GetControlRect (false);
            r = new Rect (r.x, r.y + 4, 60, 8);
            GUI.Label (r, GUIContent.none, "RL Footer");
            r = new Rect (r.x + 7, r.y - 4, 30, 16);
            if (GUI.Button (r, GUIContent.none, "OL Plus"))
                list.InsertArrayElementAtIndex (_arrayEnd);
            r.x += r.width;
            if (GUI.Button (r, GUIContent.none, "OL Minus"))
                list.DeleteArrayElementAtIndex (_arrayEnd);
            EditorGUI.indentLevel = indentTemp;
        }

        //ReorderableList
        public ReorderableList GenerateList(SerializedObject so,SerializedProperty sp, string header)
        {
            ReorderableList list = new ReorderableList (so, sp,true,true,true,true);

            list.drawHeaderCallback = (Rect rect) => {EditorGUI.LabelField (rect, header);};

            list.elementHeightCallback = (int index) => {
                var d = sp.GetArrayElementAtIndex (index);
                var type = d.FindPropertyRelative ("type");

                var showNode = d.FindPropertyRelative ("showNode");
                float h = 45.0f + (showNode.boolValue ? 65.0f : 0f);

                switch (type.intValue) {
                case (int)DecoType.Tree:
                    float h_t = 0f;
                    for (int i = 0; i < sp.arraySize; i++){
                    
                    }
                    h += h_t;
                    break;
                case (int)DecoType.    RandomOne:
                    float h_rs = 0f;
                    h += h_rs;
                    break;
                default:
                    break;
                }
                return h;
            };

            list.drawElementCallback = (Rect rect, int index, bool isActive, bool isFocused) => {
                SerializedProperty d = sp.GetArrayElementAtIndex (index);
                SerializedProperty type = d.FindPropertyRelative ("type");

                var showNode = d.FindPropertyRelative ("showNode");
                float h = 20.0f + (showNode.boolValue ? 65.0f : 0f);

                //EditorGUI.PropertyField (rect, type, GUIContent.none);
                type.intValue = (int)EditorGUI.Popup(new Rect(rect.x, rect.y, rect.width,16), type.intValue, Enum.GetNames (typeof(DecoType)));
                rect.y += 16;
                switch (type.intValue) {
                case (int)DecoType.Node:
                    EditorGUI.PropertyField (rect, d.FindPropertyRelative ("node"));
                    break;
                case (int)DecoType.Tree:
                    EditorGUI.PropertyField (rect, d.FindPropertyRelative ("node"));
                    rect.y += h;
                    SerializedProperty tree_sp = d.FindPropertyRelative ("treeNodes");
                    SerializedObject tree_so = tree_sp.serializedObject;
                    ReorderableList rootList = GenerateList(tree_so, tree_sp, header +"(" + index + ")/");
                    rootList.DoLayoutList();
                    //rootList.DoList(EditorGUI.IndentedRect(rect));
                    break;
                case (int)DecoType.    RandomOne:
                    EditorGUI.PropertyField (rect, d.FindPropertyRelative ("node"));
                    break;
                }
            };
            return list;
        }
    }
}
