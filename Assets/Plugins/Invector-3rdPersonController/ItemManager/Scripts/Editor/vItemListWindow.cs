using UnityEngine;
using System.Collections;
using UnityEditor;
using System.Linq;
namespace Invector.ItemManager
{
    public class vItemListWindow : EditorWindow
    {
        public static vItemListWindow Instance;
        public vItemListData itemList;
        [SerializeField]
        protected GUISkin skin;
        protected SerializedObject serializedObject;
        protected vItem addItem;
        protected vItemDrawer addItemDrawer;
        protected vItemDrawer currentItemDrawer;
        protected bool inAddItem;
        protected bool openAttributeList;
        protected bool inCreateAttribute;       
        protected string attributeName;      
        protected int indexSelected;
        protected Vector2 scroolView;
        protected Vector2 attributesScroll;
        private Texture2D m_Logo = null;

        void OnEnable()
        {
            m_Logo = (Texture2D)Resources.Load("icon_v2", typeof(Texture2D));
        }

        public static void CreateWindow(vItemListData itemList)
        {
            vItemListWindow window = (vItemListWindow)EditorWindow.GetWindow(typeof(vItemListWindow), false, "ItemList Editor");
            Instance = window;
            window.itemList = itemList;
            window.skin = Resources.Load("skin") as GUISkin;
            Instance.Init();
        }

        public static void CreateWindow(vItemListData itemList, int firtItemSelected)
        {
            vItemListWindow window = (vItemListWindow)EditorWindow.GetWindow(typeof(vItemListWindow), false, "ItemList Editor");
            Instance = window;
            window.itemList = itemList;
            window.skin = Resources.Load("skin") as GUISkin;
            Instance.Init(firtItemSelected);
        }

        void Init()
        {
            serializedObject = new SerializedObject(itemList);
            var subAssets = AssetDatabase.LoadAllAssetsAtPath(AssetDatabase.GetAssetPath(itemList));
            skin = Resources.Load("skin") as GUISkin;
            if (subAssets.Length > 1)
            {
                for (int i = subAssets.Length - 1; i >= 0; i--)
                {
                    var item = subAssets[i] as vItem;

                    if (item && !itemList.items.Contains(item))
                    {
                        item.id = GetUniqueID();
                        itemList.items.Add(item);
                    }
                }
                EditorUtility.SetDirty(itemList);
                OrderByID();
            }
            itemList.inEdition = true;
            this.Show();
        }

        void Init(int firtItemSelected)
        {
            Init();
            SetCurrentSelectedItem(firtItemSelected);
        }

        public void OnGUI()
        {
            if (skin) GUI.skin = skin;

            GUILayout.BeginVertical("Item List", "window");
            GUILayout.Label(m_Logo, GUILayout.MaxHeight(25));
            GUILayout.Space(10);

            GUILayout.BeginVertical("box");

            GUI.enabled = !Application.isPlaying;
            itemList = EditorGUILayout.ObjectField("ItemListData", itemList, typeof(vItemListData), false) as vItemListData;

            if (serializedObject == null && itemList != null)
            {
                serializedObject = new SerializedObject(itemList);
            }
            else if (itemList == null)
            {
                GUILayout.EndVertical();
                return;
            }

            serializedObject.Update();

            if (!inAddItem && GUILayout.Button("Create New Item"))
            {
                addItem = ScriptableObject.CreateInstance<vItem>();
                addItem.name = "New Item";

                currentItemDrawer = null;
                inAddItem = true;
            }
            if (inAddItem)
            {
                DrawAddItem();
            }
            if (GUILayout.Button("Open ItemEnums Editor"))
            {
                vItemEnumsWindow.CreateWindow();
            }
            GUILayout.Space(10);
            GUILayout.EndVertical();

            GUILayout.Box(itemList.items.Count.ToString("00") + " Items");
            scroolView = GUILayout.BeginScrollView(scroolView, GUILayout.ExpandWidth(true));
            for (int i = 0; i < itemList.items.Count; i++)
            {
                if (itemList.items[i] != null)
                {
                    Color color = GUI.color;
                    GUI.color = currentItemDrawer != null && currentItemDrawer.item == itemList.items[i] ? Color.green : color;
                    GUILayout.BeginVertical("box");
                    GUI.color = color;
                    GUILayout.BeginHorizontal();
                    var texture = itemList.items[i].iconTexture;
                    var name = " ID " + itemList.items[i].id.ToString("00") + "\n - " + itemList.items[i].name + "\n - " + itemList.items[i].type.ToString();
                    var content = new GUIContent(name, texture, currentItemDrawer != null && currentItemDrawer.item == itemList.items[i] ? "Click to Close" : "Click to Open");
                    GUIStyle boxStyle = new GUIStyle(GUI.skin.box);
                    GUI.skin.box.alignment = TextAnchor.UpperLeft;
                    GUI.skin.box.fontStyle = FontStyle.Italic;
                    GUI.skin.box.fontSize = 11;

                    if (GUILayout.Button(content, "box", GUILayout.Height(50), GUILayout.MinWidth(50)))
                    {
                        GUI.FocusControl("clearFocus");
                        scroolView.y = 1 + i * 60;
                        currentItemDrawer = currentItemDrawer != null ? currentItemDrawer.item == itemList.items[i] ? null : new vItemDrawer(itemList.items[i]) : new vItemDrawer(itemList.items[i]);
                    }
                    EditorGUIUtility.AddCursorRect(GUILayoutUtility.GetLastRect(), MouseCursor.Link);

                    GUI.skin.box = boxStyle;
                    var duplicateImage = Resources.Load("duplicate") as Texture;
                    if (GUILayout.Button(new GUIContent("", duplicateImage,"Duplicate Item"), GUILayout.MaxWidth(45), GUILayout.Height(45)))
                    {
                        if(EditorUtility.DisplayDialog("Duplicate the "+ itemList.items[i].name,
                        "Are you sure you want to duplicate this item? ", "Duplicate", "Cancel"))
                        {
                            DuplicateItem(itemList.items[i]);
                            GUILayout.EndHorizontal();
                            Repaint();
                            break;
                        }
                    }
                    if (GUILayout.Button(new GUIContent("x", "Delete Item"), GUILayout.MaxWidth(20), GUILayout.Height(45)))
                    {

                        if (EditorUtility.DisplayDialog("Delete the " + itemList.items[i].name,
                        "Are you sure you want to delete this item? ", "Delete", "Cancel"))
                        {

                            var item = itemList.items[i];
                            itemList.items.RemoveAt(i);
                            DestroyImmediate(item, true);
                            OrderByID();
                            AssetDatabase.SaveAssets();
                            serializedObject.ApplyModifiedProperties();
                            EditorUtility.SetDirty(itemList);
                            GUILayout.EndHorizontal();
                            Repaint();
                            break;
                        }
                    }
                   
                    GUILayout.EndHorizontal();

                    GUI.color = color;
                    if (currentItemDrawer != null && currentItemDrawer.item == itemList.items[i] && itemList.items.Contains(currentItemDrawer.item))
                    {
                        currentItemDrawer.DrawItem( ref itemList.items, false);
                    }

                    GUILayout.EndVertical();
                }
            }
            GUILayout.EndScrollView();

            GUILayout.EndVertical();
            if (GUI.changed || serializedObject.ApplyModifiedProperties())
            {
                EditorUtility.SetDirty(itemList);
            }
        }

