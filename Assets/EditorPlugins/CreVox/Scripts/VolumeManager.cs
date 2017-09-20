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

        public bool useLocalSetting = false;
        public bool saveBackupL;
        public bool volumeShowArtPackL;
        public bool GenerationL;
        public bool snapGridL;
        public bool debugRulerL;
        public bool showBlockHoldL;

        public List<Dungeon> dungeons;
        public bool useStageData = false;
        public StageData stageData;

        void Awake ()
        {
            if (gameObject.GetComponent (typeof(GlobalDriver)) == null) {
                gameObject.AddComponent (typeof(GlobalDriver));
            }
            if (useLocalSetting ? GenerationL : Generation) {
                ClearVolumes (true);
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
                    log = log + i + "/" + _event.GetInvocationList ().Length + ": " + _event.GetInvocationList () [i].Method.ToString () + "\n";
                }
                Debug.LogWarning (log);
            }
            #endif

            CreateVolumeMakers ();
        }

        public void ClearVolumes (bool runtime = true)
        {
            Volume[] v = transform.GetComponentsInChildren<Volume> (false);
            if (v.Length > 0) {
                if (useStageData) {
                    RandomDungeon ();
                } else {
                    UpdateDungeon ();
                }
                for (int i = 0; i < v.Length; i++) {
                    if (runtime)
                        GameObject.Destroy (v [i].gameObject);
                    else
                        GameObject.DestroyImmediate (v [i].gameObject, false);
                }
            }
        }

        public void CreateVolumes ()
        {
            for (int vi = 0; vi < dungeons.Count; vi++) {
                GameObject volume = new GameObject (dungeons [vi].volumeData.name);
                volume.transform.parent = transform;
                volume.transform.localPosition = dungeons [vi].position;
                volume.transform.localRotation = dungeons [vi].rotation;
                Volume v = volume.AddComponent<Volume> ();
                v.vd = dungeons [vi].volumeData;
                v.ArtPack = dungeons [vi].ArtPack;
                v.vMaterial = dungeons [vi].vMaterial;
            }
            BroadcastMessage("BuildVolume",SendMessageOptions.DontRequireReceiver);
        }

        public void CreateVolumeMakers ()
        {
            for (int vi = 0; vi < dungeons.Count; vi++) {
                GameObject volume = new GameObject (dungeons [vi].volumeData.ToString ());
                volume.transform.parent = transform;
                volume.transform.localPosition = dungeons [vi].position;
                volume.transform.localRotation = dungeons [vi].rotation;
                volume.SetActive (false);
                VolumeMaker vm = volume.AddComponent<VolumeMaker> ();
                vm.enabled = false;
                vm.m_vd = dungeons [vi].volumeData;
                vm.m_style = VolumeMaker.Style.ChunkWithPieceAndItem;
                vm.ArtPack = dungeons [vi].ArtPack;
                vm.vMaterial = dungeons [vi].vMaterial;
                volume.SetActive (true);
                if ((useLocalSetting ? GenerationL : Generation) && !VolumeAdapter.CheckSetupDungeon ()) {
                    vm.Build ();
                }
            }
        }

        public void UpdateDungeon ()
        {
            Volume[] v = transform.GetComponentsInChildren<Volume> (false);
            dungeons = new List<Dungeon> ();

            for (int i = 0; i < v.Length; i++) {
                Dungeon newDungeon = new Dungeon ();
                newDungeon.volumeData = v [i].vd;
                newDungeon.position = v [i].transform.position;
                newDungeon.rotation = v [i].transform.rotation;
                newDungeon.ArtPack = v [i].ArtPack;
                newDungeon.vMaterial = v [i].vMaterial;
                dungeons.Add (newDungeon);
            }
        }

        public void RandomDungeon()
        {
            UnityEngine.Random.InitState (System.Guid.NewGuid ().GetHashCode ());
            int i = UnityEngine.Random.Range (0, stageData.stageList.Count);
            dungeons.Clear ();
            foreach (Dungeon d in stageData.stageList[i].Dlist) {
                dungeons.Add (d);
            }
        }
    }
}
