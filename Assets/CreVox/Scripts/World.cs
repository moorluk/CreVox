using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;

namespace CreVox
{

	[SelectionBase]
	public class World : MonoBehaviour
	{
		public Dictionary<WorldPos, Chunk> chunks = new Dictionary<WorldPos, Chunk>();
		public int chunkX = 1;
		public int chunkY = 1;
		public int chunkZ = 1;

		private GameObject chunkPrefab;
		private GameObject ruler;
		private GameObject layerRuler;

		private MeshCollider mColl;
		private BoxCollider bColl;

		public string workFile;

		//BoxCursor------
		public GameObject box = null;
		public bool useBox = false;
		public float editDis = 120f;
		//---------------

		public int editY;
		public bool pointer;
		public Color YColor;

		void Awake()
		{
			Reset();
			Save save = Serialization.LoadRTWorld(PathCollect.testmap);
			Init(save.chunkX, save.chunkY, save.chunkZ);
			BuildWorld(save);
		}

		public void Init(int _chunkX, int _chunkY, int _chunkZ)
		{
//#if UNITY_EDITOR
//    		chunkPrefab = EditorUtils.GetAssetsWithScript<Chunk>("Assets")[0].gameObject;
//#else
			chunkPrefab = Resources.Load(PathCollect.chunk) as GameObject;
//#endif

			chunkX = _chunkX;
			chunkY = _chunkY;
			chunkZ = _chunkZ;

			CreateRuler();
			CreateLevelRuler();
			CreateChunks();
			//BoxCursor------
			if (!box) {
				box = BoxCursorUtils.CreateBoxCursor(this.transform, new Vector3(Block.w, Block.h, Block.d));
			}
			//---------------
		}

		public void Reset()
		{
			if (chunks != null) {
				DestoryChunks();
				chunks.Clear();
			}

			for (int i = transform.childCount - 1; i > 0; i--) {
				Object.DestroyImmediate(transform.GetChild(i).gameObject);
			}

			if (ruler)
				Object.DestroyImmediate(ruler);
			if (layerRuler)
				Object.DestroyImmediate(layerRuler);

			mColl = null;
			bColl = null;
			editY = 0;
		}

		void CreateRuler()
		{
			ruler = new GameObject("Ruler");
			ruler.layer = LayerMask.NameToLayer("Floor");
			ruler.tag = PathCollect.rularTag;
			ruler.transform.parent = transform;
			ruler.hideFlags = HideFlags.HideInHierarchy;
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
		}

