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
        List<Chunk> m_cs = new List<Chunk> ();
        List<BehaviorTree> m_bts = new List<BehaviorTree> (1024);
        GameObject nodeRoot;
        GameObject itemRoot;

        #region delgate

        private delegate void volumeAdd (GameObject volume);

        void AddComponent ()
        {
            volumeAdd AfterVolumeInit = new volumeAdd (VolumeAdapter.AfterVolumeInit);
            if (AfterVolumeInit != null)
                AfterVolumeInit (gameObject);
        }

        #endregion

        public void Build ()
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
            VGlobal vg = VGlobal.GetSetting ();
            GameObject chunkBase = Resources.Load (PathCollect.chunk) as GameObject;
            Material defMat = Resources.Load (PathCollect.defaultVoxelMaterial, typeof(Material)) as Material;
            foreach (var cData in cd) {
                GameObject chunk = Instantiate (chunkBase);
                chunk.name = "Chunk" + cData.ChunkPos;
                chunk.transform.parent = nodeRoot.transform;
                chunk.transform.localPosition = new Vector3 (cData.ChunkPos.x * vg.w, cData.ChunkPos.y * vg.h, cData.ChunkPos.z * vg.d);
                chunk.transform.localRotation = Quaternion.Euler (Vector3.zero);
                Material vMat = Resources.Load (vMaterial, typeof(Material)) as Material ?? defMat;
                chunk.GetComponent<Renderer> ().sharedMaterial = vMat;
                chunk.layer = LayerMask.NameToLayer ("Floor");
                Chunk c = chunk.GetComponent<Chunk> ();
                c.cData = cData;
                c.Init ();
                if ((style & 2) > 0) {
                    m_cs.Add (c);
                }
            }

            if (style > 1) {
                PaletteItem[] itemArray = VGlobal.GetSetting ().GetItemArray (ArtPack + m_vd.subArtPack);

                if ((style & 2) > 0) {
                    foreach (Chunk c in m_cs) {
                        PlacePieces (c, itemArray);
                    }
                }

                if ((style & 4) > 0) {
                    CreateItems (m_vd, itemArray);
                }
                isFinish = true;
                Debug.Log("<color=maroon>" + gameObject.name + " place pieces finish...</color>\n");
            }

            AddComponent ();
        }

        bool isFinish;
        public bool LoadCompeleted ()
        {
            bool result = true;
            for (int idx = 0; idx < m_bts.Count; ++idx) {
                result &= m_bts [idx].ExecutionStatus != BehaviorDesigner.Runtime.Tasks.TaskStatus.Running;
            }
            return (result & isFinish);
        }

        void PlacePieces (Chunk _chunk, PaletteItem[] itemArray)
        {
            ChunkData cData = _chunk.cData;
            foreach (BlockAir bAir in cData.blockAirs) {
                for (int i = 0; i < bAir.pieceNames.Length; i++) {
                    if (System.String.IsNullOrEmpty (bAir.pieceNames [i]))
                        continue;
                    foreach (PaletteItem pi in itemArray){
                        if (bAir.pieceNames [i] == pi.name) {
                            PlacePiece (
                                bAir.BlockPos,
                                new WorldPos (i % 3, 0, (i / 3)), 
                                pi.gameObject.GetComponent<LevelPiece> (),
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
            GameObject pObj;
            float x = bPos.x * vg.w + pos.x;
            float y = bPos.y * vg.h + pos.y;
            float z = bPos.z * vg.d + pos.z;
            if (_piece != null) {
                pObj = Object.Instantiate (_piece.gameObject);
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
            foreach (BlockItem bi in _vData.blockItems){
                foreach (PaletteItem pi in itemArray){
                    if (bi.pieceName == pi.name) {
                        CreateItem (bi, pi);
                        break;
                    }
                }
            }
        }

        void CreateItem (BlockItem blockItem, PaletteItem _piece)
        {
            GameObject pObj = Object.Instantiate (_piece.gameObject);
            LevelPiece p = pObj.GetComponent<LevelPiece> ();
            pObj.transform.parent = (p is PrefabPiece) ? nodeRoot.transform : itemRoot.transform;
            pObj.transform.localPosition = new Vector3 (blockItem.posX, blockItem.posY, blockItem.posZ);
            pObj.transform.localRotation = new Quaternion (blockItem.rotX, blockItem.rotY, blockItem.rotZ, blockItem.rotW);

            if (p != null) {
                p.SetupPiece (blockItem);
            }
        }
    }
}
