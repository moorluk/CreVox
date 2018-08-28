using UnityEngine;
using UnityEditor;
using System;
using System.Collections.Generic;

namespace CreVox
{

    public class ArtPackWindow : EditorWindow
    {
        class Item
        {
            public PaletteItem paletteitem;
            public WorldPos index;
            public GameObject itemObject;
            public string itemName;
            public string artPack;
            public Texture2D preview;
        }

        static string _path = PathCollect.resourcesPath + PathCollect.artPack;
        public static ArtPackWindow Instance;

        string m_vgName = "";
        VGlobal m_vg;

        List<string> m_artPacks = new List<string>();

        List<PaletteItem.Set> m_setList;
        List<PaletteItem.Module> m_moduleList;

        List<Item> m_items;
        Dictionary<WorldPos, List<string>> m_itemNames;
        Dictionary<string, Dictionary<WorldPos, List<Item>>> m_itemCells;

        Vector2 m_scrollPos;
        Vector2 m_scrollPosX;
        Vector2 m_scrollPosY;
        float m_buttonWidth = 140, m_buttonHeight = 40;

        public static void ShowPalette()
        {
            Instance = (ArtPackWindow)GetWindow(typeof(ArtPackWindow));
            Instance.titleContent = new GUIContent("ArtPack");
        }

        void OnEnable()
        {
            InitContent();
        }

        void OnGUI()
        {
            DrawList();
            DrawScroll();
            DrawRenameTool();
            DrawFunction();
        }

        void InitContent()
        {
            if (m_vg == null) m_vg = VGlobal.GetSetting();

            m_setList = EditorUtils.GetListFromEnum<PaletteItem.Set>();
            m_moduleList = EditorUtils.GetListFromEnum<PaletteItem.Module>();
            showModules = new bool[m_moduleList.Count];
            //GetItems
            m_items = new List<Item>();
            List<PaletteItem> itemsP = EditorUtils.GetAssetsWithScript<PaletteItem>(_path.Remove(_path.Length - 1));
            AssetPreview.SetPreviewTextureCacheSize(itemsP.Count * 2);
            foreach (PaletteItem p in itemsP)
            {
                m_items.Add(new Item
                {
                    paletteitem = p,
                    index = new WorldPos(p.m_set, (int)p.m_module, m_moduleList.IndexOf(p.m_module)),
                    itemObject = p.gameObject,
                    itemName = p.gameObject.name,
                    preview = GetPreview(p.gameObject),
                    artPack = AssetDatabase.GetAssetPath(p.gameObject).Replace(_path, "").Split(new string[] { "/" }, StringSplitOptions.None)[0]
                });
            }
            var t = new Comparison<Item>((x, y) => x.artPack.CompareTo(y.artPack));
            m_items.Sort(t);

            //GetCategories
            List<WorldPos> _index = new List<WorldPos>();
            for (int i_s = 0; i_s < m_setList.Count; i_s++)
                for (int i_m = 0; i_m < m_moduleList.Count; i_m++)
                    _index.Add(new WorldPos((int)m_setList[i_s], (int)m_moduleList[i_m], i_m));

            m_artPacks = VGlobal.GetArtPacks();
            UpdateAppDict();

            //GetItemCells
            m_itemCells = new Dictionary<string, Dictionary<WorldPos, List<Item>>>();
            foreach (string a in m_artPacks)
            {
                var p = new Dictionary<WorldPos, List<Item>>();
                foreach (var c in _index)
                    p.Add(c, new List<Item>());
                m_itemCells.Add(a, p);
            }
            foreach (Item i in m_items)
            {
                if (!m_itemCells.ContainsKey(i.artPack)) Debug.LogError(i.artPack + " : " + i.itemName);
                else
                {
                    foreach (var di2 in m_itemCells[i.artPack])
                    {
                        if (i.index.y == di2.Key.y)
                            if ((i.index.x & di2.Key.x) > 0)
                                di2.Value.Add(i);
                    }
                    //if (!m_itemCells[i.artPack].ContainsKey(i.index)) Debug.LogError(i.artPack + " : " + i.index + " : " + i.itemName);
                    //else m_itemCells[i.artPack][i.index].Add(i);
                }
            }

            //GetItemNames
            m_itemNames = new Dictionary<WorldPos, List<string>>();
            foreach (WorldPos c in _index)
            {
                m_itemNames.Add(c, new List<string>());
                foreach (Item i in m_items)
                    if (!m_itemNames[c].Contains(i.itemName))
                        if (i.index.y == c.y)
                            if ((i.index.x & c.x) > 0)
                                m_itemNames[c].Add(i.itemName);
                m_itemNames[c].Sort();
            }

            //Log
            string logCell = "";
            foreach (var a in m_artPacks)
            {
                foreach (var c in _index)
                    logCell += a + "/" + c + ": " + m_itemCells[a][c].Count + "\n";
                logCell += " -------------\n";
            }
            Debug.Log(logCell);

            UpdateAppList();
            UpdateItemArrays(m_vg);
        }

