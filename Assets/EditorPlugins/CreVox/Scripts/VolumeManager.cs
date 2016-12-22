using UnityEngine;
using System.Collections.Generic;
using System;

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

		void Awake ()
		{
			Volume[] v = transform.GetComponentsInChildren<Volume> (false);
			if (VGlobal.GetSetting ().FakeDeco)
				for (int i = 0; i < v.Length; i++)
					v [i].gameObject.SetActive (false);
		}

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

			if (VGlobal.GetSetting().FakeDeco) {
				if (!deco) {
					deco = new GameObject ("Decoration");
					deco.transform.parent = transform;
					deco.transform.localPosition = Vector3.zero;
					deco.transform.localRotation = Quaternion.Euler (Vector3.zero);
				}
				CreateVoxels ();
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
					VGlobal vg = VGlobal.GetSetting ();
					chunk.transform.localPosition = new Vector3 (cData.ChunkPos.x * vg.w, cData.ChunkPos.y * vg.h, cData.ChunkPos.z * vg.d);
					chunk.transform.localRotation = Quaternion.Euler (Vector3.zero);
					Material vMat = Resources.Load(dungeons [vi].volumeData.vMaterial,typeof(Material)) as Material;
					if (vMat == null)
						vMat = Resources.Load(PathCollect.pieces + "/Materials/Mat_Voxel", typeof(Material)) as Material;
					chunk.GetComponent<Renderer> ().sharedMaterial = vMat;
					chunk.layer = LayerMask.NameToLayer("Floor");

					Chunk c = chunk.GetComponent<Chunk> ();
					c.Init ();
					c.cData = cData;
					c.UpdateMeshCollider ();
					c.UpdateMeshFilter ();

					CreateMarkers (c, dungeons [vi].volumeData.ArtPack);
				}
			}
		}

		void CreateMarkers (Chunk _chunk,String _ArtPack)
		{
			PaletteItem[] itemArray = new PaletteItem[0];
			if (_ArtPack.Length > 0)
				itemArray = Resources.LoadAll<PaletteItem> (_ArtPack);
			if (itemArray.Length < 1) {
				itemArray = Resources.LoadAll<PaletteItem> (PathCollect.pieces);
			}
			ChunkData cData = _chunk.cData;
			foreach (BlockAir bAir in cData.blockAirs) {
				for (int i = 0; i < bAir.pieceNames.Length; i++) {
					for (int k = 0; k < itemArray.Length; k++) {
						if (bAir.pieceNames [i] == itemArray [k].name) {
							PlacePiece (
								new WorldPos (
									bAir.BlockPos.x,
									bAir.BlockPos.y,
									bAir.BlockPos.z),
								new WorldPos (i % 3, 0, (int)(i / 3)), 
								itemArray [k].gameObject.GetComponent<LevelPiece> (),
								_chunk.transform);
						}
					}
				}
			}
		}

		public void PlacePiece (WorldPos bPos, WorldPos gPos, LevelPiece _piece, Transform _parent)
		{
			Vector3 pos = Volume.GetPieceOffset (gPos.x, gPos.z);
			VGlobal vg = VGlobal.GetSetting ();
			GameObject pObj = null;
			float x = bPos.x * vg.w + pos.x;
			float y = bPos.y * vg.h + pos.y;
			float z = bPos.z * vg.d + pos.z;
			if (_piece != null) {
				pObj = GameObject.Instantiate (_piece.gameObject);
				pObj.transform.parent = _parent;
				pObj.transform.localPosition = new Vector3 (x, y, z);
				pObj.transform.localRotation = Quaternion.Euler (0, Volume.GetPieceAngle (gPos.x, gPos.z), 0);
			}
		}

		#if UNITY_EDITOR
		public void UpdateDungeon ()
		{
			Volume[] v = transform.GetComponentsInChildren<Volume> (false);
			dungeons = new List<Dungeon> ();

			for (int i = 0; i < v.Length; i++) {
				Dungeon newDungeon;
				newDungeon.volumeData = v [i].vd;
				newDungeon.position = v [i].transform.position;
				newDungeon.rotation = v [i].transform.rotation;
				dungeons.Add (newDungeon);
			}
		}
		#endif
	}
}
