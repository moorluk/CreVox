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
//			GenerateDecoration ();
		}

		#region Volume Control

		public List<Dungeon> dungeons;

		public void UpdateDungeon ()
		{
			markers.Clear ();
			Volume[] v = transform.GetComponentsInChildren<Volume> (true);
			dungeons = new List<Dungeon>();

			for (int i = 0; i < v.Length; i++) {
				Dungeon newDungeon;
				newDungeon.volumeData = v [i].vd;
				newDungeon.position = v [i].transform.position;
				newDungeon.rotation = v [i].transform.rotation;
				dungeons.Add (newDungeon);
				PaletteItem[] pieces = v [i].nodeRoot.transform.GetComponentsInChildren<PaletteItem> ();
				for (int p = 0; p < pieces.Length; p++) {
					markers.Add (pieces [i].gameObject);
				}
			}
		}

		void GenerateDecoration()
		{
			
			BehaviorTree bTree = GetComponent<BehaviorTree> ();
			for (int i = 0; i < markers.Count; i++) {
				((SharedGameObject)bTree.GetVariable ("root")).Value = markers [i];
				((SharedString)bTree.GetVariable ("Marker")).Value = markers [i].GetComponent<PaletteItem> ().markType.ToString ();
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
		public List<GameObject> markers;

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
