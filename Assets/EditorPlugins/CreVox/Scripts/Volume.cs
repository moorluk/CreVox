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
//		public static VGlobal vg;

		public string workFile;
		public string tempPath;

		public Material vertexMaterial;

		void Awake ()
		{
			volume = this;
//			vg = VGlobal.GetSetting ();
			if (EditorApplication.isPlaying)
				this.gameObject.SetActive (!VGlobal.GetSetting ().FakeDeco);
		}

		void Start ()
		{
//			if (!vg)
//				vg = VGlobal.GetSetting ();
			if (nodes == null)
				nodes = new Dictionary<WorldPos, Node> ();
			if (chunks == null)
				chunks = new List<Chunk> ();
			LoadTempWorld ();
		}

		void Update ()
		{
//			if (!vg)
//				vg = VGlobal.GetSetting ();
			#if UNITY_EDITOR
			if (VGlobal.GetSetting ().saveBackup)
				CompileSave ();
			#endif
		}

		#region VolumeData

		public VolumeData vd;
		public bool _useBytes;

		public void WriteVData ()
		{
			if (vd == null) {
				if (workFile != "")
					vd = VolumeData.GetVData (workFile + "_vData");
				else {
					string sPath = Application.dataPath + PathCollect.resourcesPath.Substring (6) + PathCollect.save;
					sPath = EditorUtility.SaveFilePanel ("save vData", sPath, volume.name + "_vData", "asset");
					sPath = sPath.Substring (sPath.IndexOf (PathCollect.resourceSubPath));
					vd = VolumeData.GetVData (sPath);
				}

				vd.chunkX = chunkX;
				vd.chunkY = chunkY;
				vd.chunkZ = chunkZ;
				vd.chunkDatas = new List<ChunkData> ();
				foreach (Chunk _chunk in chunks) {
					WorldPos _pos = _chunk.cData.ChunkPos;

					ChunkData newChunkData = new ChunkData ();
					newChunkData.ChunkPos = _pos;
					newChunkData.blocks = _chunk.cData.blocks;
					newChunkData.blockAirs = _chunk.cData.blockAirs;
					newChunkData.blockHolds = _chunk.cData.blockHolds;

					vd.chunkDatas.Add (newChunkData);
					_chunk.cData = newChunkData;
				}
			}
		}

		#endregion

		#region Chunk

		private GameObject chunkPrefab;
		public List<Chunk> chunks = new List<Chunk>();
		public int chunkX = 1;
		public int chunkY = 1;
		public int chunkZ = 1;

		public void BuildVolume (Save _save, VolumeData a_data = null)
		{
			if (a_data == null && _useBytes == false) {
				return;
			}
			Reset ();

			if (_useBytes) { //load .bytes
				vd = null;
				Init (_save.chunkX, _save.chunkY, _save.chunkZ);
				foreach (var blockPair in _save.blocks) {
					Block block = blockPair.Value;
					if (block != null) {
						if (block is BlockAir) {
							BlockAir bAir = blockPair.Value as BlockAir;
							bool notEmpty = false;
							for (int i = 0; i < bAir.pieceNames.Length; i++) {
								if (bAir.pieceNames [i] != "") {
									notEmpty = true;
									break;
								}
							}
							if (notEmpty)
								SetBlock (blockPair.Key.x, blockPair.Key.y, blockPair.Key.z, bAir);
							else
								Debug.Log (bAir.BlockPos);
						} else {
							SetBlock (blockPair.Key.x, blockPair.Key.y, blockPair.Key.z, new Block());
						}
					}
				}
			} else { //load ScriptableObject
				Init (a_data.chunkX, a_data.chunkY, a_data.chunkZ);
				foreach (Chunk c in chunks) {
					c.cData = a_data.GetChunk (c.cData.ChunkPos);
				}
			}
			PlacePieces ();
			UpdateChunks ();
		}

		public void Init (int _chunkX, int _chunkY, int _chunkZ)
		{
			chunkPrefab = Resources.Load (PathCollect.chunk) as GameObject;

			chunkX = _chunkX;
			chunkY = _chunkY;
			chunkZ = _chunkZ;

			nodeRoot = new GameObject ("Pieces");
			nodeRoot.transform.parent = transform;
			nodeRoot.transform.localPosition = Vector3.zero;
			nodeRoot.transform.localRotation = Quaternion.Euler (Vector3.zero);

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
			nodes.Clear();

			#if UNITY_EDITOR
			mColl = null;
			bColl = null;
			if (ruler)
				GameObject.DestroyImmediate (ruler);
			if (layerRuler)
				GameObject.DestroyImmediate (layerRuler);
			#endif

			if (nodeRoot)
				GameObject.DestroyImmediate (nodeRoot);
			for (int i = transform.childCount; i > 0; i--) {
				GameObject.DestroyImmediate (transform.GetChild (i - 1).gameObject);
			}
		}

		public void UpdateChunks ()
		{
			foreach (Chunk chunk in chunks)
				chunk.UpdateChunk ();
		}

		void CreateChunks ()
		{
			int chunksize = VGlobal.GetSetting ().chunkSize;
			for (int x = 0; x < chunkX; x++) {
				for (int y = 0; y < chunkY; y++) {
					for (int z = 0; z < chunkZ; z++) {
						Chunk newChunk = CreateChunk (x * chunksize, y * chunksize, z * chunksize);
						newChunk.Init ();
					}
				}
			}
		}

		Chunk CreateChunk (int x, int y, int z)
		{
			VGlobal vg = VGlobal.GetSetting ();
			WorldPos chunkPos = new WorldPos (x, y, z);

			//Instantiate the chunk at the coordinates using the chunk prefab
			GameObject newChunkObject = Instantiate (
				                            chunkPrefab, new Vector3 (x * vg.w, y * vg.h, z * vg.d),
				                            Quaternion.Euler (Vector3.zero)
			                            ) as GameObject;
			newChunkObject.name = "Chunk(" + x + "," + y + "," + z + ")";
			newChunkObject.transform.parent = transform;
			newChunkObject.transform.localPosition = new Vector3 (x * vg.w, y * vg.h, z * vg.d);
			newChunkObject.transform.localRotation = Quaternion.Euler (Vector3.zero);
			#if UNITY_EDITOR
			if (vertexMaterial != null && EditorApplication.isPlaying)
				newChunkObject.GetComponent<Renderer> ().material = vertexMaterial;
			newChunkObject.layer = LayerMask.NameToLayer ((EditorApplication.isPlaying) ? "Floor" : "Editor");
			#else
			if (vertexMaterial != null)
				newChunkObject.GetComponent<Renderer> ().material = vertexMaterial;
			newChunkObject.layer = LayerMask.NameToLayer("Floor");
			#endif
			Chunk newChunk = newChunkObject.GetComponent<Chunk> ();

			newChunk.cData.ChunkPos = chunkPos;
			newChunk.volume = this;

			chunks.Add (newChunk);

			for (int xi = 0; xi < vg.chunkSize; xi++) {
				for (int yi = 0; yi < vg.chunkSize; yi++) {
					for (int zi = 0; zi < vg.chunkSize; zi++) {
						SetBlock (x + xi, y + yi, z + zi, null);
					}
				}
			}
			return newChunk;
		}

		void DestoryChunks ()
		{
			int chunkSize = VGlobal.GetSetting ().chunkSize;
			for (int x = 0; x < chunkX; x++) {
				for (int y = 0; y < chunkY; y++) {
					for (int z = 0; z < chunkZ; z++) {
						DestroyChunk (x * chunkSize, y * chunkSize, z * chunkSize);
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
					UnityEngine.Object.Destroy(chunk.gameObject);
					#endif
					chunk.Destroy ();
					chunks.Remove (chunk);
				}
			}
		}

		public Chunk GetChunk (int x, int y, int z)
		{
			WorldPos pos = new WorldPos ();
			float multiple = VGlobal.GetSetting().chunkSize;
			pos.x = Mathf.FloorToInt (x / multiple) * (int)multiple;
			pos.y = Mathf.FloorToInt (y / multiple) * (int)multiple;
			pos.z = Mathf.FloorToInt (z / multiple) * (int)multiple;
			Chunk containerChunk = null;
			foreach (Chunk _chunk in chunks) {
				if (_chunk.cData.ChunkPos.Compare (pos))
					containerChunk = _chunk;
			}
			return containerChunk;
		}

		#endregion

		#region Block

		class Node{
			public GameObject pieceRoot;
			public GameObject[] pieces;
		}

		public GameObject nodeRoot;
		Dictionary<WorldPos,Node> nodes = new Dictionary<WorldPos, Node>();

		public GameObject GetNode(WorldPos _volumePos)
		{
			if (nodes.ContainsKey (_volumePos))
				return nodes [_volumePos].pieceRoot;
			else{
				Debug.LogWarning ("(" + _volumePos + ") has no Node; try another artpack !!!");
				return null;
			}
		}

		public Block GetBlock (int x, int y, int z)
		{
			Chunk containerChunk = GetChunk (x, y, z);
			if (containerChunk != null) {
				Block block = containerChunk.GetBlock (
					              x - containerChunk.cData.ChunkPos.x,
					              y - containerChunk.cData.ChunkPos.y,
					              z - containerChunk.cData.ChunkPos.z);
				return block;
			} else {
				return null;
			}

		}

		public void SetBlock (int x, int y, int z, Block _block)
		{
			Chunk chunk = GetChunk (x, y, z);
			if (chunk != null) {
				WorldPos newBlockPos = new WorldPos (x - chunk.cData.ChunkPos.x, y - chunk.cData.ChunkPos.y, z - chunk.cData.ChunkPos.z);
				if (_block != null) {
					_block.BlockPos = newBlockPos;
					if (_block is BlockAir) {
						BlockAir _bAir = _block as BlockAir;
						chunk.SetBlock (newBlockPos.x, newBlockPos.y, newBlockPos.z, _bAir);
					} else if (_block is BlockHold) {
						BlockHold _bHold = _block as BlockHold;
						chunk.SetBlock (newBlockPos.x, newBlockPos.y, newBlockPos.z, _bHold);
					} else {
						chunk.SetBlock (newBlockPos.x, newBlockPos.y, newBlockPos.z, _block);
						if (nodes.ContainsKey (newBlockPos)) {
							GameObject.DestroyImmediate (nodes [newBlockPos].pieceRoot);
							nodes.Remove (newBlockPos);
						}
					}
				} else {
					chunk.SetBlock (newBlockPos.x, newBlockPos.y, newBlockPos.z, null);
				}
			}
		}

		public void PlacePiece (WorldPos bPos, WorldPos gPos, LevelPiece _piece)
		{
			GameObject pObj;
			BlockAir blockAir = null;
			if (_piece != null) {
				if (!nodes.ContainsKey (bPos)) {
					Node newNode = new Node ();

					GameObject _pieceRoot = new GameObject ();
					_pieceRoot.name = bPos.ToString ();
					_pieceRoot.transform.parent = nodeRoot.transform;
					_pieceRoot.transform.localPosition = Vector3.zero;
					_pieceRoot.transform.localRotation = Quaternion.Euler (Vector3.zero);
					newNode.pieceRoot = _pieceRoot;

					newNode.pieces = new GameObject[9];

					nodes.Add (bPos, newNode);
				}

				pObj = nodes [bPos].pieces [gPos.z * 3 + gPos.x];
				if (pObj != null) {
					PlaceBlockHold (bPos, gPos.z * 3 + gPos.x, pObj.GetComponent<LevelPiece> (), true);
					GameObject.DestroyImmediate (pObj);
				}
				
				Vector3 pos = GetPieceOffset (gPos.x, gPos.z);
				VGlobal vg = VGlobal.GetSetting ();
				float x = bPos.x * vg.w + pos.x;
				float y = bPos.y * vg.h + pos.y;
				float z = bPos.z * vg.d + pos.z;
				
				#if UNITY_EDITOR
				pObj = PrefabUtility.InstantiatePrefab (_piece.gameObject) as GameObject;
				#else
				pObj = GameObject.Instantiate(_piece.gameObject);
				#endif
				pObj.transform.parent = nodes [bPos].pieceRoot.transform;
				pObj.transform.localPosition = new Vector3 (x, y, z);
				pObj.transform.localRotation = Quaternion.Euler (0, GetPieceAngle (gPos.x, gPos.z), 0);
				nodes [bPos].pieces [gPos.z * 3 + gPos.x] = pObj;
				PlaceBlockHold (bPos, gPos.z * 3 + gPos.x, pObj.GetComponent<LevelPiece> (), false);

				if (GetBlock (bPos.x, bPos.y, bPos.z) == null)
					SetBlock (bPos.x, bPos.y, bPos.z, new BlockAir ());
				blockAir = GetBlock (bPos.x, bPos.y, bPos.z) as BlockAir;
				if (blockAir != null) {
					blockAir.SetPiece (bPos, gPos, pObj.GetComponent<LevelPiece> ());
					blockAir.SolidCheck (nodes [bPos].pieces);
				}
			} else {
				if (nodes.ContainsKey (bPos)) {
					pObj = nodes [bPos].pieces [gPos.z * 3 + gPos.x];
					if (pObj != null && _piece.isHold) {
						PlaceBlockHold (bPos, gPos.z * 3 + gPos.x, pObj.GetComponent<LevelPiece> (), true);
						GameObject.DestroyImmediate (pObj);
					}
				}

				if (GetBlock (bPos.x, bPos.y, bPos.z) != null) {
					blockAir = GetBlock (bPos.x, bPos.y, bPos.z) as BlockAir;
					blockAir.SetPiece (bPos, gPos, null);
					blockAir.SolidCheck (nodes [bPos].pieces);
				}
			}

			///檢查BlockAir是否為空並移除
			if (GetBlock (bPos.x, bPos.y, bPos.z) != null) {
				bool isEmpty = true;
				foreach (string p in blockAir.pieceNames) {
					if (p != null) {
						isEmpty = false;
						break;
					}
				}
				if (isEmpty) {
					SetBlock (bPos.x, bPos.y, bPos.z, null);
					GameObject.DestroyImmediate (nodes [bPos].pieceRoot);
					nodes.Remove (bPos);
				}
			}
		}
		private void PlacePieces()
		{
			PaletteItem[] itemArray;
			#if UNITY_EDITOR
			if (EditorApplication.isPlaying == false)
				itemArray = Resources.LoadAll<PaletteItem> (PathCollect.pieces);
			else
			#endif
				itemArray = Resources.LoadAll<PaletteItem> (vd.ArtPack);

			foreach (Chunk c in chunks) {
				for(int b =0 ; b < c.cData.blockAirs.Count ; b++) {
					BlockAir ba = c.cData.blockAirs [b];
					for (int i = 0; i < ba.pieceNames.Length; i++) {
						for (int k = 0; k < itemArray.Length; k++) {
							if (ba.pieceNames [i] == itemArray [k].name) {
								PlacePiece (
									new WorldPos (
										c.cData.ChunkPos.x + ba.BlockPos.x,
										c.cData.ChunkPos.y + ba.BlockPos.y,
										c.cData.ChunkPos.z + ba.BlockPos.z),
									new WorldPos (i % 3, 0, (int)(i / 3)), 
									itemArray [k].gameObject.GetComponent<LevelPiece> ());
							}
						}
					}
				}
			}
		}

		private void PlaceBlockHold(WorldPos _bPos,int _id , LevelPiece _piece, bool _isErase)
		{
//			Debug.Log ("[" + _bPos.ToString () + "](" + _id.ToString () + ")-" + (_isErase?"Delete":"Add"));
			for (int i = 0; i < _piece.holdBlocks.Count; i++) {
				LevelPiece.Hold bh = _piece.holdBlocks [i];
				int x = _bPos.x + bh.offset.x;
				int y = _bPos.y + bh.offset.y;
				int z = _bPos.z + bh.offset.z;

				BlockHold.piecePos bhData = new BlockHold.piecePos ();
				bhData.blockPos = _bPos;
				bhData.pieceID = _id;

				Predicate<BlockHold.piecePos> samePiecePos;
				samePiecePos = delegate(BlockHold.piecePos obj) {
					return (obj.blockPos.Compare (bhData.blockPos) && obj.pieceID == bhData.pieceID);
				};

				BlockHold bhBlock = GetBlock (x, y, z) as BlockHold;
				if (_isErase) {
					if (bhBlock != null && bhBlock.roots.Exists (samePiecePos))
						bhBlock.roots.RemoveAt (bhBlock.roots.FindIndex (samePiecePos));
					if (bhBlock.roots.Count == 0)
						SetBlock (x, y, z, null);
				} else {
					if (bhBlock != null){
						if (!bhBlock.roots.Exists (samePiecePos))
							bhBlock.roots.Add (bhData);
					} else {
						SetBlock (x, y, z, new BlockHold ());
						bhBlock = GetBlock (x, y, z) as BlockHold;
						if (bhBlock != null)
							bhBlock.roots.Add (bhData);
					}
					if (bhBlock != null && bh.isSolid)
						bhBlock.SetSolid(true);
				}
			}
		}

		public static Vector3 GetPieceOffset (int x, int z)
		{
			Vector3 offset = Vector3.zero;
			VGlobal vg = VGlobal.GetSetting ();
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

		public static int GetPieceAngle (int x, int z)
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
				if (VGlobal.GetSetting().saveBackup)
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
			if (_useBytes) { 
				Save save = Serialization.LoadRTWorld (tempPath);
				if (save != null)
					Debug.Log ("Volume[<B>" + transform.name + "] <color=#05EE61>Load tempPath :</color></B>\n" + tempPath);
				else {
					save = Serialization.LoadRTWorld (workFile);
					if (save != null)
						Debug.Log ("Volume<B>[" + transform.name + "] <color=#059E61>Load workFile :</color></B>\n" + workFile);
					else {
						Debug.LogError ("Volume[" + transform.name + "] Loading .bytes Fail !!!");
					}
				}
				
				if (save != null) {
					volume.BuildVolume (save);
				} else
					Debug.LogError ("No ChunkData Source!!!");
			} else {
				volume.BuildVolume (null, vd);
			}

			#if UNITY_EDITOR
			SceneView.RepaintAll ();
			#endif
		}

		#endregion

		#region Ruler

		#if UNITY_EDITOR
		[SerializeField]
		private MeshCollider mColl;
		[SerializeField]
		private BoxCollider bColl;
		[SerializeField]
		private GameObject ruler; 
		[SerializeField]
		private GameObject layerRuler;
		public GameObject box = null;
		public bool useBox = false;

		void CreateRuler ()
		{
			ruler = new GameObject ("Ruler");
			ruler.layer = LayerMask.NameToLayer ("Editor");
			ruler.tag = PathCollect.rularTag;
			ruler.transform.parent = transform;
			mColl = ruler.AddComponent<MeshCollider> ();

			MeshData meshData = new MeshData ();
			VGlobal vg = VGlobal.GetSetting ();
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
			VGlobal vg = VGlobal.GetSetting ();
			bColl.size = new Vector3 (chunkX * vg.chunkSize * vg.w, 0f, chunkZ * vg.chunkSize * vg.d);
			ChangePointY (pointY);
		}

		void CreateBox ()
		{
			if (!box) {
				VGlobal vg = VGlobal.GetSetting ();
				box = BoxCursorUtils.CreateBoxCursor (this.transform, new Vector3 (vg.w, vg.h, vg.d));
			}
		}

		public void ActiveRuler (bool _active)
		{
			if (mColl) {
				mColl.enabled = _active;
				ruler.SetActive (_active);
//				ruler.hideFlags = HideFlags.HideInHierarchy;
			}
			if (bColl) {
				bColl.enabled = _active;
				layerRuler.SetActive (_active);
//				layerRuler.hideFlags = HideFlags.HideInHierarchy;
			}
			if (box) {
				box.hideFlags = HideFlags.HideInHierarchy;
			}
			pointer = _active;
		}

		public void ShowRuler ()
		{
			bool _active = EditorApplication.isPlaying ? false : VGlobal.GetSetting().debugRuler;
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
			Matrix4x4 oldMatrix = Gizmos.matrix;
			VGlobal vg = VGlobal.GetSetting ();
			Gizmos.color = (chunks.Count == 0) ? Color.red : YColor;
			if (!EditorApplication.isPlaying && mColl)
				Gizmos.DrawWireCube (
					new Vector3 (
						mColl.bounds.center.x,
						(transform.position.y + chunkY * vg.chunkSize * vg.hh - vg.hh),
						mColl.bounds.center.z),
					new Vector3 (
						chunkX * vg.chunkSize * vg.w, 
						chunkY * vg.chunkSize * vg.h, 
						chunkZ * vg.chunkSize * vg.d)
				);
			
			Gizmos.matrix = transform.localToWorldMatrix;
			if (!EditorApplication.isPlaying) {
				DrawGizmoBoxCursor ();
				DrawGizmoLayer ();
				DrawBlockHold ();
			}
			Gizmos.matrix = oldMatrix;
		}

		void DrawBlockHold ()
		{
			VGlobal vg = VGlobal.GetSetting ();
			foreach (Chunk chunk in chunks) {
				for (int i = 0; i < chunk.cData.blockHolds.Count; i++) {
					WorldPos bh = chunk.cData.blockHolds [i].BlockPos;
					Vector3 localPos = new Vector3 (bh.x * vg.w, bh.y * vg.h, bh.z * vg.d);
					Gizmos.color = new Color (255f/255f, 244f/255f, 228f/255f, 0.15f);
					Gizmos.DrawCube (localPos, new Vector3 (vg.w, vg.h, vg.d));
					Gizmos.color = new Color (255f/255f, 244f/255f, 228f/255f, 1.0f);
					Gizmos.DrawSphere (localPos, 0.1f);
				}
			}
		}

		void DrawGizmoLayer ()
		{
			VGlobal vg = VGlobal.GetSetting ();
			if (pointer) {
				for (int xi = 0; xi < chunkX * vg.chunkSize; xi++) {
					for (int zi = 0; zi < chunkZ * vg.chunkSize; zi++) {
						float cSize;
						cSize = (GetBlock (xi, pointY, zi) == null) ? 0.3f : 1.01f;

						Vector3 localPos = new Vector3 (xi * vg.w, pointY * vg.h, zi * vg.d);
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
			VGlobal vg = VGlobal.GetSetting ();
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
			VGlobal vg = VGlobal.GetSetting ();
			_y = Mathf.Clamp (_y, 0, chunkY * vg.chunkSize - 1);
			cutY = _y;
			if (chunks != null && chunks.Count > 0)
//				PlacePieces ();
				UpdateChunks ();
		}
		#endif
		#endregion
	}
}