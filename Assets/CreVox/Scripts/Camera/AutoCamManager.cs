using UnityEngine;
using System.Collections;

namespace CreVox
{
    public class AutoCamManager : MonoBehaviour
    {
        public enum CamZoneType
        {
            front,
            left,
            right,
            left_right,
            up,
            down,
        }

        public enum CamDirection
        {
            //none,
            front,
            back,
            left,
            right,
            turn_left,
            turn_right,
            turn_up,
            turn_down,
            turn_none,
        }

        public GameObject[] camZonePreset = new GameObject[(int)CamZoneType.down + 1];
        private GameObject[] camZones = new GameObject[3 * 3];

        public int[] obsLayer = new int[3 * 3];
        public int[] sclLayer = new int[3 * 3];
        public int[] adjLayer = new int[3 * 3];
        public int[] oldIDLayer = new int[3 * 3];
        public int[] idLayer = new int[3 * 3];
        public int[] camDir = new int[3 * 3];

        public Transform target;
        private GameObject camNode;
        CreVox.WorldPos oldPos;
        CreVox.WorldPos curPos;
        public CreVox.World world;

        // Use this for initialization
        void Start()
        {
            Debug.Log((1 << (int)CamDirection.left).ToString());
            world = GetComponent<CreVox.World>();
            target = GameObject.FindGameObjectWithTag("Player").transform;
            curPos = CreVox.EditTerrain.GetBlockPos(target.position);
            oldPos = curPos;

            //UpdateObstacleLayer ();
            //UpdateScrollLayer (99, 99);
            UpdateAdjecentLayer(1 << (int)CamDirection.front);
            for (int i = 0; i < sclLayer.Length; i++)
                sclLayer[i] = -1;
            for (int i = 0; i < idLayer.Length; i++)
                idLayer[i] = i;
            //CalcCamDir ();
            LoadPreset();
            InitCamZones();
            UpdateCamZones();
        }

        void LoadPreset()
        {
            camZonePreset[(int)CamZoneType.front] = Resources.Load(PathCollect.camSettingPath + "/AutoCamZone_F") as GameObject;
            camZonePreset[(int)CamZoneType.left] = Resources.Load(PathCollect.camSettingPath + "/AutoCamZone_L") as GameObject;
            camZonePreset[(int)CamZoneType.right] = Resources.Load(PathCollect.camSettingPath + "/AutoCamZone_R") as GameObject;
            camZonePreset[(int)CamZoneType.left_right] = Resources.Load(PathCollect.camSettingPath + "/AutoCamZone_LR") as GameObject;
            camZonePreset[(int)CamZoneType.up] = Resources.Load(PathCollect.camSettingPath + "/AutoCamZone_U") as GameObject;
            camZonePreset[(int)CamZoneType.down] = Resources.Load(PathCollect.camSettingPath + "/AutoCamZone_D") as GameObject;
        }

        void InitCamZones()
        {
            camNode = new GameObject();
            camNode.name = "CameraZones";
            camNode.transform.parent = this.transform;
            for (int i = 0; i < camZones.Length; i++)
            {
                float x = (i % 3 - 1) * Block.w;
                float y = 0f;
                float z = ((int)(i / 3) - 1) * Block.d;
                GameObject co = GameObject.Instantiate(camZonePreset[(int)CamZoneType.front], Vector3.zero, Quaternion.identity) as GameObject;
                co.name = "zone" + i.ToString();
                co.transform.localPosition = new Vector3(x, y, z);
                co.transform.parent = camNode.transform;
                camZones[i] = co;
            }
        }

        // Update is called once per frame
        void Update()
        {
            curPos = CreVox.EditTerrain.GetBlockPos(target.position);
            UpdateObstacleLayer();
            //if (!oldPos.Equals(curPos)) {
            int offsetX = curPos.x - oldPos.x;
            int offsetZ = curPos.z - oldPos.z;

            Debug.Log(curPos.ToString() + "; " + oldPos.ToString());

            if (offsetX != 0 || offsetZ != 0)
                UpdateScrollLayer(offsetX, offsetZ);
            UpdateAdjecentLayer(camDir[(1 + offsetX) + (1 + offsetZ) * 3]);
            CalcCamDir();

            if (offsetX != 0 || offsetZ != 0)
                UpdateIDLayer(offsetX, offsetZ);
            UpdateCamZones();
            oldPos = curPos;
            //}
        }