		void CreateLevelRuler()
		{
			layerRuler = new GameObject("LevelRuler");
			layerRuler.layer = LayerMask.NameToLayer("EditorLevel");
			layerRuler.transform.parent = transform;
			layerRuler.hideFlags = HideFlags.HideInHierarchy;
			bColl = layerRuler.AddComponent<BoxCollider>();
			bColl.size = new Vector3(chunkX * Chunk.chunkSize * Block.w, 0f, chunkZ * Chunk.chunkSize * Block.d);
			ChangeEditY(0);
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

		public void CreateChunk(int x, int y, int z)
		{
			WorldPos worldPos = new WorldPos(x, y, z);

			//Instantiate the chunk at the coordinates using the chunk prefab
			GameObject newChunkObject = Instantiate(
				                            chunkPrefab, new Vector3(x * Block.w, y * Block.h, z * Block.d),
				                            Quaternion.Euler(Vector3.zero)
			                            ) as GameObject;
			newChunkObject.transform.parent = transform;
			newChunkObject.layer = LayerMask.NameToLayer("Floor");
			newChunkObject.name = "Chunk(" + x / Chunk.chunkSize + "," + y / Chunk.chunkSize + "," + z / Chunk.chunkSize + ")";

			Chunk newChunk = newChunkObject.GetComponent<Chunk>();

			newChunk.pos = worldPos;
			newChunk.world = this;

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

		public void DestroyChunk(int x, int y, int z)
		{
			Chunk chunk = null;
			if (chunks.TryGetValue(new WorldPos(x, y, z), out chunk)) {
#if UNITY_EDITOR
				Debug.Log("Destroy " + chunk.gameObject.name);
				Object.DestroyImmediate(chunk.gameObject);
#else
            Object.Destroy(chunk.gameObject);
#endif
				chunk.Destroy();
				chunks.Remove(new WorldPos(x, y, z));
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
				//return new BlockAir();
				return null;
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

		void OnDrawGizmos()
		{
			float x = -Block.hw;
			float z = -Block.hd;
			float w = chunkX * Chunk.chunkSize * Block.w + x;
			float d = chunkZ * Chunk.chunkSize * Block.d + z;
			Vector3 v1 = new Vector3(x, -Block.hh, z);
			Vector3 v2 = new Vector3(x, -Block.hh, d);
			Vector3 v3 = new Vector3(w, -Block.hh, d);
			Vector3 v4 = new Vector3(w, -Block.hh, z);
			Gizmos.DrawLine(v1, v2);
			Gizmos.DrawLine(v2, v3);
			Gizmos.DrawLine(v3, v4);
			Gizmos.DrawLine(v4, v1);

			//BoxCursor------
			DrawGizmoBoxCursor();
			//---------------
			//VoxelLayer------
			DrawGizmoLayer(editY);
			//----------------

			Gizmos.DrawWireCube(
				transform.position + new Vector3(
					chunkX * Chunk.chunkSize * Block.hw - Block.hw,
					chunkY * Chunk.chunkSize * Block.hh - Block.hh, 
					chunkZ * Chunk.chunkSize * Block.hd - Block.hd),
				new Vector3(
					chunkX * Chunk.chunkSize * Block.w, 
					chunkY * Chunk.chunkSize * Block.h, 
					chunkZ * Chunk.chunkSize * Block.d)
			);
		}


		public void DrawGizmoLayer(int _y)
		{
			Gizmos.color = YColor;
			if (pointer) {
				//
				for (int xi = 0; xi < chunkX * Chunk.chunkSize; xi++) {
					for (int zi = 0; zi < chunkZ * Chunk.chunkSize; zi++) {
						float cSize;
						if (GetBlock(xi, editY, zi) == null) {
							cSize = 0.1f;
						} else {
							cSize = GetBlock(xi, editY, zi).GetType() == typeof(BlockAir) ? 0.4f : 1.01f;
						}

						Gizmos.DrawCube(
							transform.position + new Vector3(xi * Block.w, editY * Block.h, zi * Block.d), 
							new Vector3(Block.w * cSize, Block.h * cSize, Block.d * cSize));
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

		public void ChangeEditY(int _y)
		{
			_y = Mathf.Clamp(_y, 0, chunkY * Chunk.chunkSize - 1);
			editY = _y;
			YColor = new Color(
				(20 + (editY % 10) * 20) / 255f, 
				(200 - Mathf.Abs((editY % 10) - 5) * 20) / 255f, 
				(200 - (editY % 10) * 20) / 255f, 
				0.4f
			);
			bColl.center = transform.position + new Vector3(
				chunkX * Chunk.chunkSize * Block.hw - Block.hw, 
				editY * Block.h + Block.hh, 
				chunkZ * Chunk.chunkSize * Block.hd - Block.hd
			);
		}

		public void BuildWorld(Save _save)
		{
//#if UNITY_EDITOR
//            List<PaletteItem> items = EditorUtils.GetAssetsWithScript<PaletteItem>(PaletteWindow.GetLevelPiecePath());
//#else
			PaletteItem[] itemArray = Resources.LoadAll<PaletteItem>(PathCollect.pieces);
//            PaletteItem[] itemArray = Resources.FindObjectsOfTypeAll(typeof(PaletteItem)) as PaletteItem[];
//#endif

			Reset();
			Init(_save.chunkX, _save.chunkY, _save.chunkZ);

			foreach (var blockPair in _save.blocks) {
				Block block = blockPair.Value;
				BlockAir bAir = block as BlockAir;
				if (bAir != null) {
					SetBlock(blockPair.Key.x, blockPair.Key.y, blockPair.Key.z, new BlockAir());
					for (int i = 0; i < bAir.pieceNames.Length; i++) {
/*#if UNITY_EDITOR
                        for (int k = 0; k < items.Count; k++)
                        {
                            if (bAir.pieceNames[i] == items[k].name)
                            {
                                PlacePiece(blockPair.Key, new WorldPos(i % 3, 0, (int)(i / 3)), items[k].gameObject.GetComponent<LevelPiece>());
                                break;
                            }
                        }
#else*/
						for (int k = 0; k < itemArray.Length; k++) {
							if (bAir.pieceNames[i] == itemArray[k].name) {
								PlacePiece(blockPair.Key, new WorldPos(i % 3, 0, (int)(i / 3)), itemArray[k].gameObject.GetComponent<LevelPiece>());
								break;
							}
						}
//#endif
					}
				} else
					SetBlock(blockPair.Key.x, blockPair.Key.y, blockPair.Key.z, new Block());
			}
			UpdateChunks();
            
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
				obj = PrefabUtility.InstantiatePrefab(_piece.gameObject) as GameObject;
				obj.transform.parent = transform;
				obj.transform.position = new Vector3(x, y, z);
				obj.transform.localRotation = Quaternion.Euler(0, GetPieceAngle(gPos.x, gPos.z), 0);
			}

			block.SetPart(bPos, gPos, obj);
		}

		private Vector3 GetPieceOffset(int x, int z)
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

		private int GetPieceAngle(int x, int z)
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