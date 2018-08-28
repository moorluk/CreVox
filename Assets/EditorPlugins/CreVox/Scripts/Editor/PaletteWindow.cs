using UnityEngine;
using UnityEditor;
using System;
using System.Collections.Generic;

namespace CreVox
{

    public class PaletteWindow : EditorWindow
    {
        [NonSerialized] public static PaletteWindow Instance;

        WorldPos m_index = new WorldPos(0, 0, 0);

        List<PaletteItem> m_items;
        Dictionary<PaletteItem, Texture2D> m_previews;
        Dictionary<WorldPos, List<PaletteItem>> m_itemSets;

        static string m_path = PathCollect.resourcesPath + PathCollect.pieces;
        Vector2 m_scrollPosition;
        static float m_buttonWidth = 90;

        public delegate void itemSelectedDelegate(PaletteItem item, Texture2D preview);
        public static event itemSelectedDelegate ItemSelectedEvent;

        public static void ShowPalette()
        {
            Instance = (PaletteWindow)GetWindow(typeof(PaletteWindow));
            Instance.titleContent = new GUIContent("Palette");
        }

        void OnEnable()
        {
            InitContent();
        }

        void OnGUI()
        {
            if (m_previews.Count != m_items.Count)
            {
                InitContent();
            }
            DrawTabs();
            DrawScroll();
            DrawFunction();
        }

        List<PaletteItem.Set> m_sList;
        List<PaletteItem.Module> m_mList;
        public void InitContent()
        {
            // Set the ScrollList
            m_items = EditorUtils.GetAssetsWithScript<PaletteItem>(m_path);
            foreach (PaletteItem item in m_items)
            {
                if (item.assetPath.Length < 1)
                    item.assetPath = AssetDatabase.GetAssetPath(item);
            }
            m_itemSets = new Dictionary<WorldPos, List<PaletteItem>>();

            // Init the Dictionary
            m_sList = EditorUtils.GetListFromEnum<PaletteItem.Set>();
            m_mList = EditorUtils.GetListFromEnum<PaletteItem.Module>();
            for (int i_s = 0; i_s < m_sList.Count; i_s++)
            {
                for (int i_m = 0; i_m < m_mList.Count; i_m++)
                {
                    WorldPos index = new WorldPos((int)m_sList[i_s], (int)m_mList[i_m], 0);
                    List<PaletteItem> pList = new List<PaletteItem>();

                    // Assign items
                    foreach (PaletteItem item in m_items)
                    {
                        if ((int)item.m_module != index.y)
                            continue;
                        if ((item.m_set & index.x) > 0)
                            pList.Add(item);
                    }

                    m_itemSets.Add(index, pList);
                }
            }

            GeneratePreviews();
        }

        void DrawFunction()
        {
            EditorGUILayout.BeginHorizontal(EditorStyles.textField);
            EditorGUILayout.LabelField(m_path, EditorStyles.miniLabel);
            EditorGUILayout.LabelField("Button Size", GUILayout.Width(70));
            m_buttonWidth = GUILayout.HorizontalSlider(m_buttonWidth, 90f, 150f, GUILayout.Width(200));
            if (GUILayout.Button("Refresh", EditorStyles.miniButton, GUILayout.Width(90)))
            {
                InitContent();
            }
            EditorGUILayout.EndHorizontal();
        }

        int m_setIndex, m_mdlIndex;
        void DrawTabs()
        {
            EditorGUILayout.Space();
            m_setIndex = GUILayout.Toolbar(m_setIndex, Enum.GetNames(typeof(PaletteItem.Set)));
            m_index.x = (int)m_sList[m_setIndex];
            m_mdlIndex = GUILayout.Toolbar(m_mdlIndex, Enum.GetNames(typeof(PaletteItem.Module)));
            m_index.y = (int)m_mList[m_mdlIndex];
            GUILayout.Label(m_index.ToString());
        }

        void DrawScroll()
        {
            if (!m_itemSets.ContainsKey(m_index) || m_itemSets[m_index].Count == 0)
            {
                EditorGUILayout.HelpBox("This category is empty!", MessageType.Info);
                return;
            }
            int rowCapacity = Mathf.FloorToInt(position.width / (m_buttonWidth));
            using (var sc = new GUILayout.ScrollViewScope(m_scrollPosition))
            {
                m_scrollPosition = sc.scrollPosition;
                int selectionGridIndex = -1;
                selectionGridIndex = GUILayout.SelectionGrid(selectionGridIndex, GetGUIContentsFromItems(), rowCapacity, GetGUIStyle());
                GetSelectedItem(selectionGridIndex);
            }
        }

        void GeneratePreviews()
        {
            m_previews = new Dictionary<PaletteItem, Texture2D>();
            AssetPreview.SetPreviewTextureCacheSize(m_items.Count * 2);
            foreach (PaletteItem item in m_items)
            {
                if (!m_previews.ContainsKey(item))
                {
                    Texture2D preview = AssetPreview.GetAssetPreview(item.gameObject) ?? AssetPreview.GetMiniTypeThumbnail(typeof(GameObject));
                    m_previews.Add(item, preview);
                }
            }
        }

        GUIContent[] GetGUIContentsFromItems()
        {
            List<GUIContent> guiContents = new List<GUIContent>();
            if (m_previews.Count == m_items.Count)
            {
                int totalItems = m_itemSets[m_index].Count;
                for (int i = 0; i < totalItems; i++)
                {
                    GUIContent guiContent = new GUIContent();
                    if (m_itemSets[m_index][i] != null)
                    {
                        guiContent.text = m_itemSets[m_index][i].gameObject.name + "\n" + m_itemSets[m_index][i].itemName;
                        guiContent.image = m_previews[m_itemSets[m_index][i]];
                    }
                    else
                    {
                        InitContent();
                        break;
                    }
                    guiContents.Add(guiContent);
                }
            }
            return guiContents.ToArray();
        }

        GUIStyle GetGUIStyle()
        {
            GUIStyle guiStyle = new GUIStyle(GUI.skin.button)
            {
                fontSize = 9,
                alignment = TextAnchor.LowerCenter,
                imagePosition = ImagePosition.ImageAbove,
                fixedWidth = m_buttonWidth,
                fixedHeight = m_buttonWidth + 9,
            };
            return guiStyle;
        }

        void GetSelectedItem(int index)
        {
            if (index != -1)
            {
                PaletteItem selectedItem = m_itemSets[m_index][index];
                if (ItemSelectedEvent != null)
                    ItemSelectedEvent(selectedItem, m_previews[selectedItem]);
            }
        }

    }
}