﻿using UnityEngine;
using System.Collections;
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
                PaletteItem[] itemArray = VGlobal.GetSetting ().GetItemArray (ArtPack, m_vd.subArtPack);

                if ((style & 2) > 0) {
                    doorObjs.Clear ();
                    foreach (Chunk c in m_cs) {
                        StartCoroutine (PlacePieces (c, itemArray));
                    }
                }

                if (((int)m_style & 4) > 0) {
                    StartCoroutine (CreateItems (m_vd, itemArray));
                }
            }

            AddComponent ();
        }

        bool isPieceFin;
        bool isItemFin;

        public bool LoadCompeleted ()
        {
            bool result = true;
            for (int idx = 0; idx < m_bts.Count; ++idx) {
                result &= m_bts [idx].ExecutionStatus != BehaviorDesigner.Runtime.Tasks.TaskStatus.Running;
            }
            return (result & isPieceFin && isItemFin);
        }

        Dictionary<WorldPos,GameObject> doorObjs = new Dictionary<WorldPos, GameObject> ();
        GameObject doorClosedObj;

        public void FixDoor (PropertyPiece pp)
        {
            if (pp == null)
                return;
            var _pos = pp.block.BlockPos;
            if (!doorObjs.ContainsKey (_pos))
                return;
            var cDoor = doorObjs [_pos].transform;
            GameObject.Instantiate (doorClosedObj, cDoor.position, cDoor.rotation, cDoor.parent);
            GameObject.Destroy (doorObjs [_pos]);
            doorObjs.Remove (_pos);
            Debug.Log ("<b>Replace Door : </b>\n" + _pos);
        }

        IEnumerator PlacePieces (Chunk _chunk, PaletteItem[] itemArray)
        {
            string log = "";
            float time = Time.deltaTime;

            ChunkData cData = _chunk.cData;
            foreach (PaletteItem pi in itemArray) {
                if (pi.markType == PaletteItem.MarkerType.Item)
                    continue;
                if (doorClosedObj == null && pi.gameObject.name == "Door.Closed")
                    doorClosedObj = pi.gameObject;
                foreach (BlockAir bAir in cData.blockAirs) {
                    for (int i = 0; i < bAir.pieceNames.Length; i++) {
                        if (System.String.IsNullOrEmpty (bAir.pieceNames [i]))
                            continue;
                        if (bAir.pieceNames [i] == pi.name) {
                            var pObj = PlacePiece (
                                           bAir.BlockPos,
                                           new WorldPos (i % 3, 0, (i / 3)), 
                                           pi.gameObject.GetComponent<LevelPiece> (),
                                           _chunk.transform
                                       );
                            if (pi.markType == PaletteItem.MarkerType.Door)
                                doorObjs.Add (new WorldPos (
                                    cData.ChunkPos.x + bAir.BlockPos.x,
                                    cData.ChunkPos.y + bAir.BlockPos.y,
                                    cData.ChunkPos.z + bAir.BlockPos.z),
                                    pObj);
                        }
                    }
                }
                time = Time.deltaTime - time;
                log += (string.Format ("<color={0}>{1} : {2}</color>\n", ((time > 0.2f) ? "red" : "black"), pi.name, time));
                yield return new WaitForSeconds (0.00f);
            }
            Debug.Log (log);
            isPieceFin = true;
        }

        GameObject PlacePiece (WorldPos bPos, WorldPos gPos, LevelPiece _piece, Transform _parent)
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
                return pObj;
            }
            return null;
        }

        IEnumerator CreateItems (VolumeData _vData, PaletteItem[] itemArray)
        {
            while (!isPieceFin)
                yield return new WaitForSeconds (0.01f);
            Debug.Log (gameObject.name + ".StartCoroutine (CreateItems)");
            foreach (BlockItem bi in _vData.blockItems) {
                foreach (PaletteItem pi in itemArray) {
                    if (pi.markType != PaletteItem.MarkerType.Item)
                        continue;
                    if (bi.pieceName == pi.name) {
                        CreateItem (bi, pi);
                        break;
                    }
                }
                yield return new WaitForSeconds (0.00f);
            }
            Debug.Log (gameObject.name + " create Item finish...");
            isItemFin = true;
        }

        void CreateItem (BlockItem blockItem, PaletteItem _piece)
        {
            GameObject pObj = Object.Instantiate (_piece.gameObject);
            LevelPiece p = pObj.GetComponent<LevelPiece> ();
            pObj.transform.parent = (p is PrefabPiece) ? nodeRoot.transform : itemRoot.transform;
            pObj.transform.localPosition = new Vector3 (blockItem.posX, blockItem.posY, blockItem.posZ);
            pObj.transform.localRotation = new Quaternion (blockItem.rotX, blockItem.rotY, blockItem.rotZ, blockItem.rotW);

            if (p != null) {
                p.block = blockItem;
                p.SetupPiece (blockItem);
            }
        }
    }
}
