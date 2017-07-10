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
		public List<Dungeon> dungeons;
		public bool autoRun = true;

		void Awake ()
		{
			if (autoRun) {
				Volume[] v = transform.GetComponentsInChildren<Volume> (false);
				if (v.Length > 0) {
					UpdateDungeon ();
					if (gameObject.GetComponent (typeof(GlobalDriver)) == null) {
						gameObject.AddComponent (typeof(GlobalDriver));
					}
					if (VGlobal.GetSetting ().Generation) {
						for (int i = 0; i < v.Length; i++) {
							GameObject.Destroy (v [i].gameObject);
						}
					}
				}
			}
		}

		void Start ()
		{
			#if UNITY_EDITOR
			if (!UnityEditor.EditorApplication.isPlaying && VGlobal.GetSetting ().saveBackup) {
				BroadcastMessage ("SubscribeEvent", SendMessageOptions.RequireReceiver);

				UnityEditor.EditorApplication.CallbackFunction _event = UnityEditor.EditorApplication.playmodeStateChanged;
				string log = "";
				for (int i = 0; i < _event.GetInvocationList ().Length; i++) {
					log = log + i + "/" + _event.GetInvocationList ().Length + ": " + _event.GetInvocationList () [i].Method.ToString () + "\n";
				}
				Debug.LogWarning (log);
			}
			#endif

			if (VGlobal.GetSetting().Generation && autoRun) {
				CreateVolumes ();
            }
		}

		public void CreateVolumes ()
		{
			for (int vi = 0; vi < dungeons.Count; vi++) {
				GameObject volume = new GameObject ("Volume" + dungeons [vi].volumeData.ToString());
				volume.transform.parent = transform;
				volume.transform.localPosition = dungeons[vi].position;
				volume.transform.localRotation = dungeons[vi].rotation;
				volume.SetActive (false);
				VolumeMaker vm = volume.AddComponent<VolumeMaker> ();
				vm.enabled = false;
				vm.m_vd = dungeons [vi].volumeData;
				vm.m_style = VolumeMaker.Style.ChunkWithPieceAndItem;
				vm.ArtPack = dungeons [vi].ArtPack;
				vm.vMaterial = dungeons [vi].vMaterial;
				volume.SetActive (true);
				vm.Build ();
			}
		}

		public void UpdateDungeon ()
		{
			Volume[] v = transform.GetComponentsInChildren<Volume> (false);
			dungeons = new List<Dungeon> ();

			for (int i = 0; i < v.Length; i++) {
				Dungeon newDungeon = new Dungeon();
				newDungeon.volumeData = v [i].vd;
				newDungeon.position = v [i].transform.position;
				newDungeon.rotation = v [i].transform.rotation;
				newDungeon.ArtPack = v [i].ArtPack;
				newDungeon.vMaterial = v [i].vMaterial;
				dungeons.Add (newDungeon);
			}
		}
	}
}
