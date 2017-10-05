﻿using UnityEngine;
using System.Collections.Generic;
using System;
using BehaviorDesigner.Runtime;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace CreVox
{
    [SelectionBase]
    [ExecuteInEditMode]
    public class Volume : MonoBehaviour
    {
        public string ArtPack = PathCollect.pieces;
        public string vMaterial = PathCollect.defaultVoxelMaterial;
        public Material vertexMaterial;
        public VolumeManager vm;
        public VolumeData vd;

        #region delgate

        private delegate void volumeAdd (GameObject volume);

        private void AddComponent ()
        {
            volumeAdd AfterVolumeInit = new volumeAdd (VolumeAdapter.AfterVolumeInit);
            if (AfterVolumeInit != null)
                AfterVolumeInit (this.gameObject);
        }

        #endregion

        void Start ()
        {
            if (nodes == null)
                nodes = new Dictionary<WorldPos, Node> ();
            if (itemNodes == null)
                itemNodes = new Dictionary<BlockItem, GameObject> ();
            if (chunks == null)
                chunks = new Dictionary<WorldPos, Chunk> ();
            if (vm != null ? vm.GenerationL : VolumeManager.Generation)
                LoadTempWorld ();
        }

        void Update ()
        {
            VGlobal vg = VGlobal.GetSetting ();
            if (vm != null ? vm.snapGridL : VolumeManager.snapGrid) {
                float x = transform.position.x - transform.position.x % vg.w;
                float y = transform.position.y - transform.position.y % vg.h;
                float z = transform.position.z - transform.position.z % vg.d;
                transform.position = new Vector3 (x, y, z);
            }
            #if UNITY_EDITOR
            if (vm != null ? vm.saveBackupL : VolumeManager.saveBackup)
                CompileSave ();
            #endif
        }

        #region Chunk

        private GameObject chunkPrefab;
        GameObject chunkRoot;
        Chunk freeChunk;
        public Dictionary<WorldPos,Chunk> chunks = new Dictionary<WorldPos, Chunk> ();
        public int chunkX = 1;
        public int chunkY = 1;
        public int chunkZ = 1;

        public void BuildVolume ()
        {
            if (vd == null) {
                return;
            }
            Init (vd.chunkX, vd.chunkY, vd.chunkZ);
            foreach (Chunk c in GetChunks().Values) {
                c.cData = vd.GetChunkData (c.cData.ChunkPos);
            }
            itemArray = VGlobal.GetSetting ().GetItemArray (ArtPack + vd.subArtPack, (vm != null ? vm.volumeShowArtPackL : VolumeManager.volumeShowArtPack));
            PlacePieces ();
            PlaceItems ();

            AddComponent ();
            UpdateChunks ();
        }

        public void Init (int _chunkX, int _chunkY, int _chunkZ)
        {
            chunkPrefab = Resources.Load (PathCollect.chunk) as GameObject;
            Reset ();

            chunkX = _chunkX;
            chunkY = _chunkY;
            chunkZ = _chunkZ;

            nodeRoot = new GameObject ("DecorationRoot");
            nodeRoot.transform.parent = transform;
            nodeRoot.transform.localPosition = Vector3.zero;
            nodeRoot.transform.localRotation = Quaternion.Euler (Vector3.zero);

            itemRoot = new GameObject ("ItemRoot");
            itemRoot.transform.parent = transform;
            itemRoot.transform.localPosition = Vector3.zero;
            itemRoot.transform.localRotation = Quaternion.Euler (Vector3.zero);

            chunkRoot = new GameObject ("ChunkRoot");
            chunkRoot.transform.parent = transform;
            chunkRoot.transform.localPosition = Vector3.zero;
            chunkRoot.transform.localRotation = Quaternion.Euler (Vector3.zero);

            if (vd.useFreeChunk) {
                CreateFreeChunk ();
            } else {
                CreateChunks ();
            }

            #if UNITY_EDITOR
            CreateRuler ();
            CreateLevelRuler ();
            CreateBox ();
            ShowRuler ();
            #endif
        }

        private void Reset ()
        {
            if (chunkRoot) {
                Dictionary<WorldPos,Chunk> c = GetChunks ();
                if (c != null) {
                    DestoryChunks ();
                    c.Clear ();
                }
            }
            nodes.Clear ();
            itemNodes.Clear ();

            for (int i = transform.childCount; i > 0; i--)
                GameObject.DestroyImmediate (transform.GetChild (i - 1).gameObject);

            #if UNITY_EDITOR
            mColl = null;
            bColl = null;
            if (ruler)
                GameObject.DestroyImmediate (ruler);
            if (layerRuler)
                GameObject.DestroyImmediate (layerRuler);
            #endif
        }

        public Dictionary<WorldPos,Chunk> GetChunks ()
        {
            if (vd.useFreeChunk) {
                Dictionary<WorldPos,Chunk> c = new Dictionary<WorldPos, CreVox.Chunk> ();
                if (freeChunk == null) {
                    CreateFreeChunk ();
                }
                c.Add (freeChunk.cData.ChunkPos, freeChunk);
                return c;
            } else {
                return chunks;
            }
        }

        public void UpdateChunks ()
        {
            foreach (Chunk chunk in GetChunks().Values) {
                chunk.UpdateChunk ();
                UpdateChunkNodes (chunk.cData);
            }
        }

        void CreateChunks ()
        {
            if (vd.chunkSize == 0)
                vd.chunkSize = VGlobal.GetSetting ().chunkSize;
            for (int x = 0; x < chunkX; x++) {
                for (int y = 0; y < chunkY; y++) {
                    for (int z = 0; z < chunkZ; z++) {
                        CreateChunk (x * vd.chunkSize, y * vd.chunkSize, z * vd.chunkSize);
                    }
                }
            }
        }

        void CreateChunk (int x, int y, int z)
        {
            VGlobal vg = VGlobal.GetSetting ();
            WorldPos chunkPos = new WorldPos (x, y, z);

            GameObject newChunkObject = Instantiate (chunkPrefab, Vector3.zero, Quaternion.Euler (Vector3.zero)) as GameObject;
            newChunkObject.name = "Chunk(" + x + "," + y + "," + z + ")";
            newChunkObject.transform.parent = chunkRoot.transform;
            newChunkObject.transform.localPosition = new Vector3 (x * vg.w, y * vg.h, z * vg.d);
            newChunkObject.transform.localRotation = Quaternion.Euler (Vector3.zero);
            #if UNITY_EDITOR
            if (vertexMaterial != null)
                newChunkObject.GetComponent<Renderer> ().material = (vm != null ? vm.volumeShowArtPackL : VolumeManager.volumeShowArtPack) ? vertexMaterial : Resources.Load (PathCollect.defaultVoxelMaterial, typeof(Material)) as Material;
            newChunkObject.layer = LayerMask.NameToLayer ((EditorApplication.isPlaying) ? "Floor" : "Editor");
            #else
            if (vertexMaterial != null)
                newChunkObject.GetComponent<Renderer> ().material = vertexMaterial;
            newChunkObject.layer = LayerMask.NameToLayer("Floor");
            #endif
            Chunk newChunk = newChunkObject.GetComponent<Chunk> ();
            newChunk.cData.ChunkPos = chunkPos;
            newChunk.volume = this;
            newChunk.Init ();
            chunks.Add (chunkPos, newChunk);
        }

        void CreateFreeChunk ()
        {
            GameObject newChunkObject = Instantiate (chunkPrefab, Vector3.zero, Quaternion.Euler (Vector3.zero)) as GameObject;
            newChunkObject.name = "FreeChunk";
            newChunkObject.transform.parent = chunkRoot.transform;
            newChunkObject.transform.localPosition = Vector3.zero;
            newChunkObject.transform.localRotation = Quaternion.Euler (Vector3.zero);
            #if UNITY_EDITOR
            if (vertexMaterial != null)
                newChunkObject.GetComponent<Renderer> ().material = (vm != null ? vm.volumeShowArtPackL : VolumeManager.volumeShowArtPack) ? vertexMaterial : Resources.Load (PathCollect.defaultVoxelMaterial, typeof(Material)) as Material;
            newChunkObject.layer = LayerMask.NameToLayer ((EditorApplication.isPlaying) ? "Floor" : "Editor");
            #else
            if (vertexMaterial != null)
            newChunkObject.GetComponent<Renderer> ().material = vertexMaterial;
            newChunkObject.layer = LayerMask.NameToLayer("Floor");
            #endif
            freeChunk = newChunkObject.GetComponent<Chunk> ();
            freeChunk.cData.ChunkPos = new WorldPos (0, 0, 0);
            freeChunk.volume = this;
            freeChunk.Init ();
        }

        void DestoryChunks ()
        {
            if (vd.useFreeChunk) {
                #if UNITY_EDITOR
                GameObject.DestroyImmediate (freeChunk.gameObject);
                #else
                UnityEngine.Object.Destroy(freeChunk.gameObject);
                #endif
            } else {
                for (int x = 0; x < chunkX; x++) {
                    for (int y = 0; y < chunkY; y++) {
                        for (int z = 0; z < chunkZ; z++) {
                            DestroyChunk (x * vd.chunkSize, y * vd.chunkSize, z * vd.chunkSize);
                        }
                    }
                }
            }
        }

        void DestroyChunk (int x, int y, int z)
        {
            WorldPos chunkPos = new WorldPos (x, y, z);
            if (chunks.ContainsKey (chunkPos) && chunks [chunkPos] != null) {
                if (chunks [chunkPos].gameObject) {
                    #if UNITY_EDITOR
                    GameObject.DestroyImmediate (chunks [chunkPos].gameObject);
                    #else
                    UnityEngine.Object.Destroy(chunks [chunkPos].gameObject);
                    #endif
                    chunks [chunkPos].Destroy ();
                    chunks.Remove (chunkPos);
                }
            }
        }

        public Chunk GetChunk (int x, int y, int z)
        {
            if (vd.useFreeChunk) {
                return freeChunk;
            } else {
                WorldPos pos = new WorldPos ();
                pos.x = Mathf.FloorToInt (x / vd.chunkSize) * vd.chunkSize;
                pos.y = Mathf.FloorToInt (y / vd.chunkSize) * vd.chunkSize;
                pos.z = Mathf.FloorToInt (z / vd.chunkSize) * vd.chunkSize;

                return chunks.ContainsKey (pos) ? chunks [pos] : null;
            }
        }

        #endregion

        #region Node

        class Node
        {
            public GameObject pieceRoot;
            public GameObject[] pieces;
        }

        GameObject nodeRoot;
        Dictionary<WorldPos,Node> nodes = new Dictionary<WorldPos, Node> ();

        public GameObject GetNode (WorldPos _volumePos)
        {
            if (nodes.ContainsKey (_volumePos))
                return nodes [_volumePos].pieceRoot;
            else {
                Debug.Log ("(" + _volumePos + ") has no Node; try another artpack !!!");
                return null;
            }
        }

        void CreateNode (WorldPos bPos)
        {
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

        public void UpdateChunkNodes (ChunkData cData)
        {
            for (int i = 0; i < cData.blockAirs.Count; i++) {
                BlockAir bAir = cData.blockAirs [i];
                bool isEmpty = true;
                foreach (string p in bAir.pieceNames) {
                    if (p != "") {
                        isEmpty = false;
                        break;
                    }
                }
                if (isEmpty) {
                    cData.blockAirs.Remove (bAir);
                } else { 
                    WorldPos volumePos = new WorldPos (
                                             cData.ChunkPos.x + bAir.BlockPos.x, 
                                             cData.ChunkPos.y + bAir.BlockPos.y, 
                                             cData.ChunkPos.z + bAir.BlockPos.z);
                    GameObject node = GetNode (volumePos);
                    if (node != null) {
                        #if UNITY_EDITOR
                        if (!UnityEditor.EditorApplication.isPlaying && cuter && bAir.BlockPos.y + cData.ChunkPos.y > cutY)
                            node.SetActive (false);
                        else
                            node.SetActive (true);
                        #else
                            node.SetActive (true);
                        #endif
                    }
                }
            }
        }

        bool RemoveNodeIfIsEmpty (WorldPos bPos)
        {
            BlockAir blockAir = GetBlock (bPos.x, bPos.y, bPos.z) as BlockAir;
            bool isEmpty = true;
            if (blockAir != null) {
                foreach (string p in blockAir.pieceNames) {
                    if (p != null && p.Length > 0) {
                        isEmpty = false;
                        break;
                    }
                }
                if (isEmpty) {
                    if (nodes.ContainsKey (bPos)) {
                        GameObject.DestroyImmediate (nodes [bPos].pieceRoot);
                        nodes.Remove (bPos);
                    }
                }
            }
            return isEmpty;
        }

        public void RemoveNode (WorldPos bPos)
        {
            for (int i = 0; i < vd.blockItems.Count; ++i) {
                BlockItem blockItem = vd.blockItems [i];
                if (blockItem.BlockPos.Equals (bPos)) {
                    itemNodes.Remove (blockItem);
                    vd.blockItems.RemoveAt (i);

                    if (nodes.ContainsKey (bPos)) {
                        nodes [bPos].pieces [i] = null;
                    }
                    break;
                }
            }
        }

        PaletteItem[] itemArray = new PaletteItem[0];
        GameObject itemRoot;
        Dictionary<BlockItem,GameObject> itemNodes = new Dictionary<BlockItem, GameObject> ();

        public GameObject GetItemNode (BlockItem blockItem)
        {
            if (itemNodes.ContainsKey (blockItem))
                return itemNodes [blockItem];
            else {
                return null;
            }
        }

        #endregion

        #region Block

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
            Block oldBlock = GetBlock (x, y, z);
            if (chunk != null) {
                WorldPos chunkBlockPos = new WorldPos (x - chunk.cData.ChunkPos.x, y - chunk.cData.ChunkPos.y, z - chunk.cData.ChunkPos.z);
                if (_block != null) {
                    _block.BlockPos = chunkBlockPos;
                    Predicate<BlockAir> sameBlockAir = delegate(BlockAir b) {
                        return b.BlockPos.Compare (chunkBlockPos);
                    };
                    switch (_block.GetType ().ToString ()) {
                    case "CreVox.BlockAir":
                        if (!chunk.cData.blockAirs.Exists (sameBlockAir)) {
                            chunk.cData.blockAirs.Add (_block as BlockAir);
                        }
                        break;
                    case "CreVox.BlockHold":
                        Predicate<BlockHold> sameBlockHold = delegate(BlockHold b) {
                            return b.BlockPos.Compare (chunkBlockPos);
                        };
                        if (!chunk.cData.blockHolds.Exists (sameBlockHold)) {
                            chunk.cData.blockHolds.Add (_block as BlockHold);
                        }
                        break;
                    case "CreVox.Block":
                        Predicate<Block> sameBlock = delegate(Block b) {
                            return b.BlockPos.Compare (chunkBlockPos);
                        };
                        if (chunk.cData.blockAirs.Exists (sameBlockAir)) {
                            BlockAir ba = oldBlock as BlockAir;
                            for (int i = 0; i < 8; i++) {
                                PlacePiece (ba.BlockPos, new WorldPos (i % 3, 0, (int)(i / 3)), null);
                            }
                        }
                        if (!chunk.cData.blocks.Exists (sameBlock)) {
                            chunk.cData.blocks.Add (_block);
                        }
                        break;
                    }
                } else if (oldBlock != null) {
                    switch (oldBlock.GetType ().ToString ()) {
                    case "CreVox.BlockAir":
                        List<BlockAir> bAirs = chunk.cData.blockAirs;
                        for (int i = bAirs.Count - 1; i > -1; i--) {
                            if (bAirs [i].BlockPos.Compare (chunkBlockPos))
                                bAirs.RemoveAt (i);
                        }
                        break;
                    case "CreVox.BlockHold":
                        List<BlockHold> bHolds = chunk.cData.blockHolds;
                        for (int i = bHolds.Count - 1; i > -1; i--) {
                            if (bHolds [i].BlockPos.Compare (chunkBlockPos))
                                bHolds.RemoveAt (i);
                        }
                        break;
                    case "CreVox.Block":
                        List<Block> blocks = chunk.cData.blocks;
                        for (int i = blocks.Count - 1; i > -1; i--) {
                            if (blocks [i].BlockPos.Compare (chunkBlockPos))
                                blocks.RemoveAt (i);
                        }
                        break;
                    }
                }
            }
        }

        public void PlaceItem (int _id, LevelPiece _piece, Vector3 _position = default(Vector3))
        {
            BlockItem blockItem;
            if (_piece != null) {
                if (_piece.GetComponent<PaletteItem> ().markType != PaletteItem.MarkerType.Item)
                    return;
                
                if (_id < vd.blockItems.Count) {
                    blockItem = vd.blockItems [_id];
                } else {
                    blockItem = new BlockItem ();
                    blockItem.BlockPos = EditTerrain.GetBlockPos (_position);
                    blockItem.pieceName = _piece.gameObject.name;
                    blockItem.posX = _position.x;
                    blockItem.posY = _position.y;
                    blockItem.posZ = _position.z;
                    blockItem.rotX = _piece.transform.localRotation.x;
                    blockItem.rotY = _piece.transform.localRotation.y;
                    blockItem.rotZ = _piece.transform.localRotation.z;
                    blockItem.rotW = _piece.transform.localRotation.w;
                    vd.blockItems.Add (blockItem);
                }
                if (!itemNodes.ContainsKey (blockItem)) {
                    GameObject pObj;
                    #if UNITY_EDITOR
                    pObj = PrefabUtility.InstantiatePrefab (_piece.gameObject) as GameObject;
                    #else
                    pObj = GameObject.Instantiate(_piece.gameObject);
                    #endif
                    pObj.transform.parent = (_piece is PrefabPiece) ? nodeRoot.transform : itemRoot.transform;
                    pObj.transform.localPosition = new Vector3 (blockItem.posX, blockItem.posY, blockItem.posZ);
                    pObj.transform.localRotation = new Quaternion (blockItem.rotX, blockItem.rotY, blockItem.rotZ, blockItem.rotW);
                    if (_piece.name == "Missing") {
                        pObj.GetComponentInChildren<TextMesh> ().text += ("\n" + vd.blockItems [_id].pieceName);
                    }
                    itemNodes.Add (blockItem, pObj);
                    LevelPiece p = pObj.GetComponent<LevelPiece> ();
                    if (p != null) {
                        p.block = blockItem;
                        p.SetupPiece (blockItem);
                    }
                }
            } else {
                if (!(_id < vd.blockItems.Count))
                    return;
                
                blockItem = vd.blockItems [_id];
                GameObject.DestroyImmediate (itemNodes [blockItem]);
                itemNodes.Remove (blockItem);
                vd.blockItems.RemoveAt (_id);
            }
            
        }

        private void PlaceItems ()
        {
            GameObject _missing = Resources.Load (PathCollect.resourceSubPath + "Missing", typeof(GameObject)) as GameObject;
            LevelPiece _missingP = _missing.GetComponent<LevelPiece> ();
            for (int i = 0; i < vd.blockItems.Count; i++) {
                BlockItem bItem = vd.blockItems [i];
                LevelPiece p = _missingP;
                for (int k = 0; k < itemArray.Length; k++) {
                    if (bItem.pieceName == itemArray [k].name) {
                        p = itemArray [k].gameObject.GetComponent<LevelPiece> ();
                        break;
                    }
                }
                PlaceItem (i, p);
            }
        }

        public void PlacePiece (WorldPos bPos, WorldPos gPos, LevelPiece _piece, bool isNew = true)
        {
            Block block = GetBlock (bPos.x, bPos.y, bPos.z);
            BlockAir blockAir = null;
            int id = gPos.z * 3 + gPos.x;
            GameObject pObj;

            if (block != null && !(block is BlockAir))
                return;

            if (_piece != null) {
                if (_piece.GetComponent<PaletteItem> ().markType == PaletteItem.MarkerType.Item) {
                    if (_piece.name != "Missing")
                        return;
                }
                
                if (block == null) {
                    SetBlock (bPos.x, bPos.y, bPos.z, new BlockAir ());
                    block = GetBlock (bPos.x, bPos.y, bPos.z);
                }

                if (!nodes.ContainsKey (bPos))
                    CreateNode (bPos);
                
                pObj = nodes [bPos].pieces [id];
                if (pObj != null) {
                    PlaceBlockHold (bPos, id, pObj.GetComponent<LevelPiece> (), true);
                    GameObject.DestroyImmediate (pObj);
                }

                #if UNITY_EDITOR
                if (isNew) {
                    pObj = PrefabUtility.InstantiatePrefab (_piece.gameObject) as GameObject;
                } else {
                    pObj = _piece.gameObject;
                }
                #else
                pObj = GameObject.Instantiate(_piece.gameObject);
                #endif
                pObj.transform.parent = nodes [bPos].pieceRoot.transform;
                pObj.GetComponent<LevelPiece> ().block = block;
                Vector3 pos = GetPieceOffset (gPos.x, gPos.z);
                VGlobal vg = VGlobal.GetSetting ();
                float x = bPos.x * vg.w + pos.x;
                float y = bPos.y * vg.h + pos.y;
                float z = bPos.z * vg.d + pos.z;
                pObj.transform.localPosition = new Vector3 (x, y, z);
                pObj.transform.localRotation = Quaternion.Euler (0, GetPieceAngle (gPos.x, gPos.z), 0);
                nodes [bPos].pieces [id] = pObj;

                if (block is BlockAir) {
                    blockAir = block as BlockAir;
                    if (_piece.name != "Missing") {
                        blockAir.SetPiece (bPos, gPos, pObj.GetComponent<LevelPiece> ());
                        blockAir.SolidCheck (nodes [bPos].pieces);
                        SetBlock (bPos.x, bPos.y, bPos.z, blockAir);
                    } else {
                        pObj.GetComponentInChildren<TextMesh> ().text += ("\n" + blockAir.pieceNames [id]);
                    }

                    if (_piece.isHold == true)
                        PlaceBlockHold (bPos, id, pObj.GetComponent<LevelPiece> (), false);
                }
            } else {
                if (block is BlockAir) {
                    blockAir = block as BlockAir;
                    blockAir.SetPiece (bPos, gPos, null);
                    blockAir.SolidCheck (nodes [bPos].pieces);
                }

                if (nodes.ContainsKey (bPos)) {
                    pObj = nodes [bPos].pieces [id];
                    if (pObj != null) {
                        PlaceBlockHold (bPos, id, pObj.GetComponent<LevelPiece> (), true);
                        GameObject.DestroyImmediate (pObj);
                    }
                }

                if (RemoveNodeIfIsEmpty (bPos))
                    SetBlock (bPos.x, bPos.y, bPos.z, null);
            }
        }

        #if UNITY_EDITOR
        public GameObject CopyPiece (WorldPos bPos, WorldPos gPos, bool a_cut)
        {
            int id = gPos.z * 3 + gPos.x;
            GameObject pObj = null;

            if (nodes.ContainsKey (bPos) && nodes [bPos].pieces.Length > id) {
                pObj = nodes [bPos].pieces [id];

                if (pObj != null) {
                    Vector3 pos = pObj.transform.position;
                    PaletteItem pi = pObj.GetComponent<PaletteItem> ();
                    if (pi != null) {
                        GameObject asset = AssetDatabase.LoadAssetAtPath (pi.assetPath,
                                               typeof(GameObject)) as GameObject;
                        pObj = PrefabUtility.InstantiatePrefab (asset) as GameObject;
                        pObj.transform.parent = nodeRoot.transform;
                        pObj.transform.position = pos;
                    }

                    if (a_cut) {
                        GameObject.DestroyImmediate (nodes [bPos].pieces [id]);
                        nodes [bPos].pieces [id] = null;
                    }
                }
            }

            return pObj;
        }
        #endif

        static GameObject _missing;
        static LevelPiece _missingP;

        private void PlacePieces ()
        {
            if (_missing == null) {
                _missing = Resources.Load (PathCollect.resourceSubPath + "Missing", typeof(GameObject)) as GameObject;
            }
            if (_missingP == null) {
                _missingP = _missing.GetComponent<LevelPiece> ();
            }
            foreach (Chunk c in GetChunks().Values) {
                for (int b = 0; b < c.cData.blockAirs.Count; b++) {
                    BlockAir ba = c.cData.blockAirs [b];
                    for (int i = 0; i < ba.pieceNames.Length; i++) {
                        if (ba.pieceNames [i] != null && ba.pieceNames [i] != "") {
                            LevelPiece p = _missingP;
                            for (int k = 0; k < itemArray.Length; k++) {
                                if (ba.pieceNames [i] == itemArray [k].name) {
                                    p = itemArray [k].gameObject.GetComponent<LevelPiece> ();
                                }
                            }
                            PlacePiece (
                                new WorldPos (
                                    c.cData.ChunkPos.x + ba.BlockPos.x,
                                    c.cData.ChunkPos.y + ba.BlockPos.y,
                                    c.cData.ChunkPos.z + ba.BlockPos.z),
                                new WorldPos (i % 3, 0, (int)(i / 3)),
                                p
                            );
                        }
                    }
                }
            }
            BehaviorManager _bm = this.gameObject.GetComponentInParent<BehaviorManager> ();
            if (_bm) {
                BehaviorTree[] _tree = this.gameObject.GetComponentsInChildren<BehaviorTree> ();
                if (_tree.Length > 0) {
                    for (int i = 0; i < _tree.Length; i++) {
                        _tree [i].EnableBehavior ();
                        _bm.EnableBehavior (_tree [i] as Behavior);
                        if (_tree [i].ExternalBehavior != null) {
                            List<SharedVariable> n = _tree [i].GetAllVariables ();
                            for (int j = 0; j < n.Count; j++) {
                                _tree [i].ExternalBehavior.SetVariable (n [j].Name, n [j]);
                            }
                        }
                        _bm.Tick (_tree [i]);
                    }
                }
            }
        }

        private int GetBlockHoldIndex (int x, int y, int z, Chunk containerChunk)
        {
            WorldPos bPos = new WorldPos (x, y, z);
            Predicate <BlockHold> checkBlockPos = delegate (BlockHold bh) {
                return bh.BlockPos.Compare (bPos);
            };
            if (containerChunk != null) {
                int _index = containerChunk.cData.blockHolds.FindIndex (checkBlockPos);
                return _index;
            } else {
                return -1;
            }
        }

        private void PlaceBlockHold (WorldPos _bPos, int _id, LevelPiece _piece, bool _isErase)
        {
//            Debug.Log ("[" + _bPos.ToString () + "](" + _id.ToString () + ")-" + (_isErase?"Delete":"Add"));
            for (int i = 0; i < _piece.holdBlocks.Count; i++) {
                LevelPiece.Hold bh = _piece.holdBlocks [i];
                int x = _bPos.x + bh.offset.x;
                int y = _bPos.y + bh.offset.y;
                int z = _bPos.z + bh.offset.z;
                Chunk _chunk = GetChunk (x, y, z);
                if (_chunk != null) {
                    x -= _chunk.cData.ChunkPos.x;
                    y -= _chunk.cData.ChunkPos.y;
                    z -= _chunk.cData.ChunkPos.z;

                    BlockHold.piecePos bhData = new BlockHold.piecePos ();
                    bhData.blockPos = _bPos;
                    bhData.pieceID = _id;


                    Predicate<BlockHold.piecePos> samePiecePos = delegate(BlockHold.piecePos obj) {
                        return (obj.blockPos.Compare (bhData.blockPos) && obj.pieceID == bhData.pieceID);
                    };

                    BlockHold bhBlock = null;
                    int _index = GetBlockHoldIndex (x, y, z, _chunk);
                    if (_index > -1)
                        bhBlock = _chunk.cData.blockHolds [_index];
                    
                    if (_isErase) {
                        if (bhBlock != null) { 
                            if (bhBlock.roots.Exists (samePiecePos))
                                bhBlock.roots.RemoveAt (bhBlock.roots.FindIndex (samePiecePos));
                            if (bhBlock.roots.Count == 0)
                                _chunk.cData.blockHolds.Remove (bhBlock);
                        }
                    } else {
                        if (bhBlock == null) {
                            bhBlock = new BlockHold ();
                            bhBlock.BlockPos = new WorldPos (x, y, z);
                            bhBlock.roots.Add (bhData);
                            _chunk.cData.blockHolds.Add (bhBlock);
                        } else if (!bhBlock.roots.Exists (samePiecePos))
                            bhBlock.roots.Add (bhData);
                        
                        if (bh.isSolid)
                            bhBlock.SetSolid (true);
                    }
                }
            }
        }

        public static Vector3 GetPieceOffset (int x, int z)
        {
            Vector3 offset = Vector3.zero;
            VGlobal vg = VGlobal.GetSetting ();
            float hw = vg.w / 2;
            float hh = vg.h / 2;
            float hd = vg.d / 2;

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
            string vdName = AssetDatabase.GetAssetPath (vd);
            vdName = vdName.Substring (vdName.LastIndexOf ("/") + 1);
            string date = System.DateTime.Now.ToString ("yyyyMMdd") + "-" + System.DateTime.Now.ToString ("HHmmss");
            string backupPath = PathCollect.resourcesPath + PathCollect.save + "/_TempBackup/" + date + "_" + vdName;

            VolumeData vdBackup = ScriptableObject.CreateInstance<VolumeData> ();
            vdBackup.blockItems = vd.blockItems;
            vdBackup.chunkDatas = vd.chunkDatas;
            vdBackup.chunkX = vd.chunkX;
            vdBackup.chunkY = vd.chunkY;
            vdBackup.chunkZ = vd.chunkZ;
            UnityEditor.AssetDatabase.CreateAsset (vdBackup, backupPath);
            AssetDatabase.Refresh ();
        }
        #endif
        public void LoadTempWorld ()
        {
            BuildVolume ();

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
            float x = -vg.w / 2;
            float y = -vg.h / 2;
            float z = -vg.d / 2;
            float w = (vd.useFreeChunk ? vd.freeChunk.freeChunkSize.x : chunkX * vd.chunkSize) * vg.w + x;
            float d = (vd.useFreeChunk ? vd.freeChunk.freeChunkSize.z : chunkZ * vd.chunkSize) * vg.d + z;
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
            VGlobal vg = VGlobal.GetSetting ();
            float w = (vd.useFreeChunk ? vd.freeChunk.freeChunkSize.x : chunkX * vd.chunkSize) * vg.w;
            float d = (vd.useFreeChunk ? vd.freeChunk.freeChunkSize.z : chunkZ * vd.chunkSize) * vg.d;
            layerRuler = new GameObject ("LevelRuler");
            layerRuler.layer = LayerMask.NameToLayer ("EditorLevel");
            layerRuler.transform.parent = transform;
            layerRuler.transform.localPosition = new Vector3 (w / 2 - vg.w / 2, 0f, d / 2 - vg.d / 2);
            layerRuler.transform.localRotation = Quaternion.Euler (Vector3.zero);
            bColl = layerRuler.AddComponent<BoxCollider> ();
            bColl.size = new Vector3 (w, 0f, d);
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
            bool r = (vm != null ? vm.debugRulerL : VolumeManager.debugRuler);
            if (mColl) {
                mColl.enabled = _active;
                ruler.SetActive (_active);
                ruler.hideFlags = r ? HideFlags.None : HideFlags.HideInHierarchy;
            }
            if (bColl) {
                bColl.enabled = _active;
                layerRuler.SetActive (_active);
                layerRuler.hideFlags = r ? HideFlags.None : HideFlags.HideInHierarchy;
            }
            if (box) {
                box.hideFlags = r ? HideFlags.None : HideFlags.HideInHierarchy;
            }
            pointer = _active;
        }

        public void ShowRuler ()
        {
            bool _active = EditorApplication.isPlaying ? false : (vm != null ? vm.debugRulerL : VolumeManager.debugRuler);
            ActiveRuler (_active);
        }
        #endif
        #endregion

        #region Editor Scene UI

        #if UNITY_EDITOR
        public static Volume focusVolume = null;
        public Color YColor;
        public bool pointer;
        public int pointY;
        public bool cuter;
        public int cutY;
        [NonSerialized]public PaletteItem _itemInspected;

        void OnDrawGizmos ()
        {
            if (focusVolume == this && (vm != null ? vm.debugRulerL : VolumeManager.debugRuler)) {
                Matrix4x4 oldMatrix = Gizmos.matrix;
                VGlobal vg = VGlobal.GetSetting ();
                if ((vd == null) || (!vd.useFreeChunk && chunks.Count == 0) || (vd.useFreeChunk && freeChunk == null))
                    Gizmos.color = Color.red;
                else
                    Gizmos.color = new Color (YColor.r, YColor.g, YColor.b, 0.4f);
                Gizmos.matrix = transform.localToWorldMatrix;
                if (mColl)
                    Gizmos.DrawWireCube (
                        new Vector3 (
                            ((vd.useFreeChunk) ? freeChunk.cData.freeChunkSize.x - 1 : (chunkX * vd.chunkSize - 1)) * vg.w / 2,
                            ((vd.useFreeChunk) ? freeChunk.cData.freeChunkSize.y - 1 : (chunkY * vd.chunkSize - 1)) * vg.h / 2,
                            ((vd.useFreeChunk) ? freeChunk.cData.freeChunkSize.z - 1 : (chunkZ * vd.chunkSize - 1)) * vg.d / 2),
                        new Vector3 (
                            ((vd.useFreeChunk) ? freeChunk.cData.freeChunkSize.x : chunkX * vd.chunkSize) * vg.w,
                            ((vd.useFreeChunk) ? freeChunk.cData.freeChunkSize.y : chunkY * vd.chunkSize) * vg.h,
                            ((vd.useFreeChunk) ? freeChunk.cData.freeChunkSize.z : chunkZ * vd.chunkSize) * vg.d)
                    );
                
                DrawGizmoBoxCursor ();
                DrawGizmoLayer ();
                DrawBlockItem ();
                if (vm != null ? vm.showBlockHoldL : VolumeManager.showBlockHold)
                    DrawBlockHold ();
                Gizmos.matrix = oldMatrix;
            }
        }

        void DrawBlockHold ()
        {
            VGlobal vg = VGlobal.GetSetting ();
            foreach (Chunk chunk in GetChunks().Values) {
                if (chunk != null) {
                    for (int i = 0; i < chunk.cData.blockHolds.Count; i++) {
                        WorldPos blockHoldPos = chunk.cData.blockHolds [i].BlockPos;
                        WorldPos chunkPos = chunk.cData.ChunkPos;
                        Vector3 localPos = new Vector3 (
                                               (blockHoldPos.x + chunkPos.x) * vg.w, 
                                               (blockHoldPos.y + chunkPos.y) * vg.h, 
                                               (blockHoldPos.z + chunkPos.z) * vg.d
                                           );
                        Gizmos.color = new Color (255f / 255f, 244f / 255f, 228f / 255f, 0.05f);
                        Gizmos.DrawCube (localPos, new Vector3 (vg.w, vg.h, vg.d));
                    }
                }
            }
        }

        void DrawBlockItem ()
        {
            VGlobal vg = VGlobal.GetSetting ();
            for (int i = 0; i < vd.blockItems.Count; i++) {
                BlockItem item = vd.blockItems [i];
                Vector3 localPos = new Vector3 (
                                       Mathf.Round (item.posX / vg.w) * vg.w,
                                       Mathf.Round (item.posY / vg.h) * vg.h,
                                       Mathf.Round (item.posZ / vg.d) * vg.d
                                   );
                Gizmos.color = new Color (0f / 255f, 202f / 255f, 255f / 255f, 0.3f);
                Gizmos.DrawCube (localPos, new Vector3 (vg.w, vg.h, vg.d));
                Vector3 localPos2 = new Vector3 (
                                        item.BlockPos.x * vg.w,
                                        item.BlockPos.y * vg.h,
                                        item.BlockPos.z * vg.d
                                    );
                Gizmos.DrawWireCube (localPos2, new Vector3 (vg.w, vg.h, vg.d));
            }
        }

        void DrawGizmoLayer ()
        {
            VGlobal vg = VGlobal.GetSetting ();
            if (pointer) {
                for (int xi = 0; xi < ((vd.useFreeChunk) ? freeChunk.cData.freeChunkSize.x : (chunkX * vd.chunkSize)); xi++) {
                    for (int zi = 0; zi < ((vd.useFreeChunk) ? freeChunk.cData.freeChunkSize.z : (chunkZ * vd.chunkSize)); zi++) {
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
            _y = Mathf.Clamp (_y, 0, ((vd.useFreeChunk) ? freeChunk.cData.freeChunkSize.y : (chunkY * vd.chunkSize)) - 1);
            pointY = _y;
            YColor = new Color (
                (20 + (pointY % 10) * 20) / 255f,
                (200 - Mathf.Abs ((pointY % 10) - 5) * 20) / 255f,
                (200 - (pointY % 10) * 20) / 255f
            );
            if (bColl) {
                bColl.center = new Vector3 (bColl.center.x, (pointY + 0.5f) * vg.h, bColl.center.z);
            }
            if (chunks != null && chunks.Count > 0)
                UpdateChunks ();
        }

        public void ChangeCutY (int _y)
        {
            _y = Mathf.Clamp (_y, 0, ((vd.useFreeChunk) ? freeChunk.cData.freeChunkSize.x : (chunkX * vd.chunkSize)) - 1);
            cutY = _y;
            if (chunks != null && chunks.Count > 0)
                UpdateChunks ();
        }

        #endif
        #endregion

        private List<ConnectionInfo> _connectionInfos;

        public List<ConnectionInfo> ConnectionInfos {
            get { return _connectionInfos; }
            set { _connectionInfos = value; }
        }
    }

    public enum ConnectionInfoType
    {
        StartingNode,
        Connection
    }

    public class ConnectionInfo
    {
        public WorldPos position;
        public Quaternion rotation;
        public ConnectionInfoType type;
        public string connectionName;
        public Guid connectedObjectGuid;
        public GameObject connectedGameObject;
        public bool used;
        // Just for connection.
        public ConnectionInfo (WorldPos position, Quaternion rotation, ConnectionInfoType type, string name = "")
        {
            this.position = position;
            this.rotation = rotation;
            this.type = type;
            this.connectionName = name;
            used = false;
            connectedGameObject = null;
        }

        public ConnectionInfo (ConnectionInfo clone)
        {
            this.position = clone.position;
            this.rotation = clone.rotation;
            this.type = clone.type;
            this.connectionName = clone.connectionName;
            this.used = clone.used;
            connectedObjectGuid = clone.connectedObjectGuid;
            connectedGameObject = clone.connectedGameObject;
        }

        public ConnectionInfo Clone ()
        {
            return new ConnectionInfo (this);
        }

        public bool Compare (ConnectionInfo obj)
        {
            return this.position.Compare (obj.position) && this.rotation == obj.rotation && this.type == obj.type && this.used == obj.used && this.connectedObjectGuid == obj.connectedObjectGuid;
        }

        public WorldPos RelativePosition (float degree)
        {
            int absoluteDegree = ((int)(degree + this.rotation.eulerAngles.y) % 360);
            return DirectionOffset [absoluteDegree / 90];
        }
        // Constant array.
        public static WorldPos[] DirectionOffset = new WorldPos[] {
            new WorldPos (0, 0, 1),
            new WorldPos (1, 0, 0),
            new WorldPos (0, 0, -1),
            new WorldPos (-1, 0, 0)
        };
    }
}