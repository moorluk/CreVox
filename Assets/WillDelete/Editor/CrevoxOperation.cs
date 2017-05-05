using System.Collections.Generic;
using UnityEngine;
using CreVox;
using UnityEditor;
using System.Linq;
using System.Text.RegularExpressions;

namespace CrevoxExtend {

	public class CrevoxOperation {
		// Compute the position after rotated.
		public static WorldPos AbsolutePosition(WorldPos position, float degree) {
			Vector2 aPoint = new Vector2(position.x, position.z);
			// Set 4, 4 to be center point.
			float rad = degree * Mathf.Deg2Rad;
			float sin = Mathf.Sin(rad);
			float cos = Mathf.Cos(rad);
			return new WorldPos((int) Mathf.Round(aPoint.x * cos + aPoint.y * sin),
				position.y,
				(int) Mathf.Round(aPoint.y * cos - aPoint.x * sin));
		}
		public static Vector3 AbsolutePosition(Vector3 position, float degree) {
			Vector2 aPoint = new Vector2(position.x, position.z);
			// Set 4, 4 to be center point.
			float rad = degree * Mathf.Deg2Rad;
			float sin = Mathf.Sin(rad);
			float cos = Mathf.Cos(rad);
			return new Vector3(Mathf.Round(aPoint.x * cos + aPoint.y * sin),
				position.y,
				Mathf.Round(aPoint.y * cos - aPoint.x * sin));
		}
		// Volume Manager object.
		public static VolumeManager resultVolumeManager;
		private static Dictionary<VolumeData, List<ConnectionInfo>> doorInfoVdataTable;

		// Create Volume object and return it.
		public static Volume CreateVolumeObject(CrevoxState.VolumeDataEx vdataEx) {
			GameObject volumeObject = new GameObject() { name = vdataEx.volumeData.name };
			Volume volume = volumeObject.AddComponent<Volume>();
			volume.vd = vdataEx.volumeData;
			VolumeExtend volumeExtend = volumeObject.AddComponent<VolumeExtend>();
			volumeExtend.ConnectionInfos = vdataEx.ConnectionInfos;

			volumeObject.transform.parent = resultVolumeManager.transform;
			volumeObject.transform.position = vdataEx.position;
			volumeObject.transform.rotation = vdataEx.rotation;
			return volume;
		}

		// Initial the resultVolumeData and create the VolumeManager.
		public static void InitialVolume(List<CrevoxState.VolumeDataEx> vdataExList) {
			if (resultVolumeManager != null) { DestroyVolume(); }
			doorInfoVdataTable = new Dictionary<VolumeData, List<ConnectionInfo>>();
			GameObject volumeMangerObject = new GameObject() { name = "VolumeManger(Generated)" };
			resultVolumeManager = volumeMangerObject.AddComponent<VolumeManager>();
			foreach (var vdataEx in vdataExList) {
				CreateVolumeObject(vdataEx);
			}
			RefreshVolume();
		}
		// Update and repaint.
		public static void RefreshVolume() {
			resultVolumeManager.UpdateDungeon();
			SceneView.RepaintAll();
		}
		// Destroy all volume.
		public static void DestroyVolume() {
			MonoBehaviour.DestroyImmediate(resultVolumeManager.gameObject);
			resultVolumeManager = null;
		}
		// Get volumedata via path as string.
		public static VolumeData GetVolumeData(string path) {
			VolumeData vdata = (VolumeData) AssetDatabase.LoadAssetAtPath(path, typeof(VolumeData));
			return vdata;
		}
	}
}