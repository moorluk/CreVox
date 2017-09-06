using UnityEngine;
using UnityEditor;
using System;
using System.Collections.Generic;
using System.IO;

namespace CreVox
{

	public class ArtPackWindow : EditorWindow
	{
		private class Item{
			public PaletteItem paletteitem;
			public PaletteItem.Category category;
			public GameObject itemObject;
			public string itemName;
			public string artPack;
			public Texture2D preview;
		}
		public static ArtPackWindow instance;

		private Dictionary<string,Dictionary<PaletteItem.Category,List<Item>>> _itemCells;
		private string vgName = "";
		private VGlobal vg;

		private List<string> _artPacks= new List<string> ();

		private List<Item> _items;
		private Dictionary<PaletteItem.Category, List<string>> itemNames;

		private static string _path = PathCollect.resourcesPath + PathCollect.artPack;
		private Vector2 _scrollPosition;
		private Vector2 _scrollPositionX;
		private Vector2 _scrollPositionY;
		private float ButtonWidth = 140;

		public static void ShowPalette ()
		{
			instance = (ArtPackWindow)EditorWindow.GetWindow (typeof(ArtPackWindow));
			instance.titleContent = new GUIContent ("ArtPack");
		}

		private void OnEnable ()
		{
			InitContent ();
		}

		private void OnGUI ()
		{
			DrawList ();
			DrawScroll ();
			DrawRenameTool ();
			DrawFunction ();
		}

		private void InitContent ()
		{
			if (vg == null)
				vg = VGlobal.GetSetting ();

			//GetItems
			_items = new List<Item> ();
			List<PaletteItem> itemsP = EditorUtils.GetAssetsWithScript<PaletteItem> (_path);
			AssetPreview.SetPreviewTextureCacheSize (itemsP.Count *2);
			foreach (PaletteItem p in itemsP) {
				Item newItem = new Item ();
				newItem.paletteitem = p;
				newItem.category = p.category;
				newItem.itemObject = p.gameObject;
				newItem.itemName = p.gameObject.name;
				newItem.artPack = AssetDatabase.GetAssetPath (p.gameObject).Replace(_path + "/","");
				newItem.artPack = newItem.artPack.Remove (newItem.artPack.IndexOf ("/"));
				newItem.preview = GetPreview (p.gameObject);

				_items.Add (newItem);
			}
			Comparison<Item> t = new Comparison<Item> (delegate (Item x, Item y) {
						return x.artPack.CompareTo (y.artPack);
			});
			_items.Sort (t);

			//GetCategories
			List<PaletteItem.Category> _categories;
			_categories = EditorUtils.GetListFromEnum<PaletteItem.Category> ();

			_artPacks = VGlobal.GetArtPacks ();
			UpdateAppDict ();

			//GetItemCells
			_itemCells = new Dictionary<string, Dictionary<PaletteItem.Category, List<Item>>> ();
			for (int a = 0; a < _artPacks.Count; a++) {
				var p = new Dictionary<PaletteItem.Category,List<Item>> ();
				for (int c = 0; c < _categories.Count; c++) {
					p.Add (_categories [c], new List<Item> ());
				}
				_itemCells.Add (_artPacks [a], p);
			}
			foreach (Item i in _items) {
				_itemCells [i.artPack] [i.category].Add (i);
			}

			//GetItemNames
			itemNames = new Dictionary<PaletteItem.Category, List<string>> ();
			foreach (PaletteItem.Category c in _categories) {
				itemNames.Add (c, new List<string> ());
				foreach (Item i in _items) {
					if (!itemNames[c].Contains (i.itemName) && i.category == c)
						itemNames[c].Add (i.itemName);
				}
				itemNames[c].Sort ();
			}

			//Log
			string logCell = "";
			for (int a = 0; a < _artPacks.Count; a++) {
				for (int c = 0; c < _categories.Count; c++) {
					_itemCells [_artPacks [a]] [_categories [c]].Sort (t);
					logCell = logCell + _artPacks [a] + 
						"/" + _categories [c].ToString () + ": " + 
						_itemCells [_artPacks [a]] [_categories [c]].Count + "\n";
				}
				logCell = logCell + " -------------\n";
			}
			Debug.Log (logCell);

			UpdateAppList ();
			UpdateItemArrays (vg);
		}

