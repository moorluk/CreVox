using UnityEngine;
using System;
using System.Collections.Generic;
using System.IO;
using CrevoxExtend;
using MissionGrammarSystem;

namespace CreVox
{
    [CreateAssetMenu (menuName = "CreVox/Global Setting")]
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

        [System.Serializable]
        public class VGSetting
        {
            public bool saveBackup;
            public bool useArtPack = true;
            public bool snapGridL;
            public bool debugRulerL;
            public bool showBlockHoldL;
            public bool debugLog;
        }
        public VGSetting setting = new VGSetting();

        public float editDis;

        public int chunkSize = 16;
        public float tileSize = 0.5f;
        public float w = 3f;
        public float h = 2f;
        public float d = 3f;

        #region Static Function

        static VGlobal instance;
        public static VGlobal GetSetting (string _settingPath = "")
        {
            if (instance == null)
                instance = (VGlobal)Resources.Load((_settingPath != "") ? _settingPath : PathCollect.setting, typeof(VGlobal));
            return instance;
        }

        public static List<string> GetArtPacks ()
        {
            List<string> _result = new List<string>() { Path.GetFileName(PathCollect.pieces) };
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
        Dictionary<string, PaletteItem[]> itemArrays = new Dictionary<string, PaletteItem[]>();
        public PaletteItem[] GetItemArray (string _artPackPath, string _subArtPack, bool _showArtPack = true)
        {
            String _artPackName = Path.GetFileName (_artPackPath);

            if (itemArrays.ContainsKey(_artPackName + _subArtPack))
                return itemArrays[_artPackName + _subArtPack];

            if (artPackParentList.Exists (a => a.pack == _artPackName + _subArtPack))
                _artPackName += _subArtPack;
            if (!artPackParentList.Exists (a => a.pack == _artPackName))
                _artPackName = Path.GetFileName (PathCollect.pieces);
            
            //
            string[] _itemPaths = APItemPathList.Find (a => a.name == _artPackName).itemPath.ToArray ();

            PaletteItem[] result;
            GameObject _missing = Resources.Load (PathCollect.resourceSubPath + "Missing", typeof(GameObject)) as GameObject;
            if (_showArtPack || Application.isPlaying) {
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
            Array.Sort<PaletteItem> (result, (x, y) => x.markType.CompareTo (y.markType));
            if (!itemArrays.ContainsKey(_artPackName + _subArtPack))
                itemArrays.Add(_artPackName + _subArtPack, result);
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
            public string GrammarXmlPath;
            //public string SpaceXmlPath;
            public string VGXmlPath;
        }

        public List<Stage> StageList;

        public void AddStage (int _stageNumber, string _artPack/*, string _spaceXmlPath*/, string _XmlPath, string _vDataPath, string _VGXmlPath)
        {
            Predicate<Stage> findStage = s => s.number == _stageNumber;
            if (!StageList.Exists (findStage)) {
                Stage s = new Stage {
                    number = _stageNumber,
                    artPack = _artPack,
                    GrammarXmlPath = _XmlPath,
                    //SpaceXmlPath = _spaceXmlPath,
                    VGXmlPath = _VGXmlPath
                };
                StageList.Add (s);
            } else {
                Debug.LogWarning ("Stage[" + _stageNumber + "] already exist...");
            }
        }

        public Stage GetStageSetting (int _stageNumber)
        {
            Predicate<Stage> findStage = s => s.number == _stageNumber;
            return StageList.Find (findStage);
        }

        public delegate bool CreateStage (int _stageNumber,int seed);

        public bool GenerateStage (int _stageNumber, int seed = 0)
        {
            Stage _s = GetStageSetting (_stageNumber);
            if (seed == 0)
                seed = UnityEngine.Random.Range (0, int.MaxValue);
            if (_s.GrammarXmlPath.Length > 0 && _s.VGXmlPath.Length > 0) {
                CreVoxNode root = CreVoxAttach.GenerateMissionGraph (PathCollect.gram + "/" + _s.GrammarXmlPath, seed);
                return CrevoxGeneration.GenerateRealLevel (root, _s, seed);
            }
            return false;
        }
    }
}