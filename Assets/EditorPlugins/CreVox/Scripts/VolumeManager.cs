﻿using UnityEngine;
using System.Collections.Generic;
using System;

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
        public bool useLocalSetting;

        [SerializeField]bool saveBackup;
        public bool SaveBackup {
            get { return useLocalSetting ? saveBackup : false; }
            set { saveBackup = useLocalSetting ? value : saveBackup; }
        }

        [SerializeField]bool useArtPack;
        public bool UseArtPack {
            get { return useLocalSetting ? useArtPack : true; }
            set { useArtPack = useLocalSetting ? value : useArtPack; }
        }

        [SerializeField]bool snapGridL;
        public bool SnapGrid {
            get { return useLocalSetting ? snapGridL : false; }
            set { snapGridL = useLocalSetting ? value : snapGridL; }
        }

        [SerializeField]bool debugRulerL;
        public bool DebugRuler {
            get { return useLocalSetting ? debugRulerL : false; }
            set { debugRulerL = useLocalSetting ? value : debugRulerL; }
        }

        [SerializeField]bool showBlockHoldL;
        public bool ShowBlockHold {
            get { return useLocalSetting ? showBlockHoldL : false; }
            set { showBlockHoldL = useLocalSetting ? value : showBlockHoldL; }
        }

        public List<Dungeon> dungeons = new List<Dungeon> ();
        public bool useStageData;
        public int currentStageData = -1;
        public StageData stageData;

        void Awake ()
        {
            if (gameObject.GetComponent (typeof(GlobalDriver)) == null) {
                gameObject.AddComponent (typeof(GlobalDriver));
            }
            if (useStageData) {
                ClearVolumes ();
                GenerateDungeonByStageData ();
                CreateVolumeMakers ();
                BuildVolumes ();
            } else {
                GenerateDungeonByChildVolumes ();
                ClearVolumes ();
                CreateVolumeMakers ();
                if (!VolumeAdapter.CheckActiveComponent ("SetupDungeon"))
                    BuildVolumes ();
            }
        }

        void Start ()
        {
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
                UnityEngine.Object.DestroyImmediate (v.gameObject, false);
            }
        }

        public void BuildVolumes ()
        {
            BroadcastMessage ("Build", SendMessageOptions.DontRequireReceiver);
        }

        void CreateVolumeMakers ()
        {
            foreach (Dungeon d in dungeons) {
                if (d.volumeData == null)
                    continue;
                GameObject volume = new GameObject (d.volumeData.ToString ());
                volume.transform.parent = transform;
                volume.transform.localPosition = d.position;
                volume.transform.localRotation = d.rotation;
                volume.SetActive (false);
                VolumeMaker vm = volume.AddComponent<VolumeMaker> ();
                vm.enabled = false;
                vm.m_vd = d.volumeData;
                vm.m_style = VolumeMaker.Style.ChunkWithPieceAndItem;
                vm.ArtPack = d.ArtPack;
                vm.vMaterial = d.vMaterial;
                volume.SetActive (true);
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
