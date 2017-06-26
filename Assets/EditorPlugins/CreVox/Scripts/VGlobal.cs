using UnityEngine;
using System;
using System.Collections.Generic;
using System.IO;
using CrevoxExtend;
using MissionGrammarSystem;

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

		#region Static Function
		public static VGlobal GetSetting(string _settingPath = "")
		{
			VGlobal vg;
			vg = (VGlobal)Resources.Load ((_settingPath != "") ? _settingPath : PathCollect.setting, typeof(VGlobal));
			return vg;
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
		#endregion

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
			GameObject _missing = Resources.Load (PathCollect.resourceSubPath + "Missing", typeof(GameObject)) as GameObject;
			if (volumeShowArtPack || Application.isPlaying) {
				result = new PaletteItem[_itemPaths.Length];
				for (int i = 0; i < _itemPaths.Length; i++) {
					GameObject _obj = Resources.Load (_itemPaths [i])as GameObject;
					if (_obj == null) {
						Debug.LogWarning ("cannot find " + _itemPaths [i]);
						result.SetValue (_missing.GetComponent<PaletteItem> (), i);
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

		[System.Serializable]
		public struct Stage
		{
			public int number;
			public string artPack;
			public string XmlPath;
			public string vDataPath;
			public string VGXmlPath;
		}
		public List<Stage> StageList;

		public void AddStage(int _stageNumber, string _artPack, string _XmlPath, string _vDataPath, string _VGXmlPath)
		{
			Predicate<Stage> findStage = delegate(Stage s) {
				return s.number == _stageNumber;
			};
			if (!StageList.Exists (findStage)) {
				Stage s = new Stage () {
					number = _stageNumber,
					artPack = _artPack,
					XmlPath = _XmlPath,
					vDataPath = _vDataPath,
					VGXmlPath = _VGXmlPath
				};
				StageList.Add (s);
			} else {
				Debug.LogWarning ("Stage[" + _stageNumber + "] already exist...");
			}
		}

		public Stage GetStageSetting(int _stageNumber)
		{
			Predicate<Stage> findStage = delegate(Stage s) {
				return s.number == _stageNumber;
			};
			return StageList.Find (findStage);
		}

		public delegate bool CreateStage(int _stageNumber,int seed);
		public bool GenerateStage(int _stageNumber,int seed = 0)
		{
			Stage _s = GetStageSetting(_stageNumber);
			if (seed == 0)
				seed = UnityEngine.Random.Range (0, int.MaxValue);
			// if (_s.XmlPath.Length > 0) {
			if (_s.XmlPath.Length > 0 && _s.VGXmlPath.Length > 0) {
				CreVoxNode root = CreVoxAttach.GenerateMissionGraph (PathCollect.gram + "/" + _s.XmlPath, seed);
				// return CrevoxGeneration.GenerateLevel (root, _s, seed);
				Debug.Log ("_s.VGXmlPath of GenerateRealLevel" + _s.VGXmlPath);
				return CrevoxGeneration.GenerateRealLevel(root, _s, seed);
			} else {
				return false;
			}
		}
	}
}