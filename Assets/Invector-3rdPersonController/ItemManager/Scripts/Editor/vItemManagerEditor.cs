using UnityEngine;
using System.Collections;
using UnityEditor;
using System.Collections.Generic;
using System;
using UnityEditor.Events;
using System.Linq;

namespace Invector.ItemManager
{
    [CustomEditor(typeof(vItemManager))]
    [System.Serializable]
    public class vItemManagerEditor : Editor
    {
        #region Variables

        protected vItemManager manager;
        protected SerializedProperty itemReferenceList;
        GUISkin skin, oldSkin;
        bool inAddItem;
        bool openWindow;
        int selectedItem;
        Vector2 scroll;
        bool showManagerEvents;
        bool showItemAttributes;
        string[] ignoreProperties = new string[] { "equipPoints", "applyAttributeEvents", "items", "startItems", "onUseItem", "onAddItem", "onLeaveItem", "onDropItem", "onOpenCloseInventory", "onEquipItem", "onUnequipItem" };
        bool[] inEdition;
        string[] newPointNames;
        Transform parentBone;
        Animator animator;
        List<vItem> filteredItems;

        #endregion

        private Texture2D m_Logo = null;

        public virtual void OnEnable()
        {
            m_Logo = (Texture2D)Resources.Load("icon_v2", typeof(Texture2D));
            manager = (vItemManager)target;
            itemReferenceList = serializedObject.FindProperty("startItems");
            skin = Resources.Load("skin") as GUISkin;
            var meleeManager = manager.GetComponent<vMeleeManager>();
            vItemManagerUtilities.CreateDefaultEquipPoints(manager, meleeManager);
            animator = manager.GetComponent<Animator>();
            if (manager.equipPoints != null)
            {
                inEdition = new bool[manager.equipPoints.Count];
                newPointNames = new string[manager.equipPoints.Count];
            }

            else
                manager.equipPoints = new List<EquipPoint>();
        }

