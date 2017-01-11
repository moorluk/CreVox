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

		private List<string> _artPacks;
//		private List<PaletteItem.Category> _categories;

		private List<Item> _items;
		private Dictionary<PaletteItem.Category, List<string>> itemNames;

		private static string _path = PathCollect.resourcesPath + PathCollect.artPack;
		private Vector2 _scrollPosition;
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
			DrawFunction ();
		}

		private void InitContent ()
		{
			//GetItems
			_items = new List<Item> ();
			List<PaletteItem> itemsP = EditorUtils.GetAssetsWithScript<PaletteItem> (_path);
			foreach (PaletteItem p in itemsP) {
				Item newItem = new Item();
				newItem.paletteitem = p;
				newItem.category = p.category;
				newItem.itemObject = p.gameObject;
				newItem.itemName = p.gameObject.name;
				newItem.artPack = AssetDatabase.GetAssetPath (p.gameObject).Replace(_path + "/","");
				newItem.artPack = newItem.artPack.Remove (newItem.artPack.IndexOf ("/"));
				newItem.preview = (AssetPreview.GetAssetPreview (p.gameObject) == null) ? 
					Texture2D.whiteTexture : AssetPreview.GetAssetPreview (p.gameObject);
				_items.Add (newItem);
			}
			Comparison<Item> t = new Comparison<Item> (delegate (Item x, Item y) {
						return x.artPack.CompareTo (y.artPack);
			});
			_items.Sort (t);

			//GetCategories
			List<PaletteItem.Category> _categories;
			_categories = EditorUtils.GetListFromEnum<PaletteItem.Category> ();

			GetArtPackNames ();

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
		}

		private void DrawList ()
		{
			GUIStyle listLabel = new GUIStyle(GUI.skin.FindStyle("ProgressBarBack"));
			listLabel.alignment = TextAnchor.MiddleLeft;
			listLabel.fontSize = 14;
			listLabel.fontStyle = FontStyle.Bold;

			using (var h = new EditorGUILayout.HorizontalScope ()) {
				for (int i = 0; i < _artPacks.Count; i++) {
						GUILayout.Label (_artPacks [i], listLabel, GUILayout.Width (ButtonWidth));
				}
			}
		}

		private void DrawScroll ()
		{
			Color def = GUI.color;
			_scrollPosition = GUILayout.BeginScrollView (_scrollPosition);
//			int rowCapacity = Mathf.FloorToInt (position.width / (ButtonWidth));
//			int selectionGridIndex = -1;
//			selectionGridIndex = GUILayout.SelectionGrid (selectionGridIndex, GetGUIContentsFromItems (), rowCapacity, GetGUIStyle ());
//			GetSelectedItem (selectionGridIndex);
			foreach (KeyValuePair<PaletteItem.Category, List<string>> k in itemNames) {
				GUILayout.Label (k.Key.ToString (), "In Title"/*, GUILayout.Width (ButtonWidth+3)*/);
				foreach (string n in k.Value) {
					Predicate<Item> findItem = delegate(Item obj) {
						return obj.itemName == n;
					};
					using (var h = new EditorGUILayout.HorizontalScope ()) {
//						GUILayout.Button (n, GUILayout.Width (ButtonWidth));
						for (int i = 0; i < _artPacks.Count; i++) {
							Item _item = _itemCells [_artPacks [i]] [k.Key].Find (findItem);
							if (_item != null) {
								string itemLebel = _item.itemName + "\n(" + _item.paletteitem.itemName + ")";
								if (GUILayout.Button (new GUIContent (itemLebel, _item.preview), GetGUIStyle ()))
									Selection.activeGameObject = _item.itemObject;
							} else {
								GUI.color = Color.red;
								GUILayout.Box ("", GetGUIStyle ());
								GUI.color = def;
							}
						}
					}
				}
			}
			GUILayout.EndScrollView ();
		}

		private void DrawFunction ()
		{
			DrawRenameTool ();
			using (var h = new EditorGUILayout.HorizontalScope (EditorStyles.textField)) {
				GUILayout.Label ("", GUILayout.Width (Mathf.Clamp (Screen.width - 290, 0, Screen.width)));
				GUILayout.Label ("Button Size", GUILayout.Width (70));
				ButtonWidth = GUILayout.HorizontalSlider (ButtonWidth, 100f, 300f, GUILayout.Width (100));
				if (GUILayout.Button ("Refresh Preview", EditorStyles.miniButton, GUILayout.Width (90))) {
					InitContent ();
				}
			}
		}

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
						oldMarker = (GameObject)EditorGUILayout.ObjectField ("Old Marker", oldMarker, typeof(GameObject), true);
						using (var h = new EditorGUILayout.HorizontalScope ()) {
							if (GUILayout.Button ("Get", GUILayout.Width (EditorGUIUtility.labelWidth - 4)))
								oldName = oldMarker.name;
							oldName = EditorGUILayout.TextField (oldName);
						}

						newMarker = (GameObject)EditorGUILayout.ObjectField ("New Marker", newMarker, typeof(GameObject), true);
						using (var h = new EditorGUILayout.HorizontalScope ()) {
							if (GUILayout.Button ("Get", GUILayout.Width (EditorGUIUtility.labelWidth - 4)))
								newName = newMarker.name;
							newName = EditorGUILayout.TextField (newName);
						}
					} else {
						oldName = EditorGUILayout.TextField ("Old Name", oldName);
						newName = EditorGUILayout.TextField ("New Name", newName);
					}
					if (GUILayout.Button ("Replace")) {
						Debug.Log (oldName + " -> " + newName);
						foreach (Dungeon d in vm.dungeons) {
							foreach (ChunkData c in d.volumeData.chunkDatas) {
								foreach (BlockAir b in c.blockAirs) {
									for (int i = 0; i < b.pieceNames.Length; i++) {
										if (b.pieceNames [i] == oldName) {
											Debug.Log (d.volumeData.name + "(" + b.BlockPos.ToString () + ")[" + i.ToString () + "]: " + b.pieceNames [i]);
											if (newName != "")
												b.pieceNames [i] = newName;
											else
												b.pieceNames [i] = null;
										}
									}
								}
								EditorUtility.SetDirty (d.volumeData);
							}
						}
						Volume[] vols = vm.transform.GetComponentsInChildren<Volume> (false);
						foreach (Volume vol in vols) {
							vol.LoadTempWorld ();
						}
					}
				}
			}
			EditorGUIUtility.labelWidth = lw;
		}

		private void GetArtPackNames()
		{
			_artPacks = new List<string> ();
			_artPacks.Add (Path.GetFileName (PathCollect.pieces));
			string[] _artPacksTemp = Directory.GetDirectories (_path, "*", SearchOption.TopDirectoryOnly);
			for (int a = 0; a < _artPacksTemp.Length; a++) {
				_artPacksTemp [a] = Path.GetFileName (_artPacksTemp [a]);
				if (_artPacksTemp [a] != _artPacks [0])
					_artPacks.Add (_artPacksTemp [a]);
			}
		}

		private GUIStyle GetGUIStyle ()
		{
			GUIStyle guiStyle = new GUIStyle (GUI.skin.button);
			guiStyle.fontSize = 12;
			guiStyle.alignment = TextAnchor.MiddleLeft;
			guiStyle.imagePosition = ImagePosition.ImageLeft;
			guiStyle.fixedWidth = ButtonWidth;
			guiStyle.fixedHeight = Mathf.Clamp (ButtonWidth / 3, guiStyle.fontSize * 2 + 3, ButtonWidth);
			return guiStyle;
		}
	}
}