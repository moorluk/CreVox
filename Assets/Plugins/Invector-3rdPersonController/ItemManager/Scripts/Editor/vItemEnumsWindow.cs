using UnityEngine;
using System.Collections;
using UnityEditor;
using System;
using System.Collections.Generic;
namespace Invector.ItemManager
{
    public class vItemEnumsWindow : EditorWindow
    {
        public vItemEnumsList[] datas;
        public List<string> _itemTypeNames = new List<string>();
        public List<string> _itemAttributeNames = new List<string>();
        public GUISkin skin;
        public Vector2 scrollView;
        public static vItemEnumsWindow instance;
        [MenuItem("Invector/Inventory/ItemEnums/Open ItemEnums Editor")]
        public static void CreateWindow()
        {
            if (instance == null)
            {
                var window = vItemEnumsWindow.GetWindow<vItemEnumsWindow>("Item Enums", true);
                instance = window;
                window.skin = Resources.Load("skin") as GUISkin;
                #region Get all vItemType values of current Enum
                try
                {
                    window._itemTypeNames = Enum.GetNames(typeof(vItemType)).vToList();

                }
                catch
                {

                }
                #endregion

                #region Get all vItemAttributes values of current Enum
                try
                {
                    window._itemAttributeNames = Enum.GetNames(typeof(vItemAttributes)).vToList();
                }
                catch
                {

                }
                #endregion
                window.datas = Resources.LoadAll<vItemEnumsList>("");
                window.minSize = new Vector2(460, 600);

            }

        }

        void OnGUI()
        {
            if (skin) GUI.skin = skin;
            this.minSize = new Vector2(460, 600);
            GUILayout.BeginVertical("box");
            GUILayout.Box("vItemEnums");
            GUILayout.BeginHorizontal();
            #region Item Type current
            GUILayout.BeginVertical("box", GUILayout.Width(215));
            GUILayout.Box("Item Types", GUILayout.ExpandWidth(true));

            for (int i = 0; i < _itemTypeNames.Count; i++)
            {
                GUILayout.Label(_itemTypeNames[i], EditorStyles.miniBoldLabel);
            }

            GUILayout.EndVertical();
            #endregion
            #region Item Attribute current
            GUILayout.BeginVertical("box", GUILayout.Width(215));
            GUILayout.Box("Item Attributes", GUILayout.ExpandWidth(true));

            for (int i = 0; i < _itemAttributeNames.Count; i++)
            {
                GUILayout.Label(_itemAttributeNames[i], EditorStyles.miniBoldLabel);
            }

            GUILayout.EndVertical();
            #endregion

            GUILayout.EndHorizontal();
            GUILayout.EndVertical();
            GUILayout.BeginVertical("box");
            GUILayout.Box("Edit Enums", GUILayout.ExpandHeight(false));
            scrollView = GUILayout.BeginScrollView(scrollView, GUILayout.MinHeight(100), GUILayout.MaxHeight(600));
            for (int i = 0; i < datas.Length; i++)
            {
                DrawItemEnumListData(datas[i]);
            }
            GUILayout.EndScrollView();
            EditorGUILayout.HelpBox("If the field has is gray, the vItemEnums contains same value", MessageType.Info);
            if (GUILayout.Button(new GUIContent("Refresh ItemEnums", "Save and refesh changes to vItemEnums")))
            {
                vItemEnumsBuilder.RefreshItemEnums();
            }
            GUILayout.EndVertical();
        }
        void DrawItemEnumListData(vItemEnumsList data)
        {
            SerializedObject _data = new SerializedObject(data);
            _data.Update();
            GUILayout.BeginVertical("box");
            GUILayout.Box(data.name, GUILayout.ExpandWidth(true));
            EditorGUILayout.ObjectField(data, typeof(vItemEnumsList), false);
            GUILayout.BeginHorizontal();

            #region Item Types
            var itemTypeEnumValueList = _data.FindProperty("itemTypeEnumValues");
            GUILayout.BeginVertical("box", GUILayout.Width(200));
            GUILayout.BeginHorizontal("box", GUILayout.ExpandWidth(true));
            GUILayout.Label("Item Types", EditorStyles.miniBoldLabel);
            EditorGUILayout.PropertyField(itemTypeEnumValueList.FindPropertyRelative("Array.size"), GUIContent.none);
            GUILayout.EndHorizontal();

            var labelWidht = EditorGUIUtility.labelWidth;

            EditorGUIUtility.labelWidth = 30f;
            var color = GUI.color;
            for (int i = 0; i < itemTypeEnumValueList.arraySize; i++)
            {
                if (_itemTypeNames.Contains(itemTypeEnumValueList.GetArrayElementAtIndex(i).stringValue))
                    GUI.color = Color.gray;
                else GUI.color = color;
                EditorGUILayout.PropertyField(itemTypeEnumValueList.GetArrayElementAtIndex(i), new GUIContent(i.ToString()));
            }

            GUILayout.EndVertical();
            #endregion
            #region Item Attributes
            GUI.color = color;
            var itemAttributesEnumValuesList = _data.FindProperty("itemAttributesEnumValues");
            GUILayout.BeginVertical("box", GUILayout.Width(200));
            GUILayout.BeginHorizontal("box", GUILayout.ExpandWidth(true));
            GUILayout.Label("Item Attributes", EditorStyles.miniBoldLabel);
            EditorGUILayout.PropertyField(itemAttributesEnumValuesList.FindPropertyRelative("Array.size"), GUIContent.none);
            GUILayout.EndHorizontal();


            for (int i = 0; i < itemAttributesEnumValuesList.arraySize; i++)
            {
                if (_itemAttributeNames.Contains(itemAttributesEnumValuesList.GetArrayElementAtIndex(i).stringValue))
                    GUI.color = Color.gray;
                else GUI.color = color;
                EditorGUILayout.PropertyField(itemAttributesEnumValuesList.GetArrayElementAtIndex(i), new GUIContent(i.ToString()));
            }

            GUILayout.EndVertical();
            #endregion

            GUILayout.EndHorizontal();
            GUILayout.EndVertical();
            _data.ApplyModifiedProperties();
            if (_data.ApplyModifiedProperties())
                EditorUtility.SetDirty(data);
            EditorGUIUtility.labelWidth = labelWidht;
            GUI.color = color;
        }
    }
}