        public override void OnInspectorGUI()
        {
            oldSkin = GUI.skin;
            serializedObject.Update();
            if (skin) GUI.skin = skin;
            GUILayout.BeginVertical("Item Manager", "window");
            GUILayout.Label(m_Logo, GUILayout.MaxHeight(25));
            
            openWindow = GUILayout.Toggle(openWindow, openWindow ? "Close" : "Open", EditorStyles.toolbarButton);
            if (openWindow)
            {
                GUI.skin = oldSkin;
                DrawPropertiesExcluding(serializedObject, ignoreProperties);
                GUI.skin = skin;

                if (GUILayout.Button("Open Item List"))
                {
                    vItemListWindow.CreateWindow(manager.itemListData);
                }

                if (manager.itemListData)
                {
                    GUILayout.BeginVertical("box");
                    if (itemReferenceList.arraySize > manager.itemListData.items.Count)
                    {
                        manager.startItems.Resize(manager.itemListData.items.Count);
                    }
                    GUILayout.Box("Start Items " + manager.startItems.Count);
                    filteredItems = manager.itemsFilter.Count > 0 ? GetItemByFilter(manager.itemListData.items, manager.itemsFilter) : manager.itemListData.items;

                    if (!inAddItem && filteredItems.Count > 0 && GUILayout.Button("Add Item", EditorStyles.miniButton))
                    {
                        inAddItem = true;
                    }
                    if (inAddItem && filteredItems.Count > 0)
                    {
                        GUILayout.BeginVertical("box");
                        selectedItem = EditorGUILayout.Popup(new GUIContent("SelectItem"), selectedItem, GetItemContents(filteredItems));
                        bool isValid = true;
                        var indexSelected = manager.itemListData.items.IndexOf(filteredItems[selectedItem]);
                        if (manager.startItems.Find(i => i.id == manager.itemListData.items[indexSelected].id) != null)
                        {
                            isValid = false;
                            EditorGUILayout.HelpBox("This item already exist", MessageType.Error);
                        }
                        GUILayout.BeginHorizontal();

                        if (isValid && GUILayout.Button("Add", EditorStyles.miniButton))
                        {
                            itemReferenceList.arraySize++;

                            itemReferenceList.GetArrayElementAtIndex(itemReferenceList.arraySize - 1).FindPropertyRelative("id").intValue = manager.itemListData.items[indexSelected].id;
                            itemReferenceList.GetArrayElementAtIndex(itemReferenceList.arraySize - 1).FindPropertyRelative("amount").intValue = 1;
                            EditorUtility.SetDirty(manager);
                            serializedObject.ApplyModifiedProperties();
                            inAddItem = false;
                        }
                        if (GUILayout.Button("Cancel", EditorStyles.miniButton))
                        {
                            inAddItem = false;
                        }
                        GUILayout.EndHorizontal();


                        GUILayout.EndVertical();
                    }

                    GUIStyle boxStyle = new GUIStyle(GUI.skin.box);
                    scroll = GUILayout.BeginScrollView(scroll, GUILayout.MinHeight(200), GUILayout.ExpandHeight(false), GUILayout.ExpandWidth(false));
                    for (int i = 0; i < manager.startItems.Count; i++)
                    {
                        var item = manager.itemListData.items.Find(t => t.id.Equals(manager.startItems[i].id));
                        if (item)
                        {
                            GUILayout.BeginVertical("box");
                            GUILayout.BeginHorizontal();

                            GUILayout.BeginHorizontal("box");
                            var rect = GUILayoutUtility.GetRect(20, 20);

                            if (item.icon != null)
                            {
                                DrawTextureGUI(rect, item.icon, new Vector2(30, 30));
                            }

                            var name = " ID " + item.id.ToString("00") + "\n - " + item.name + "\n - " + item.type.ToString();
                            var content = new GUIContent(name, null, "Click to Open");
                            GUILayout.Label(content, EditorStyles.miniLabel);
                            GUILayout.BeginVertical("box", GUILayout.Height(20), GUILayout.Width(100));
                            GUILayout.BeginHorizontal();
                            GUILayout.Label("Amount", EditorStyles.miniLabel);
                            manager.startItems[i].amount = EditorGUILayout.IntField(manager.startItems[i].amount, GUILayout.Width(40));

                            if (manager.startItems[i].amount < 1)
                            {
                                manager.startItems[i].amount = 1;
                            }
                            GUILayout.EndHorizontal();
                            if (item.attributes.Count > 0)
                                manager.startItems[i].changeAttributes = GUILayout.Toggle(manager.startItems[i].changeAttributes, new GUIContent("Change Attributes", "This is a override of the original item attributes"), EditorStyles.miniButton, GUILayout.Width(100));
                            GUILayout.EndVertical();
                            GUILayout.Space(10);
                            GUILayout.EndHorizontal();
                            if (GUILayout.Button("x", GUILayout.Width(20), GUILayout.Height(50)))
                            {
                                itemReferenceList.DeleteArrayElementAtIndex(i);
                                EditorUtility.SetDirty(target);
                                serializedObject.ApplyModifiedProperties();
                                break;
                            }

                            GUILayout.EndHorizontal();
                            Color backgroundColor = GUI.backgroundColor;
                            GUI.backgroundColor = Color.clear;
                            var _rec = GUILayoutUtility.GetLastRect();
                            _rec.width -= 100;

                            EditorGUIUtility.AddCursorRect(_rec, MouseCursor.Link);

                            if (GUI.Button(_rec, ""))
                            {
                                if (manager.itemListData.inEdition)
                                {
                                    if (vItemListWindow.Instance != null)
                                        vItemListWindow.SetCurrentSelectedItem(manager.itemListData.items.IndexOf(item));
                                    else
                                        vItemListWindow.CreateWindow(manager.itemListData, manager.itemListData.items.IndexOf(item));
                                }
                                else
                                    vItemListWindow.CreateWindow(manager.itemListData, manager.itemListData.items.IndexOf(item));
                            }

                            GUI.backgroundColor = backgroundColor;
                            if (item.attributes != null && item.attributes.Count > 0)
                            {

                                if (manager.startItems[i].changeAttributes)
                                {
                                    if (GUILayout.Button("Reset", EditorStyles.miniButton))
                                    {
                                        manager.startItems[i].attributes = null;

                                    }
                                    if (manager.startItems[i].attributes == null)
                                    {
                                        manager.startItems[i].attributes = item.attributes.CopyAsNew();
                                    }
                                    else if (manager.startItems[i].attributes.Count != item.attributes.Count)
                                    {
                                        manager.startItems[i].attributes = item.attributes.CopyAsNew();
                                    }
                                    else
                                    {
                                        for (int a = 0; a < manager.startItems[i].attributes.Count; a++)
                                        {
                                            GUILayout.BeginHorizontal();
                                            GUILayout.Label(manager.startItems[i].attributes[a].name.ToString());
                                            manager.startItems[i].attributes[a].value = EditorGUILayout.IntField(manager.startItems[i].attributes[a].value, GUILayout.MaxWidth(60));
                                            GUILayout.EndHorizontal();
                                        }
                                    }
                                }
                            }

                            GUILayout.EndVertical();
                        }
                        else
                        {
                            itemReferenceList.DeleteArrayElementAtIndex(i);
                            EditorUtility.SetDirty(manager);
                            serializedObject.ApplyModifiedProperties();
                            break;
                        }
                    }

                    GUILayout.EndScrollView();
                    GUI.skin.box = boxStyle;

                    GUILayout.EndVertical();
                    if (GUI.changed)
                    {
                        EditorUtility.SetDirty(manager);
                        serializedObject.ApplyModifiedProperties();
                    }
                }

                var equipPoints = serializedObject.FindProperty("equipPoints");
                var applyAttributeEvents = serializedObject.FindProperty("applyAttributeEvents");
                var onUseItem = serializedObject.FindProperty("onUseItem");
                var onAddItem = serializedObject.FindProperty("onAddItem");
                var onLeaveItem = serializedObject.FindProperty("onLeaveItem");
                var onDropItem = serializedObject.FindProperty("onDropItem");
                var onOpenCloseInventoty = serializedObject.FindProperty("onOpenCloseInventory");
                var onEquipItem = serializedObject.FindProperty("onEquipItem");
                var onUnequipItem = serializedObject.FindProperty("onUnequipItem");
                if (equipPoints.arraySize != inEdition.Length)
                {
                    inEdition = new bool[equipPoints.arraySize];
                    newPointNames = new string[manager.equipPoints.Count];
                }
                if (equipPoints != null) DrawEquipPoints(equipPoints);
                if (applyAttributeEvents != null) DrawAttributeEvents(applyAttributeEvents);
                GUILayout.BeginVertical("box");
                showManagerEvents = GUILayout.Toggle(showManagerEvents, showManagerEvents ? "Close Events" : "Open Events", EditorStyles.miniButton);
                GUI.skin = oldSkin;
                if (showManagerEvents)
                {
                    if (onOpenCloseInventoty != null) EditorGUILayout.PropertyField(onOpenCloseInventoty);
                    if (onAddItem != null) EditorGUILayout.PropertyField(onAddItem);
                    if (onUseItem != null) EditorGUILayout.PropertyField(onUseItem);
                    if (onDropItem != null) EditorGUILayout.PropertyField(onDropItem);
                    if (onLeaveItem != null) EditorGUILayout.PropertyField(onLeaveItem);

                    if (onEquipItem != null) EditorGUILayout.PropertyField(onEquipItem);
                    if (onUnequipItem != null) EditorGUILayout.PropertyField(onUnequipItem);
                }
                GUI.skin = skin;
                GUILayout.EndVertical();
            }
            GUILayout.EndVertical();
            if (GUI.changed)
            {
                EditorUtility.SetDirty(manager);
                serializedObject.ApplyModifiedProperties();
            }

            GUI.skin = oldSkin;
        }