        void UpdateIDLayer(int _offsetX, int _offsetZ)
        {
            for (int i = 0; i < idLayer.Length; i++)
                oldIDLayer[i] = idLayer[i];

            for (int i = 0; i < sclLayer.Length; i++)
            {
                int x = (i % 3);
                int z = (int)(i / 3);

                x = (x + _offsetX + 3) % 3;
                z = (z + _offsetZ + 3) % 3;

                idLayer[i] = oldIDLayer[x + z * 3];
            }
        }

        void UpdateObstacleLayer()
        {
            for (int i = 0; i < obsLayer.Length; i++)
            {
                int dx = curPos.x + i % 3 - 1;
                int dz = curPos.z + (int)(i / 3) - 1;
                CreVox.BlockAir b = world.GetBlock(dx, curPos.y, dz) as CreVox.BlockAir;
                if (b != null)
                    obsLayer[i] = 1;
                else
                    obsLayer[i] = 0;
            }
        }

        void ResetAdjecentLayer(CamDirection _dir)
        {
            int v = 1 << (int)_dir;
            for (int i = 0; i < camDir.Length; i++)
            {
                adjLayer[i] = v;
            }
        }

        void UpdateScrollLayer(int _offsetX, int _offsetZ)
        {
            Debug.Log("offset: " + _offsetX.ToString() + "," + _offsetZ.ToString());
            for (int i = 0; i < sclLayer.Length; i++)
            {
                int x = (i % 3) + _offsetX;
                int z = (int)(i / 3) + _offsetZ;

                if (x >= 0 && z >= 0 && x < 3 && z < 3)
                {
                    sclLayer[i] = camDir[x + z * 3];
                }
                else
                    sclLayer[i] = -1;
            }
        }

        void UpdateAdjecentLayer(int _dir)
        {
            if ((_dir & (1 << (int)CamDirection.front)) != 0)
                UpdateFront();
            if ((_dir & (1 << (int)CamDirection.back)) != 0)
                UpdateBack();
            if ((_dir & (1 << (int)CamDirection.left)) != 0)
                UpdateLeft();
            if ((_dir & (1 << (int)CamDirection.right)) != 0)
                UpdateRight();
        }

        void CalcCamDir()
        {
            for (int i = 0; i < camDir.Length; i++)
            {
                if (obsLayer[i] == 0)
                    camDir[i] = 0;
                else if (sclLayer[i] != -1)
                    camDir[i] = sclLayer[i];
                else
                    camDir[i] = adjLayer[i];
            }
        }

        void UpdateCamZones()
        {
            //camNode.transform.position = new Vector3 (curPos.x * Block.w, curPos.y * Block.h, curPos.z * Block.d);
            for (int i = 0; i < idLayer.Length; i++)
            {
                int camID = idLayer[i];
                int dir = camDir[i];

                if (sclLayer[i] == -1)
                {
                    int l = 1 << (int)CamDirection.turn_left;
                    int r = 1 << (int)CamDirection.turn_right;
                    int x = i % 3;
                    int z = (int)(i / 3);
                    if (((dir & l) != 0) && ((dir & r) != 0))
                    {
                        CopyCameraZoneData(camID, CamZoneType.left_right);
                    }
                    else if ((dir & l) != 0)
                    {
                        CopyCameraZoneData(camID, CamZoneType.left);
                    }
                    else if ((dir & r) != 0)
                    {
                        CopyCameraZoneData(camID, CamZoneType.right);
                    }
                    else {
                        CopyCameraZoneData(camID, CamZoneType.front);
                    }
                    camZones[camID].transform.localRotation = Quaternion.Euler(0f, GetAngle(dir), 0f);
                    camZones[camID].transform.localPosition = new Vector3((curPos.x + x - 1) * Block.w, curPos.y * Block.h, (curPos.z + z - 1) * Block.d);
                }
            }
        }