        void DrawList()
        {
            GUIStyle listLabel = new GUIStyle(GUI.skin.FindStyle("ProgressBarBack"))
            {
                alignment = TextAnchor.MiddleLeft,
                fontSize = 14,
                fontStyle = FontStyle.Bold
            };

            string[] _ap = m_artPacks.ToArray();

            using (var h = new EditorGUILayout.HorizontalScope(GUILayout.Height(45)))
            {
                GUILayout.BeginScrollView(Vector2.zero, GUIStyle.none, GUIStyle.none, GUILayout.Width(m_buttonWidth + 5));
                using (var v = new EditorGUILayout.VerticalScope(GUILayout.Width(m_buttonWidth)))
                {
                    GUILayout.Label(m_artPacks[0], listLabel, GUILayout.Width(m_buttonWidth));
                    GUILayout.Label("Set Parent ArtPack", GUILayout.Width(m_buttonWidth));
                }
                GUILayout.EndScrollView();
                GUILayout.BeginScrollView(m_scrollPosX, GUIStyle.none, GUIStyle.none);
                EditorGUI.BeginChangeCheck();
                using (var h1 = new EditorGUILayout.HorizontalScope())
                {
                    for (int i = 1; i < m_artPacks.Count; i++)
                    {
                        using (var v = new EditorGUILayout.VerticalScope(GUILayout.Width(m_buttonWidth)))
                        {
                            string _c = m_artPacks[i];
                            GUILayout.Label(_c, listLabel, GUILayout.Width(m_buttonWidth));
                            _pDict[_c] = m_artPacks[EditorGUILayout.Popup(m_artPacks.IndexOf(_pDict[_c]), _ap, GUILayout.Width(m_buttonWidth))];
                        }
                    }
                    GUILayout.Label("", GUILayout.Width(15));
                }
                if (EditorGUI.EndChangeCheck())
                {
                    UpdateAppList();
                }
                GUILayout.EndScrollView();
            }
        }