        void DrawTextureGUI(Rect position, Sprite sprite, Vector2 size)
        {
            Rect spriteRect = new Rect(sprite.rect.x / sprite.texture.width, sprite.rect.y / sprite.texture.height,
                                       sprite.rect.width / sprite.texture.width, sprite.rect.height / sprite.texture.height);
            Vector2 actualSize = size;
            actualSize.y *= (sprite.rect.height / sprite.rect.width);
            GUI.DrawTextureWithTexCoords(new Rect(position.x, position.y + (size.y - actualSize.y) / 2, actualSize.x, actualSize.y), sprite.texture, spriteRect);

        }

        GUIContent GetItemContent(vItem item)
        {
            var texture = item.icon != null ? item.icon.texture : null;
            return new GUIContent(item.name, texture, item.description); ;
        }

        List<vItem> GetItemByFilter(List<vItem> items, List<vItemType> filter)
        {
            return items.FindAll(i => filter.Contains(i.type));
        }

        GUIContent[] GetItemContents(List<vItem> items)
        {
            GUIContent[] names = new GUIContent[items.Count];
            for (int i = 0; i < items.Count; i++)
            {
                var texture = items[i].icon != null ? items[i].icon.texture : null;
                names[i] = new GUIContent(items[i].name, texture, items[i].description);
            }
            return names;
        }