        void UpdateFront()
        {
            ResetAdjecentLayer(CamDirection.front);
            Debug.Log("update front");
            CreVox.BlockAir b = null;
            b = world.GetBlock(curPos.x - 1, curPos.y, curPos.z) as CreVox.BlockAir;
            if (b == null)
            {
                if (obsLayer[0 + 2 * 3] != 0)
                {
                    adjLayer[0 + 2 * 3] = (1 << (int)CamDirection.left);
                    if (obsLayer[1 + 2 * 3] != 0)
                        adjLayer[1 + 2 * 3] |= (1 << (int)CamDirection.turn_left);
                }
            }
            b = world.GetBlock(curPos.x + 1, curPos.y, curPos.z) as CreVox.BlockAir;
            if (b == null)
            {
                if (obsLayer[2 + 2 * 3] != 0)
                {
                    adjLayer[2 + 2 * 3] = 1 << (int)CamDirection.right;
                    if (obsLayer[1 + 2 * 3] != 0)
                        adjLayer[1 + 2 * 3] |= 1 << (int)CamDirection.turn_right;
                }
            }

            b = world.GetBlock(curPos.x - 1, curPos.y, curPos.z - 1) as CreVox.BlockAir;
            if (b == null)
            {
                if (obsLayer[0 + 1 * 3] != 0)
                {
                    adjLayer[0 + 1 * 3] = 1 << (int)CamDirection.left;
                    if (obsLayer[1 + 1 * 3] != 0)
                        adjLayer[1 + 1 * 3] |= 1 << (int)CamDirection.turn_left;
                }
            }
            b = world.GetBlock(curPos.x + 1, curPos.y, curPos.z - 1) as CreVox.BlockAir;
            if (b == null)
            {
                if (obsLayer[2 + 1 * 3] != 0)
                {
                    adjLayer[2 + 1 * 3] = 1 << (int)CamDirection.right;
                    if (obsLayer[1 + 1 * 3] != 0)
                        adjLayer[1 + 1 * 3] |= 1 << (int)CamDirection.turn_right;
                }
            }

            b = world.GetBlock(curPos.x - 1, curPos.y, curPos.z - 2) as CreVox.BlockAir;
            if (b == null)
            {
                if (obsLayer[0 + 0 * 3] != 0)
                {
                    adjLayer[0 + 0 * 3] = 1 << (int)CamDirection.left;
                    if (obsLayer[1 + 0 * 3] != 0)
                        adjLayer[1 + 0 * 3] |= 1 << (int)CamDirection.turn_left;
                }
            }
            b = world.GetBlock(curPos.x + 1, curPos.y, curPos.z - 2) as CreVox.BlockAir;
            if (b == null)
            {
                if (obsLayer[2 + 0 * 3] != 0)
                {
                    adjLayer[2 + 0 * 3] = 1 << (int)CamDirection.right;
                    if (obsLayer[1 + 0 * 3] != 0)
                        adjLayer[1 + 0 * 3] |= 1 << (int)CamDirection.turn_right;
                }
            }

            b = world.GetBlock(curPos.x, curPos.y, curPos.z - 2) as CreVox.BlockAir;
            if (b == null)
            {
                if (obsLayer[1 + 0 * 3] != 0) adjLayer[1 + 0 * 3] = (1 << (int)CamDirection.turn_none) + (1 << (int)CamDirection.front);
            }
        }