		private void DrawList ()
		{
			GUIStyle listLabel = new GUIStyle (GUI.skin.FindStyle ("ProgressBarBack"));
			listLabel.alignment = TextAnchor.MiddleLeft;
			listLabel.fontSize = 14;
			listLabel.fontStyle = FontStyle.Bold;

			string[] _ap = _artPacks.ToArray ();

			using (var h = new EditorGUILayout.HorizontalScope (GUILayout.Height (45))) {
				GUILayout.BeginScrollView (Vector2.zero, GUIStyle.none, GUIStyle.none, GUILayout.Width (ButtonWidth + 5));
				using (var v = new EditorGUILayout.VerticalScope (GUILayout.Width (ButtonWidth))) {
					GUILayout.Label (_artPacks [0], listLabel, GUILayout.Width (ButtonWidth));
					GUILayout.Label ("Set Parent ArtPack", GUILayout.Width (ButtonWidth));
				}
				GUILayout.EndScrollView ();
				GUILayout.BeginScrollView (_scrollPositionX, GUIStyle.none, GUIStyle.none);
				EditorGUI.BeginChangeCheck ();
				using (var h1 = new EditorGUILayout.HorizontalScope ()) {
					for (int i = 1; i < _artPacks.Count; i++) {
						using (var v = new EditorGUILayout.VerticalScope (GUILayout.Width (ButtonWidth))) {
							string _c = _artPacks [i];
							GUILayout.Label (_c, listLabel, GUILayout.Width (ButtonWidth));
							_pDict[_c] = _artPacks[EditorGUILayout.Popup (_artPacks.IndexOf(_pDict[_c]), _ap, GUILayout.Width (ButtonWidth))];
						}
					}
					GUILayout.Label ("", GUILayout.Width (15));
				}
				if (EditorGUI.EndChangeCheck ()) {
					UpdateAppList ();
				}
				GUILayout.EndScrollView ();
			}
		}

		bool showScroll = true;
		private void DrawScroll ()
		{
			Color def = GUI.color;
			using (var h = new EditorGUILayout.HorizontalScope (EditorStyles.textField)) {
				showScroll = EditorGUILayout.Toggle (showScroll, "foldout", GUILayout.Width (15));
				EditorGUILayout.LabelField ("Item List", EditorStyles.boldLabel);
			}
			if (showScroll) {
				using (var h0 = new EditorGUILayout.HorizontalScope ()) {
					GUILayout.BeginScrollView (_scrollPositionY, GUIStyle.none, GUIStyle.none, GUILayout.Width (ButtonWidth + 5));
					using (var v = new EditorGUILayout.VerticalScope (GUILayout.Width (ButtonWidth))) {
						GUILayout.Space(2);
						foreach (KeyValuePair<PaletteItem.Category, List<string>> k in itemNames) {
							GUILayout.Label (k.Key.ToString (), "In Title");
							foreach (string n in k.Value) {
								Predicate<Item> findItem = delegate(Item obj) {
									return obj.itemName == n;
								};
								using (var h = new EditorGUILayout.HorizontalScope ()) {
									Item _item = _itemCells [_artPacks [0]] [k.Key].Find (findItem);
									if (_item != null) {
										GUILayout.Label (_item.preview, GetPreviewStyle ());
										string itemLebel = _item.itemName + "\n(" + _item.paletteitem.itemName + ")";
										if (GUILayout.Button (itemLebel, GetLabelStyle ()))
											Selection.activeGameObject = _item.itemObject;
									} else {
										GUI.color = Color.red;
										GUILayout.Label (GetPreview (), GetPreviewStyle ());
										GUILayout.Box ("", GetLabelStyle (), GUILayout.ExpandHeight (true));
										GUI.color = def;
									}
								}
							}
						}
						GUILayout.Space(15);
					}
					GUILayout.EndScrollView ();

					_scrollPosition = GUILayout.BeginScrollView (_scrollPosition);
					_scrollPositionX.x = _scrollPosition.x;
					_scrollPositionY.y = _scrollPosition.y;
					using (var v = new EditorGUILayout.VerticalScope ()) {
						foreach (KeyValuePair<PaletteItem.Category, List<string>> k in itemNames) {
							GUILayout.Label ("", "In Title");
							foreach (string n in k.Value) {
								Predicate<Item> findItem = delegate(Item obj) {
									return obj.itemName == n;
								};
								using (var h = new EditorGUILayout.HorizontalScope ()) {
									for (int i = 1; i < _artPacks.Count; i++) {
										Item _item = _itemCells [_artPacks [i]] [k.Key].Find (findItem);
										if (_item != null) {
											GUILayout.Label (_item.preview, GetPreviewStyle ());
											string itemLebel = _item.itemName + "\n(" + _item.paletteitem.itemName + ")";
											if (GUILayout.Button (itemLebel, GetLabelStyle ()))
												Selection.activeGameObject = _item.itemObject;
										} else {
											GUI.color = Color.red;
											GUILayout.Label (GetPreview (), GetPreviewStyle ());
											GUILayout.Box ("", GetLabelStyle (), GUILayout.ExpandHeight (true));
											GUI.color = def;
										}
									}
								}
							}
						}
					}
					GUILayout.EndScrollView ();
				}
			}
		}

