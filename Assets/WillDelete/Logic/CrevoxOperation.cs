using System.Collections.Generic;
using UnityEngine;
using CreVox;

namespace CrevoxExtend {

	public class CrevoxOperation {
		// Volume Manager object.
		public static VolumeManager resultVolumeManager;
		// Initial the resultVolumeData and create the VolumeManager.
		public static void TransformStateIntoObject(CrevoxState state, string artPack, bool generateVolume = false) {
			if (resultVolumeManager != null) { DestroyVolume(); }
			GameObject volumeMangerObject = new GameObject() { name = "VolumeManager(Generated)" };
			resultVolumeManager = volumeMangerObject.AddComponent<VolumeManager>();
			resultVolumeManager.dungeons = new List<Dungeon> ();
			foreach (var vdataEx in state.ResultVolumeDatas) {
				CreateDungeon (vdataEx, artPack);
				if (generateVolume) {
					CreateVolumeObject (vdataEx);
				}
			}
		}
		// Create Volume object.
		public static void CreateVolumeObject(CrevoxState.VolumeDataEx vdataEx) {
			GameObject volumeObject = new GameObject() { name = vdataEx.volumeData.name };
			volumeObject.transform.parent = resultVolumeManager.transform;
			volumeObject.transform.position = vdataEx.position;
			volumeObject.transform.rotation = vdataEx.rotation;
			Volume volume = volumeObject.AddComponent<Volume>();
			volume.vd = vdataEx.volumeData;
			volume.ConnectionInfos = vdataEx.ConnectionInfos;
		}
		// Create VolumeManager Dungeons.
		public static void CreateDungeon(CrevoxState.VolumeDataEx vdataEx, string artPack) {
			Dungeon _d = new Dungeon ();
			_d.position = vdataEx.position;
			_d.rotation = vdataEx.rotation;
			_d.volumeData = vdataEx.volumeData;
			_d.ArtPack = PathCollect.artPack + "/" + artPack;
			_d.vMaterial = _d.ArtPack + "/" + artPack + "_voxel" ;
			resultVolumeManager.dungeons.Add (_d);
		}
		// Destroy all volume.
		private static void DestroyVolume() {
			MonoBehaviour.DestroyImmediate(resultVolumeManager.gameObject);
			resultVolumeManager = null;
		}
		// Get volumedata via path as string.
		public static VolumeData GetVolumeData(string path) {
			path = path.Substring(path.IndexOf("Resources") + 10, path.Length - path.IndexOf("Resources") - 10);
			path = path.Replace("\\", "/").Replace(".asset", "");
			VolumeData vdata = Resources.Load<VolumeData>(path);
			return vdata;
		}
	}
}