        void DrawEquipPoints(SerializedProperty prop)
        {
            GUILayout.BeginVertical("box");
            prop.isExpanded = GUILayout.Toggle(prop.isExpanded, prop.isExpanded ? "Close Equip Points" : "Open Equip Points", EditorStyles.miniButton);
            if (prop.isExpanded)
            {
                prop.arraySize = EditorGUILayout.IntField("Points", prop.arraySize);
                for (int i = 0; i < prop.arraySize; i++)
                {
                    var handler = prop.GetArrayElementAtIndex(i).FindPropertyRelative("handler");
                    var equipPointName = prop.GetArrayElementAtIndex(i).FindPropertyRelative("equipPointName");
                    var defaultPoint = handler.FindPropertyRelative("defaultHandler");
                    var points = handler.FindPropertyRelative("customHandlers");
                    var onInstantiateEquiment = prop.GetArrayElementAtIndex(i).FindPropertyRelative("onInstantiateEquiment");

                    try
                    {
                        GUILayout.BeginVertical("box");
                        GUILayout.BeginHorizontal();
                        EditorGUILayout.PropertyField(equipPointName);
                        if (GUILayout.Button("X", EditorStyles.miniButton, GUILayout.Width(20)))
                        {
                            prop.DeleteArrayElementAtIndex(i);
                            GUILayout.EndHorizontal();
                            break;
                        }
                        GUILayout.EndHorizontal();
                        EditorGUILayout.PropertyField(defaultPoint);
                        GUILayout.BeginVertical("box");
                        points.isExpanded = GUILayout.Toggle(points.isExpanded, "Custom Handles", EditorStyles.miniButton);
                        if (points.isExpanded)
                        {
                            GUILayout.Space(5);
                            if (!inEdition[i] && GUILayout.Button("New Handler", EditorStyles.miniButton))
                            {
                                inEdition[i] = true;
                                if (equipPointName.stringValue.Contains("Left") || equipPointName.stringValue.Contains("left"))
                                {
                                    if (animator)
                                        parentBone = animator.GetBoneTransform(HumanBodyBones.LeftHand);

                                }
                                else
                                {
                                    if (animator)
                                        parentBone = animator.GetBoneTransform(HumanBodyBones.RightHand);
                                }
                            }

                            if (inEdition[i])
                            {
                                GUILayout.Box("New Custom Handler");

                                parentBone = (Transform)EditorGUILayout.ObjectField("Parent Bone", parentBone, typeof(Transform), true);
                                newPointNames[i] = EditorGUILayout.TextField("Custom Handler Name", newPointNames[i]);
                                bool valid = true;
                                if (string.IsNullOrEmpty(newPointNames[i]))
                                {
                                    valid = false;
                                    EditorGUILayout.HelpBox("Custom Handler Name is empty", MessageType.Error);
                                }
                                var array = ConvertToArray<Transform>(points);
                                if (Array.Exists<Transform>(array, point => point.gameObject.name.Equals(newPointNames[i])))
                                {
                                    valid = false;
                                    EditorGUILayout.HelpBox("Custom Handler Name already exist", MessageType.Error);
                                }

                                GUILayout.BeginHorizontal();
                                if (GUILayout.Button("Cancel", EditorStyles.miniButton))
                                {
                                    inEdition[i] = false;
                                }
                                GUI.enabled = parentBone && valid;

                                if (GUILayout.Button("Create", EditorStyles.miniButton))
                                {
                                    var customPoint = new GameObject(newPointNames[i]);

                                    customPoint.transform.parent = parentBone;
                                    customPoint.transform.localPosition = Vector3.zero;
                                    customPoint.transform.forward = manager.transform.forward;
                                    points.arraySize++;
                                    points.GetArrayElementAtIndex(points.arraySize - 1).objectReferenceValue = customPoint.transform;
                                    EditorUtility.SetDirty(manager);
                                    serializedObject.ApplyModifiedProperties();
                                    inEdition[i] = false;
                                }

                                GUI.enabled = true;
                                GUILayout.EndHorizontal();
                            }

                            GUILayout.Space(5);
                            for (int a = 0; a < points.arraySize; a++)
                            {
                                var remove = false;
                                GUILayout.BeginHorizontal();
                                EditorGUILayout.PropertyField(points.GetArrayElementAtIndex(a), true);
                                if (GUILayout.Button("X", EditorStyles.miniButton, GUILayout.Width(20)))
                                    remove = true;
                                GUILayout.EndHorizontal();
                                if (remove)
                                {
                                    var obj = (Transform)points.GetArrayElementAtIndex(a).objectReferenceValue;
                                    points.DeleteArrayElementAtIndex(a);
                                    if (obj)
                                    {
                                        points.DeleteArrayElementAtIndex(a);
                                        DestroyImmediate(obj.gameObject);
                                    }
                                    EditorUtility.SetDirty(manager);
                                    serializedObject.ApplyModifiedProperties();
                                    break;
                                }
                            }
                        }
                        GUILayout.EndVertical();

                        GUI.skin = oldSkin;
                        if (onInstantiateEquiment != null) EditorGUILayout.PropertyField(onInstantiateEquiment);

                        GUI.skin = skin;
                        GUILayout.EndVertical();
                    }
                    catch { }

                }
            }
            GUILayout.EndVertical();
        }