		private void DrawFunction ()
		{
			using (var h = new EditorGUILayout.HorizontalScope (EditorStyles.textField, GUILayout.Width (Screen.width))) {
				GUILayout.Label ("");
				EditorGUI.BeginChangeCheck ();
				vg = (VGlobal)EditorGUILayout.ObjectField (vg, typeof(VGlobal), false, GUILayout.Width (170));
				if (EditorGUI.EndChangeCheck () && vgName.Length > 0) {
					vgName = AssetDatabase.GetAssetPath (vg);
					vgName = vgName.Substring (vgName.LastIndexOf (PathCollect.resourceSubPath)).Replace (".asset", "");
				}
				GUILayout.Label ("Button Size", GUILayout.Width (70));
				ButtonWidth = GUILayout.HorizontalSlider (ButtonWidth, 100f, 300f, GUILayout.Width (90));
				if (GUILayout.Button ("Refresh Preview", EditorStyles.miniButton, GUILayout.Width (90))) {
					InitContent ();
				}
			}
		}

		#region rename tool
		bool showRenameTool = false;
		VolumeManager vm = null;
		bool usePrefab = true;
		GameObject oldMarker = null;
		GameObject newMarker = null;
		string oldName = "";
		string newName = "";
		void DrawRenameTool ()
		{
			float lw = EditorGUIUtility.labelWidth;
			EditorGUIUtility.labelWidth = 120f;
			using (var v = new EditorGUILayout.VerticalScope (EditorStyles.textField)) {
				using (var h = new EditorGUILayout.HorizontalScope ()) {
					showRenameTool = EditorGUILayout.Toggle (showRenameTool, "foldout", GUILayout.Width (15));
					EditorGUILayout.LabelField ("Marker Replace Tool", EditorStyles.boldLabel);
				}
				if (showRenameTool) {
					using (var h = new EditorGUILayout.HorizontalScope ()) {
						EditorGUILayout.PrefixLabel ("Volume Manager");
						vm = (VolumeManager)EditorGUILayout.ObjectField (vm, typeof(VolumeManager), true);
					}
					usePrefab = EditorGUILayout.Toggle ("Use Prefab", usePrefab);
					if (usePrefab) {
						EditorGUI.BeginChangeCheck ();
						oldMarker = (GameObject)EditorGUILayout.ObjectField ("Old Marker", oldMarker, typeof(GameObject), true);
						using (var h = new EditorGUILayout.HorizontalScope ()) {
							if (GUILayout.Button ("Get", GUILayout.Width (EditorGUIUtility.labelWidth - 4)) || EditorGUI.EndChangeCheck()){
								if (oldMarker != null)
									oldName = oldMarker.name;
							}
							oldName = EditorGUILayout.TextField (oldName);
						}

						EditorGUI.BeginChangeCheck ();
						newMarker = (GameObject)EditorGUILayout.ObjectField ("New Marker", newMarker, typeof(GameObject), true);
						using (var h = new EditorGUILayout.HorizontalScope ()) {
							if (GUILayout.Button ("Get", GUILayout.Width (EditorGUIUtility.labelWidth - 4)) || EditorGUI.EndChangeCheck ()) {
								if (oldMarker != null)
									newName = newMarker.name;
							}
							newName = EditorGUILayout.TextField (newName);
						}
					} else {
						oldName = EditorGUILayout.TextField ("Old Name", oldName);
						newName = EditorGUILayout.TextField ("New Name", newName);
					}
					if (GUILayout.Button ("Replace")) {
						string log = "<b>" + oldName + " -> " + newName + "</b>\n";
						foreach (Dungeon d in vm.dungeons) {
							foreach (BlockItem b in d.volumeData.blockItems) {
								if (b.pieceName == oldName) {
										Debug.Log (d.volumeData.name + "(" + b.BlockPos.ToString () + "): " + b.pieceName);
										if (newName != "")
											b.pieceName = newName;
										else
											b.pieceName = null;
									}
							}
							foreach (ChunkData c in d.volumeData.chunkDatas) {
								foreach (BlockAir b in c.blockAirs) {
									for (int i = 0; i < b.pieceNames.Length; i++) {
										if (b.pieceNames [i] == oldName) {
											log += d.volumeData.name + "(" + b.BlockPos.ToString () + ")[" + i.ToString () + "]: " + b.pieceNames [i] + "\n";
											if (newName != "")
												b.pieceNames [i] = newName;
											else
												b.pieceNames [i] = null;
										}
									}
								}
							}
							EditorUtility.SetDirty (d.volumeData);
						}
                        Debug.Log (log + "<b>End.</b>");
                        Volume[] vols = vm.transform.GetComponentsInChildren<Volume> (false);
						foreach (Volume vol in vols) {
							vol.LoadTempWorld ();
						}
					}
				}
			}
			EditorGUIUtility.labelWidth = lw;
		}
		#endregion
		#region ArtPackParent
		private Dictionary<string,string> _pDict = new Dictionary<string, string>();
		private void UpdateAppDict ()
		{
			_pDict.Clear();
			for (int i = 1; i < _artPacks.Count; i++) {
				for (int j = 0; j < vg.artPackParentList.Count; j++) {
					if (vg.artPackParentList [j].pack == _artPacks [i]) {
						_pDict.Add (vg.artPackParentList [j].pack, vg.artPackParentList [j].parentPack);
						break;
					}
				}
				if(!_pDict.ContainsKey(_artPacks[i]))
					_pDict.Add (_artPacks[i], "LevelPieces");
			}
		}

