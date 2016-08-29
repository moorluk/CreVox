using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using CreVox;

[Serializable]
public class Save {
	public int chunkX;
	public int chunkY;
	public int chunkZ;

	public Dictionary<WorldPos, Block> blocks = new Dictionary<WorldPos, Block> ();

	public Save(World world){
		chunkX = world.chunkX;
		chunkY = world.chunkY;
		chunkZ = world.chunkZ;

		for (int x = 0; x < chunkX; x++) {
			for (int y = 0; y < chunkY; y++) {
				for (int z = 0; z < chunkZ; z++) {
					Debug.Log ("Add chunk: " + x.ToString() + "," + y.ToString() + "," + z.ToString());
					Chunk chunk = world.GetChunk (x* Chunk.chunkSize, y* Chunk.chunkSize, z* Chunk.chunkSize);
					AddChunk (x, y, z, chunk);
				}
			}
		}
	}

	public void AddChunk(int _x, int _y, int _z, Chunk chunk) {
		int cx = _x * Chunk.chunkSize;
		int cy = _y * Chunk.chunkSize;
		int cz = _z * Chunk.chunkSize;

		for (int x = 0; x < Chunk.chunkSize; x++) {
			for (int y = 0; y < Chunk.chunkSize; y++) {
				for (int z = 0; z < Chunk.chunkSize; z++) {
					global::Block block = (global::Block)chunk.blocks [x, y, z];
					bool add = false;
					if (block == null)
						add = false;

					if (block is BlockAir) {
						BlockAir bAir = (BlockAir)block;
						if (bAir.pieceNames != null)
							add = (bAir.pieceNames.Length > 0) ? true : false;
					} else
						add = true;

					if (add) {
						WorldPos pos = new WorldPos (cx + x, cy + y, cz + z);
						Debug.Log ("Save: " + pos.ToString ());
						blocks.Add (pos, block);
					}
				}
			}
		}
	}
}