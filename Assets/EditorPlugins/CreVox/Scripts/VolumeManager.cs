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
		public string ArtPack;
		public string vMaterial;
	}

	public class VolumeManager : MonoBehaviour
	{
		public List<Dungeon> dungeons;
		public PaletteItem[] itemArray;

		void Awake ()
		{
			Volume[] v = transform.GetComponentsInChildren<Volume> (false);
			if (v.Length > 0) {
				UpdateDungeon ();
			}

			if (VGlobal.GetSetting ().FakeDeco) {
				for (int i = 0; i < v.Length; i++) {
					v [i].enabled = false;
					GameObject.Destroy (v [i].gameObject);
				}
			}
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
				CreateVoxels ();
            }
		}

		void CreateVoxels ()
		{
			for (int vi = 0; vi < dungeons.Count; vi++) {
				GameObject volume = new GameObject ("Volume" + dungeons [vi].position.ToString ());
				volume.transform.parent = transform;
				volume.transform.localPosition = dungeons[vi].position;
				volume.transform.localRotation = dungeons[vi].rotation;

				itemArray = new PaletteItem[0];
				if (dungeons [vi].ArtPack.Length > 0)
					itemArray = VGlobal.GetSetting().UpdateItemArray (dungeons [vi].ArtPack + dungeons [vi].volumeData.subArtPack);
				if (itemArray.Length < 1) {
					itemArray = Resources.LoadAll<PaletteItem> (PathCollect.pieces);
				}

				for (int ci = 0; ci < dungeons [vi].volumeData.chunkDatas.Count; ci++) {
					ChunkData cData = dungeons [vi].volumeData.chunkDatas [ci];
					GameObject chunk = Instantiate (Resources.Load (PathCollect.chunk) as GameObject, Vector3.zero,Quaternion.Euler (Vector3.zero)) as GameObject;
					chunk.name = "Chunk" + cData.ChunkPos.ToString ();
					chunk.transform.parent = volume.transform;
					VGlobal vg = VGlobal.GetSetting ();
					chunk.transform.localPosition = new Vector3 (cData.ChunkPos.x * vg.w, cData.ChunkPos.y * vg.h, cData.ChunkPos.z * vg.d);
					chunk.transform.localRotation = Quaternion.Euler (Vector3.zero);
					Material vMat = Resources.Load(dungeons [vi].vMaterial,typeof(Material)) as Material;
					if (vMat == null)
						vMat = Resources.Load(PathCollect.defaultVoxelMaterial, typeof(Material)) as Material;
					chunk.GetComponent<Renderer> ().sharedMaterial = vMat;
					chunk.layer = LayerMask.NameToLayer("Floor");

					Chunk c = chunk.GetComponent<Chunk> ();
					c.cData = cData;
					c.Init ();

					PlacePieces (c);
				}
				CreateItems (dungeons [vi].volumeData,volume.transform);
			}
		}

		void PlacePieces (Chunk _chunk)
		{
			ChunkData cData = _chunk.cData;
			foreach (BlockAir bAir in cData.blockAirs) {
				for (int i = 0; i < bAir.pieceNames.Length; i++) {
					for (int k = 0; k < itemArray.Length; k++) {
						if (bAir.pieceNames [i] == itemArray [k].name) {
							PlacePiece (
								bAir.BlockPos,
								new WorldPos (i % 3, 0, (int)(i / 3)), 
								itemArray [k].gameObject.GetComponent<LevelPiece> (),
								_chunk.transform
							);
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

		void CreateItems(VolumeData _vData,Transform _parent)
		{
			for (int i = 0; i < _vData.blockItems.Count; i++) {
				for (int k = 0; k < itemArray.Length; k++) {
					BlockItem blockItem = _vData.blockItems [i];
					if (blockItem.pieceName == itemArray [k].name) {
						CreateItem (blockItem, i, itemArray [k].gameObject.GetComponent<LevelPiece> (), _parent);
					}
				}
			}
		}
        
		public void CreateItem(BlockItem blockItem, int _id, LevelPiece _piece, Transform _parent)
		{
			GameObject pObj;
			pObj = GameObject.Instantiate(_piece.gameObject);
			pObj.transform.parent = _parent;
			pObj.transform.localPosition = new Vector3 (blockItem.posX, blockItem.posY, blockItem.posZ);
			pObj.transform.localRotation = new Quaternion (blockItem.rotX, blockItem.rotY, blockItem.rotZ, blockItem.rotW);
            LevelPiece p = (LevelPiece)pObj.GetComponent<LevelPiece>();
            if (p != null)
            {
                p.SetupPiece(blockItem);
            }
        }

		public void UpdateDungeon ()
		{
			#if UNITY_EDITOR
			Volume[] v = transform.GetComponentsInChildren<Volume> (false);
			dungeons = new List<Dungeon> ();

			for (int i = 0; i < v.Length; i++) {
				Dungeon newDungeon = new Dungeon();
				newDungeon.volumeData = v [i].vd;
				newDungeon.position = v [i].transform.position;
				newDungeon.rotation = v [i].transform.rotation;
				newDungeon.ArtPack = v [i].ArtPack;
				newDungeon.vMaterial = v [i].vMaterial;
				dungeons.Add (newDungeon);
			}
			#endif
		}
	}
}
