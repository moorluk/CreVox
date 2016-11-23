using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace CreVox
{
	[SelectionBase]
	[ExecuteInEditMode]
	public class Volume : MonoBehaviour
	{
		public Volume volume;
		public VGlobal vg;

		public GameObject pieces;

		public string workFile/* = PathCollect.save + "/temp"*/;
		public string tempPath;

		public string piecePack = PathCollect.pieces;
		public Material vertexMaterial;

		void Awake ()
		{
			volume = this;
		}

		void Start ()
		{
			vg = VGlobal.GetSetting ();
			LoadTempWorld ();
//			LinkAllVData ();
		}

		void Update ()
		{
			if (!vg)
				vg = VGlobal.GetSetting ();
			#if UNITY_EDITOR
			CompileSave ();
			#endif
//			if (chunks.Count < 1)
//				LinkAllVData ();
		}

		#region VolumeData

		public VolumeData vd;
		/*
		public Save VData2Save()
		{
			Save _save = new Save ();
			_save.chunkX = vd.chunkX;
			_save.chunkY = vd.chunkY;
			_save.chunkZ = vd.chunkZ;
			_save.blocks = new Dictionary<WorldPos, Block> ();
			foreach (VolumeData.ChunkData _cd in vd.chunkDatas) {
				for (int i = 0; i < _cd.blocks.Count; i++) {
					Block _b = _cd.blocks [i];
					WorldPos volPos = new WorldPos (
						                  (int)(_cd.blocks [i].BlockPos.x + _cd.ChunkPos.x),
						                  (int)(_cd.blocks [i].BlockPos.y + _cd.ChunkPos.y),
						                  (int)(_cd.blocks [i].BlockPos.z + _cd.ChunkPos.z)
					                  );
					if (_cd.blocks [i] is BlockAir) {
						Debug.Log (volPos.ToString ()); 
						BlockAir _bAir = (BlockAir)(_cd.blocks [i]);
						_save.blocks.Add (volPos, _bAir);
					} else {
						_save.blocks.Add (volPos, _b);
					}
				}
			}
			return _save;
		}

		public void LinkVData (int x, int y, int z, Block _VDblock)
		{
			WorldPos _pos = _VDblock.BlockPos;
			GetChunk (x, y, z).SetBlock(_pos.x, _pos.y, _pos.z,ref _VDblock);
		}

		public void LinkAllVData()
		{
//			BuildVolume (VData2Save ());
			Init (vd.chunkX, vd.chunkY, vd.chunkZ);

			foreach (Chunk _chunk in chunks) {
				WorldPos _pos = _chunk.pos;

				VolumeData.ChunkData CData = vd.GetChunk (_pos);
				_chunk.pos = CData.ChunkPos;

//				Dictionary<WorldPos,Block> BDatas = vd.GetBlockDictionary (CData);
				foreach (Block b in CData.blocks) {
					var _b = b;
					_chunk.SetBlock (b.BlockPos.x, b.BlockPos.y, b.BlockPos.z, ref _b);
				}
			}
		}

		public void WriteVData ()
		{
			if (vd == null)
				vd = VolumeData.GetVData (workFile);
			vd.chunkDatas = new List<VolumeData.ChunkData> ();
			foreach (Chunk _chunk in chunks) {
				WorldPos _pos = _chunk.pos;
				VolumeData.ChunkData newChunkData = new VolumeData.ChunkData ();

				newChunkData.ChunkPos = _pos;

				for (int b2 = 0; b2 < vg.chunkSize; b2++) {
					for (int b1 = 0; b1 < vg.chunkSize; b1++) {
						for (int b3 = 0; b3 < vg.chunkSize; b3++) {
							Block block = _chunk.GetBlock (b1, b2, b3);
							if (block != null) {
								BlockAir blockAir = _chunk.GetBlock (b1, b2, b3) as BlockAir;
								newChunkData.blocks.Add ((blockAir != null) ? blockAir : block);
							}
						}
					}
				}
				vd.chunkDatas.Add (newChunkData);
			}

			VDataSetDirty ();
			LinkAllVData ();
		}

		void VDataSetDirty()
		{
			#if UNITY_EDITOR
			EditorUtility.SetDirty (vd);
			#endif
		}
*/
		#endregion

		#region Chunk

		private GameObject chunkPrefab;
		public List<Chunk> chunks = new List<Chunk> ();
//		public Dictionary<WorldPos, Chunk> chunks = new Dictionary<WorldPos, Chunk> ();
		public int chunkX = 1;
		public int chunkY = 1;
		public int chunkZ = 1;

		public void BuildVolume (Save _save)
		{
			PaletteItem[] itemArray = Resources.LoadAll<PaletteItem> (vg.FakeDeco ? piecePack : PathCollect.pieces);

			Reset ();
			Init (_save.chunkX, _save.chunkY, _save.chunkZ);
			foreach (var blockPair in _save.blocks) {
				Block block = blockPair.Value;
				if (block != null) {
					if (block is BlockAir) {
						SetBlock (blockPair.Key.x, blockPair.Key.y, blockPair.Key.z, new BlockAir ());
						BlockAir bAir = blockPair.Value as BlockAir;
						for (int i = 0; i < bAir.pieceNames.Length; i++) {
							for (int k = 0; k < itemArray.Length; k++) {
								if (bAir.pieceNames [i] == itemArray [k].name) {
									PlacePiece (blockPair.Key, new WorldPos (i % 3, 0, (int)(i / 3)), itemArray [k].gameObject.GetComponent<LevelPiece> ());
									break;
								}
							}
						}
					} else {
						SetBlock (blockPair.Key.x, blockPair.Key.y, blockPair.Key.z, block);
					}
				}
			}
			UpdateChunks ();
		}

		public void Init (int _chunkX, int _chunkY, int _chunkZ)
		{
			chunkPrefab = Resources.Load (PathCollect.chunk) as GameObject;

			chunkX = _chunkX;
			chunkY = _chunkY;
			chunkZ = _chunkZ;

			pieces = new GameObject ("Pieces");
			pieces.transform.parent = transform;
			pieces.transform.localPosition = Vector3.zero;
			pieces.transform.localRotation = Quaternion.Euler (Vector3.zero);

			CreateChunks ();

			#if UNITY_EDITOR
			if (!EditorApplication.isPlaying) {
				CreateRuler ();
				CreateLevelRuler ();
				CreateBox ();
				ShowRuler ();
			}
			#endif
		}

		public void Reset ()
		{
			if (chunks != null) {
				DestoryChunks ();
				chunks.Clear ();
			}
			if (pieces)
				GameObject.DestroyImmediate (pieces);
			#if UNITY_EDITOR
			if (ruler)
				GameObject.DestroyImmediate (ruler);
			if (layerRuler)
				GameObject.DestroyImmediate (layerRuler);

			mColl = null;
			bColl = null;
			#endif

			for (int i = transform.childCount; i > 0; i--) {
				GameObject.DestroyImmediate (transform.GetChild (i - 1).gameObject);
			}
		}

		public void UpdateChunks ()
		{
			for (int x = 0; x < chunkX; x++) {
				for (int y = 0; y < chunkY; y++) {
					for (int z = 0; z < chunkZ; z++) {
						GetChunk (x * vg.chunkSize, y * vg.chunkSize, z * vg.chunkSize).UpdateChunk ();
					}
				}
			}
		}

		void CreateChunks ()
		{
			for (int x = 0; x < chunkX; x++) {
				for (int y = 0; y < chunkY; y++) {
					for (int z = 0; z < chunkZ; z++) {
						CreateChunk (x * vg.chunkSize, y * vg.chunkSize, z * vg.chunkSize);
						Chunk newChunk = GetChunk (x * vg.chunkSize, y * vg.chunkSize, z * vg.chunkSize);
						newChunk.Init ();
//						vd.Add (newChunk);
					}
				}
			}
		}

		void CreateChunk (int x, int y, int z)
		{
			WorldPos worldPos = new WorldPos (x, y, z);

			//Instantiate the chunk at the coordinates using the chunk prefab
			GameObject newChunkObject = Instantiate (
				                            chunkPrefab, new Vector3 (x * vg.w, y * vg.h, z * vg.d),
				                            Quaternion.Euler (Vector3.zero)
			                            ) as GameObject;
			newChunkObject.name = "Chunk(" + x + "," + y + "," + z + ")";
			newChunkObject.transform.parent = transform;
			newChunkObject.transform.localPosition = new Vector3 (x * vg.w, y * vg.h, z * vg.d);
			newChunkObject.transform.localRotation = Quaternion.Euler (Vector3.zero);
			if (vertexMaterial != null)
				newChunkObject.GetComponent<Renderer> ().material = vertexMaterial;
			#if UNITY_EDITOR
			newChunkObject.layer = LayerMask.NameToLayer ((EditorApplication.isPlaying)?"Floor":"Editor");
			#else
			newChunkObject.layer = LayerMask.NameToLayer("Floor");
			#endif
			Chunk newChunk = newChunkObject.GetComponent<Chunk> ();

			newChunk.pos = worldPos;
			newChunk.volume = this;

			chunks.Add (newChunk);

			for (int xi = 0; xi < vg.chunkSize; xi++) {
				for (int yi = 0; yi < vg.chunkSize; yi++) {
					for (int zi = 0; zi < vg.chunkSize; zi++) {
						SetBlock (x + xi, y + yi, z + zi, null);
					}
				}
			}
		}

		void DestoryChunks ()
		{
			for (int x = 0; x < chunkX; x++) {
				for (int y = 0; y < chunkY; y++) {
					for (int z = 0; z < chunkZ; z++) {
						DestroyChunk (x * vg.chunkSize, y * vg.chunkSize, z * vg.chunkSize);
					}
				}
			}
		}

		void DestroyChunk (int x, int y, int z)
		{
			Chunk chunk = GetChunk (x, y, z);
			if (chunk != null) {
				if (chunk.gameObject) {
					#if UNITY_EDITOR
					GameObject.DestroyImmediate (chunk.gameObject);
					#else
					Object.Destroy(chunk.gameObject);
					#endif
					chunk.Destroy ();
					chunks.Remove (chunk);
				}
			}
		}

		public Chunk GetChunk (int x, int y, int z)
		{
			WorldPos pos = new WorldPos ();
			float multiple = vg.chunkSize;
			pos.x = Mathf.FloorToInt (x / multiple) * vg.chunkSize;
			pos.y = Mathf.FloorToInt (y / multiple) * vg.chunkSize;
			pos.z = Mathf.FloorToInt (z / multiple) * vg.chunkSize;
			Chunk containerChunk = null;
			foreach (Chunk _chunk in chunks) {
				if (_chunk.pos.Compare (pos))
					containerChunk = _chunk;
			}
			return containerChunk;
		}

		#endregion

		#region Block

		public Block GetBlock (int x, int y, int z)
		{
			Chunk containerChunk = GetChunk (x, y, z);
			if (containerChunk != null) {
				Block block = containerChunk.GetBlock (
					x - containerChunk.pos.x,
					y - containerChunk.pos.y,
					z - containerChunk.pos.z);

				return block;
			} else {
				return null;
			}

		}

		public void SetBlock (int x, int y, int z, Block _block)
		{
			Chunk chunk = GetChunk (x, y, z);
			if (chunk != null) {
				WorldPos newBlockPos = new WorldPos (x - chunk.pos.x, y - chunk.pos.y, z - chunk.pos.z);
				if (_block != null) {
					_block.BlockPos = newBlockPos;
				}
				if (_block is BlockAir) {
					BlockAir _bAir = _block as BlockAir;
					chunk.SetBlock (newBlockPos.x, newBlockPos.y, newBlockPos.z, _bAir);
				} else {
					chunk.SetBlock (newBlockPos.x, newBlockPos.y, newBlockPos.z, _block);
				}

//				if (vd != null) {
//					VolumeData.ChunkData _VDchunk = vd.GetChunk (chunk.pos);
//					if (_VDchunk == null)
//						vd.AddChunk (chunk);
//					Block _VDblock = vd.GetBlock (newBlockPos, chunk.pos);
//					if (_block != null) {
//						if (_VDblock != null) {
//							_VDblock = _block;
//						} else {
//							_VDchunk.blocks.Add (_block);
//							VDataSetDirty ();
//							LinkVData (x, y, z, vd.GetBlock (newBlockPos, chunk.pos));
//						}
//					} else if (_VDblock != null) {
//						_VDchunk.blocks.Remove (_VDblock);
//						VDataSetDirty ();
//					}
//				}
			}
		}

		public void PlacePiece (WorldPos bPos, WorldPos gPos, LevelPiece _piece)
		{
			GameObject obj = null;
			if (GetBlock (bPos.x, bPos.y, bPos.z) == null) {
				SetBlock (bPos.x, bPos.y, bPos.z, new BlockAir ());
			}

			Block block = GetBlock (bPos.x, bPos.y, bPos.z);
			if (block is BlockAir) {
				BlockAir blockAir = GetBlock (bPos.x, bPos.y, bPos.z) as BlockAir;
				Vector3 pos = GetPieceOffset (gPos.x, gPos.z);

				float x = bPos.x * vg.w + pos.x;
				float y = bPos.y * vg.h + pos.y;
				float z = bPos.z * vg.d + pos.z;

				if (_piece != null) {
					#if UNITY_EDITOR
					obj = PrefabUtility.InstantiatePrefab (_piece.gameObject) as GameObject;
					#else
					obj = GameObject.Instantiate(_piece.gameObject);
					#endif
					obj.transform.parent = pieces.transform;
					obj.transform.localPosition = new Vector3 (x, y, z);
					obj.transform.localRotation = Quaternion.Euler (0, GetPieceAngle (gPos.x, gPos.z), 0);
				}
				blockAir.SetPiece (bPos, gPos, (obj != null) ? obj.GetComponent<LevelPiece> () : null);

				foreach (string p in blockAir.pieceNames)
					if (p != null)
						return;
				SetBlock (bPos.x, bPos.y, bPos.z, null);
			}
		}

		Vector3 GetPieceOffset (int x, int z)
		{
			Vector3 offset = Vector3.zero;
			float hw = vg.hw;
			float hh = vg.hh;
			float hd = vg.hd;

			if (x == 0 && z == 0)
				return new Vector3 (-hw, -hh, -hd);
			if (x == 1 && z == 0)
				return new Vector3 (0, -hh, -hd);
			if (x == 2 && z == 0)
				return new Vector3 (hw, -hh, -hd);

			if (x == 0 && z == 1)
				return new Vector3 (-hw, -hh, 0);
			if (x == 1 && z == 1)
				return new Vector3 (0, -hh, 0);
			if (x == 2 && z == 1)
				return new Vector3 (hw, -hh, 0);

			if (x == 0 && z == 2)
				return new Vector3 (-hw, -hh, hd);
			if (x == 1 && z == 2)
				return new Vector3 (0, -hh, hd);
			if (x == 2 && z == 2)
				return new Vector3 (hw, -hh, hd);
			return offset;
		}

		int GetPieceAngle (int x, int z)
		{
			if (x == 0 && z >= 1)
				return 90;
			if (z == 2 && x >= 1)
				return 180;
			if (x == 2 && z <= 1)
				return 270;
			return 0;
		}

		#endregion

		#region Temp Save & Load

		#if UNITY_EDITOR
		public bool compileSave;

		void CompileSave ()
		{
			if (EditorApplication.isCompiling && !compileSave) {
				if (vg.saveBackup)
					SaveTempWorld ();
				compileSave = true;
			}

			if (!EditorApplication.isCompiling && compileSave) {
				LoadTempWorld ();
				compileSave = false;
			}
		}

		void SubscribeEvent ()
		{
			EditorApplication.playmodeStateChanged += new EditorApplication.CallbackFunction (OnBeforePlay);
		}

		public void OnBeforePlay ()
		{
			if (!EditorApplication.isPlaying && EditorApplication.isPlayingOrWillChangePlaymode) {
				SaveTempWorld ();
				EditorApplication.playmodeStateChanged -= new EditorApplication.CallbackFunction (OnBeforePlay);
			}
		}

		public void SaveTempWorld ()
		{
			string date = System.DateTime.Now.ToString ("yyyyMMdd") + "-" + System.DateTime.Now.ToString ("HHmmss");
			tempPath = PathCollect.save + "/_TempBackup/" + date + "_" + workFile.Substring (workFile.LastIndexOf ("/") + 1);
			Serialization.SaveWorld (volume, PathCollect.resourcesPath + tempPath + ".bytes");
			AssetDatabase.Refresh ();
		}
		#endif
		public void LoadTempWorld ()
		{
			Save save = null;
			if (Serialization.LoadRTWorld (tempPath) != null) {
				Debug.Log ("Volume[<B>" + transform.name + "] <color=#05EE61>Load tempPath :</color></B>\n" + tempPath);
				save = Serialization.LoadRTWorld (tempPath);
			} else if (Serialization.LoadRTWorld (workFile) != null) {
				Debug.Log ("Volume<B>[" + transform.name + "] <color=#059E61>Load workFile :</color></B>\n" + workFile);
				save = Serialization.LoadRTWorld (workFile);
			} else {
				Debug.LogError ("Volume[" + transform.name + "] Loading Fail !!!");
				return;
			}

			volume.BuildVolume (save);
			#if UNITY_EDITOR
			SceneView.RepaintAll ();
			#endif
		}

		#endregion

		#region Ruler

		#if UNITY_EDITOR
		private GameObject ruler, layerRuler;
		public GameObject box = null;
		private MeshCollider mColl;
		private BoxCollider bColl;
		public bool useBox = false;

		void CreateRuler ()
		{
			ruler = new GameObject ("Ruler");
			ruler.layer = LayerMask.NameToLayer ("Editor");
			ruler.tag = PathCollect.rularTag;
			ruler.transform.parent = transform;
			mColl = ruler.AddComponent<MeshCollider> ();

			MeshData meshData = new MeshData ();
			float x = -vg.hw;
			float y = -vg.hh;
			float z = -vg.hd;
			float w = chunkX * vg.chunkSize * vg.w + x;
			float d = chunkZ * vg.chunkSize * vg.d + z;
			meshData.useRenderDataForCol = true;
			meshData.AddVertex (new Vector3 (x, y, z));
			meshData.AddVertex (new Vector3 (x, y, d));
			meshData.AddVertex (new Vector3 (w, y, d));
			meshData.AddVertex (new Vector3 (w, y, z));
			meshData.AddQuadTriangles ();

			mColl.sharedMesh = null;
			Mesh cmesh = new Mesh ();
			cmesh.vertices = meshData.colVertices.ToArray ();
			cmesh.triangles = meshData.colTriangles.ToArray ();
			cmesh.RecalculateNormals ();

			mColl.sharedMesh = cmesh;

			ruler.transform.localPosition = Vector3.zero;
			ruler.transform.localRotation = Quaternion.Euler (Vector3.zero);
		}

		void CreateLevelRuler ()
		{
			layerRuler = new GameObject ("LevelRuler");
			layerRuler.layer = LayerMask.NameToLayer ("EditorLevel");
			layerRuler.transform.parent = transform;
			layerRuler.transform.localPosition = Vector3.zero;
			layerRuler.transform.localRotation = Quaternion.Euler (Vector3.zero);
			bColl = layerRuler.AddComponent<BoxCollider> ();
			bColl.size = new Vector3 (chunkX * vg.chunkSize * vg.w, 0f, chunkZ * vg.chunkSize * vg.d);
			ChangePointY (pointY);
		}

		void CreateBox ()
		{
			if (!box) {
				box = BoxCursorUtils.CreateBoxCursor (this.transform, new Vector3 (vg.w, vg.h, vg.d));
			}
		}

		public void ActiveRuler (bool _active)
		{
			if (mColl) {
				mColl.enabled = _active;
				ruler.SetActive (_active);
				ruler.hideFlags = HideFlags.HideInHierarchy;
			}
			if (bColl) {
				bColl.enabled = _active;
				layerRuler.SetActive (_active);
				layerRuler.hideFlags = HideFlags.HideInHierarchy;
			}
			if (box) {
				box.hideFlags = HideFlags.HideInHierarchy;
			}
			pointer = _active;
		}

		public void ShowRuler ()
		{
			bool _active = EditorApplication.isPlaying ? false : vg.debugRuler;
			ActiveRuler (_active);
		}
		#endif
		#endregion

		#region Editor Scene UI

		#if UNITY_EDITOR
		
		public Color YColor;
		public bool pointer;
		public int pointY;
		public bool cuter;
		public int cutY;

		void OnDrawGizmos ()
		{
			Gizmos.color = (chunks.Count == 0) ? Color.red : Color.white;
			DrawGizmoBoxCursor ();
			DrawGizmoLayer ();

		}

		void DrawGizmoLayer ()
		{
			if (chunks.Count != 0)
				Gizmos.color = YColor;
			
			if (pointer) {
				if (!EditorApplication.isPlaying && mColl)
					Gizmos.DrawWireCube (
						new Vector3 (
							mColl.bounds.center.x,
							transform.position.y + chunkY * vg.chunkSize * vg.hh - vg.hh,
							mColl.bounds.center.z),
						new Vector3 (
							chunkX * vg.chunkSize * vg.w, 
							chunkY * vg.chunkSize * vg.h, 
							chunkZ * vg.chunkSize * vg.d)
					);
				
				for (int xi = 0; xi < chunkX * vg.chunkSize; xi++) {
					for (int zi = 0; zi < chunkZ * vg.chunkSize; zi++) {
						float cSize;
						cSize = (GetBlock (xi, pointY, zi) == null) ? 0.3f : 1.01f;

						Vector3 localPos = transform.TransformPoint (xi * vg.w, pointY * vg.h, zi * vg.d);
						Gizmos.DrawCube (localPos, new Vector3 (vg.w * cSize, vg.h * cSize, vg.d * cSize));
					}
				}
			}
		}

		void DrawGizmoBoxCursor ()
		{
			if (box != null) {
				if (!Selection.Contains (gameObject.GetInstanceID ()) || Event.current.alt) {
					box.SetActive (false);
				} else {
					box.SetActive (useBox);
				}
			}
		}

		public void ChangePointY (int _y)
		{
			_y = Mathf.Clamp (_y, 0, chunkY * vg.chunkSize - 1);
			pointY = _y;
			YColor = new Color (
				(20 + (pointY % 10) * 20) / 255f, 
				(200 - Mathf.Abs ((pointY % 10) - 5) * 20) / 255f, 
				(200 - (pointY % 10) * 20) / 255f, 
				0.4f
			);
			if (bColl) {
				bColl.center = new Vector3 (
					chunkX * vg.chunkSize * vg.hw - vg.hw, 
					pointY * vg.h + vg.hh, 
					chunkZ * vg.chunkSize * vg.hd - vg.hd
				);
			}
			if (chunks != null && chunks.Count > 0)
				UpdateChunks ();
		}

		public void ChangeCutY (int _y)
		{
			_y = Mathf.Clamp (_y, 0, chunkY * vg.chunkSize - 1);
			cutY = _y;
			if (chunks != null && chunks.Count > 0)
				UpdateChunks ();
		}
		#endif
		#endregion
	}
}