        bool showScroll = true;
        int showSetId = 0;
        bool[] showModules = new bool[0];
        void DrawScroll()
        {
            Color def = GUI.color;
            var slist = Enum.GetNames(typeof(PaletteItem.Set));
            var mlist = Enum.GetNames(typeof(PaletteItem.Module));
            if (showModules.Length != mlist.Length || m_itemNames == null || m_itemNames.Count < 1) InitContent();

            using (var h = new EditorGUILayout.HorizontalScope(EditorStyles.helpBox))
            {
                showScroll = EditorGUILayout.Toggle(showScroll, "foldout", GUILayout.Width(10));
                EditorGUILayout.LabelField("Item List", EditorStyles.boldLabel, GUILayout.MaxWidth(m_buttonWidth - 15));
                showSetId = GUILayout.Toolbar(showSetId, slist, GUILayout.MaxWidth(slist.Length * 60));
            }
            if (!showScroll) return;

            using (var h0 = new EditorGUILayout.HorizontalScope())
            {
                GUILayout.BeginScrollView(m_scrollPosY, GUIStyle.none, GUIStyle.none, GUILayout.Width(m_buttonWidth + 5));
                using (var v = new EditorGUILayout.VerticalScope(GUILayout.Width(m_buttonWidth)))
                {
                    GUILayout.Space(2);
                    foreach (var k in m_itemNames)
                    {
                        if ((k.Key.x & (1 << showSetId)) == 0)
                            continue;
                        int kz = k.Key.z;
                        using (var h = new EditorGUILayout.HorizontalScope())
                        {
                            showModules[kz] = EditorGUILayout.Toggle(showModules[kz], "foldout", GUILayout.Width(15));
                            GUILayout.Label(mlist[kz], "In Title");
                        }
                        if (!showModules[kz]) continue;
                        foreach (string n in k.Value)
                            using (var h = new EditorGUILayout.HorizontalScope())
                                DrawCell(m_itemCells[m_artPacks[0]][k.Key].Find(obj => obj.itemName == n));
                    }
                    GUILayout.Space(15);
                }
                GUILayout.EndScrollView();

                m_scrollPos = GUILayout.BeginScrollView(m_scrollPos);
                m_scrollPosX.x = m_scrollPos.x;
                m_scrollPosY.y = m_scrollPos.y;
                using (var v = new EditorGUILayout.VerticalScope())
                {
                    foreach (var k in m_itemNames)
                    {
                        if ((k.Key.x & (1 << showSetId)) == 0)
                            continue;
                        GUILayout.Label("", "In Title");
                        if (!showModules[k.Key.z]) continue;

                        foreach (string n in k.Value)
                            using (var h = new EditorGUILayout.HorizontalScope())
                                for (int i = 1; i < m_artPacks.Count; i++)
                                    DrawCell(m_itemCells[m_artPacks[i]][k.Key].Find(obj => obj.itemName == n));
                    }
                }
                GUILayout.EndScrollView();
            }
        }

        void DrawCell(Item a_item)
        {
            Color def = GUI.color;
            if (a_item != null)
            {
                GUILayout.Label(a_item.preview, GetPreviewStyle());
                string itemLebel = a_item.itemName + "\n(" + a_item.paletteitem.itemName + ")";
                if (GUILayout.Button(itemLebel, GetLabelStyle()))
                    Selection.activeGameObject = a_item.itemObject;
            }
            else
            {
                GUI.color = Color.red;
                GUILayout.Label(GetPreview(), GetPreviewStyle());
                GUILayout.Box("", GetLabelStyle());
                GUI.color = def;
            }
        }

        void DrawFunction()
        {
            using (var h = new EditorGUILayout.HorizontalScope(EditorStyles.helpBox, GUILayout.Width(Screen.width - 9)))
            {
                EditorGUI.BeginChangeCheck();
                m_vg = (VGlobal)EditorGUILayout.ObjectField(m_vg, typeof(VGlobal), false, GUILayout.Width(100));
                EditorGUILayout.Space();
                if (EditorGUI.EndChangeCheck() && m_vgName.Length > 0)
                {
                    m_vgName = AssetDatabase.GetAssetPath(m_vg);
                    m_vgName = m_vgName.Substring(m_vgName.LastIndexOf(PathCollect.resourceSubPath)).Replace(".asset", "");
                }
                GUILayout.Label("Size", GUILayout.Width(30));
                m_buttonHeight = Mathf.FloorToInt(GUILayout.HorizontalSlider(m_buttonHeight, 30f, 80f, GUILayout.Width(50)));
                m_buttonHeight = EditorGUILayout.DelayedFloatField(m_buttonHeight, GUILayout.Width(20));
                m_buttonWidth = Mathf.FloorToInt(GUILayout.HorizontalSlider(m_buttonWidth, 100f, 300f, GUILayout.Width(50)));
                m_buttonWidth = EditorGUILayout.DelayedFloatField(m_buttonWidth, GUILayout.Width(28));
                if (GUILayout.Button("Refresh", EditorStyles.miniButton, GUILayout.Width(60)))
                {
                    InitContent();
                }
            }
        }

