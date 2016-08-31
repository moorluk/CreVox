using UnityEngine;
using System.Collections;
using System;

namespace CreVox
{

    [Serializable]
    public class Block
    {
        const float tileSize = 0.25f;
        public const float w = 3f;
        public const float h = 2f;
        public const float d = 3f;
        public const float hw = 1.5f;
        public const float hh = 1f;
        public const float hd = 1.5f;

        public enum Direction
        {
            north,
            east,
            south,
            west,
            up,
            down

        }

        public struct Tile
        {
            public int x;
            public int y;

        }

        //Base block constructor
        public Block()
        {
        }

        public virtual void Destroy()
        {
        }

        public virtual MeshData Blockdata
         (Chunk chunk, int x, int y, int z, MeshData meshData)
        {
            meshData.useRenderDataForCol = true;

            if (chunk.GetBlock(x, y + 1, z) == null || !chunk.GetBlock(x, y + 1, z).IsSolid(Direction.down)) {
                meshData = FaceDataUp(chunk, x, y, z, meshData);
            }

            if (chunk.GetBlock(x, y - 1, z) == null || !chunk.GetBlock(x, y - 1, z).IsSolid(Direction.up)) {
                meshData = FaceDataDown(chunk, x, y, z, meshData);
            }

            if (chunk.GetBlock(x, y, z + 1) == null || !chunk.GetBlock(x, y, z + 1).IsSolid(Direction.south)) {
                meshData = FaceDataNorth(chunk, x, y, z, meshData);
            }

            if (chunk.GetBlock(x, y, z - 1) == null || !chunk.GetBlock(x, y, z - 1).IsSolid(Direction.north)) {
                meshData = FaceDataSouth(chunk, x, y, z, meshData);
            }

            if (chunk.GetBlock(x + 1, y, z) == null || !chunk.GetBlock(x + 1, y, z).IsSolid(Direction.west)) {
                meshData = FaceDataEast(chunk, x, y, z, meshData);
            }

            if (chunk.GetBlock(x - 1, y, z) == null || !chunk.GetBlock(x - 1, y, z).IsSolid(Direction.east)) {
                meshData = FaceDataWest(chunk, x, y, z, meshData);
            }

            return meshData;

        }

        public virtual bool IsSolid(Direction direction)
        {
            switch (direction) {
                case Direction.north:
                    return true;
                case Direction.east:
                    return true;
                case Direction.south:
                    return true;
                case Direction.west:
                    return true;
                case Direction.up:
                    return true;
                case Direction.down:
                    return true;
            }
            return false;
        }

        protected virtual MeshData FaceDataUp
         (Chunk chunk, int x, int y, int z, MeshData meshData)
        {
            meshData.AddVertex(new Vector3(x * w - hw, y * h + hh, z * d + hd));
            meshData.AddVertex(new Vector3(x * w + hw, y * h + hh, z * d + hd));
            meshData.AddVertex(new Vector3(x * w + hw, y * h + hh, z * d - hd));
            meshData.AddVertex(new Vector3(x * w - hw, y * h + hh, z * d - hd));
            meshData.AddQuadTriangles();
            //Add the following line to every FaceData function with the direction of the face
            meshData.uv.AddRange(FaceUVs(Direction.up));
            return meshData;
        }

        protected virtual MeshData FaceDataDown
     (Chunk chunk, int x, int y, int z, MeshData meshData)
        {
            meshData.AddVertex(new Vector3(x * w - hw, y * h - hh, z * d - hd));
            meshData.AddVertex(new Vector3(x * w + hw, y * h - hh, z * d - hd));
            meshData.AddVertex(new Vector3(x * w + hw, y * h - hh, z * d + hd));
            meshData.AddVertex(new Vector3(x * w - hw, y * h - hh, z * d + hd));

            meshData.AddQuadTriangles();
            //Add the following line to every FaceData function with the direction of the face
            meshData.uv.AddRange(FaceUVs(Direction.down));
            return meshData;
        }

