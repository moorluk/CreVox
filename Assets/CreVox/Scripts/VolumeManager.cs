using UnityEngine;
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
		public Dungeon[] volumes;
		public Volume volume;

		#if UNITY_EDITOR
		public static bool saveBackup = true;
		public static bool debugRuler = false;
		public bool saveBackupFile;
		public bool showDebugRuler;

		void Start ()
		{
			if (!UnityEditor.EditorApplication.isPlaying && saveBackup) {
				BroadcastMessage ("SubscribeEvent", SendMessageOptions.RequireReceiver);

				UnityEditor.EditorApplication.CallbackFunction _event = UnityEditor.EditorApplication.playmodeStateChanged;
				string log = "";
				for (int i = 0; i < _event.GetInvocationList ().Length; i++) {
					log = log + i + "/" + _event.GetInvocationList ().Length + ": " + _event.GetInvocationList () [i].Method.ToString () + "\n";
				}
				Debug.LogWarning (log);
			}
		}
	
		void LateUpdate ()
		{
			if (saveBackup != saveBackupFile) {
				saveBackup = saveBackupFile;
			}
			if (debugRuler != showDebugRuler) {
				debugRuler = showDebugRuler;
				Debug.LogWarning ("Show Debug Ruler : " + debugRuler);
				BroadcastMessage ("ShowRuler", SendMessageOptions.DontRequireReceiver);
			}
		}
		#endif
	}
}
