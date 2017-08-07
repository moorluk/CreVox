using UnityEngine;
using System.Collections.Generic;
using BehaviorDesigner.Runtime;
namespace CreVox
{
    public class VolumeMaker : MonoBehaviour
    {
        public enum Style
        {
            ChunkOnly = 1,
            ChunkWithPiece = 3,
            ChunkWithPieceAndItem = 7,
        }
        public VolumeData m_vd;
        public Style m_style = Style.ChunkWithPiece;
		public string ArtPack = PathCollect.pieces;
		public string vMaterial = PathCollect.defaultVoxelMaterial;
        private List<Chunk> m_cs = new List<Chunk>();
		private List<BehaviorTree> m_bts = new List<BehaviorTree>(1024);
		GameObject nodeRoot;
		GameObject itemRoot;

		#region delgate
		private delegate void volumeAdd(GameObject volume);
		private void AddComponent()
		{
			volumeAdd AfterVolumeInit = new volumeAdd(VolumeAdapter.AfterVolumeInit);
			if (AfterVolumeInit != null)
				AfterVolumeInit(this.gameObject);
		}
		#endregion

        public void Build()
        {
			int style = (int)m_style;

			nodeRoot = new GameObject ("DecorationRoot");
			nodeRoot.transform.parent = transform;
			nodeRoot.transform.localPosition = Vector3.zero;
			nodeRoot.transform.localRotation = Quaternion.Euler (Vector3.zero);

			itemRoot = new GameObject ("ItemRoot");
			itemRoot.transform.parent = transform;
			itemRoot.transform.localPosition = Vector3.zero;
			itemRoot.transform.localRotation = Quaternion.Euler (Vector3.zero);

            List<ChunkData> cd = new List<ChunkData> ();
            if (m_vd.useFreeChunk) {
                cd.Add (m_vd.freeChunk);
            } else {
                cd = m_vd.chunkDatas;
            }
            for (int ci = 0; ci < cd.Count; ci++)
			{
                ChunkData cData = cd[ci];
                GameObject chunk = Instantiate(Resources.Load(PathCollect.chunk) as GameObject, Vector3.zero, Quaternion.Euler(Vector3.zero)) as GameObject;
                chunk.name = "Chunk" + cData.ChunkPos.ToString();
				chunk.transform.parent = nodeRoot.transform;
                VGlobal vg = VGlobal.GetSetting();
                chunk.transform.localPosition = new Vector3(cData.ChunkPos.x * vg.w, cData.ChunkPos.y * vg.h, cData.ChunkPos.z * vg.d);
                chunk.transform.localRotation = Quaternion.Euler(Vector3.zero);
                Material vMat = Resources.Load(vMaterial, typeof(Material)) as Material;
                if (vMat == null)
                    vMat = Resources.Load(PathCollect.defaultVoxelMaterial, typeof(Material)) as Material;
                chunk.GetComponent<Renderer>().sharedMaterial = vMat;
                chunk.layer = LayerMask.NameToLayer("Floor");

                Chunk c = chunk.GetComponent<Chunk>();
                c.cData = cData;
                c.Init();

                if ((style & 2) > 0)
                {
                    m_cs.Add(c);
                }
            }

            if (style > 1)
            {
                PaletteItem[] itemArray = VGlobal.GetSetting ().GetItemArray (ArtPack + m_vd.subArtPack);

                if ((style & 2) > 0)
                {
                    for (int p = 0; p < m_cs.Count; ++p)
                    {
                        PlacePieces(m_cs[p], itemArray);
                    }
                }

                if ((style & 4) > 0)
                {
                    CreateItems(m_vd, itemArray);
                }
			}

			AddComponent ();
        }

        public bool LoadCompeleted()
        {
            bool result = true;
            for (int idx = 0; idx < m_bts.Count; ++idx)
            {
                if (m_bts[idx].ExecutionStatus == BehaviorDesigner.Runtime.Tasks.TaskStatus.Running)
                {
                    result = false;
                }
            }
            return result;
		}

		void PlacePieces (Chunk _chunk, PaletteItem[] itemArray)
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

		void PlacePiece (WorldPos bPos, WorldPos gPos, LevelPiece _piece, Transform _parent)
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
				BehaviorTree bt = pObj.GetComponent<BehaviorTree> ();
				if (bt != null) {
					m_bts.Add (bt);
				}
			}
		}

		void CreateItems (VolumeData _vData, PaletteItem[] itemArray)
		{
			for (int i = 0; i < _vData.blockItems.Count; i++) {
				for (int k = 0; k < itemArray.Length; k++) {
					BlockItem blockItem = _vData.blockItems [i];
					if (blockItem.pieceName == itemArray [k].name) {
						CreateItem (blockItem, i, itemArray [k]);
					}
				}
			}
		}

		void CreateItem (BlockItem blockItem, int _id, PaletteItem _piece)
		{
			GameObject pObj = GameObject.Instantiate (_piece.gameObject);
			LevelPiece p = (LevelPiece)pObj.GetComponent<LevelPiece> ();
			pObj.transform.parent = (p is PrefabPiece) ? nodeRoot.transform : itemRoot.transform;
			pObj.transform.localPosition = new Vector3 (blockItem.posX, blockItem.posY, blockItem.posZ);
			pObj.transform.localRotation = new Quaternion (blockItem.rotX, blockItem.rotY, blockItem.rotZ, blockItem.rotW);

			if (p != null) {
				p.SetupPiece (blockItem);
			}
		}
	}
}
