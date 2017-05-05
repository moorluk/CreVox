using System.Collections.Generic;
using UnityEngine;
using CreVox;
using UnityEditor;
using System.Linq;
using System.Text.RegularExpressions;

namespace CrevoxExtend {

	public class CrevoxOperation {
		// Volume Manager object.
		public static VolumeManager resultVolumeManager;
		// Initial the resultVolumeData and create the VolumeManager.
		public static void TransformStateIntoObject(CrevoxState state) {
			if (resultVolumeManager != null) { DestroyVolume(); }
			GameObject volumeMangerObject = new GameObject() { name = "VolumeManger(Generated)" };
			resultVolumeManager = volumeMangerObject.AddComponent<VolumeManager>();
			foreach (var vdataEx in state.ResultVolumeDatas) {
				CreateVolumeObject(vdataEx);
			}
			RefreshVolume();
		}
		// Create Volume object.
		public static void CreateVolumeObject(CrevoxState.VolumeDataEx vdataEx) {
			GameObject volumeObject = new GameObject() { name = vdataEx.volumeData.name };
			volumeObject.transform.parent = resultVolumeManager.transform;
			volumeObject.transform.position = vdataEx.position;
			volumeObject.transform.rotation = vdataEx.rotation;
			Volume volume = volumeObject.AddComponent<Volume>();
			volume.vd = vdataEx.volumeData;
			VolumeExtend volumeExtend = volumeObject.AddComponent<VolumeExtend>();
			volumeExtend.ConnectionInfos = vdataEx.ConnectionInfos;
		}
		// Update and repaint.
		private static void RefreshVolume() {
			resultVolumeManager.UpdateDungeon();
			SceneView.RepaintAll();
		}
		// Destroy all volume.
		private static void DestroyVolume() {
			MonoBehaviour.DestroyImmediate(resultVolumeManager.gameObject);
			resultVolumeManager = null;
		}
		// Get volumedata via path as string.
		public static VolumeData GetVolumeData(string path) {
			VolumeData vdata = (VolumeData) UnityEditor.AssetDatabase.LoadAssetAtPath(path, typeof(VolumeData));
			return vdata;
		}
	}
}