        #region rename tool
        [Serializable] public struct Name { public string oldName, newName; }
        int nLength;
        string nameCode = "";
        Name[] m_names = new Name[1];
        bool showRenameTool,fixVData,fixMarker;
        Vector2 m_scrollPosRenameTool;

        void DrawRenameTool()
        {
            float lw = EditorGUIUtility.labelWidth;
            EditorGUIUtility.labelWidth = 120f;
            using (var v = new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
            {
                using (var h = new EditorGUILayout.HorizontalScope())
                {
                    showRenameTool = EditorGUILayout.Toggle(showRenameTool, "foldout", GUILayout.Width(10));
                    EditorGUILayout.LabelField("Replace Marker", EditorStyles.boldLabel, GUILayout.MaxWidth(m_buttonWidth - 15));
                    nLength = EditorGUILayout.DelayedIntField("", nLength, GUILayout.Width(30));
                    using (var c = new EditorGUI.ChangeCheckScope())
                    {
                        GUILayout.Label("Rename Code", GUILayout.ExpandWidth(false));
                        nameCode = EditorGUILayout.DelayedTextField(nameCode);
                        if (c.changed)
                        {
                            nameCode = nameCode.Replace("\n", "");
                            nameCode = nameCode.Replace("	", "");
                            nameCode = nameCode.Replace(" ", "");
                            string[] codes = nameCode.Split(new string[] { ";" }, StringSplitOptions.RemoveEmptyEntries);
                            Name[] newNames = new Name[codes.Length];
                            for (int i = 0; i < codes.Length; i++)
                            {
                                string[] ns = codes[i].Split(new string[] { "," }, StringSplitOptions.None);
                                if (ns.Length == 2)
                                    newNames[i] = new Name { oldName = ns[0], newName = ns[1] };
                            }
                            m_names = newNames;
                            nLength = m_names.Length;
                            nameCode = "";
                            return;
                        }
                    }
                    GUILayout.Label("Fix Marker",GUILayout.ExpandWidth(false));
                    fixMarker = EditorGUILayout.Toggle(fixMarker, GUILayout.Width(30));
                    GUILayout.Label("Fix VData", GUILayout.ExpandWidth(false));
                    fixVData = EditorGUILayout.Toggle(fixVData, GUILayout.Width(30));
                    using (var c = new EditorGUI.DisabledGroupScope(!(fixMarker||fixVData) || nLength==0))
                    {
                        if (GUILayout.Button("Replace", EditorStyles.miniButton, GUILayout.Width(60)))
                        {
                            for (int i = 0; i < m_names.Length; i++)
                            {
                                string log = "";
                                log += "<b>" + m_names[i].oldName + " -> " + m_names[i].newName + "</b>\n";
                                if (m_names[i].oldName == null || m_names[i].oldName.Length < 1) continue;
                                if (fixMarker) log += FixMarker(m_names[i]);
                                if (fixVData) log += FixVData(m_names[i]);
                                Debug.Log(log + "<b>End.</b>");
                            }
                        }
                    }
                }
            }
            if (nLength != m_names.Length)
            {
                Name[] newNames = new Name[nLength];
                for (int i = 0; i < newNames.Length; i++)
                {
                    if (i < m_names.Length)
                        newNames[i] = m_names[i];
                }
                m_names = newNames;
            }
            else if (showRenameTool)
            {
                using (var v = new EditorGUILayout.VerticalScope(GUILayout.MaxHeight(200)))
                {
                    m_scrollPosRenameTool = GUILayout.BeginScrollView(m_scrollPosRenameTool);
                    for (int i = 0; i < m_names.Length; i++)
                    {
                        using (var h = new EditorGUILayout.HorizontalScope())
                        {
                            EditorGUILayout.LabelField(i.ToString(), GUILayout.Width(20));
                            m_names[i].oldName = EditorGUILayout.DelayedTextField(m_names[i].oldName);
                            m_names[i].newName = EditorGUILayout.DelayedTextField(m_names[i].newName);
                        }
                    }
                    GUILayout.EndScrollView();
                }
            }
            EditorGUIUtility.labelWidth = lw;
        }

        string FixMarker(Name a_name)
        {
            string log = "";
            Type t = Type.GetType("SvnToolsApi.SvnToolsUtility, SvnTools, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null", true, true);
            bool useSvn = (t != null);
            //find match markers
            string[] apGuids = AssetDatabase.FindAssets(a_name.oldName + " t:Prefab");
            for (int i_ap = 0; i_ap < apGuids.Length; i_ap++)
            {
                string assetPath = AssetDatabase.GUIDToAssetPath(apGuids[i_ap]);
                if (assetPath.Substring(assetPath.LastIndexOf("/") + 1).Equals(a_name.oldName + ".prefab"))
                {
                    log += assetPath + " ----> " + a_name.newName + "\n";
                    if (useSvn)
                    {
                        string n = assetPath.Replace(a_name.oldName, a_name.newName);
                        string[] olds = new string[] { assetPath, assetPath + ".meta" };
                        string[] news = new string[] { n, n + ".meta" };
                        //SvnToolsApi.SvnToolsUtility.Move(olds, news);
                        t.GetMethod("Move").Invoke(null, new object[] { olds, news, null, null, true });
                    }
                    else
                        AssetDatabase.RenameAsset(assetPath, a_name.newName);
                }
            }
            return log;
        }

        string FixVData(Name a_name)
        {
            string log = "";
            //find all VDatas
            var vds = new List<VolumeData>();
            string[] vdGuids = AssetDatabase.FindAssets("t:VolumeData");
            for (int i_vd = 0; i_vd < vdGuids.Length; i_vd++)
            {
                string assetPath = AssetDatabase.GUIDToAssetPath(vdGuids[i_vd]);
                vds.Add(AssetDatabase.LoadAssetAtPath<VolumeData>(assetPath));
            }

            //replace name
            foreach (VolumeData vd in vds)
            {
                foreach (BlockItem b in vd.blockItems)
                {
                    if (b.pieceName == a_name.oldName)
                    {
                        log += vd.name + "(" + b.BlockPos + "): " + b.pieceName + "\n";
                        b.pieceName = a_name.newName != "" ? a_name.newName : null;
                    }
                }
                foreach (ChunkData c in vd.GetChunkDatas())
                {
                    foreach (BlockAir b in c.blockAirs)
                    {
                        for (int p = 0; p < b.pieceNames.Length; p++)
                        {
                            if (b.pieceNames[p] == a_name.oldName)
                            {
                                log += vd.name + "(" + b.BlockPos + ")[" + p + "]: " + b.pieceNames[p] + "\n";
                                b.pieceNames[p] = a_name.newName != "" ? a_name.newName : null;
                            }
                        }
                    }
                }
                EditorUtility.SetDirty(vd);
            }
            return log;
        }

        #endregion

        #region ArtPackParent

        Dictionary<string, string> _pDict = new Dictionary<string, string>();

        void UpdateAppDict()
        {
            _pDict.Clear();
            foreach (string apName in m_artPacks)
            {
                if (apName == "LevelPieces")
                    continue;
                foreach (VGlobal.ArtPackParent app in m_vg.artPackParentList)
                {
                    if (app.pack == apName)
                    {
                        _pDict.Add(app.pack, app.parentPack);
                        break;
                    }
                }
                if (!_pDict.ContainsKey(apName))
                    _pDict.Add(apName, "LevelPieces");
            }
        }

        void UpdateAppList()
        {
            List<VGlobal.ArtPackParent> _pList = new List<VGlobal.ArtPackParent>();
            foreach (KeyValuePair<string, string> k in _pDict)
                _pList.Add(new VGlobal.ArtPackParent { pack = k.Key, parentPack = k.Value });
            m_vg.artPackParentList = _pList;
            EditorUtility.SetDirty(m_vg);
        }

        public static void UpdateItemArrays(VGlobal _vg = null)
        {
            if (_vg == null)
                _vg = VGlobal.GetSetting();
            List<string> artPacks = VGlobal.GetArtPacks();
            _vg.APItemPathList.Clear();
            foreach (string apName in artPacks)
            {
                var _n = new VGlobal.APItemPath { name = apName, itemPath = new List<string>() };
                PaletteItem[] _ps = UpdateItemArray(PathCollect.artPack + _n.name, _vg);
                foreach (PaletteItem p in _ps)
                {
                    string _itemPath = AssetDatabase.GetAssetPath(p);
                    _itemPath = _itemPath.Substring(_itemPath.IndexOf(PathCollect.resourceSubPath)).Replace(".prefab", "");
                    _n.itemPath.Add(_itemPath);
                }
                _vg.APItemPathList.Add(_n);
            }
            EditorUtility.SetDirty(_vg);
        }

        static PaletteItem[] UpdateItemArray(string _artPackPath, VGlobal _vg = null)
        {
            PaletteItem[] _final = Resources.LoadAll<PaletteItem>(PathCollect.pieces);

            string cName = _artPackPath.Substring(_artPackPath.LastIndexOf("/") + 1);
            string pName = _vg.GetParentArtPack(cName);
            string pPath = PathCollect.artPack + pName;
            PaletteItem[] _child = Resources.LoadAll<PaletteItem>(_artPackPath);
            while (pPath != PathCollect.pieces)
            {
                PaletteItem[] _parent = Resources.LoadAll<PaletteItem>(pPath);
                for (int p = 0; p < _parent.Length; p++)
                {
                    foreach (PaletteItem c in _child)
                    {
                        if (c.name == _parent[p].name)
                        {
                            _parent.SetValue(c, p);
                            break;
                        }
                    }
                }
                foreach (PaletteItem c in _child)
                {
                    bool _finded = false;
                    foreach (PaletteItem p in _parent)
                    {
                        if (p.name == c.name)
                        {
                            _finded = true;
                            break;
                        }
                    }
                    if (!_finded)
                    {
                        PaletteItem[] _parentTemp = _parent;
                        _parent = new PaletteItem[_parent.Length + 1];
                        _parentTemp.CopyTo(_parent, 0);
                        _parent.SetValue(c, _parent.Length - 1);
                    }
                }
                _child = _parent;
                cName = pName;
                pName = _vg.GetParentArtPack(cName);
                pPath = PathCollect.artPack + pName;
            }

            for (int i = 0; i < _final.Length; i++)
            {
                bool _finded = false;
                for (int j = 0; j < _child.Length; j++)
                {
                    if (_child[j].name == _final[i].name)
                    {
                        _final.SetValue(_child[j], i);
                        _finded = true;
                    }
                    if (_finded)
                        break;
                }
            }

            return _final;
        }

        #endregion

        #region Get

        static Texture2D GetPreview(UnityEngine.Object obj = null)
        {
            Texture2D thumbnail = null;
            if (obj != null)
                thumbnail = AssetPreview.GetAssetPreview(obj);
            if (thumbnail == null)
                thumbnail = AssetPreview.GetMiniTypeThumbnail(typeof(GameObject));
            return thumbnail;
        }

        GUIStyle GetLabelStyle()
        {
            GUIStyle guiStyle = new GUIStyle(GUI.skin.button)
            {
                fontSize = Mathf.FloorToInt(Mathf.Clamp(m_buttonHeight / 3f, 1f, 12f)),
                alignment = TextAnchor.MiddleLeft,
                imagePosition = ImagePosition.ImageLeft,
                fixedHeight = m_buttonHeight,
                fixedWidth = m_buttonWidth - m_buttonHeight - 4f
            };
            return guiStyle;
        }

        GUIStyle GetPreviewStyle()
        {
            GUIStyle guiStyle = new GUIStyle(GUI.skin.label)
            {
                alignment = TextAnchor.LowerCenter,
                imagePosition = ImagePosition.ImageAbove,
                fixedHeight = m_buttonHeight,
                fixedWidth = m_buttonHeight
            };
            return guiStyle;
        }

        #endregion
    }
}