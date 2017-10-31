﻿using System.Collections.Generic;
using System;

namespace CreVox
{

    [Serializable]
    public class Save
    {
        public int chunkX = 1;
        public int chunkY = 1;
        public int chunkZ = 1;
        public Dictionary<WorldPos, Block> blocks = new Dictionary<WorldPos, Block> ();
        public List<BlockItem> blockItems;

        public Save ()
        {
        }

        public Save (Volume volume)
        {
            chunkX = volume.vd.chunkX;
            chunkY = volume.vd.chunkY;
            chunkZ = volume.vd.chunkZ;

            for (int x = 0; x < chunkX; x++) {
                for (int y = 0; y < chunkY; y++) {
                    for (int z = 0; z < chunkZ; z++) {
                        //Debug.Log ("Add chunk: " + x.ToString() + "," + y.ToString() + "," + z.ToString());
                        Chunk chunk = volume.GetChunk (x * Chunk.chunkSize, y * Chunk.chunkSize, z * Chunk.chunkSize);
                        if (chunk)
                            AddChunk (x, y, z, chunk);
                    }
                }
            }
            blockItems = volume.vd.blockItems;
        }

        public void AddChunk (int _x, int _y, int _z, Chunk chunk)
        {
            int cx = _x * Chunk.chunkSize;
            int cy = _y * Chunk.chunkSize;
            int cz = _z * Chunk.chunkSize;

            for (int x = 0; x < Chunk.chunkSize; x++) {
                for (int y = 0; y < Chunk.chunkSize; y++) {
                    for (int z = 0; z < Chunk.chunkSize; z++) {
                        Block block = chunk.GetBlock (x, y, z);

                        bool add = false;
                        if (block == null)
                            add = false;
                        else {
                            var bAir = block as BlockAir;
                            if (bAir != null) {
                                if (bAir.pieceNames != null)
                                    add = (bAir.pieceNames.Length > 0);
                            } else
                                add = true;
                        }

                        if (add) {
                            WorldPos pos = new WorldPos (cx + x, cy + y, cz + z);
//							Debug.Log ("Save: " + pos.ToString ());
                            blocks.Add (pos, block);
                        }
                    }
                }
            }
        }
    }
}