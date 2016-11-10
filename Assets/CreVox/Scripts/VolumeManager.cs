using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace CreVox
{
	#if UNITY_EDITOR
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
			Volume[] v = GameObject.FindObjectsOfType<Volume> ();
			dungeons = new Dungeon[v.Length];
			for (int i = 0; i < v.Length; i++) {
				Transform vt = v [i].transform;
				dungeons [i].volume = v [i];
				dungeons [i].volumeFile = v [i].workFile;
				dungeons [i].artPack = v [i].piecePack;
				dungeons [i].position = vt.position;
				dungeons [i].rotation = vt.rotation;
			}
		}

		#endregion

		#region Decoration (Fake)

		#if UNITY_EDITOR
		private GameObject deco;

		void CreateDeco ()
		{
			deco = new GameObject ("Decoration");
			deco.transform.parent = transform;
			deco.transform.localPosition = Vector3.zero;
			deco.transform.localRotation = Quaternion.Euler (Vector3.zero);
		}

		void ClearDeco ()
		{
			if (deco)
				Object.DestroyImmediate (deco);
		}
		#endif
		#endregion
	}
}
