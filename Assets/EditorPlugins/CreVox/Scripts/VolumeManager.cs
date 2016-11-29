using UnityEngine;
using System.Collections.Generic;

namespace CreVox
{
	[System.Serializable]
	public struct Dungeon
	{
		public VolumeData volumeData;
		public Vector3 position;
		public Quaternion rotation;
	}

	[ExecuteInEditMode]
	public class VolumeManager : MonoBehaviour
	{

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
		}

		#region Volume Control

		public Dungeon[] dungeons;

		public void UpdateDungeon ()
		{

			Volume[] v = transform.GetComponentsInChildren<Volume> (true);
			dungeons = new Dungeon[v.Length];

			for (int i = 0; i < v.Length; i++) {
				Transform vt = v [i].transform;
				dungeons [i].volumeData = v [i].vd;
				dungeons [i].position = vt.position;
				dungeons [i].rotation = vt.rotation;
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

				Volume[] v = transform.GetComponentsInChildren<Volume> (true);
				for (int i = 0; i < v.Length; i++) {
					v [i].gameObject.SetActive (false);
				}
			}
		}

		public void ClearDeco ()
		{
			if (deco) {
				Object.DestroyImmediate (deco);

				Volume[] v = transform.GetComponentsInChildren<Volume> (true);
				for (int i = 0; i < v.Length; i++) {
					v [i].gameObject.SetActive (true);
				}
			}
		}
		#endif
		#endregion
	}
}
