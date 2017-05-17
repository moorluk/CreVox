using UnityEngine;
using System;
using System.Collections.Generic;
using System.IO;

namespace CreVox
{
	[CreateAssetMenu(menuName = "CreVox/Global Setting")]
	public class VGlobal : ScriptableObject
	{
		[System.Serializable]
		public struct ArtPackParent
		{
			public string pack;
			public string parentPack;
		}
		public List<ArtPackParent> artPackParentList;

		[System.Serializable]
		public struct APItemPath
		{
			public string name;
			public List<string> itemPath;
		}
		public List<APItemPath> APItemPathList;

		public bool saveBackup;
		public bool volumeShowArtPack;
		public bool FakeDeco;
		public bool debugRuler;

		public float editDis;

		public int chunkSize = 16;
		public float tileSize = 0.5f;
		public float w = 3f;
		public float h = 2f;
		public float d = 3f;
		public float hw = 1.5f;
		public float hh = 1f;
		public float hd = 1.5f;

		public static VGlobal GetSetting(string _settingPath = "")
		{
			VGlobal vg;
			vg = (VGlobal)Resources.Load ((_settingPath != "") ? _settingPath : PathCollect.setting, typeof(VGlobal));
			return vg;
		}

		public PaletteItem[] GetItemArray(string _artPackPath)
		{
			String _artPackName = Path.GetFileName (_artPackPath);
			//Check SubArtPack Exist or use parent ArtPack.
			Predicate<ArtPackParent> apNamecheck = delegate(ArtPackParent a) {
				return a.pack == _artPackName;
			};
			if (!artPackParentList.Exists (apNamecheck))
				_artPackName = _artPackName.Remove (_artPackName.Length - 1);
			if (!artPackParentList.Exists (apNamecheck))
				_artPackName = Path.GetFileName(PathCollect.pieces);
			//
			string[] _itemPaths = new string[APItemPathList[0].itemPath.Count];
			for (int i = 0; i < APItemPathList.Count; i++) {
				if (APItemPathList [i].name == _artPackName) {
					_itemPaths = APItemPathList [i].itemPath.ToArray();
					break;
				}
			}

			PaletteItem[] result = new PaletteItem[_itemPaths.Length];
			if (volumeShowArtPack || Application.isPlaying) {
				result = new PaletteItem[_itemPaths.Length];
				for (int i = 0; i < _itemPaths.Length; i++) {
					GameObject _obj = Resources.Load (_itemPaths [i])as GameObject;
					if (_obj == null) {
						Debug.LogWarning ("cannot find " + _itemPaths [i]);
					} else {
						PaletteItem _item = _obj.GetComponent<PaletteItem> ();
						result.SetValue (_item, i);
					}
				}
			} else {
				result = Resources.LoadAll<PaletteItem> (PathCollect.pieces);
			}
			return result;
		}

		public string GetParentArtPack (string _child)
		{
			string parent = _child;
			for (int i = 0; i < artPackParentList.Count; i++) {
				if (artPackParentList [i].pack == _child) {
					parent = artPackParentList [i].parentPack;
					break;
				}
			}
			return parent;
		}

		public static List<string> GetArtPacks ()
		{
			List<string> _result = new List<string> (0);
			_result.Add (Path.GetFileName (PathCollect.pieces));
			string[] _artPacksTemp = Directory.GetDirectories (
				PathCollect.resourcesPath + PathCollect.artPack,
				"*",
				SearchOption.TopDirectoryOnly
			);
			for (int a = 0; a < _artPacksTemp.Length; a++) {
				_artPacksTemp [a] = Path.GetFileName (_artPacksTemp [a]);
				if (_artPacksTemp [a] != _result [0])
					_result.Add (_artPacksTemp [a]);
			}
			return _result;
		}
	}
}