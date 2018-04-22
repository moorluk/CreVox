using UnityEngine;
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
        public static Volume focusVolume;

        public string ArtPack = PathCollect.pieces;
        public string vMaterial = PathCollect.defaultVoxelMaterial;

        Material vertexMaterial;

        public Material VertexMaterial {
            get {
                if (vertexMaterial == null) {
                    vertexMaterial = Resources.Load (vMaterial, typeof(Material)) as Material;
                    if (vertexMaterial == null)
                        return Resources.Load (PathCollect.defaultVoxelMaterial, typeof(Material)) as Material;
                }
                return vertexMaterial;
            }
            set { vertexMaterial = value; }
        }

        VGlobal vg;

        public VGlobal Vg {
            get { return vg ?? VGlobal.GetSetting (); }
            set { vg = value; }
        }

        VolumeManager vm;

        public VolumeManager Vm {
            get { return vm ?? transform.GetComponentInParent<VolumeManager> (); }
            set { vm = value; }
        }

        public VolumeData vd;

        #region delgate

        private delegate void volumeAdd (GameObject volume);

        void AddComponent ()
        {
            volumeAdd AfterVolumeInit = new volumeAdd (VolumeAdapter.AfterVolumeInit);
            if (AfterVolumeInit != null)
                AfterVolumeInit (gameObject);
        }

        #endregion

        void Awake ()
        {
            if (!gameObject.activeSelf)
                return;
            if (nodes == null)
                nodes = new Dictionary<WorldPos, Node> ();
            if (itemNodes == null)
                itemNodes = new Dictionary<BlockItem, GameObject> ();
            if (chunks == null)
                chunks = new Dictionary<WorldPos, Chunk> ();
        }

        void Start ()
        {
            if (isLost) {
                Debug.Log (string.Format ("<color=purple>Volume({0}) Reload...</color>\n", gameObject.name));
                Build ();
            }
        }

        #if UNITY_EDITOR
        void Update ()
        {
            if (Vm.SaveBackup)
                CompileSave ();
            if (Vm.SnapGrid) {
                float x = transform.position.x - transform.position.x % Vg.w;
                float y = transform.position.y - transform.position.y % Vg.h;
                float z = transform.position.z - transform.position.z % Vg.d;
                transform.position = new Vector3 (x, y, z);
            }
            if (nodeRoot)
                nodeRoot.SetActive (!Vm.ShowBlockHold);
            if (chunkRoot)
                chunkRoot.SetActive (!Vm.ShowBlockHold);
        }
        #endif

        public void Build ()
        {
            ClearNodes ();
            CreateNodeRoots ();

            if (vd == null)
                return;

            if (vd.useFreeChunk) {
                CreateFreeChunk ();
            } else {
                CreateChunks ();
            }
            UpdateChunks ();

            itemArray = Vg.GetItemArray (ArtPack, vd.subArtPack, (Vm.UseArtPack));

            PlacePieces ();
            PlaceItems ();

            AddComponent ();
        }

        void ClearNodes ()
        {
            //clear LevelPiece
            nodes.Clear ();
            ClearNode (nodeRoot);

            //clear Item
            itemNodes.Clear ();
            ClearNode (itemRoot);

            //clear Chunk
            chunks.Clear ();
            ClearNode (chunkRoot);

            string log = "<b>Other GameObject :</b>\n";
            for (int i = transform.childCount; i > 0; i--) {
                log += transform.GetChild (i - 1).gameObject.name + "\n";
                ClearNode (transform.GetChild (i - 1).gameObject);
            }
            Debug.Log (log);
        }
        void ClearNode (GameObject _node)
        {
            if (Application.isPlaying)
                UnityEngine.Object.Destroy (_node);
            else
                UnityEngine.Object.DestroyImmediate (_node);
        }

        void CreateNodeRoots ()
        {
            chunkRoot = new GameObject ("ChunkRoot");
            chunkRoot.transform.parent = transform;
            chunkRoot.transform.localPosition = Vector3.zero;
            chunkRoot.transform.localRotation = Quaternion.Euler (Vector3.zero);

            nodeRoot = new GameObject ("DecorationRoot");
            nodeRoot.transform.parent = transform;
            nodeRoot.transform.localPosition = Vector3.zero;
            nodeRoot.transform.localRotation = Quaternion.Euler (Vector3.zero);

            itemRoot = new GameObject ("ItemRoot");
            itemRoot.transform.parent = transform;
            itemRoot.transform.localPosition = Vector3.zero;
            itemRoot.transform.localRotation = Quaternion.Euler (Vector3.zero);
        }

        #region Chunk

        GameObject chunkRoot;
        GameObject chunkPrefab;

        GameObject ChunkPrefab {
            get {
                if (!chunkPrefab)
                    chunkPrefab = Resources.Load (PathCollect.chunk) as GameObject;
                return chunkPrefab;
            }
        }

        Chunk freeChunk;
        Dictionary<WorldPos,Chunk> chunks = new Dictionary<WorldPos, Chunk> ();

        public Dictionary<WorldPos,Chunk> Chunks {
            get {
                if (vd.useFreeChunk) {
                    var c = new Dictionary<WorldPos, Chunk> ();
                    if (freeChunk == null)
                        CreateFreeChunk ();
                    c.Add (freeChunk.cData.ChunkPos, freeChunk);
                    return c;
                }
                return chunks;
            }
        }

        public Chunk GetChunk (int x, int y, int z)
        {
            if (vd.useFreeChunk) {
                return freeChunk;
            } else {
                WorldPos pos = new WorldPos (
                    Mathf.FloorToInt (x / vd.chunkSize) * vd.chunkSize,
                    Mathf.FloorToInt (y / vd.chunkSize) * vd.chunkSize,
                    Mathf.FloorToInt (z / vd.chunkSize) * vd.chunkSize
                );
                return chunks.ContainsKey (pos) ? chunks [pos] : null;
            }
        }

        public void UpdateChunks ()
        {
            foreach (Chunk c in Chunks.Values) {
                c.UpdateChunk ();
            }
        }

        void CreateChunks ()
        {
            if (vd.chunkSize == 0)
                vd.chunkSize = Vg.chunkSize;
            foreach (ChunkData cd in vd.chunkDatas)
                CreateChunk (cd);
        }

        void CreateChunk (ChunkData cData)
        {
            WorldPos chunkPos = cData.ChunkPos;
            GameObject newChunkObject = Instantiate (ChunkPrefab);
            newChunkObject.name = "Chunk(" + chunkPos + ")";
            newChunkObject.transform.parent = chunkRoot.transform;
            newChunkObject.transform.localPosition = new Vector3 (chunkPos.x * Vg.w, chunkPos.y * Vg.h, chunkPos.z * Vg.d);
            newChunkObject.transform.localRotation = Quaternion.Euler (Vector3.zero);
            newChunkObject.GetComponent<Renderer> ().material = (Vm.UseArtPack) ? VertexMaterial : Resources.Load (PathCollect.defaultVoxelMaterial, typeof(Material)) as Material;
            #if UNITY_EDITOR
            newChunkObject.layer = LayerMask.NameToLayer ((EditorApplication.isPlaying) ? "Floor" : "Editor");
            #else
            newChunkObject.layer = LayerMask.NameToLayer("Floor");
            #endif
            Chunk newChunk = newChunkObject.GetComponent<Chunk> ();
            newChunk.cData = cData;
            newChunk.volume = this;
            newChunk.Init ();
            chunks.Add (chunkPos, newChunk);
        }

        void CreateFreeChunk ()
        {
            GameObject newChunkObject = Instantiate (ChunkPrefab);
            newChunkObject.name = "FreeChunk";
            newChunkObject.transform.parent = chunkRoot.transform;
            newChunkObject.transform.localPosition = Vector3.zero;
            newChunkObject.transform.localRotation = Quaternion.Euler (Vector3.zero);
            newChunkObject.GetComponent<Renderer> ().material = (Vm.UseArtPack) ? VertexMaterial : Resources.Load (PathCollect.defaultVoxelMaterial, typeof(Material)) as Material;
            #if UNITY_EDITOR
            newChunkObject.layer = LayerMask.NameToLayer ((EditorApplication.isPlaying) ? "Floor" : "Editor");
            #else
            newChunkObject.layer = LayerMask.NameToLayer("Floor");
            #endif
            freeChunk = newChunkObject.GetComponent<Chunk> ();
            freeChunk.cData = vd.freeChunk;
            freeChunk.volume = this;
            freeChunk.Init ();
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
            return nodes.ContainsKey (_volumePos) ? nodes [_volumePos].pieceRoot : null;
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

        bool RemoveNodeIfIsEmpty (WorldPos bPos)
        {
            BlockAir blockAir = GetBlock (bPos.x, bPos.y, bPos.z) as BlockAir;
            bool isEmpty = true;
            if (blockAir != null) {
                foreach (string p in blockAir.pieceNames) {
                    if (!String.IsNullOrEmpty (p)) {
                        isEmpty = false;
                        break;
                    }
                }
                if (isEmpty) {
                    if (nodes.ContainsKey (bPos)) {
                        UnityEngine.Object.DestroyImmediate (nodes [bPos].pieceRoot);
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
                }
            }
        }

        PaletteItem[] itemArray = new PaletteItem[0];
        GameObject itemRoot;
        Dictionary<BlockItem,GameObject> itemNodes = new Dictionary<BlockItem, GameObject> ();

        public GameObject GetItemNode (BlockItem blockItem)
        {
            GameObject obj;
            itemNodes.TryGetValue (blockItem, out obj);
            return obj;
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
            }
            return null;
        }

        public void SetBlock (int x, int y, int z, Block _block)
        {
            Chunk chunk = GetChunk (x, y, z);
            Block oldBlock = GetBlock (x, y, z);
            if (chunk != null) {
                WorldPos chunkBlockPos = new WorldPos (x - chunk.cData.ChunkPos.x, y - chunk.cData.ChunkPos.y, z - chunk.cData.ChunkPos.z);
                if (_block != null) {
                    _block.BlockPos = chunkBlockPos;
                    Predicate<BlockAir> sameBlockAir = b => b.BlockPos.Compare (chunkBlockPos);
                    switch (_block.GetType ().ToString ()) {
                    case "CreVox.BlockAir":
                        if (!chunk.cData.blockAirs.Exists (sameBlockAir)) {
                            chunk.cData.blockAirs.Add (_block as BlockAir);
                        }
                        break;
                    case "CreVox.BlockHold":
                        Predicate<BlockHold> sameBlockHold = b => b.BlockPos.Compare (chunkBlockPos);
                        if (!chunk.cData.blockHolds.Exists (sameBlockHold)) {
                            chunk.cData.blockHolds.Add (_block as BlockHold);
                        }
                        break;
                    case "CreVox.Block":
                        Predicate<Block> sameBlock = b => b.BlockPos.Compare (chunkBlockPos);
                        if (chunk.cData.blockAirs.Exists (sameBlockAir)) {
                            BlockAir ba = oldBlock as BlockAir;
                            for (int i = 0; i < 8; i++) {
                                PlacePiece (ba.BlockPos, new WorldPos (i % 3, 0, (i / 3)), null);
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
                if (_id > vd.blockItems.Count - 1)
                    return;
                
                blockItem = vd.blockItems [_id];
                UnityEngine.Object.DestroyImmediate (itemNodes [blockItem]);
                itemNodes.Remove (blockItem);
                vd.blockItems.RemoveAt (_id);
            }
            
        }

        void PlaceItems ()
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
            BlockAir blockAir;
            int _id = gPos.z * 3 + gPos.x;
            GameObject pObj;

            if (block != null && !(block is BlockAir))
                return;

            if (_piece != null) {
                if (_piece.GetComponent<PaletteItem> ().markType == PaletteItem.MarkerType.Item && _piece.name != "Missing")
                    return;
                
                if (block == null) {
                    SetBlock (bPos.x, bPos.y, bPos.z, new BlockAir ());
                    block = GetBlock (bPos.x, bPos.y, bPos.z);
                }

                if (!nodes.ContainsKey (bPos))
                    CreateNode (bPos);
                
                pObj = nodes [bPos].pieces [_id];
                if (pObj != null) {
                    PlaceBlockHold (bPos, _id, pObj.GetComponent<LevelPiece> (), true);
                    UnityEngine.Object.DestroyImmediate (pObj);
                }

                #if UNITY_EDITOR
                pObj = isNew ? PrefabUtility.InstantiatePrefab (_piece.gameObject) as GameObject : _piece.gameObject;
                #else
                pObj = GameObject.Instantiate(_piece.gameObject);
                #endif
                pObj.transform.parent = nodes [bPos].pieceRoot.transform;
                LevelPiece p = pObj.GetComponent<LevelPiece> ();
                if (p != null) {
                    p.block = block;
                }
                Vector3 pos = GetPieceOffset (gPos.x, gPos.z);
                float x = bPos.x * Vg.w + pos.x;
                float y = bPos.y * Vg.h + pos.y;
                float z = bPos.z * Vg.d + pos.z;
                pObj.transform.localPosition = new Vector3 (x, y, z);
                pObj.transform.localRotation = Quaternion.Euler (0, GetPieceAngle (gPos.x, gPos.z), 0);
                nodes [bPos].pieces [_id] = pObj;

                blockAir = block as BlockAir;
                if (blockAir != null) {
                    if (_piece.name != "Missing") {
                        blockAir.SetPiece (gPos, pObj.GetComponent<LevelPiece> ());
                        blockAir.SolidCheck (nodes [bPos].pieces);
                        SetBlock (bPos.x, bPos.y, bPos.z, blockAir);
                    } else {
                        pObj.GetComponentInChildren<TextMesh> ().text += ("\n" + blockAir.pieceNames [_id]);
                    }

                    if (_piece.isHold)
                        PlaceBlockHold (bPos, _id, pObj.GetComponent<LevelPiece> (), false);
                }
            } else {
                blockAir = block as BlockAir;
                if (blockAir != null) {
                    blockAir.SetPiece (gPos, null);
                    blockAir.SolidCheck (nodes [bPos].pieces);
                }

                if (nodes.ContainsKey (bPos)) {
                    pObj = nodes [bPos].pieces [_id];
                    if (pObj != null) {
                        PlaceBlockHold (bPos, _id, pObj.GetComponent<LevelPiece> (), true);
                        UnityEngine.Object.DestroyImmediate (pObj);
                    }
                }

                if (RemoveNodeIfIsEmpty (bPos))
                    SetBlock (bPos.x, bPos.y, bPos.z, null);
            }
        }

        #if UNITY_EDITOR
        public GameObject CopyPiece (WorldPos bPos, WorldPos gPos, bool a_cut)
        {
            int _id = gPos.z * 3 + gPos.x;
            GameObject pObj = null;

            if (nodes.ContainsKey (bPos) && nodes [bPos].pieces.Length > _id) {
                pObj = nodes [bPos].pieces [_id];

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
                        UnityEngine.Object.DestroyImmediate (nodes [bPos].pieces [_id]);
                        nodes [bPos].pieces [_id] = null;
                    }
                }
            }

            return pObj;
        }
        #endif

        static GameObject _missing;
        static LevelPiece _missingP;

        void PlacePieces ()
        {
            if (_missing == null) {
                _missing = Resources.Load (PathCollect.resourceSubPath + "Missing", typeof(GameObject)) as GameObject;
            }
            if (_missingP == null) {
                _missingP = _missing.GetComponent<LevelPiece> ();
            }
            foreach (Chunk c in Chunks.Values) {
                foreach (var ba in c.cData.blockAirs) {
                    for (int i = 0; i < ba.pieceNames.Length; i++) {
                        if (String.IsNullOrEmpty (ba.pieceNames [i]))
                            continue;
                        LevelPiece p = _missingP;
                        foreach (PaletteItem pi in itemArray) {
                            if (ba.pieceNames [i] != pi.name)
                                continue;
                            p = pi.GetComponent<LevelPiece> ();
                            break;
                        }
                        PlacePiece (
                            new WorldPos (c.cData.ChunkPos.x + ba.BlockPos.x, c.cData.ChunkPos.y + ba.BlockPos.y, c.cData.ChunkPos.z + ba.BlockPos.z),
                            new WorldPos (i % 3, 0, (i / 3)),
                            p
                        );
                    }
                }
            }
            BehaviorManager _bm = gameObject.GetComponentInParent<BehaviorManager> ();
            if (_bm) {
                BehaviorTree[] _tree = gameObject.GetComponentsInChildren<BehaviorTree> ();
                if (_tree.Length > 0) {
                    for (int i = 0; i < _tree.Length; i++) {
                        _tree [i].EnableBehavior ();
                        _bm.EnableBehavior (_tree [i]);
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

        static int GetBlockHoldIndex (int x, int y, int z, Chunk containerChunk)
        {
            WorldPos bPos = new WorldPos (x, y, z);
            Predicate <BlockHold> checkBlockPos = bh => bh.BlockPos.Compare (bPos);
            return (containerChunk != null) ? containerChunk.cData.blockHolds.FindIndex (checkBlockPos) : -1;
        }

        void PlaceBlockHold (WorldPos _bPos, int _id, LevelPiece _piece, bool _isErase)
        {
//            Debug.Log ("[" + _bPos.ToString () + "](" + _id.ToString () + ")-" + (_isErase?"Delete":"Add"));
            foreach (LevelPiece.Hold bh in _piece.holdBlocks) {
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

                    Predicate<BlockHold.piecePos> samePiecePos = obj => (obj.blockPos.Compare (bhData.blockPos) && obj.pieceID == bhData.pieceID);

                    int _index = GetBlockHoldIndex (x, y, z, _chunk);
                    BlockHold bhBlock = (_index > -1) ? _chunk.cData.blockHolds [_index] : null;
                    
                    if (_isErase) {
                        if (bhBlock != null) {
                            if (bhBlock.roots.Exists (samePiecePos))
                                bhBlock.roots.RemoveAt (bhBlock.roots.FindIndex (samePiecePos));
                            if (bhBlock.roots.Count == 0)
                                _chunk.cData.blockHolds.Remove (bhBlock);
                        }
                    } else {
                        if (bhBlock == null) {
                            bhBlock = new BlockHold (x, y, z);
                            bhBlock.roots.Add (bhData);
                            _chunk.cData.blockHolds.Add (bhBlock);
                        } else if (!bhBlock.roots.Exists (samePiecePos)) {
                            bhBlock.roots.Add (bhData);
                        }
                        
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
                Build ();
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
            string date = DateTime.Now.ToString ("yyyyMMdd") + "-" + DateTime.Now.ToString ("HHmmss");
            string backupPath = PathCollect.resourcesPath + PathCollect.save + "/_TempBackup/" + date + "_" + vdName;

            VolumeData vdBackup = ScriptableObject.CreateInstance<VolumeData> ();
            vdBackup.blockItems = vd.blockItems;
            vdBackup.chunkDatas = vd.chunkDatas;
            vdBackup.chunkX = vd.chunkX;
            vdBackup.chunkY = vd.chunkY;
            vdBackup.chunkZ = vd.chunkZ;
            AssetDatabase.CreateAsset (vdBackup, backupPath);
            AssetDatabase.Refresh ();
        }
        #endif
        #endregion

        #region Editor Scene UI

        public bool isLost {
            get { return (vd == null) || (!vd.useFreeChunk && chunks.Count == 0) || (vd.useFreeChunk && freeChunk == null); }
        }

        #if UNITY_EDITOR
        public Color YColor;
        public bool pointer;
        public int pointY;
        public bool cuter;
        public int cutY;
        public PaletteItem _itemInspected;

        void OnDrawGizmos ()
        {
            Matrix4x4 oldMatrix = Gizmos.matrix;
            Gizmos.matrix = transform.localToWorldMatrix;

            if (focusVolume == this && Vm.DebugRuler) {
                Gizmos.color = isLost ? Color.red : new Color (YColor.r, YColor.g, YColor.b, 0.4f);

                Vector3 center, size;
                if (vd == null) {
                    center = Vector3.zero;
                    size = new Vector3 (Vg.w, Vg.h, Vg.d);
                } else {
                    if (vd.useFreeChunk) {
                        WorldPos c = vd.freeChunk.freeChunkSize;
                        center = new Vector3 ((c.x - 1) * Vg.w / 2, (c.y - 1) * Vg.h / 2, (c.z - 1) * Vg.d / 2);
                        size = new Vector3 (c.x * Vg.w, c.y * Vg.h, c.z * Vg.d);
                    } else {
                        int s = vd.chunkSize;
                        center = new Vector3 ((vd.chunkX * s - 1) * Vg.w / 2, (vd.chunkY * s - 1) * Vg.h / 2, (vd.chunkZ * s - 1) * Vg.d / 2);
                        size = new Vector3 (vd.chunkX * s * Vg.w, vd.chunkY * s * Vg.h, vd.chunkZ * s * Vg.d);
                    }
                }
                Gizmos.DrawWireCube (center, size);

                if (vd != null) {
                    DrawGizmoLayer ();
                    DrawBlockItem ();
                }
            }

            if (Vm.ShowBlockHold)
                DrawBlockHold ();
            
            Gizmos.matrix = oldMatrix;
        }

        void DrawBlockHold ()
        {
            foreach (Chunk c in Chunks.Values) {
                if (c != null) {
                    for (int i = 0; i < c.cData.blockHolds.Count; i++) {
                        WorldPos blockHoldPos = c.cData.blockHolds [i].BlockPos;
                        WorldPos chunkPos = c.cData.ChunkPos;
                        Vector3 localPos = new Vector3 (
                                               (blockHoldPos.x + chunkPos.x) * Vg.w, 
                                               (blockHoldPos.y + chunkPos.y) * Vg.h, 
                                               (blockHoldPos.z + chunkPos.z) * Vg.d
                                           );
                        Gizmos.color = new Color (255f / 255f, 244f / 255f, 228f / 255f, 0.05f);
                        Gizmos.DrawCube (localPos, new Vector3 (Vg.w, Vg.h, Vg.d));
                    }
                }
            }
        }

        void DrawBlockItem ()
        {
            foreach (var item in vd.blockItems) {
                Vector3 localPos = new Vector3 (
                                       Mathf.Round (item.posX / Vg.w) * Vg.w, 
                                       Mathf.Round (item.posY / Vg.h) * Vg.h, 
                                       Mathf.Round (item.posZ / Vg.d) * Vg.d
                                   );
                Gizmos.color = new Color (0f / 255f, 202f / 255f, 255f / 255f, 0.3f);
                Gizmos.DrawCube (localPos, new Vector3 (Vg.w, Vg.h, Vg.d));
                Vector3 localPos2 = new Vector3 (
                                        item.BlockPos.x * Vg.w, 
                                        item.BlockPos.y * Vg.h, 
                                        item.BlockPos.z * Vg.d
                                    );
                Gizmos.DrawWireCube (localPos2, new Vector3 (Vg.w, Vg.h, Vg.d));
            }
        }

        void DrawGizmoLayer ()
        {
            if (!pointer)
                return;
            float wSize = (vd.useFreeChunk ? vd.freeChunk.freeChunkSize.x : vd.chunkX * vd.chunkSize);
            float dSize = (vd.useFreeChunk ? vd.freeChunk.freeChunkSize.z : vd.chunkZ * vd.chunkSize);
            Vector3 center = new Vector3 ((wSize - 1) * Vg.w / 2, pointY * Vg.h, (dSize - 1) * Vg.d / 2);
            Vector3 size = new Vector3 (wSize * Vg.w + 0.02f, Vg.h + 0.02f, dSize * Vg.d + 0.02f);
            Gizmos.DrawCube (center, size);
            for (int xi = 0; xi < wSize; xi++) {
                for (int zi = 0; zi < dSize; zi++) {
                    float cSize;
                    cSize = (GetBlock (xi, pointY, zi) == null) ? 0.1f : 1.0f;

                    Vector3 localPos = new Vector3 (xi * Vg.w, (pointY + 0.5f) * Vg.h, zi * Vg.d);
                    Gizmos.DrawWireCube (localPos, new Vector3 (Vg.w * cSize, Vg.h * cSize * 0, Vg.d * cSize));
                }
            }
        }

        public void ChangeCutY (int _y)
        {
            _y = Mathf.Clamp (_y, 0, (vd.useFreeChunk ? vd.freeChunk.freeChunkSize.x : (vd.chunkX * vd.chunkSize)) - 1);
            cutY = _y;
            UpdateChunks ();
        }

        #endif
        #endregion

        public List<ConnectionInfo> ConnectionInfos {
            get;
            set;
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
        public ConnectionInfo (WorldPos _position, Quaternion _rotation, ConnectionInfoType _type, string _name = "")
        {
            position = _position;
            rotation = _rotation;
            type = _type;
            connectionName = _name;
            used = false;
            connectedGameObject = null;
        }

        public ConnectionInfo (ConnectionInfo clone)
        {
            position = clone.position;
            rotation = clone.rotation;
            type = clone.type;
            connectionName = clone.connectionName;
            used = clone.used;
            connectedObjectGuid = clone.connectedObjectGuid;
            connectedGameObject = clone.connectedGameObject;
        }

        public ConnectionInfo Clone ()
        {
            return new ConnectionInfo (this);
        }

        public bool Compare (ConnectionInfo obj)
        {
            return (
                position.Compare (obj.position)
                && rotation == obj.rotation
                && type == obj.type
                && used == obj.used
                && connectedObjectGuid == obj.connectedObjectGuid
            );
        }

        public WorldPos RelativePosition (float degree)
        {
            int absoluteDegree = ((int)(degree + rotation.eulerAngles.y) % 360);
            return DirectionOffset [absoluteDegree / 90];
        }
        // Constant array.
        public static WorldPos[] DirectionOffset = {
            new WorldPos (0, 0, 1),
            new WorldPos (1, 0, 0),
            new WorldPos (0, 0, -1),
            new WorldPos (-1, 0, 0)
        };
    }
}