        public static void SetCurrentSelectedItem(int index)
        {
            if (Instance != null && Instance.itemList != null && Instance.itemList.items != null && Instance.itemList.items.Count > 0 && index < Instance.itemList.items.Count)
            {
                Instance.currentItemDrawer = Instance.currentItemDrawer != null ? Instance.currentItemDrawer.item == Instance.itemList.items[index] ? null : new vItemDrawer(Instance.itemList.items[index]) : new vItemDrawer(Instance.itemList.items[index]);
                Instance.scroolView.y = 1 + index * 60;
                Instance.Repaint();
            }

        }

        void OnDestroy()
        {
            if (itemList)
            {
                itemList.inEdition = false;
            }
        }

        private void DrawAddItem()
        {
            GUILayout.BeginVertical("box");
            if (addItem != null)
            {
                if (addItemDrawer == null || addItemDrawer.item == null || addItemDrawer.item != addItem)
                    addItemDrawer = new vItemDrawer(addItem);
                bool isValid = true;
                if (addItemDrawer != null)
                {
                    GUILayout.Box("Create Item Window");
                    addItemDrawer.DrawItem( ref itemList.items, false, true);
                }

                if (string.IsNullOrEmpty(addItem.name))
                {
                    isValid = false;
                    EditorGUILayout.HelpBox("This item name cant be null or empty,please type a name", MessageType.Error);
                }

                if (itemList.items.FindAll(item => item.name.Equals(addItemDrawer.item.name)).Count > 0)
                {
                    isValid = false;
                    EditorGUILayout.HelpBox("This item name already exists", MessageType.Error);
                }
                GUILayout.BeginHorizontal("box", GUILayout.ExpandWidth(false));

                if (isValid && GUILayout.Button("Create"))
                {
                    AssetDatabase.AddObjectToAsset(addItem, AssetDatabase.GetAssetPath(itemList));
                    addItem.hideFlags = HideFlags.HideInHierarchy;
                    addItem.id = GetUniqueID();
                    itemList.items.Add(addItem);
                    OrderByID();
                    addItem = null;
                    inAddItem = false;
                    addItemDrawer = null;
                    serializedObject.ApplyModifiedProperties();
                    EditorUtility.SetDirty(itemList);
                    AssetDatabase.SaveAssets();
                }

                if (GUILayout.Button("Cancel"))
                {
                    addItem = null;
                    inAddItem = false;
                    addItemDrawer = null;
                    serializedObject.ApplyModifiedProperties();
                    EditorUtility.SetDirty(itemList);
                }
                GUILayout.EndHorizontal();
            }
            else
            {
                EditorGUILayout.HelpBox("Error", MessageType.Error);
            }
            GUILayout.EndVertical();
        }
        void DuplicateItem(vItem targetItem)
        {
            addItem = Instantiate(targetItem);
            AssetDatabase.AddObjectToAsset(addItem, AssetDatabase.GetAssetPath(itemList));
            addItem.hideFlags = HideFlags.HideInHierarchy;
            addItem.id = GetUniqueID();
            itemList.items.Add(addItem);
            OrderByID();
            addItem = null;
            inAddItem = false;
            addItemDrawer = null;
            serializedObject.ApplyModifiedProperties();
            EditorUtility.SetDirty(itemList);
            AssetDatabase.SaveAssets();
        }
        int GetUniqueID(int value = 0)
        {
            var result = value;


            for (int i = 0; i < itemList.items.Count + 1; i++)
            {
                var item = itemList.items.Find(t => t.id == i);
                if (item == null)
                {
                    result = i;
                    break;
                }

            }

            return result;
        }

        void OrderByID()
        {
            if (itemList && itemList.items != null && itemList.items.Count > 0)
            {
                var list = itemList.items.OrderBy(i => i.id).ToList();
                itemList.items = list;
            }

        }
    }
}
