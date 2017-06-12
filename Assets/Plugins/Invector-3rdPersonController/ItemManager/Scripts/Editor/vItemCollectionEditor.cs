using UnityEngine;
using System.Collections;
using UnityEditor;
using System.Collections.Generic;
using System.Linq;

namespace Invector.ItemManager
{
    [CanEditMultipleObjects]
    [CustomEditor(typeof(vItemCollection))]
    public class vItemCollectionEditor : Editor
    {
        vItemCollection collection;
        SerializedProperty itemReferenceList;
        GUISkin skin;
        bool inAddItem;
        int selectedItem;
        List<vItem> filteredItems;
        Vector2 scroll;
        private Texture2D m_Logo = null;

        void OnEnable()
        {
            m_Logo = (Texture2D)Resources.Load("icon_v2", typeof(Texture2D));
            collection = (vItemCollection)target;
            skin = Resources.Load("skin") as GUISkin;
            itemReferenceList = serializedObject.FindProperty("items");
        }

        public override void OnInspectorGUI()
        {
            var oldSkin = GUI.skin;

            serializedObject.Update();
            if (skin) GUI.skin = skin;
            GUILayout.BeginVertical("Item Collection", "window");
            GUILayout.Label(m_Logo, GUILayout.MaxHeight(25));
            GUILayout.Space(10);

            GUI.skin = oldSkin;
            base.OnInspectorGUI();
            if (skin) GUI.skin = skin;

            if (collection.itemListData)
            {
                GUILayout.BeginVertical("box");
                if (itemReferenceList.arraySize > collection.itemListData.items.Count)
                {
                    collection.items.Resize(collection.itemListData.items.Count);
                }
                GUILayout.Box("Item Collection " + collection.items.Count);
                filteredItems = collection.itemsFilter.Count > 0 ? GetItemByFilter(collection.itemListData.items, collection.itemsFilter) : collection.itemListData.items;

                if (!inAddItem && filteredItems.Count > 0 && GUILayout.Button("Add Item", EditorStyles.miniButton))
                {
                    inAddItem = true;
                }
                if (inAddItem && filteredItems.Count > 0)
                {
                    GUILayout.BeginVertical("box");
                    selectedItem = EditorGUILayout.Popup(new GUIContent("SelectItem"), selectedItem, GetItemContents(filteredItems));
                    bool isValid = true;
                    var indexSelected = collection.itemListData.items.IndexOf(filteredItems[selectedItem]);
                    if (collection.items.Find(i => i.id == collection.itemListData.items[indexSelected].id) != null)
                    {
                        isValid = false;
                        EditorGUILayout.HelpBox("This item already exist", MessageType.Error);
                    }
                    GUILayout.BeginHorizontal();

                    if (isValid && GUILayout.Button("Add", EditorStyles.miniButton))
                    {
                        itemReferenceList.arraySize++;
                        itemReferenceList.GetArrayElementAtIndex(itemReferenceList.arraySize - 1).FindPropertyRelative("id").intValue = collection.itemListData.items[indexSelected].id;
                        itemReferenceList.GetArrayElementAtIndex(itemReferenceList.arraySize - 1).FindPropertyRelative("amount").intValue = 1;
                        EditorUtility.SetDirty(collection);
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
                for (int i = 0; i < collection.items.Count; i++)
                {
                    var item = collection.itemListData.items.Find(t => t.id.Equals(collection.items[i].id));
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
                        collection.items[i].amount = EditorGUILayout.IntField(collection.items[i].amount, GUILayout.Width(40));

                        if (collection.items[i].amount < 1)
                        {
                            collection.items[i].amount = 1;
                        }
                        GUILayout.EndHorizontal();
                        if (item.attributes.Count > 0)
                            collection.items[i].changeAttributes = GUILayout.Toggle(collection.items[i].changeAttributes, new GUIContent("Change Attributes", "This is a override of the original item attributes"), EditorStyles.miniButton, GUILayout.Width(100));
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
                            if (collection.itemListData.inEdition)
                            {
                                if (vItemListWindow.Instance != null)
                                    vItemListWindow.SetCurrentSelectedItem(collection.itemListData.items.IndexOf(item));
                                else
                                    vItemListWindow.CreateWindow(collection.itemListData, collection.itemListData.items.IndexOf(item));
                            }
                            else
                                vItemListWindow.CreateWindow(collection.itemListData, collection.itemListData.items.IndexOf(item));
                        }
                        GUI.backgroundColor = backgroundColor;
                        if (item.attributes != null && item.attributes.Count > 0)
                        {
                            if (collection.items[i].changeAttributes)
                            {
                                if (GUILayout.Button("Reset", EditorStyles.miniButton))
                                {
                                    collection.items[i].attributes = null;
                                }
                                if (collection.items[i].attributes == null)
                                {
                                    collection.items[i].attributes = item.attributes.CopyAsNew();
                                }
                                else if (collection.items[i].attributes.Count != item.attributes.Count)
                                {
                                    collection.items[i].attributes = item.attributes.CopyAsNew();
                                }
                                else
                                {
                                    for (int a = 0; a < collection.items[i].attributes.Count; a++)
                                    {
                                        GUILayout.BeginHorizontal();
                                        GUILayout.Label(collection.items[i].attributes[a].name.ToString());
                                        collection.items[i].attributes[a].value = EditorGUILayout.IntField(collection.items[i].attributes[a].value, GUILayout.MaxWidth(60));
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
                        EditorUtility.SetDirty(collection);
                        serializedObject.ApplyModifiedProperties();
                        break;
                    }
                }

                GUILayout.EndScrollView();
                GUI.skin.box = boxStyle;

                GUILayout.EndVertical();
                if (GUI.changed)
                {
                    EditorUtility.SetDirty(collection);
                    serializedObject.ApplyModifiedProperties();
                }
            }
            GUILayout.EndVertical();
            if (GUI.changed)
            {
                EditorUtility.SetDirty(target);
            }
            serializedObject.ApplyModifiedProperties();
            GUI.skin = oldSkin;
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

        List<vItem> GetItemByFilter(List<vItem> items, List<vItemType> filter)
        {
            return items.FindAll(i => filter.Contains(i.type));
        }

        void DrawTextureGUI(Rect position, Sprite sprite, Vector2 size)
        {
            Rect spriteRect = new Rect(sprite.rect.x / sprite.texture.width, sprite.rect.y / sprite.texture.height,
                                       sprite.rect.width / sprite.texture.width, sprite.rect.height / sprite.texture.height);
            Vector2 actualSize = size;

            actualSize.y *= (sprite.rect.height / sprite.rect.width);
            GUI.DrawTextureWithTexCoords(new Rect(position.x, position.y + (size.y - actualSize.y) / 2, actualSize.x, actualSize.y), sprite.texture, spriteRect);
        }
    }
}