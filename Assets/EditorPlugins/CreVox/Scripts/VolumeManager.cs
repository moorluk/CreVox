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

	public class VolumeManager : MonoBehaviour
	{
		public List<Dungeon> dungeons;
		private GameObject deco;
		public List<GameObject> markers;

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

			if (Volume.vg.FakeDeco) {
				if (!deco) {
					deco = new GameObject ("Decoration");
					deco.transform.parent = transform;
					deco.transform.localPosition = Vector3.zero;
					deco.transform.localRotation = Quaternion.Euler (Vector3.zero);
				}
				CreateVoxels ();
				CreateMarkers ();
			}
		}

		void CreateVoxels ()
		{
			for (int vi = 0; vi < dungeons.Count; vi++) {
				GameObject volume = new GameObject ();
				volume.name = "Volume" + dungeons [vi].position.ToString ();
				volume.transform.parent = deco.transform;
				volume.transform.localPosition = dungeons[vi].position;
				volume.transform.localRotation = dungeons[vi].rotation;
				for (int ci = 0; ci < dungeons [vi].volumeData.chunkDatas.Count; ci++) {
					ChunkData cData = dungeons [vi].volumeData.chunkDatas [ci];
					GameObject chunk = Instantiate (Resources.Load (PathCollect.chunk) as GameObject, Vector3.zero,Quaternion.Euler (Vector3.zero)) as GameObject;
					chunk.name = "Chunk" + cData.ChunkPos.ToString ();
					chunk.transform.parent = volume.transform;
					chunk.transform.localPosition = new Vector3 (cData.ChunkPos.x * Volume.vg.w, cData.ChunkPos.y * Volume.vg.h, cData.ChunkPos.z * Volume.vg.d);
					chunk.transform.localRotation = Quaternion.Euler (Vector3.zero);
					Material vMat = Resources.Load(dungeons [vi].volumeData.vMaterial,typeof(Material)) as Material;
					chunk.GetComponent<Renderer> ().sharedMaterial = vMat;
					chunk.layer = LayerMask.NameToLayer("Floor");
					Chunk c = chunk.GetComponent<Chunk> ();
					c.Init ();
					c.cData.blocks = cData.blocks;
					c.UpdateMeshCollider ();
					c.UpdateMeshFilter ();
				}
			}
		}

		void CreateMarkers ()
		{
			for (int vi = 0; vi < dungeons.Count; vi++) {
				PaletteItem[] itemArray;
				itemArray = Resources.LoadAll<PaletteItem> (dungeons[vi].volumeData.ArtPack);
				for (int ci = 0; ci < dungeons [vi].volumeData.chunkDatas.Count; ci++) {
					ChunkData cData = dungeons [vi].volumeData.chunkDatas [ci];
					foreach (BlockAir bAir in cData.blockAirs) {
						for (int i = 0; i < bAir.pieceNames.Length; i++) {
							for (int k = 0; k < itemArray.Length; k++) {
								if (bAir.pieceNames [i] == itemArray [k].name) {
									PlacePiece (
										new WorldPos (
											cData.ChunkPos.x + bAir.BlockPos.x,
											cData.ChunkPos.y + bAir.BlockPos.y,
											cData.ChunkPos.z + bAir.BlockPos.z),
										new WorldPos (i % 3, 0, (int)(i / 3)), 
										itemArray [k].gameObject.GetComponent<LevelPiece> ());
								}
							}
						}
					}
				}
			}
		}

		public void PlacePiece (WorldPos bPos, WorldPos gPos, LevelPiece _piece)
		{
			Vector3 pos = Volume.GetPieceOffset (gPos.x, gPos.z);
			float x = bPos.x * Volume.vg.w + pos.x;
			float y = bPos.y * Volume.vg.h + pos.y;
			float z = bPos.z * Volume.vg.d + pos.z;
			if (_piece != null) {
				GameObject pObj = GameObject.Instantiate (_piece.gameObject);
				pObj.transform.parent = deco.transform;
				pObj.transform.localPosition = new Vector3 (x, y, z);
				pObj.transform.localRotation = Quaternion.Euler (0, Volume.GetPieceAngle (gPos.x, gPos.z), 0);
			}
		}

		#if UNITY_EDITOR
		public void UpdateDungeon ()
		{
			markers.Clear ();
			Volume[] v = transform.GetComponentsInChildren<Volume> (true);
			dungeons = new List<Dungeon> ();

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
	}
}
