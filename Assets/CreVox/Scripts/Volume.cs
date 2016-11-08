using UnityEngine;
using System.Collections;
using System.Collections.Generic;
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

		private GameObject pieces, deco;

		public string workFile = PathCollect.save + "/temp";
		public string tempPath;

		public string piecePack = PathCollect.pieces;
		public Material vertexMaterial;

		void Awake()
		{
			volume = this;
		}
		void Start()
		{
			LoadTempWorld ();
		}
		void Update()
		{
			#if UNITY_EDITOR
			CompileSave ();
			#endif
		}

		#region Chunk
		private GameObject chunkPrefab;
		public Dictionary<WorldPos, Chunk> chunks = new Dictionary<WorldPos, Chunk>();
		public int chunkX = 1;
		public int chunkY = 1;
		public int chunkZ = 1;	

		public void Init(int _chunkX, int _chunkY, int _chunkZ)
		{
			chunkPrefab = Resources.Load(PathCollect.chunk) as GameObject;

			chunkX = _chunkX;
			chunkY = _chunkY;
			chunkZ = _chunkZ;

			pieces = new GameObject("Pieces");
			pieces.transform.parent = transform;
			pieces.transform.localPosition = Vector3.zero;
			pieces.transform.localRotation = Quaternion.Euler(Vector3.zero);

			deco = new GameObject("Decoration");
			deco.transform.parent = transform;
			deco.transform.localPosition = Vector3.zero;
			deco.transform.localRotation = Quaternion.Euler(Vector3.zero);

			CreateChunks();

			#if UNITY_EDITOR
			if (!EditorApplication.isPlaying) {
				CreateRuler ();
				CreateLevelRuler ();
				CreateBox ();
				ShowRuler();
			}
			#endif
		}

		public void Reset()
		{
			if (chunks != null) {
				DestoryChunks();
				chunks.Clear();
			}
			if (pieces)
				Object.DestroyImmediate(pieces);
			if (deco)
				Object.DestroyImmediate(deco);
			#if UNITY_EDITOR
			if (ruler)
				Object.DestroyImmediate(ruler);
			if (layerRuler)
				Object.DestroyImmediate(layerRuler);

			mColl = null;
			bColl = null;
			#endif

			for (int i = transform.childCount; i > 0; i--) {
				Object.DestroyImmediate (transform.GetChild (i - 1).gameObject);
			}
		}

		public void UpdateChunks()
		{
			for (int x = 0; x < chunkX; x++) {
				for (int y = 0; y < chunkY; y++) {
					for (int z = 0; z < chunkZ; z++) {
						GetChunk(x * Chunk.chunkSize, y * Chunk.chunkSize, z * Chunk.chunkSize).UpdateChunk();
					}
				}
			}
		}
		void CreateChunks()
		{
			for (int x = 0; x < chunkX; x++) {
				for (int y = 0; y < chunkY; y++) {
					for (int z = 0; z < chunkZ; z++) {
						CreateChunk(x * Chunk.chunkSize, y * Chunk.chunkSize, z * Chunk.chunkSize);
						GetChunk(x * Chunk.chunkSize, y * Chunk.chunkSize, z * Chunk.chunkSize).Init();
					}
				}
			}
		}
		void DestoryChunks()
		{
			for (int x = 0; x < chunkX; x++) {
				for (int y = 0; y < chunkY; y++) {
					for (int z = 0; z < chunkZ; z++) {
						DestroyChunk(x * Chunk.chunkSize, y * Chunk.chunkSize, z * Chunk.chunkSize);
					}
				}
			}
		}
		void CreateChunk(int x, int y, int z)
		{
			WorldPos worldPos = new WorldPos(x, y, z);

			//Instantiate the chunk at the coordinates using the chunk prefab
			GameObject newChunkObject = Instantiate (
				chunkPrefab, new Vector3 (x * Block.w, y * Block.h, z * Block.d),
				Quaternion.Euler (Vector3.zero)
			) as GameObject;
			newChunkObject.name = "Chunk(" + x / Chunk.chunkSize + "," + y / Chunk.chunkSize + "," + z / Chunk.chunkSize + ")";
			newChunkObject.transform.parent = transform;
			newChunkObject.transform.localPosition = new Vector3 (x * Block.w, y * Block.h, z * Block.d);
			newChunkObject.transform.localRotation = Quaternion.Euler(Vector3.zero);
			if (vertexMaterial != null)
				newChunkObject.GetComponent<Renderer> ().material = vertexMaterial;
			#if UNITY_EDITOR
			if (EditorApplication.isPlaying)
				newChunkObject.layer = LayerMask.NameToLayer("Floor");
			else
				newChunkObject.layer = LayerMask.NameToLayer("Editor");
			#else
			newChunkObject.layer = LayerMask.NameToLayer("Floor");
			#endif
			Chunk newChunk = newChunkObject.GetComponent<Chunk>();

			newChunk.pos = worldPos;
			newChunk.volume = this;

			//Add it to the chunks dictionary with the position as the key
			chunks.Add(worldPos, newChunk);

			//Add the following:
			for (int xi = 0; xi < Chunk.chunkSize; xi++) {
				for (int yi = 0; yi < Chunk.chunkSize; yi++) {
					for (int zi = 0; zi < Chunk.chunkSize; zi++) {
						SetBlock(x + xi, y + yi, z + zi, new BlockAir());
					}
				}
			}
		}
		void DestroyChunk(int x, int y, int z)
		{
			Chunk chunk = null;
			if (chunks.TryGetValue (new WorldPos (x, y, z), out chunk)) {
				#if UNITY_EDITOR
				if (chunk.gameObject) {
					Object.DestroyImmediate (chunk.gameObject);
					chunk.Destroy ();
					chunks.Remove (new WorldPos (x, y, z));
				}
				#else
				if(chunk.gameObject){
					Object.Destroy(chunk.gameObject);
					chunk.Destroy();
					chunks.Remove(new WorldPos(x, y, z));
				}
				#endif
			}
		}
		public Chunk GetChunk(int x, int y, int z)
		{
			WorldPos pos = new WorldPos();
			float multiple = Chunk.chunkSize;
			pos.x = Mathf.FloorToInt(x / multiple) * Chunk.chunkSize;
			pos.y = Mathf.FloorToInt(y / multiple) * Chunk.chunkSize;
			pos.z = Mathf.FloorToInt(z / multiple) * Chunk.chunkSize;
			Chunk containerChunk = null;
			chunks.TryGetValue(pos, out containerChunk);

			return containerChunk;
		}
		#endregion

		#region Temp Save & Load
		#if UNITY_EDITOR
		public bool compileSave;

		void CompileSave()
		{
			if (EditorApplication.isCompiling && !compileSave) {
				if (VolumeManager.saveBackup)
					SaveTempWorld ();
				compileSave = true;
			}

			if (!EditorApplication.isCompiling && compileSave) {
				LoadTempWorld ();
				ActiveRuler (false);
				compileSave = false;
			}
		}

		void SubscribeEvent()
		{
			EditorApplication.playmodeStateChanged += new EditorApplication.CallbackFunction (OnBeforePlay);
		}

		public void OnBeforePlay()
		{
			if (!EditorApplication.isPlaying && EditorApplication.isPlayingOrWillChangePlaymode) {
				SaveTempWorld ();
				EditorApplication.playmodeStateChanged -= new EditorApplication.CallbackFunction(OnBeforePlay);
			}
		}
		public void SaveTempWorld()
		{
			string date = System.DateTime.Now.ToString ("yyyyMMdd") + "-" + System.DateTime.Now.ToString ("HHmmss");
			tempPath = PathCollect.save + "/_TempBackup/" + date + "_" + workFile.Substring (workFile.LastIndexOf ("/") + 1);
			Serialization.SaveWorld (volume, PathCollect.resourcesPath + tempPath + ".bytes");
			AssetDatabase.Refresh();
		}
		#endif
		public void LoadTempWorld()
		{
			Save save = null;
			if (Serialization.LoadRTWorld (tempPath) != null) {
				Debug.Log ("Volume[<B>" + transform.name + "] <color=#05EE61>Load tempPath :</color></B>\n" + tempPath);
				save = Serialization.LoadRTWorld (tempPath);
			} else if (Serialization.LoadRTWorld (workFile) != null) {
				tempPath = null;
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

		void CreateRuler()
		{
			ruler = new GameObject("Ruler");
			ruler.layer = LayerMask.NameToLayer("Editor");
			ruler.tag = PathCollect.rularTag;
			ruler.transform.parent = transform;
//			ruler.hideFlags = HideFlags.HideInHierarchy;
			mColl = ruler.AddComponent<MeshCollider>();

			MeshData meshData = new MeshData();
			float x = -Block.hw;
			float y = -Block.hh;
			float z = -Block.hd;
			float w = chunkX * Chunk.chunkSize * Block.w + x;
			float d = chunkZ * Chunk.chunkSize * Block.d + z;
			meshData.useRenderDataForCol = true;
			meshData.AddVertex(new Vector3(x, y, z));
			meshData.AddVertex(new Vector3(x, y, d));
			meshData.AddVertex(new Vector3(w, y, d));
			meshData.AddVertex(new Vector3(w, y, z));
			meshData.AddQuadTriangles();

			mColl.sharedMesh = null;
			Mesh cmesh = new Mesh();
			cmesh.vertices = meshData.colVertices.ToArray();
			cmesh.triangles = meshData.colTriangles.ToArray();
			cmesh.RecalculateNormals();

			mColl.sharedMesh = cmesh;

			ruler.transform.localPosition = Vector3.zero;//FreePos
			ruler.transform.localRotation = Quaternion.Euler(Vector3.zero);
		}
		void CreateLevelRuler()
		{
			layerRuler = new GameObject("LevelRuler");
			layerRuler.layer = LayerMask.NameToLayer("EditorLevel");
			layerRuler.transform.parent = transform;
			layerRuler.transform.localPosition = Vector3.zero;//FreePos
			layerRuler.transform.localRotation = Quaternion.Euler(Vector3.zero);
//			layerRuler.hideFlags = HideFlags.HideInHierarchy;
			bColl = layerRuler.AddComponent<BoxCollider>();
			bColl.size = new Vector3(chunkX * Chunk.chunkSize * Block.w, 0f, chunkZ * Chunk.chunkSize * Block.d);
			ChangePointY(pointY);
		}
		void CreateBox()
		{
			if (!box) {
				box = BoxCursorUtils.CreateBoxCursor (this.transform, new Vector3 (Block.w, Block.h, Block.d));
//				box.hideFlags = HideFlags.HideInHierarchy;
			}
		}
		public void ActiveRuler(bool _active)
		{
			if (mColl) {
				mColl.enabled = _active;
				ruler.SetActive (_active);
			}
			if (bColl) {
				bColl.enabled = _active;
				layerRuler.SetActive (_active);
			}
			pointer = _active;
		}
		public void ShowRuler()
		{
			bool _active = EditorApplication.isPlaying ? false : VolumeManager.debugRuler;
			ruler.hideFlags = _active ? HideFlags.NotEditable : HideFlags.HideInHierarchy;
			layerRuler.hideFlags = _active ? HideFlags.NotEditable : HideFlags.HideInHierarchy;
			box.hideFlags = _active ? HideFlags.NotEditable : HideFlags.HideInHierarchy;
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
		public float editDis = 120f;

		void OnDrawGizmos()
		{
			Gizmos.color = (chunks.Count == 0) ? Color.red : Color.white;

//			float x = -Block.hw;
//			float z = -Block.hd;
//			float w = chunkX * Chunk.chunkSize * Block.w + x;
//			float d = chunkZ * Chunk.chunkSize * Block.d + z;
//			Vector3 v1 = new Vector3(x, -Block.hh, z);
//			Vector3 v2 = new Vector3(x, -Block.hh, d);
//			Vector3 v3 = new Vector3(w, -Block.hh, d);
//			Vector3 v4 = new Vector3(w, -Block.hh, z);
//			Gizmos.DrawLine(v1, v2);
//			Gizmos.DrawLine(v2, v3);
//			Gizmos.DrawLine(v3, v4);
//			Gizmos.DrawLine(v4, v1);

			DrawGizmoBoxCursor();
			DrawGizmoLayer();

		}
		void DrawGizmoLayer()
		{
			if (chunks.Count != 0)
				Gizmos.color = YColor;
			
			if (pointer) {
				if (!EditorApplication.isPlaying && mColl)
					Gizmos.DrawWireCube (
						new Vector3 (
							mColl.bounds.center.x,
							transform.position.y + chunkY * Chunk.chunkSize * Block.hh - Block.hh,
							mColl.bounds.center.z),
						new Vector3 (
							chunkX * Chunk.chunkSize * Block.w, 
							chunkY * Chunk.chunkSize * Block.h, 
							chunkZ * Chunk.chunkSize * Block.d)
					);
				
				for (int xi = 0; xi < chunkX * Chunk.chunkSize; xi++) {
					for (int zi = 0; zi < chunkZ * Chunk.chunkSize; zi++) {
						float cSize;
						cSize = GetBlock(xi, pointY, zi).GetType() == typeof(BlockAir) ? 0.3f : 1.01f;

						Vector3 localPos = transform.TransformPoint (xi * Block.w, pointY * Block.h, zi * Block.d);
						Gizmos.DrawCube(localPos,new Vector3(Block.w * cSize, Block.h * cSize, Block.d * cSize));
					}
				}
			}
		}
		void DrawGizmoBoxCursor()
		{
			if (box != null) {
				if (!Selection.Contains(gameObject.GetInstanceID()) || Event.current.alt) {
					box.SetActive(false);
				} else {
					box.SetActive(useBox);
				}
			}
		}

		public void ChangePointY(int _y)
		{
			_y = Mathf.Clamp(_y, 0, chunkY * Chunk.chunkSize - 1);
			pointY = _y;
			YColor = new Color(
				(20 + (pointY % 10) * 20) / 255f, 
				(200 - Mathf.Abs((pointY % 10) - 5) * 20) / 255f, 
				(200 - (pointY % 10) * 20) / 255f, 
				0.4f
			);
			if (bColl) {
				bColl.center = new Vector3 (
					chunkX * Chunk.chunkSize * Block.hw - Block.hw, 
					pointY * Block.h + Block.hh, 
					chunkZ * Chunk.chunkSize * Block.hd - Block.hd
				);
			}
			if (chunks != null && chunks.Count > 0)
				UpdateChunks ();
		}

		public void ChangeCutY(int _y)
		{
			_y = Mathf.Clamp(_y, 0, chunkY * Chunk.chunkSize - 1);
			cutY = _y;
			if (chunks != null && chunks.Count > 0)
				UpdateChunks ();
		}
		#endif
		#endregion

		public void BuildVolume(Save _save)
		{
			PaletteItem[] itemArray = Resources.LoadAll<PaletteItem>(piecePack);

			Reset();
			Init(_save.chunkX, _save.chunkY, _save.chunkZ);
			foreach (var blockPair in _save.blocks) {
				Block block = blockPair.Value;
				BlockAir bAir = block as BlockAir;
				if (bAir != null) {
					SetBlock (blockPair.Key.x, blockPair.Key.y, blockPair.Key.z, new BlockAir ());
					for (int i = 0; i < bAir.pieceNames.Length; i++) {
						for (int k = 0; k < itemArray.Length; k++) {
							if (bAir.pieceNames [i] == itemArray [k].name) {
								PlacePiece (blockPair.Key, new WorldPos (i % 3, 0, (int)(i / 3)), itemArray [k].gameObject.GetComponent<LevelPiece> ());
								break;
							}
						}
					}
				} else
					SetBlock (blockPair.Key.x, blockPair.Key.y, blockPair.Key.z, new Block ());
			}
			UpdateChunks();

		}

		public Block GetBlock(int x, int y, int z)
		{
			Chunk containerChunk = GetChunk(x, y, z);
			if (containerChunk != null) {
				Block block = containerChunk.GetBlock(
					x - containerChunk.pos.x,
					y - containerChunk.pos.y,
					z - containerChunk.pos.z);

				return block;
			} else {
				return new BlockAir();
			}

		}
		
		public void SetBlock(int x, int y, int z, Block block)
		{
			Chunk chunk = GetChunk(x, y, z);

			if (chunk != null) {
				chunk.SetBlock(x - chunk.pos.x, y - chunk.pos.y, z - chunk.pos.z, block);
				chunk.update = true;
			}
		}

		public void PlacePiece(WorldPos bPos, WorldPos gPos, LevelPiece _piece)
		{
			GameObject obj = null;
			BlockAir block = GetBlock(bPos.x, bPos.y, bPos.z) as BlockAir;
			if (block == null)
				return;

			Vector3 pos = GetPieceOffset(gPos.x, gPos.z);

			float x = bPos.x * Block.w + pos.x;
			float y = bPos.y * Block.h + pos.y;
			float z = bPos.z * Block.d + pos.z;

			if (_piece != null) {
				#if UNITY_EDITOR
				obj = PrefabUtility.InstantiatePrefab(_piece.gameObject) as GameObject;
				#else
				obj = GameObject.Instantiate(_piece.gameObject);
				#endif
				obj.transform.parent = pieces.transform;
				obj.transform.localPosition = new Vector3(x, y, z);
				obj.transform.localRotation = Quaternion.Euler(0, GetPieceAngle(gPos.x, gPos.z), 0);
			}
			block.SetPiece (bPos, gPos, (obj != null) ? obj.GetComponent<LevelPiece> () : null);
		}

		Vector3 GetPieceOffset(int x, int z)
		{
			Vector3 offset = Vector3.zero;
			float hw = Block.hw;
			float hh = Block.hh;
			float hd = Block.hd;

			if (x == 0 && z == 0)
				return new Vector3(-hw, -hh, -hd);
			if (x == 1 && z == 0)
				return new Vector3(0, -hh, -hd);
			if (x == 2 && z == 0)
				return new Vector3(hw, -hh, -hd);

			if (x == 0 && z == 1)
				return new Vector3(-hw, -hh, 0);
			if (x == 1 && z == 1)
				return new Vector3(0, -hh, 0);
			if (x == 2 && z == 1)
				return new Vector3(hw, -hh, 0);

			if (x == 0 && z == 2)
				return new Vector3(-hw, -hh, hd);
			if (x == 1 && z == 2)
				return new Vector3(0, -hh, hd);
			if (x == 2 && z == 2)
				return new Vector3(hw, -hh, hd);
			return offset;
		}

		int GetPieceAngle(int x, int z)
		{
			if (x == 0 && z >= 1)
				return 90;
			if (z == 2 && x >= 1)
				return 180;
			if (x == 2 && z <= 1)
				return 270;
			return 0;
		}
	}
}