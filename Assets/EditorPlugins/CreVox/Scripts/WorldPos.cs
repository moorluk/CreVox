using UnityEngine;
using System;

namespace CreVox
{

    [Serializable]
    public struct WorldPos
    {
        public int x, y, z;

        public WorldPos (int x, int y, int z)
        {
            this.x = x;
            this.y = y;
            this.z = z;
        }
        //Add this function:
        public override bool Equals (object obj)
        {
            return GetHashCode () == obj.GetHashCode ();
        }

        public bool Compare (WorldPos _pos)
        {
            return _pos.x == x && _pos.y == y && _pos.z == z;
        }

        public override int GetHashCode ()
        {
            unchecked {
                int hash = 47;

                hash = hash * 227 + x.GetHashCode ();
                hash = hash * 227 + y.GetHashCode ();
                hash = hash * 227 + z.GetHashCode ();

                return hash;
            }
        }

        public override string ToString ()
        {
            return (x + ", " + y + ", " + z);
        }
        // [XAOCX Add]
        public static WorldPos operator + (WorldPos pos1, WorldPos pos2)
        {
            return new WorldPos (pos1.x + pos2.x, pos1.y + pos2.y, pos1.z + pos2.z);
        }

        public static WorldPos operator - (WorldPos pos1, WorldPos pos2)
        {
            return new WorldPos (pos1.x - pos2.x, pos1.y - pos2.y, pos1.z - pos2.z);
        }

        public WorldPos (Vector3 v3)
        {
            x = (int)v3.x;
            y = (int)v3.y;
            z = (int)v3.z;
        }

        public Vector3 ToVector3 ()
        {
            return new Vector3 (x, y, z);
        }

        public Vector3 ToRealPosition ()
        {
            return Vector3.Scale (ToVector3 (), new Vector3 (3, 2, 3));
        }

        public float Distance (WorldPos another)
        {
            return Vector3.Distance (ToVector3 (), another.ToVector3 ());
        }
        //===============
    }
}