		private void UpdateAppList ()
		{
			List<VGlobal.ArtPackParent> _pList = new List<VGlobal.ArtPackParent>();
			VGlobal.ArtPackParent _a = new VGlobal.ArtPackParent ();
			foreach (KeyValuePair<string,string> k in _pDict) {
				_a = new VGlobal.ArtPackParent ();
				_a.pack = k.Key;
				_a.parentPack = k.Value;
				_pList.Add(_a);
			}
			vg.artPackParentList = _pList;
			EditorUtility.SetDirty (vg);
		}

		public static void UpdateItemArrays(VGlobal _vg = null)
		{
			if (_vg == null)
				_vg = VGlobal.GetSetting ();
			List<string> artPacks = VGlobal.GetArtPacks ();
			_vg.APItemPathList.Clear ();
			for (int i = 0; i < artPacks.Count; i++) {
				VGlobal.APItemPath _n = new VGlobal.APItemPath ();
				_n.name = artPacks [i];
				_n.itemPath = new List<string> ();
				PaletteItem[] _p = UpdateItemArray (PathCollect.artPack + "/" + _n.name, _vg);
				for (int j = 0; j < _p.Length; j++) {
					string _itemPath = AssetDatabase.GetAssetPath (_p [j]);
					_itemPath = _itemPath.Substring (_itemPath.IndexOf (PathCollect.resourceSubPath)).Replace(".prefab","");
					_n.itemPath.Add (_itemPath);
				}
				_vg.APItemPathList.Add (_n);
			}
			EditorUtility.SetDirty (_vg);
		}