        void UpdateBack()
        {
            ResetAdjecentLayer(CamDirection.back);
            CreVox.BlockAir b = null;
            b = world.GetBlock(curPos.x - 1, curPos.y, curPos.z) as CreVox.BlockAir;
            if (b == null)
            {
                if (obsLayer[0 + 0 * 3] != 0)
                {
                    adjLayer[0 + 0 * 3] = 1 << (int)CamDirection.left;
                    if (obsLayer[1 + 0 * 3] != 0)
                        adjLayer[1 + 0 * 3] |= 1 << (int)CamDirection.turn_right;
                }
            }
            b = world.GetBlock(curPos.x + 1, curPos.y, curPos.z) as CreVox.BlockAir;
            if (b == null)
            {
                if (obsLayer[2 + 0 * 3] != 0)
                {
                    adjLayer[2 + 0 * 3] = 1 << (int)CamDirection.right;
                    if (obsLayer[1 + 0 * 3] != 0)
                        adjLayer[1 + 0 * 3] |= 1 << (int)CamDirection.turn_left;
                }
            }

            b = world.GetBlock(curPos.x - 1, curPos.y, curPos.z + 1) as CreVox.BlockAir;
            if (b == null)
            {
                if (obsLayer[0 + 1 * 3] != 0)
                {
                    adjLayer[0 + 1 * 3] = 1 << (int)CamDirection.left;
                    if (obsLayer[1 + 1 * 3] != 0)
                        adjLayer[1 + 1 * 3] |= 1 << (int)CamDirection.turn_right;
                }
            }
            b = world.GetBlock(curPos.x + 1, curPos.y, curPos.z + 1) as CreVox.BlockAir;
            if (b == null)
            {
                if (obsLayer[2 + 1 * 3] != 0)
                {
                    adjLayer[2 + 1 * 3] = 1 << (int)CamDirection.right;
                    if (obsLayer[1 + 1 * 3] != 0)
                        adjLayer[1 + 1 * 3] |= 1 << (int)CamDirection.turn_left;
                }
            }

            b = world.GetBlock(curPos.x - 1, curPos.y, curPos.z + 2) as CreVox.BlockAir;
            if (b == null)
            {
                if (obsLayer[0 + 2 * 3] != 0)
                {
                    adjLayer[0 + 2 * 3] = 1 << (int)CamDirection.left;
                    if (obsLayer[1 + 2 * 3] != 0)
                        adjLayer[1 + 2 * 3] |= 1 << (int)CamDirection.turn_right;
                }
            }
            b = world.GetBlock(curPos.x + 1, curPos.y, curPos.z + 2) as CreVox.BlockAir;
            if (b == null)
            {
                if (obsLayer[2 + 2 * 3] != 0)
                {
                    adjLayer[2 + 2 * 3] = 1 << (int)CamDirection.right;
                    if (obsLayer[1 + 2 * 3] != 0)
                        adjLayer[1 + 2 * 3] |= 1 << (int)CamDirection.turn_left;
                }
            }

            b = world.GetBlock(curPos.x, curPos.y, curPos.z + 2) as CreVox.BlockAir;
            if (b == null)
            {
                if (obsLayer[1 + 2 * 3] != 0) adjLayer[1 + 2 * 3] = (1 << (int)CamDirection.turn_none) + (1 << (int)CamDirection.back);
            }
        }

        void UpdateRight()
        {
            ResetAdjecentLayer(CamDirection.right);
            CreVox.BlockAir b = null;
            b = world.GetBlock(curPos.x, curPos.y, curPos.z + 1) as CreVox.BlockAir;
            if (b == null)
            {
                if (obsLayer[2 + 2 * 3] != 0)
                {
                    adjLayer[2 + 2 * 3] = 1 << (int)CamDirection.front;
                    if (obsLayer[2 + 1 * 3] != 0)
                        adjLayer[2 + 1 * 3] |= 1 << (int)CamDirection.turn_left;
                }
            }
            b = world.GetBlock(curPos.x, curPos.y, curPos.z - 1) as CreVox.BlockAir;
            if (b == null)
            {
                if (obsLayer[2 + 0 * 3] != 0)
                {
                    adjLayer[2 + 0 * 3] = 1 << (int)CamDirection.back;
                    if (obsLayer[2 + 1 * 3] != 0)
                        adjLayer[2 + 1 * 3] |= 1 << (int)CamDirection.turn_right;
                }
            }

            b = world.GetBlock(curPos.x - 1, curPos.y, curPos.z + 1) as CreVox.BlockAir;
            if (b == null)
            {
                if (obsLayer[1 + 2 * 3] != 0)
                {
                    adjLayer[1 + 2 * 3] = 1 << (int)CamDirection.front;
                    if (obsLayer[1 + 1 * 3] != 0)
                        adjLayer[1 + 1 * 3] |= 1 << (int)CamDirection.turn_left;
                }
            }
            b = world.GetBlock(curPos.x - 1, curPos.y, curPos.z - 1) as CreVox.BlockAir;
            if (b == null)
            {
                if (obsLayer[1 + 0 * 3] != 0)
                {
                    adjLayer[1 + 0 * 3] = 1 << (int)CamDirection.back;
                    if (obsLayer[1 + 1 * 3] != 0)
                        adjLayer[1 + 1 * 3] |= 1 << (int)CamDirection.turn_right;
                }
            }

            b = world.GetBlock(curPos.x - 2, curPos.y, curPos.z + 1) as CreVox.BlockAir;
            if (b == null)
            {
                if (obsLayer[0 + 2 * 3] != 0)
                {
                    adjLayer[0 + 2 * 3] = 1 << (int)CamDirection.front;
                    if (obsLayer[0 + 1 * 3] != 0)
                        adjLayer[0 + 1 * 3] |= 1 << (int)CamDirection.turn_left;
                }
            }
            b = world.GetBlock(curPos.x - 2, curPos.y, curPos.z - 1) as CreVox.BlockAir;
            if (b == null)
            {
                if (obsLayer[0 + 0 * 3] != 0)
                {
                    adjLayer[0 + 0 * 3] = 1 << (int)CamDirection.back;
                    if (obsLayer[0 + 1 * 3] != 0)
                        adjLayer[0 + 1 * 3] |= 1 << (int)CamDirection.turn_right;
                }
            }

            b = world.GetBlock(curPos.x - 2, curPos.y, curPos.z) as CreVox.BlockAir;
            if (b == null)
            {
                if (obsLayer[0 + 1 * 3] != 0) adjLayer[0 + 1 * 3] = (1 << (int)CamDirection.turn_none) + (1 << (int)CamDirection.right);
            }
        }

