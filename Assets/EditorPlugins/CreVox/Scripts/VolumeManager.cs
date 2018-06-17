using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

namespace CreVox
{
    [System.Serializable]
    public struct Dungeon
    {
        public VolumeData volumeData;
        public Vector3 position;
        public Quaternion rotation;
        public string ArtPack;
        public string vMaterial;
    }

    public class VolumeManager : MonoBehaviour
    {
        #region LocalSetting

        public bool useLocalSetting;
        public VGlobal.VGSetting setting;
        
        public bool SaveBackup {
            get { return useLocalSetting ? setting.saveBackup : VGlobal.GetSetting().setting.saveBackup; }
            set { setting.saveBackup = useLocalSetting ? value : setting.saveBackup; }
        }
        
        public bool UseArtPack {
            get { return useLocalSetting ? setting.useArtPack : VGlobal.GetSetting().setting.useArtPack; }
            set { setting.useArtPack = useLocalSetting ? value : setting.useArtPack; }
        }
        
        public bool SnapGrid {
            get { return useLocalSetting ? setting.snapGridL : VGlobal.GetSetting().setting.snapGridL; }
            set { setting.snapGridL = useLocalSetting ? value : setting.snapGridL; }
        }
        
        public bool DebugRuler {
            get { return useLocalSetting ? setting.debugRulerL : VGlobal.GetSetting().setting.debugRulerL; }
            set { setting.debugRulerL = useLocalSetting ? value : setting.debugRulerL; }
        }
        
        public bool ShowBlockHold {
            get { return useLocalSetting ? setting.showBlockHoldL : VGlobal.GetSetting().setting.showBlockHoldL; }
            set { setting.showBlockHoldL = useLocalSetting ? value : setting.showBlockHoldL; }
        }

        public bool DebugLog
        {
            get { return useLocalSetting ? setting.debugLog : VGlobal.GetSetting().setting.debugLog; }
            set { setting.debugLog = useLocalSetting ? value : setting.debugLog; }
        }

        #endregion

        public List<Dungeon> dungeons = new List<Dungeon> ();
        public bool useStageData;
        public int currentStageData = -1;
        public StageData stageData;

        void Awake ()
        {
            ClearVolumes();
            if (gameObject.GetComponent (typeof(GlobalDriver)) == null) {
                gameObject.AddComponent (typeof(GlobalDriver));
            }
        }

        List<VolumeMaker> vms = new List<VolumeMaker>();
        public bool loaded;

        IEnumerator CheckLoadCompeleted ()
        {
            while (!loaded) {
                loaded = true;
                foreach (var vm in vms) {
                    if (!vm.IsLoadCompeleted ()) {
                        loaded = false;
                        break;
                    }
                }
                if (loaded) {
                    VolumeAdapter.UpdatePortals (gameObject);
                    yield return null;
                    BroadcastMessage("CollectMiniMap");
                    var sectrs = GetComponentsInChildren<SECTR_Sector>();
                    for (int i = 0; i < sectrs.Length; i++)
                        sectrs[i].enabled = true;
                    VolumeAdapter.AfterLoadComplete();
                    if (VGlobal.GetSetting().setting.debugLog) Debug.Log("<color=teal>Load volumes complete</color> time: " + Time.realtimeSinceStartup);
                    yield break;
                }
                yield return null;
            }
        }

        void Start ()
        {
            if (useStageData) {
                GenerateDungeonByStageData();
            } else {
                GenerateDungeonByChildVolumes();
            }
            CreateVolumeMakers();
            BuildVms();
            StartCoroutine(CheckLoadCompeleted());

            #if UNITY_EDITOR
            if (!UnityEditor.EditorApplication.isPlaying && SaveBackup) {
                BroadcastMessage ("SubscribeEvent", SendMessageOptions.RequireReceiver);

                UnityEditor.EditorApplication.CallbackFunction _event = UnityEditor.EditorApplication.playmodeStateChanged;
                string log = "";
                for (int i = 0; i < _event.GetInvocationList ().Length; i++) {
                    log += i + "/" + _event.GetInvocationList ().Length + ": " + _event.GetInvocationList () [i].Method + "\n";
                }
                Debug.LogWarning (log);
            }
            #endif
        }

        public void ClearVolumes ()
        {
            Volume[] vs = transform.GetComponentsInChildren<Volume> (false);
            foreach (Volume v in vs) {
                DestroyImmediate (v.gameObject, false);
            }
        }

        public void BuildVolumes()
        {
            Debug.Log("Many Volume Build !!!");
            Volume[] vs = transform.GetComponentsInChildren<Volume>(false);
            foreach (Volume v in vs)
            {
                v.SendMessage("Build", SendMessageOptions.DontRequireReceiver);
            }
        }

        void BuildVms ()
        {
            foreach (var v in vms)
                v.SendMessage ("Build", SendMessageOptions.DontRequireReceiver);
        }

        void CreateVolumeMakers ()
        {
            vms.Clear ();
            foreach (Dungeon d in dungeons) {
                if (d.volumeData == null)
                    continue;
                GameObject volume = new GameObject (d.volumeData.name);
                volume.transform.parent = transform;
                volume.transform.localPosition = d.position;
                volume.transform.localRotation = d.rotation;
                VolumeMaker vm = volume.AddComponent<VolumeMaker> ();
                vm.m_vd = d.volumeData;
                vm.m_style = VolumeMaker.Style.ChunkWithPieceAndItem;
                vm.ArtPack = d.ArtPack;
                vm.vMaterial = d.vMaterial;
                vms.Add (vm);
            }
        }

        /// <summary>
        /// Search all volumes in children and create new Dungeon list.
        /// </summary>
        public void GenerateDungeonByChildVolumes ()
        {
            Volume[] vs = transform.GetComponentsInChildren<Volume> (false);
            if (vs.Length < 1 && dungeons.Count > 0)
                return;
            dungeons.Clear ();
            foreach (Volume v in vs) {
                Dungeon newDungeon = new Dungeon ();
                newDungeon.volumeData = v.vd;
                newDungeon.position = v.transform.position;
                newDungeon.rotation = v.transform.rotation;
                newDungeon.ArtPack = v.ArtPack;
                newDungeon.vMaterial = v.vMaterial;
                dungeons.Add (newDungeon);
            }
        }

        /// <summary>
        /// Generates the dungeon by StageData.
        /// </summary>
        public void GenerateDungeonByStageData ()
        {
            UnityEngine.Random.InitState (Guid.NewGuid ().GetHashCode ());
            int i = UnityEngine.Random.Range (0, stageData.stageList.Count);
            dungeons.Clear ();
            foreach (Dungeon d in stageData.stageList[i].Dlist) {
                dungeons.Add (d);
            }
            currentStageData = i;
        }
    }
}