		private static PaletteItem[] UpdateItemArray(string _artPackPath, VGlobal _vg = null)
		{
			PaletteItem[] _final = Resources.LoadAll<PaletteItem> (PathCollect.pieces);

			string cName = _artPackPath.Substring (_artPackPath.LastIndexOf ("/") + 1);
			string pName = _vg.GetParentArtPack (cName);
			string pPath = PathCollect.artPack + "/" + pName;
			PaletteItem[] _child = Resources.LoadAll<PaletteItem> (_artPackPath);
			while (pPath != PathCollect.pieces) {
				PaletteItem[] _parent = Resources.LoadAll<PaletteItem> (pPath);
				for (int p = 0; p < _parent.Length; p++) {
					for (int c = 0; c < _child.Length; c++) {
						if (_child [c].name == _parent [p].name) {
							_parent.SetValue (_child [c], p);
							break;
						}
					}
				}
				for (int c = 0; c < _child.Length; c++) {
					bool _finded = false;
					string _name = _child [c].name;
					for (int p = 0; p < _parent.Length; p++) {
						if (_parent [p].name == _name) {
							_finded = true;
							break;
						}
					}
					if (!_finded) {
						PaletteItem[] _parentTemp = _parent;
						_parent = new PaletteItem[_parent.Length + 1];
						_parent.SetValue (_child [c], _parent.Length - 1);
						for (int p = 0; p < _parentTemp.Length; p++) {
							_parent.SetValue (_parentTemp [p], p);
						}
					}
				}
				_child = _parent;
				cName = pName;
				pName = _vg.GetParentArtPack (cName);
				pPath = PathCollect.artPack + "/" + pName;
			}

			for (int i = 0; i < _final.Length; i++) {
				bool _finded = false;
				for (int j = 0; j < _child.Length; j++) {
					if (_child [j].name == _final [i].name) {
						_final.SetValue(_child [j], i);
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
		private Texture2D GetPreview(GameObject obj = null)
		{
			Texture2D thumbnail = null;
			if (obj != null)
				thumbnail = AssetPreview.GetAssetPreview (obj);
			if (thumbnail == null)
				thumbnail = AssetPreview.GetMiniTypeThumbnail (typeof(GameObject));
			return thumbnail;
		}

		private GUIStyle GetLabelStyle ()
		{
			GUIStyle guiStyle = new GUIStyle (GUI.skin.button);
			guiStyle.fontSize = Mathf.FloorToInt (Mathf.Clamp (ButtonWidth/12f,1f,14f));
			guiStyle.alignment = TextAnchor.MiddleLeft;
			guiStyle.imagePosition = ImagePosition.ImageLeft;
			guiStyle.fixedHeight = Mathf.Clamp (ButtonWidth / 3 , guiStyle.fontSize * 2, ButtonWidth);
			guiStyle.fixedWidth = ButtonWidth - guiStyle.fixedHeight - 4f;
			return guiStyle;
		}

		private GUIStyle GetPreviewStyle ()
		{
			float size = ButtonWidth / 3;
			GUIStyle guiStyle = new GUIStyle (GUI.skin.label);
			guiStyle.alignment = TextAnchor.LowerCenter;
			guiStyle.imagePosition = ImagePosition.ImageAbove;
			guiStyle.fixedHeight = size;
			guiStyle.fixedWidth = size;
			return guiStyle;
		}
		#endregion
	}
}