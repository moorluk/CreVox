using UnityEngine;
using System.Collections;
using System;

namespace CreVox
{

	[Serializable]
	public struct WorldPos
	{
		public int x, y, z;

		public WorldPos(int x, int y, int z)
		{
			this.x = x;
			this.y = y;
			this.z = z;
		}
		public WorldPos(Vector3 v3) {
			this.x = (int) v3.x;
			this.y = (int) v3.y;
			this.z = (int) v3.z;
		}
		public Vector3 ToVector3() {
			return new Vector3(this.x, this.y, this.z);
		}
		//Add this function:
		public override bool Equals(object obj)
		{
			if (GetHashCode() == obj.GetHashCode())
				return true;

			return false;
		}

		public bool Compare(WorldPos _pos)
		{
			if (_pos.x == this.x && _pos.y == this.y && _pos.z == this.z)
				return true;
			else
				return false;
		}

		public override int GetHashCode()
		{
			unchecked {
				int hash = 47;

				hash = hash * 227 + x.GetHashCode();
				hash = hash * 227 + y.GetHashCode();
				hash = hash * 227 + z.GetHashCode();

				return hash;
			}
		}

		public override string ToString()
		{
			return (x.ToString() + ", " + y.ToString() + ", " + z.ToString());
		}
		// [XAOCX Add]
		public static WorldPos operator +(WorldPos pos1, WorldPos pos2) {
			return new WorldPos(pos1.x + pos2.x, pos1.y + pos2.y, pos1.z + pos2.z);
		}
		public static WorldPos operator -(WorldPos pos1, WorldPos pos2) {
			return new WorldPos(pos1.x - pos2.x, pos1.y - pos2.y, pos1.z - pos2.z);
		}
	}
}