        protected virtual MeshData FaceDataNorth
        (Chunk chunk, int x, int y, int z, MeshData meshData)
        {
            meshData.AddVertex(new Vector3(x * w + hw, y * h - hh, z * d + hd));
            meshData.AddVertex(new Vector3(x * w + hw, y * h + hh, z * d + hd));
            meshData.AddVertex(new Vector3(x * w - hw, y * h + hh, z * d + hd));
            meshData.AddVertex(new Vector3(x * w - hw, y * h - hh, z * d + hd));

            meshData.AddQuadTriangles();
            //Add the following line to every FaceData function with the direction of the face
            meshData.uv.AddRange(FaceUVs(Direction.north));
            return meshData;
        }

        protected virtual MeshData FaceDataEast
        (Chunk chunk, int x, int y, int z, MeshData meshData)
        {
            meshData.AddVertex(new Vector3(x * w + hw, y * h - hh, z * d - hd));
            meshData.AddVertex(new Vector3(x * w + hw, y * h + hh, z * d - hd));
            meshData.AddVertex(new Vector3(x * w + hw, y * h + hh, z * d + hd));
            meshData.AddVertex(new Vector3(x * w + hw, y * h - hh, z * d + hd));

            meshData.AddQuadTriangles();
            //Add the following line to every FaceData function with the direction of the face
            meshData.uv.AddRange(FaceUVs(Direction.east));
            return meshData;
        }

        protected virtual MeshData FaceDataSouth
        (Chunk chunk, int x, int y, int z, MeshData meshData)
        {
            meshData.AddVertex(new Vector3(x * w - hw, y * h - hh, z * d - hd));
            meshData.AddVertex(new Vector3(x * w - hw, y * h + hh, z * d - hd));
            meshData.AddVertex(new Vector3(x * w + hw, y * h + hh, z * d - hd));
            meshData.AddVertex(new Vector3(x * w + hw, y * h - hh, z * d - hd));

            meshData.AddQuadTriangles();
            //Add the following line to every FaceData function with the direction of the face
            meshData.uv.AddRange(FaceUVs(Direction.south));
            return meshData;
        }

        protected virtual MeshData FaceDataWest
        (Chunk chunk, int x, int y, int z, MeshData meshData)
        {
            meshData.AddVertex(new Vector3(x * w - hw, y * h - hh, z * d + hd));
            meshData.AddVertex(new Vector3(x * w - hw, y * h + hh, z * d + hd));
            meshData.AddVertex(new Vector3(x * w - hw, y * h + hh, z * d - hd));
            meshData.AddVertex(new Vector3(x * w - hw, y * h - hh, z * d - hd));

            meshData.AddQuadTriangles();
            //Add the following line to every FaceData function with the direction of the face
            meshData.uv.AddRange(FaceUVs(Direction.west));
            return meshData;
        }

        public virtual Vector2[] FaceUVs(Direction direction)
        {
            Vector2[] UVs = new Vector2[4];
            Tile tilePos = TexturePosition(direction);
            switch (direction) {
                default:
                    UVs[0] = new Vector2(tileSize * tilePos.x + tileSize * Block.w, tileSize * tilePos.y);
                    UVs[1] = new Vector2(tileSize * tilePos.x + tileSize * Block.w, tileSize * tilePos.y + tileSize * Block.h);
                    UVs[2] = new Vector2(tileSize * tilePos.x, tileSize * tilePos.y + tileSize * Block.h);
                    UVs[3] = new Vector2(tileSize * tilePos.x, tileSize * tilePos.y);
                    return UVs;

                case Direction.up:
                case Direction.down:
                    UVs[0] = new Vector2(tileSize * tilePos.x + tileSize * Block.w, tileSize * tilePos.y);
                    UVs[1] = new Vector2(tileSize * tilePos.x + tileSize * Block.w, tileSize * tilePos.y + tileSize * Block.d);
                    UVs[2] = new Vector2(tileSize * tilePos.x, tileSize * tilePos.y + tileSize * Block.d);
                    UVs[3] = new Vector2(tileSize * tilePos.x, tileSize * tilePos.y);
                    return UVs;
            }
        }

        public virtual Tile TexturePosition(Direction direction)
        {
            Tile tile = new Tile();
            tile.x = 1;
            tile.y = 1;
            return tile;
        }
    }
}