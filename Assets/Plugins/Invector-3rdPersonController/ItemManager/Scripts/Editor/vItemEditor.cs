using UnityEngine;
using System.Collections;
using UnityEditor;
using System.Collections.Generic;
using System;
using Invector;

namespace Invector.ItemManager
{
    [CanEditMultipleObjects]
    [CustomEditor(typeof(vItem))]
    public class vItemEditor : Editor
    {
        public vItem item;
        public bool inAddAttribute;
        public vItemAttributes attribute;
        public int attributeValue;
        public int index;

        public bool inEditName;
        public string currentName;
        public GUISkin skin;
        public string[] drawPropertiesExcluding = new string[] { "id", "description", "type", "icon", "stackable", "maxStack", "amount", "originalObject","dropObject", "attributes", "isInEquipArea" };
        void OnEnable()
        {
            skin = Resources.Load("skin") as GUISkin;
            
        }
        public override void OnInspectorGUI()
        {
            if (skin) GUI.skin = skin;
            DrawItem();
        }
        public void DrawItem()
        {            
            if(item ==null) item = target as vItem;           
           
            serializedObject.Update();

            GUILayout.BeginVertical("box");
            GUILayout.BeginHorizontal("box");
            var name = " ID " + item.id.ToString("00") + "\n - " + item.name + "\n - " + item.type.ToString();
            var content = new GUIContent(name);
            GUILayout.Label(content, GUILayout.ExpandWidth(true));
            GUILayout.EndHorizontal();
            EditorGUILayout.LabelField("Description");
            EditorGUILayout.PropertyField(serializedObject.FindProperty("description"),new GUIContent(""));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("type"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("stackable"));
            //item.description = EditorGUILayout.TextArea(item.description);
            //item.type = (vItemType)EditorGUILayout.EnumPopup("Item Type", item.type);
            //item.stackable = EditorGUILayout.Toggle("Stackable", item.stackable);

            if (item.stackable)
            {
                if (item.maxStack <= 0) item.maxStack = 1;
                item.maxStack = EditorGUILayout.IntField("Max Stack", item.maxStack);
            }
            else item.maxStack = 1;

            GUILayout.EndVertical();

            GUILayout.BeginVertical("box");
            GUILayout.BeginHorizontal();
            GUILayout.Label("Icon");
            EditorGUILayout.PropertyField(serializedObject.FindProperty("icon"), new GUIContent(""));
            //item.icon = (Sprite)EditorGUILayout.ObjectField(item.icon, typeof(Sprite), false);
            var rect = GUILayoutUtility.GetRect(40, 40);

            if (item.icon != null)
            {
                DrawTextureGUI(rect, item.icon, new Vector2(40, 40));
            }
            GUILayout.EndHorizontal();
            GUILayout.BeginHorizontal();
            GUILayout.BeginVertical("box");
            GUILayout.Label("Original Object");
            EditorGUILayout.PropertyField(serializedObject.FindProperty("originalObject"),new GUIContent(""));
            //item.originalObject = (GameObject)EditorGUILayout.ObjectField(item.originalObject, typeof(GameObject), false);
            GUILayout.EndVertical();
            GUILayout.BeginVertical("box");
            GUILayout.Label("Drop Object");
            EditorGUILayout.PropertyField(serializedObject.FindProperty("dropObject"), new GUIContent(""));
            // item.dropObject = (GameObject)EditorGUILayout.ObjectField(item.dropObject, typeof(GameObject), false);
            GUILayout.EndVertical();
            GUILayout.EndHorizontal();

            GUILayout.EndVertical();           
            DrawAttributes();
            GUILayout.BeginVertical("box");
            GUILayout.Box(new GUIContent("Custom Settings", "This area is used for additional properties\n in vItem Properties in defaultInspector region"));
            DrawPropertiesExcluding(serializedObject, drawPropertiesExcluding);
            GUILayout.EndVertical();
            if (GUI.changed )
            {
                EditorUtility.SetDirty(item);
            }
            serializedObject.ApplyModifiedProperties();
        }

