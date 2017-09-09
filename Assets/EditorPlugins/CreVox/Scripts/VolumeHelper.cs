using UnityEngine;
using System.Collections.Generic;
using System.Linq;

namespace CreVox
{
    public class VolumeHelper
    {
        public static void Mirror(Volume a_v,
            WorldPos a_position, 
            WorldPos a_rotation, 
            WorldPos a_scale, 
            LevelPiece a_p, 
            bool a_mappingX,
            bool a_mappingZ)
        {
            WorldPos mPos = a_position;
            WorldPos mgPos = a_rotation;
            if (mPos.x == a_position.x && mPos.z == a_position.z)
            {
                a_v.PlacePiece(a_position, a_rotation, a_p);
            }
            mPos.z = a_scale.z * 9 - 1 - a_position.z;
            mgPos.z = 2 - a_rotation.z;
            if (a_mappingZ && mPos.x == a_position.x && mPos.z != a_position.z)
            {
                a_v.PlacePiece(mPos, mgPos, a_p);
            }
            mPos.x = a_scale.x * 9 - 1 - a_position.x;
            mgPos.x = 2 - a_rotation.x;
            if (a_mappingX && a_mappingZ && mPos.x != a_position.x && mPos.z != a_position.z)
            {
                a_v.PlacePiece(mPos, mgPos, a_p);
            }
            mPos.z = a_position.z;
            mgPos.z = a_rotation.z;
            if (a_mappingX && mPos.x != a_position.x && mPos.z == a_position.z)
            {
                a_v.PlacePiece(mPos, mgPos, a_p);
            }
        }

        public static void MirrorPosition(Volume a_v, 
            WorldPos a_position,
            WorldPos a_scale, 
            bool a_erase,
            bool a_mappingX,
            bool a_mappingZ)
        {
            WorldPos mPos = a_position;
            if (mPos.x == a_position.x && mPos.z == a_position.z)
            {
                a_v.SetBlock(mPos.x, mPos.y, mPos.z, a_erase ? null: new Block());
            }
            mPos.z = a_scale.z * 9 - 1 - a_position.z;
            if (a_mappingZ && mPos.x == a_position.x && mPos.z != a_position.z)
            {
                a_v.SetBlock(mPos.x, mPos.y, mPos.z, a_erase ? null : new Block());
            }
            mPos.x = a_scale.x * 9 - 1 - a_position.x;
            if (a_mappingX && a_mappingZ && mPos.x != a_position.x && mPos.z != a_position.z)
            {
                a_v.SetBlock(mPos.x, mPos.y, mPos.z, a_erase ? null : new Block());
            }
            mPos.z = a_position.z;
            if (a_mappingX && mPos.x != a_position.x && mPos.z == a_position.z)
            {
                a_v.SetBlock(mPos.x, mPos.y, mPos.z, a_erase ? null : new Block());
            }
        }

        public static void SelectedAdd(ref List<Vector3> a_blocks, Vector3 a_min, Vector3 a_max)
        {
            List<Vector3> added = new List<Vector3> ();
            for (int x = (int)a_min.x; x <= (int)a_max.x; ++x)
            {
                for (int y = (int)a_min.y; y <= (int)a_max.y; ++y)
                {
                    for (int z = (int)a_min.z; z <= (int)a_max.z; ++z)
                    {
                        added.Add( new Vector3 (x, y, z) );
                    }
                }
            }

            a_blocks = a_blocks.Union(added).ToList();
        }

        public static void SelectedRemove(ref List<Vector3> a_blocks, Vector3 a_min, Vector3 a_max)
        {
            int last = a_blocks.Count - 1;
            for (int i = last; i >= 0; --i)
            {
                if (a_blocks[i].x >= (int)a_min.x && a_blocks[i].y >= (int)a_min.y && a_blocks[i].z >= (int)a_min.z &&
                    a_blocks[i].x <= (int)a_max.x && a_blocks[i].y <= (int)a_max.y && a_blocks[i].z <= (int)a_max.z)
                {
                    a_blocks.RemoveAt(i);
                }
            }
        }
    }
}