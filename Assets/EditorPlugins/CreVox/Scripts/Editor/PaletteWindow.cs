using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

namespace CreVox
{

	public class PaletteWindow : EditorWindow
	{

		public static PaletteWindow instance;

		private List<PaletteItem.Category> _categories;
		private List<string> _categoryLabels;
		private PaletteItem.Category _categorySelected;

		private static string _path = PathCollect.resourcesPath + PathCollect.pieces;
		private List<PaletteItem> _items;
		private Dictionary<PaletteItem.Category, List<PaletteItem>> _categorizedItems;
		private Dictionary<PaletteItem, Texture2D> _previews;
		private Vector2 _scrollPosition;
		private float ButtonWidth = 90;

		public delegate void itemSelectedDelegate(PaletteItem item,Texture2D preview);

		public static event itemSelectedDelegate ItemSelectedEvent;

		public static void ShowPalette()
		{
			instance = (PaletteWindow)EditorWindow.GetWindow(typeof(PaletteWindow));
			instance.titleContent = new GUIContent("Palette");
		}

		public static string GetLevelPiecePath()
		{
			return _path;
		}

		private void Update()
		{
			if (_previews.Count != _items.Count) {
				InitCategories();
				InitContent();
				GeneratePreviews();
			}
		}

		private void OnEnable()
		{
			if (_categories == null) {
				InitCategories();
			}
			if (_categorizedItems == null) {
				InitContent();
			}
		}

		private void OnGUI()
		{
			DrawTabs();
			DrawScroll();
			DrawFunction();
		}

		// ADDITION to reload object
		public void InitialPaletteWindow() {
			InitCategories();
			InitContent();
		}
		private void InitCategories()
		{
			Debug.Log("InitCategories called...");
			_categories = EditorUtils.GetListFromEnum<PaletteItem.Category>();
			_categoryLabels = new List<string>();
			foreach (PaletteItem.Category category in _categories) {
				_categoryLabels.Add(category.ToString());
			}
		}

		private void InitContent ()
		{
//			Debug.Log("InitContent called...");
			// Set the ScrollList
			_items = EditorUtils.GetAssetsWithScript<PaletteItem> (_path);
			_categorizedItems = new Dictionary<PaletteItem.Category,List<PaletteItem>> ();
			_previews = new Dictionary<PaletteItem, Texture2D> ();
			// Init the Dictionary
			foreach (PaletteItem.Category category in _categories) {
				_categorizedItems.Add (category, new List<PaletteItem> ());
			}
			// Assign items to each category
			foreach (PaletteItem item in _items) {
                item.assetPath = AssetDatabase.GetAssetPath(item);
				_categorizedItems [item.category].Add (item);
			}
		}

		private void DrawFunction()
		{
			EditorGUILayout.BeginHorizontal(EditorStyles.textField);
			EditorGUILayout.LabelField(label:_path,style:EditorStyles.miniLabel);
			EditorGUILayout.LabelField("Button Size",GUILayout.Width (70));
			ButtonWidth = GUILayout.HorizontalSlider (ButtonWidth, 90f, 150f, GUILayout.Width (200));
			if (GUILayout.Button("Refresh Preview", EditorStyles.miniButton, GUILayout.Width(90))) {
				InitCategories();
				InitContent();
			}
			EditorGUILayout.EndHorizontal();
		}

		private void DrawTabs()
		{
			int index = (int)_categorySelected;
			EditorGUILayout.Space();
			index = GUILayout.Toolbar(index, _categoryLabels.ToArray());
			_categorySelected = _categories[index];
		}

		private void DrawScroll()
		{
			if (_categorizedItems[_categorySelected].Count == 0) {
				EditorGUILayout.HelpBox("This category is empty!", MessageType.Info);
				return;
			}
			int rowCapacity = Mathf.FloorToInt(position.width / (ButtonWidth));
			_scrollPosition = GUILayout.BeginScrollView(_scrollPosition);
			int selectionGridIndex = -1;
			selectionGridIndex = GUILayout.SelectionGrid(selectionGridIndex, GetGUIContentsFromItems(), rowCapacity, GetGUIStyle());
			GetSelectedItem(selectionGridIndex);
			GUILayout.EndScrollView();
		}

		private void GeneratePreviews()
		{
			AssetPreview.SetPreviewTextureCacheSize (_items.Count *2);
			foreach (PaletteItem item in _items) {
				if (!_previews.ContainsKey(item)) {
					Texture2D preview = AssetPreview.GetAssetPreview(item.gameObject);
					if (preview == null)
						preview = AssetPreview.GetMiniTypeThumbnail (typeof(GameObject));
					_previews.Add(item, preview);
				}
			}
		}

		private GUIContent[] GetGUIContentsFromItems()
		{
			List<GUIContent> guiContents = new List<GUIContent>();
			if (_previews.Count == _items.Count) {
				int totalItems = _categorizedItems[_categorySelected].Count;
				for (int i = 0; i < totalItems; i++) {
					GUIContent guiContent = new GUIContent();
                    if (_categorizedItems[_categorySelected][i] != null)
                    {
                        guiContent.text = _categorizedItems[_categorySelected][i].gameObject.name + "\n" + _categorizedItems[_categorySelected][i].itemName;
                        guiContent.image = _previews[_categorizedItems[_categorySelected][i]];
                    } else {
                        InitContent();
                        break;
                    }
                    guiContents.Add(guiContent);
				}
			}
			return guiContents.ToArray();
		}

		private GUIStyle GetGUIStyle()
		{
			GUIStyle guiStyle = new GUIStyle(GUI.skin.button);
			guiStyle.fontSize = 9;
			guiStyle.alignment = TextAnchor.LowerCenter;
			guiStyle.imagePosition = ImagePosition.ImageAbove;
			guiStyle.fixedWidth = ButtonWidth;
			guiStyle.fixedHeight = ButtonWidth + (float)guiStyle.fontSize;
			return guiStyle;
		}

		private void GetSelectedItem(int index)
		{
			if (index != -1) {
				PaletteItem selectedItem = _categorizedItems[_categorySelected][index];
				Debug.Log("Selected Item is: " + selectedItem.itemName);
				if (ItemSelectedEvent != null) {
					ItemSelectedEvent(selectedItem, _previews[selectedItem]);
				}
			}
		}
	}
}