using UnityEngine;
using System.Collections.Generic;

//using System.Collections;

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

		public static VGlobal GetSetting(string _settingPath = null)
		{
			VGlobal vg;
			vg = (VGlobal)Resources.Load ((_settingPath != null) ? _settingPath : PathCollect.setting, typeof(VGlobal));
			return vg;
		}

		public PaletteItem[] UpdateItemArray(string _artPackPath)
		{
			PaletteItem[] _final = Resources.LoadAll<PaletteItem> (PathCollect.pieces);

			if (volumeShowArtPack || Application.isPlaying) {
				string cName = _artPackPath.Substring (_artPackPath.LastIndexOf ("/") + 1);
				string pName = GetParentArtPack (cName);
				Debug.LogWarning (cName + " >>> " + pName);
				string pPath = PathCollect.artPack + "/" + pName;
				PaletteItem[] _child = Resources.LoadAll<PaletteItem> (_artPackPath);
				while (pPath != PathCollect.pieces) {
					PaletteItem[] _parent = Resources.LoadAll<PaletteItem> (pPath);
					for (int i = 0; i < _parent.Length; i++) {
						bool _finded = false;
						for (int j = 0; j < _child.Length; j++) {
							if (_child [j].name == _parent [i].name) {
								_parent.SetValue (_child [j], i);
								_finded = true;
							}
							if (_finded)
								break;
						}
					}
					_child = _parent;
					cName = pName;
					pName = GetParentArtPack (cName);
					Debug.LogWarning (cName + " >>> " + pName);
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
			}

			return _final;
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
	}
}