        void UpdateLeft()
        {
            ResetAdjecentLayer(CamDirection.left);
            CreVox.BlockAir b = null;
            b = world.GetBlock(curPos.x, curPos.y, curPos.z + 1) as CreVox.BlockAir;
            if (b == null)
            {
                if (obsLayer[0 + 2 * 3] != 0)
                {
                    adjLayer[0 + 2 * 3] = 1 << (int)CamDirection.front;
                    if (obsLayer[0 + 1 * 3] != 0)
                        adjLayer[0 + 1 * 3] |= 1 << (int)CamDirection.turn_right;
                }
            }
            b = world.GetBlock(curPos.x, curPos.y, curPos.z - 1) as CreVox.BlockAir;
            if (b == null)
            {
                if (obsLayer[0 + 0 * 3] != 0)
                {
                    adjLayer[0 + 0 * 3] = 1 << (int)CamDirection.back;
                    if (obsLayer[0 + 1 * 3] != 0)
                        adjLayer[0 + 1 * 3] |= 1 << (int)CamDirection.turn_left;
                }
            }

            b = world.GetBlock(curPos.x + 1, curPos.y, curPos.z + 1) as CreVox.BlockAir;
            if (b == null)
            {
                if (obsLayer[1 + 2 * 3] != 0)
                {
                    adjLayer[1 + 2 * 3] = 1 << (int)CamDirection.front;
                    if (obsLayer[1 + 1 * 3] != 0)
                        adjLayer[1 + 1 * 3] |= 1 << (int)CamDirection.turn_right;
                }
            }
            b = world.GetBlock(curPos.x + 1, curPos.y, curPos.z - 1) as CreVox.BlockAir;
            if (b == null)
            {
                if (obsLayer[1 + 0 * 3] != 0)
                {
                    adjLayer[1 + 0 * 3] = 1 << (int)CamDirection.back;
                    if (obsLayer[1 + 1 * 3] != 0)
                        adjLayer[1 + 1 * 3] |= 1 << (int)CamDirection.turn_left;
                }
            }

            b = world.GetBlock(curPos.x + 2, curPos.y, curPos.z + 1) as CreVox.BlockAir;
            if (b == null)
            {
                if (obsLayer[2 + 2 * 3] != 0)
                {
                    adjLayer[2 + 2 * 3] = 1 << (int)CamDirection.front;
                    if (obsLayer[2 + 1 * 3] != 0)
                        adjLayer[2 + 1 * 3] |= 1 << (int)CamDirection.turn_right;
                }
            }
            b = world.GetBlock(curPos.x + 2, curPos.y, curPos.z - 1) as CreVox.BlockAir;
            if (b == null)
            {
                if (obsLayer[2 + 0 * 3] != 0)
                {
                    adjLayer[2 + 0 * 3] = 1 << (int)CamDirection.back;
                    if (obsLayer[2 + 1 * 3] != 0)
                        adjLayer[2 + 1 * 3] |= 1 << (int)CamDirection.turn_left;
                }
            }

            b = world.GetBlock(curPos.x + 2, curPos.y, curPos.z) as CreVox.BlockAir;
            if (b == null)
            {
                if (obsLayer[2 + 1 * 3] != 0) adjLayer[2 + 1 * 3] = (1 << (int)CamDirection.turn_none) + (1 << (int)CamDirection.left);
            }
        }

