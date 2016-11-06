using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;

namespace CreVox
{
	[ExecuteInEditMode]
	public class VolumeManager : MonoBehaviour
	{
		[System.Serializable]
		public struct Dungeon
		{
			public Volume volume;
			public Vector3 position;
			public Vector3 rotation;
		}
		public static bool saveBackup = true;
		public bool saveBackupFile;

		public Dungeon[] volumes;
		public Volume volume;

		void Start ()
		{
			if (!EditorApplication.isPlaying && saveBackup) {
				BroadcastMessage ("SubscribeEvent", SendMessageOptions.RequireReceiver);

				EditorApplication.CallbackFunction _event = EditorApplication.playmodeStateChanged;
				string log = "";
				for (int i = 0; i < _event.GetInvocationList ().Length; i++) {
					log = log + i + "/" + _event.GetInvocationList ().Length + ": " + _event.GetInvocationList () [i].Method.ToString () + "\n";
				}
				Debug.LogWarning (log);
			}
		}
	
		void LateUpdate ()
		{
			if (saveBackupFile != saveBackup)
				saveBackup = saveBackupFile;
		}
	}
}
