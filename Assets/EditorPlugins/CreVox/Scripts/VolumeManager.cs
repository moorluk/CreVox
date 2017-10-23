using UnityEngine;
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
        public static bool saveBackup;
        public static bool volumeShowArtPack = true;
        public static bool Generation = true;
        public static bool snapGrid;
        public static bool debugRuler;
        public static bool showBlockHold;

        public bool useLocalSetting;
        public bool saveBackupL;
        public bool volumeShowArtPackL;
        public bool GenerationL;
        public bool snapGridL;
        public bool debugRulerL;
        public bool showBlockHoldL;

        public List<Dungeon> dungeons = new List<Dungeon> ();
        public bool useStageData;
        public int currentStageData = -1;
        public StageData stageData;

        void Awake ()
        {
            if (gameObject.GetComponent (typeof(GlobalDriver)) == null) {
                gameObject.AddComponent (typeof(GlobalDriver));
            }
            if (useLocalSetting ? GenerationL : Generation) {
                ClearVolumes ();
                if (useStageData)
                    RandomDungeon ();
                else
                    UpdateDungeon ();
            }
        }

        void Start ()
        {
            #if UNITY_EDITOR
            if (!UnityEditor.EditorApplication.isPlaying && useLocalSetting ? saveBackupL : saveBackup) {
                BroadcastMessage ("SubscribeEvent", SendMessageOptions.RequireReceiver);

                UnityEditor.EditorApplication.CallbackFunction _event = UnityEditor.EditorApplication.playmodeStateChanged;
                string log = "";
                for (int i = 0; i < _event.GetInvocationList ().Length; i++) {
                    log += i + "/" + _event.GetInvocationList ().Length + ": " + _event.GetInvocationList () [i].Method + "\n";
                }
                Debug.LogWarning (log);
            }
            #endif

            if (useLocalSetting ? GenerationL : Generation) {
                CreateVolumeMakers ();
            }
        }

        public void ClearVolumes (bool runtime = true)
        {
            Volume[] vs = transform.GetComponentsInChildren<Volume>(false);
            foreach(Volume v in vs){
                if (runtime) UnityEngine.Object.Destroy(v.gameObject);
                else UnityEngine.Object.DestroyImmediate(v.gameObject, false);
            }
        }

        public void CreateVolumeMakers ()
        {
            foreach (Dungeon d in dungeons) {
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
                if ((useLocalSetting ? GenerationL : Generation) && !VolumeAdapter.CheckSetupDungeon ()) {
                    vm.Build ();
                }
            }
        }

        /// <summary>
        /// Search all volumes in children and create new Dungeon list.
        /// </summary>
        public void UpdateDungeon ()
        {
            Volume[] vs = transform.GetComponentsInChildren<Volume> (false);

            if (vs.Length < 1 && dungeons.Count > 0)
                return;
            dungeons.Clear();
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

        public void RandomDungeon()
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