        float GetAngle(int _dir)
        {
            if ((_dir & 1 << (int)CamDirection.front) != 0)
            {
                return 0f;
            }
            if ((_dir & 1 << (int)CamDirection.back) != 0)
            {
                return 180f;
            }
            if ((_dir & 1 << (int)CamDirection.right) != 0)
            {
                return 90f;
            }
            if ((_dir & 1 << (int)CamDirection.left) != 0)
            {
                return 270f;
            }

            return 0f;
        }

        void OnDrawGizmos()
        {
            Vector3 pos = new Vector3(curPos.x * Block.w, curPos.y * Block.h, curPos.z * Block.d);
            Color oldColor = Gizmos.color;
            Gizmos.DrawWireCube(pos, new Vector3(Block.w, Block.h, Block.d));
            Gizmos.color = Color.red;
            for (int i = 0; i < camDir.Length; i++)
            {
                CreVox.WorldPos wPos;
                wPos.x = curPos.x + i % 3 - 1;
                wPos.y = curPos.y;
                wPos.z = curPos.z + (int)(i / 3) - 1;
                DrawCamDir(wPos, camDir[i]);
            }
            Gizmos.color = oldColor;
        }

        void DrawCamDir(CreVox.WorldPos _pos, int _dir)
        {
            Vector3 v = new Vector3(_pos.x * Block.w, _pos.y * Block.h, _pos.z * Block.d);
            Gizmos.DrawCube(v, Vector3.one * 0.1f);
            if ((_dir & 1 << (int)CamDirection.front) != 0)
            {
                Gizmos.DrawLine(v, v + Vector3.forward * 1f);
            }
            if ((_dir & 1 << (int)CamDirection.back) != 0)
            {
                Gizmos.DrawLine(v, v - Vector3.forward * 1f);
            }
            if ((_dir & 1 << (int)CamDirection.right) != 0)
            {
                Gizmos.DrawLine(v, v + Vector3.right * 1f);
            }
            if ((_dir & 1 << (int)CamDirection.left) != 0)
            {
                Gizmos.DrawLine(v, v - Vector3.right * 1f);
            }
        }

        void CopyCameraZoneData(int _id, CamZoneType _type)
        {
            GameObject fGO = camZonePreset[(int)_type];
            GameObject tGO = camZones[_id];

            //camerazone setting
            DynamicCameraZone fZone = fGO.GetComponent<DynamicCameraZone>();
            DynamicCameraZone tZone = tGO.GetComponent<DynamicCameraZone>();
            tZone.curve = fZone.curve;
            tZone.blendTime = fZone.blendTime;

            //BoxCollider fBCol = fGO.GetComponent<BoxCollider> ();
            //BoxCollider tBCol = tGO.GetComponent<BoxCollider> ();
            GameObject fDCamGO = fGO.transform.GetChild(0).gameObject;
            GameObject tDCamGO = tGO.transform.GetChild(0).gameObject;
            DynamicCamera fDCam = fDCamGO.GetComponent<DynamicCamera>();
            DynamicCamera tDCam = tDCamGO.GetComponent<DynamicCamera>();
            Camera fCam = fDCamGO.GetComponent<Camera>();
            Camera tCam = tDCamGO.GetComponent<Camera>();

            //collider
            //tBCol.size = fBCol.size;

            //dynamiccamera
            //cam pos
            tDCamGO.transform.localPosition = fDCamGO.transform.localPosition;
            tDCamGO.transform.localRotation = fDCamGO.transform.localRotation;

            //cam angle Y/P
            tDCam.minAngleP = fDCam.minAngleP;
            tDCam.maxAngleP = fDCam.maxAngleP;
            tDCam.minAngleY = fDCam.minAngleY;
            tDCam.maxAngleY = fDCam.maxAngleY;

            //dist min/max
            tDCam.minDist = fDCam.minDist;
            tDCam.maxDist = fDCam.maxDist;

            //safe frame
            tDCam.minSafeFrameHorizon = fDCam.minSafeFrameHorizon;
            tDCam.maxSafeFrameHorizon = fDCam.maxSafeFrameHorizon;
            tDCam.minSafeFrameVertical = fDCam.minSafeFrameHorizon;
            tDCam.maxSafeFrameVertical = fDCam.maxSafeFrameVertical;

            //cam fov
            tCam.fieldOfView = fCam.fieldOfView;
        }
    }
}