        T[] ConvertToArray<T>(SerializedProperty prop)
        {
            T[] value = new T[prop.arraySize];
            for (int i = 0; i < prop.arraySize; i++)
            {
                object element = prop.GetArrayElementAtIndex(i).objectReferenceValue;
                value[i] = (T)element;
            }
            return value;
        }

        void DrawAttributeEvents(SerializedProperty prop)
        {
            GUILayout.BeginVertical("box");
            prop.isExpanded = GUILayout.Toggle(prop.isExpanded, prop.isExpanded ? "Close Attribute Events" : "Open Attribute Events", EditorStyles.miniButton);
            if (prop.isExpanded)
            {
                prop.arraySize = EditorGUILayout.IntField("Attributes", prop.arraySize);
                for (int i = 0; i < prop.arraySize; i++)
                {

                    var attributeName = prop.GetArrayElementAtIndex(i).FindPropertyRelative("attribute");
                    var onApplyAttribute = prop.GetArrayElementAtIndex(i).FindPropertyRelative("onApplyAttribute");
                    try
                    {
                        GUILayout.BeginVertical("box");
                        GUILayout.BeginHorizontal();
                        EditorGUILayout.PropertyField(attributeName);
                        if (GUILayout.Button("X", EditorStyles.miniButton, GUILayout.Width(20)))
                        {
                            prop.DeleteArrayElementAtIndex(i);
                            GUILayout.EndHorizontal();
                            break;
                        }
                        GUILayout.EndHorizontal();
                        GUI.skin = oldSkin;
                        EditorGUILayout.PropertyField(onApplyAttribute);
                        GUI.skin = skin;
                        GUILayout.EndVertical();
                    }
                    catch { }

                }
            }
            GUILayout.EndVertical();
        }
    }
}