        void DrawAttributes()
        {
            try
            {

                GUILayout.BeginVertical("box");
                GUILayout.Box("Attributes", GUILayout.ExpandWidth(true));
                EditorGUILayout.Space();
                if (!inAddAttribute && GUILayout.Button("Add Attribute", EditorStyles.miniButton))
                    inAddAttribute = true;
                if (inAddAttribute)
                {
                    GUILayout.BeginHorizontal("box");
                    attribute = (vItemAttributes)EditorGUILayout.EnumPopup(attribute);
                    EditorGUILayout.LabelField("Value", GUILayout.MinWidth(60));
                    attributeValue = EditorGUILayout.IntField(attributeValue);
                    GUILayout.EndHorizontal();
                    if (item.attributes != null && item.attributes.Contains(attribute))
                    {
                        EditorGUILayout.HelpBox("This attribute already exist ", MessageType.Error);
                        if (GUILayout.Button("Cancel", EditorStyles.miniButton, GUILayout.MinWidth(60)))
                        {
                            inAddAttribute = false;
                        }
                    }
                    else
                    {
                        GUILayout.BeginHorizontal("box");
                        if (GUILayout.Button("Add", EditorStyles.miniButton, GUILayout.MinWidth(60)))
                        {
                            item.attributes.Add(new vItemAttribute(attribute, attributeValue));

                            attributeValue = 0;
                            inAddAttribute = false;

                        }
                        if (GUILayout.Button("Cancel", EditorStyles.miniButton, GUILayout.MinWidth(60)))
                        {
                            attributeValue = 0;
                            inAddAttribute = false;
                        }
                        GUILayout.EndHorizontal();
                    }
                }
                EditorGUILayout.Space();
                var attributesProp = serializedObject.FindProperty("attributes");
                for (int i = 0; i < item.attributes.Count; i++)
                {
                    GUILayout.BeginHorizontal("box");
                    var attributeValue = attributesProp.GetArrayElementAtIndex(i).FindPropertyRelative("value");
                    attributeValue.serializedObject.Update();
                    EditorGUILayout.LabelField(item.attributes[i].name.ToString(), GUILayout.MinWidth(60));
                    EditorGUILayout.PropertyField(attributeValue, new GUIContent(""));
                    //item.attributes[i].value = EditorGUILayout.IntField(item.attributes[i].value);

                    EditorGUILayout.Space();
                    if (GUILayout.Button("x", GUILayout.MaxWidth(30)))
                    {
                        item.attributes.RemoveAt(i);
                        GUILayout.EndHorizontal();
                        
                        break;
                    }
                    attributeValue.serializedObject.ApplyModifiedProperties();
                    GUILayout.EndHorizontal();
                }
                
                GUILayout.EndVertical();
            }
            catch { }
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

    public class vItemDrawer
    {
        public vItem item;
        bool inAddAttribute;
        vItemAttributes attribute;
        int attributeValue;
        int index;

        bool inEditName;
        string currentName;
        // public string[] drawPropertiesExcluding = new string[] { "id", "description", "type", "icon", "stackable", "maxStack", "amount", "originalObject", "dropObject", "attributes", "isInEquipArea" };
        Editor defaultEditor;
        public vItemDrawer(vItem item)
        {
            this.item = item;
            defaultEditor = Editor.CreateEditor(this.item);        
        }

        public void DrawItem(ref List<vItem> items, bool showObject = true, bool editName = false)
        {
            if (!item) return;
            SerializedObject _item = new SerializedObject(item);
            _item.Update();
            
            GUILayout.BeginVertical("box");

            if (showObject)
                EditorGUILayout.ObjectField(item, typeof(vItem), false);

          
            if (editName)
                item.name = EditorGUILayout.TextField("Item name", item.name);
            else
            {
                GUILayout.BeginHorizontal("box");
                GUILayout.Label(item.name, GUILayout.ExpandWidth(true));
                if(!inEditName && GUILayout.Button("EditName", EditorStyles.miniButton))
                {
                    currentName = item.name;
                    inEditName = true;
                }
                GUILayout.EndHorizontal();
            }
            if(inEditName)
            {
                var sameItemName = items.Find(i => i.name == currentName && i != item);
                currentName = EditorGUILayout.TextField("New Name", currentName);

                GUILayout.BeginHorizontal("box");
                if (sameItemName==null && !string.IsNullOrEmpty(currentName)&& GUILayout.Button("OK", EditorStyles.miniButton, GUILayout.MinWidth(60)))
                {
                    item.name = currentName;
                    inEditName = false;

                }
                if (GUILayout.Button("Cancel", EditorStyles.miniButton, GUILayout.MinWidth(60)))
                {
                    inEditName = false;
                }
                GUILayout.EndHorizontal();
                if (sameItemName != null)
                    EditorGUILayout.HelpBox("This name already exist", MessageType.Error);
                if(string.IsNullOrEmpty(currentName))
                    EditorGUILayout.HelpBox("This name can not be empty", MessageType.Error);
            }  

            EditorGUILayout.LabelField("Description");

            item.description = EditorGUILayout.TextArea(item.description);
            item.type = (vItemType)EditorGUILayout.EnumPopup("Item Type", item.type); 
            item.stackable = EditorGUILayout.Toggle("Stackable", item.stackable);

            if (item.stackable)
            {
                if (item.maxStack <= 0) item.maxStack = 1;
                item.maxStack = EditorGUILayout.IntField("Max Stack", item.maxStack);
            }
            else item.maxStack = 1;

            GUILayout.EndVertical();

            GUILayout.BeginVertical("box");
            GUILayout.BeginHorizontal();
            GUILayout.Label("Icon");
            item.icon = (Sprite)EditorGUILayout.ObjectField(item.icon, typeof(Sprite), false);
            var rect = GUILayoutUtility.GetRect(40, 40);

            if (item.icon != null)
            {
                DrawTextureGUI(rect, item.icon, new Vector2(40, 40));
            }
            GUILayout.EndHorizontal();
            GUILayout.BeginHorizontal();
            GUILayout.BeginVertical("box");
            GUILayout.Label("Original Object");
            item.originalObject = (GameObject)EditorGUILayout.ObjectField(item.originalObject, typeof(GameObject), false);
            GUILayout.EndVertical();
            GUILayout.BeginVertical("box");
            GUILayout.Label("Drop Object");
            item.dropObject = (GameObject)EditorGUILayout.ObjectField(item.dropObject, typeof(GameObject), false);
            GUILayout.EndVertical();
            GUILayout.EndHorizontal();
           
            GUILayout.EndVertical();

            DrawAttributes();
            GUILayout.BeginVertical("box");
            GUILayout.Box(new GUIContent("Custom Settings", "This area is used for additional properties\n in vItem Properties in defaultInspector region"));
            defaultEditor.DrawDefaultInspector();
            GUILayout.EndVertical();
            if (GUI.changed || _item.ApplyModifiedProperties())
            {
                EditorUtility.SetDirty(item);
            }
        }

        //public void DrawItem()
        //{
        //    if (!item) return;
        //    SerializedObject _item = new SerializedObject(item);
        //    _item.Update();

        //    GUILayout.BeginVertical("box");
        //    GUILayout.BeginHorizontal("box");
        //    var name = " ID " + item.id.ToString("00") + "\n - " + item.name + "\n - " + item.type.ToString();
        //    var content = new GUIContent(name);
        //    GUILayout.Label(content, GUILayout.ExpandWidth(true));
        //    GUILayout.EndHorizontal();
        //    EditorGUILayout.LabelField("Description");

        //    item.description = EditorGUILayout.TextArea(item.description);
        //    item.type = (vItemType)EditorGUILayout.EnumPopup("Item Type", item.type);
        //    item.stackable = EditorGUILayout.Toggle("Stackable", item.stackable);

        //    if (item.stackable)
        //    {
        //        if (item.maxStack <= 0) item.maxStack = 1;
        //        item.maxStack = EditorGUILayout.IntField("Max Stack", item.maxStack);
        //    }
        //    else item.maxStack = 1;

        //    GUILayout.EndVertical();

        //    GUILayout.BeginVertical("box");
        //    GUILayout.BeginHorizontal();
        //    GUILayout.Label("Icon");
        //    item.icon = (Sprite)EditorGUILayout.ObjectField(item.icon, typeof(Sprite), false);
        //    var rect = GUILayoutUtility.GetRect(40, 40);

        //    if (item.icon != null)
        //    {
        //        DrawTextureGUI(rect, item.icon, new Vector2(40, 40));
        //    }
        //    GUILayout.EndHorizontal();
        //    GUILayout.BeginHorizontal();
        //    GUILayout.BeginVertical("box");
        //    GUILayout.Label("Original Object");
        //    item.originalObject = (GameObject)EditorGUILayout.ObjectField(item.originalObject, typeof(GameObject), false);
        //    GUILayout.EndVertical();
        //    GUILayout.BeginVertical("box");
        //    GUILayout.Label("Drop Object");
        //    item.dropObject = (GameObject)EditorGUILayout.ObjectField(item.dropObject, typeof(GameObject), false);
        //    GUILayout.EndVertical();
        //    GUILayout.EndHorizontal();

        //    GUILayout.EndVertical();
        //    Debug.Log("OPA");
        //    DrawAttributes();
        //    GUILayout.BeginVertical("box");
        //    GUILayout.Box(new GUIContent("Custom Settings", "This area is used for additional properties\n in vItem Properties in defaultInspector region"));
        //    defaultInspector.DrawDefaultInspector();
        //    GUILayout.EndVertical();
        //    if (GUI.changed || _item.ApplyModifiedProperties())
        //    {
        //        EditorUtility.SetDirty(item);
        //    }
        //}

        void DrawAttributes()
        {
            try
            {
                
                GUILayout.BeginVertical("box");
                GUILayout.Box("Attributes", GUILayout.ExpandWidth(true)); 
                EditorGUILayout.Space();
                if (!inAddAttribute && GUILayout.Button("Add Attribute", EditorStyles.miniButton))
                    inAddAttribute = true;
                if (inAddAttribute)
                {
                    GUILayout.BeginHorizontal("box");
                    attribute = (vItemAttributes)EditorGUILayout.EnumPopup(attribute);
                    EditorGUILayout.LabelField("Value", GUILayout.MinWidth(60));
                    attributeValue = EditorGUILayout.IntField(attributeValue);
                    GUILayout.EndHorizontal();
                    if (item.attributes != null && item.attributes.Contains(attribute))
                    {
                        EditorGUILayout.HelpBox("This attribute already exist ", MessageType.Error);
                        if (GUILayout.Button("Cancel", EditorStyles.miniButton, GUILayout.MinWidth(60)))
                        {
                            inAddAttribute = false;
                        }
                    }
                    else
                    {
                        GUILayout.BeginHorizontal("box");
                        if (GUILayout.Button("Add", EditorStyles.miniButton, GUILayout.MinWidth(60)))
                        {
                            item.attributes.Add(new vItemAttribute(attribute, attributeValue));
                          
                            attributeValue = 0;
                            inAddAttribute = false;

                        }
                        if (GUILayout.Button("Cancel", EditorStyles.miniButton, GUILayout.MinWidth(60)))
                        {                          
                            attributeValue = 0;
                            inAddAttribute = false;
                        }
                        GUILayout.EndHorizontal();
                    }
                }
                EditorGUILayout.Space();
                for (int i = 0; i < item.attributes.Count; i++)
                {
                    GUILayout.BeginHorizontal("box");
                    EditorGUILayout.LabelField(item.attributes[i].name.ToString(), GUILayout.MinWidth(60));
                    item.attributes[i].value = EditorGUILayout.IntField(item.attributes[i].value);

                    EditorGUILayout.Space();
                    if (GUILayout.Button("x", GUILayout.MaxWidth(30)))
                    {
                        item.attributes.RemoveAt(i);
                        GUILayout.EndHorizontal();
                        break;
                    }
                    GUILayout.EndHorizontal();
                }
              
                GUILayout.EndVertical();
            }
            catch { }
        }

        void DrawTextureGUI(Rect position, Sprite sprite, Vector2 size)
        {
            Rect spriteRect = new Rect(sprite.rect.x / sprite.texture.width, sprite.rect.y / sprite.texture.height,
                                       sprite.rect.width / sprite.texture.width, sprite.rect.height / sprite.texture.height);
            Vector2 actualSize = size;

            actualSize.y *= (sprite.rect.height / sprite.rect.width);
            GUI.DrawTextureWithTexCoords(new Rect(position.x, position.y + (size.y - actualSize.y) / 2, actualSize.x, actualSize.y), sprite.texture, spriteRect);
        }

        public static List<T> FindAssetsByType<T>() where T : UnityEngine.Object
        {
            List<T> assets = new List<T>();
            string[] guids = AssetDatabase.FindAssets(string.Format("t:{0}", typeof(T)));
            for (int i = 0; i < guids.Length; i++)
            {
                string assetPath = AssetDatabase.GUIDToAssetPath(guids[i]);
                T asset = AssetDatabase.LoadAssetAtPath<T>(assetPath);
                if (asset != null)
                {
                    assets.Add(asset);
                }
            }

            return assets;
        }
    }
}

