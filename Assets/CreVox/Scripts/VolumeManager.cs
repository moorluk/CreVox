using UnityEngine;
//using System.Collections;
using System.Collections.Generic;
//using System.Xml.Schema;

namespace CreVox
{
	[System.Serializable]
	public struct Dungeon
	{
		public Volume volume;
		public string volumeFile;
		public Vector3 position;
		public Quaternion rotation;
		public string artPack;
		public Material vertexMaterial;
	}

	#if UNITY_EDITOR
	[System.Serializable]
	public class VolumeGlobal
	{
		public static bool saveBackup = true;
		public static bool debugRuler = false;
	}
	#endif

	[ExecuteInEditMode]
	public class VolumeManager : MonoBehaviour
	{
		#if UNITY_EDITOR
		public bool saveBackupFile;
		public bool showDebugRuler;
		#endif

		void Start ()
		{
			#if UNITY_EDITOR
			if (!UnityEditor.EditorApplication.isPlaying && VolumeGlobal.saveBackup) {
				BroadcastMessage ("SubscribeEvent", SendMessageOptions.RequireReceiver);

				UnityEditor.EditorApplication.CallbackFunction _event = UnityEditor.EditorApplication.playmodeStateChanged;
				string log = "";
				for (int i = 0; i < _event.GetInvocationList ().Length; i++) {
					log = log + i + "/" + _event.GetInvocationList ().Length + ": " + _event.GetInvocationList () [i].Method.ToString () + "\n";
				}
				Debug.LogWarning (log);
			}
			#endif
		}

		#region Volume Control

		public Dungeon[] dungeons;

		public void UpdateDungeon ()
		{
			Dictionary<Volume,string> oldDungeon = new Dictionary<Volume, string> ();
			for (int i = 0; i < dungeons.Length; i++) {
				oldDungeon.Add (dungeons [i].volume, dungeons [i].artPack);
			}

			Volume[] v = transform.GetComponentsInChildren<Volume> (true);
			dungeons = new Dungeon[v.Length];

			for (int i = 0; i < v.Length; i++) {
				Transform vt = v [i].transform;
				dungeons [i].volume = v [i];
				dungeons [i].volumeFile = v [i].workFile;
				dungeons [i].position = vt.position;
				dungeons [i].rotation = vt.rotation;
				if (oldDungeon.ContainsKey (v [i])) {
					dungeons [i].artPack = oldDungeon [v [i]];
					dungeons [i].vertexMaterial = FindMaterial (dungeons [i].artPack);
				} else
					dungeons [i].artPack = PathCollect.pieces;
			}
		}

		public Material FindMaterial (string _path)
		{
			Material[] tempM = Resources.LoadAll<Material> (_path);
			for (int i = 0; i < tempM.Length; i++) {
				if (tempM [i].name.Contains ("voxel")) {
					return tempM [i];
				}
			}
			return null;
		}

		#endregion

		#region Decoration (Fake)

		#if UNITY_EDITOR
		private GameObject deco;

		public void CreateDeco ()
		{
			if (!deco) {
				deco = new GameObject ("Decoration");
				deco.transform.parent = transform;
				deco.transform.localPosition = Vector3.zero;
				deco.transform.localRotation = Quaternion.Euler (Vector3.zero);
				for (int i = 0; i < dungeons.Length; i++) {
					dungeons [i].volume.gameObject.SetActive (false);
				}
			}
		}

		public void ClearDeco ()
		{
			if (deco) {
				Object.DestroyImmediate (deco);
				for (int i = 0; i < dungeons.Length; i++) {
					dungeons [i].volume.gameObject.SetActive (true);
				}
			}
		}
		#endif
		#endregion
	}
}
