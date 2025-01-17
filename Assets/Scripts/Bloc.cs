﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

[CreateAssetMenu(fileName = "NewBloc", menuName = "Blocs/Bloc")]
public class Bloc : ScriptableObject
{
    const float tileSize = 0.25f;
    public byte id = 1;
    public bool isVisible = true;

    public Vector2Int TextureTilePosition;

    public Bloc(Bloc bloc)
    {
        id = bloc.id;
        isVisible = bloc.isVisible;
        TextureTilePosition = bloc.TextureTilePosition;
    }

    public virtual MeshData FillData(Chunk chunk, int x, int y, int z, MeshData meshData, List<Bloc> definitions)
    {
        if (!isVisible)
            return meshData;

        if (!definitions[chunk.GetBlock(x, y + 1, z)].IsSolid(Direction.D))
        {
            meshData = FaceDataUp(chunk, x, y, z, meshData);
        }

        if (!definitions[chunk.GetBlock(x, y - 1, z)].IsSolid(Direction.U))
        {
            meshData = FaceDataDown(chunk, x, y, z, meshData);
        }

        if (!definitions[chunk.GetBlock(x, y, z + 1)].IsSolid(Direction.S))
        {
            meshData = FaceDataNorth(chunk, x, y, z, meshData);
        }

        if (!definitions[chunk.GetBlock(x, y, z - 1)].IsSolid(Direction.N))
        {
            meshData = FaceDataSouth(chunk, x, y, z, meshData);
        }

        if (!definitions[chunk.GetBlock(x + 1, y, z)].IsSolid(Direction.W))
        {
            meshData = FaceDataEast(chunk, x, y, z, meshData);
        }

        if (!definitions[chunk.GetBlock(x - 1, y, z)].IsSolid(Direction.E))
        {
            meshData = FaceDataWest(chunk, x, y, z, meshData);
        }

        return meshData;
    }

    public virtual bool IsSolid(Direction direction)
    {
        if (!isVisible)
            return false;

        switch (direction)
        {
            case Direction.N:
                return true;
            case Direction.E:
                return true;
            case Direction.S:
                return true;
            case Direction.W:
                return true;
            case Direction.U:
                return true;
            case Direction.D:
                return true;
        }

        return false;
    }

    protected virtual MeshData FaceDataUp
         (Chunk chunk, int x, int y, int z, MeshData meshData)
    {
        meshData.vertices.Add(new Vector3(x - 0.5f, y + 0.5f, z + 0.5f));
        meshData.vertices.Add(new Vector3(x + 0.5f, y + 0.5f, z + 0.5f));
        meshData.vertices.Add(new Vector3(x + 0.5f, y + 0.5f, z - 0.5f));
        meshData.vertices.Add(new Vector3(x - 0.5f, y + 0.5f, z - 0.5f));

        meshData.AddQuadTriangles();

        meshData.uv.AddRange(FaceUVs(Direction.U));

        return meshData;
    }

    protected virtual MeshData FaceDataDown
        (Chunk chunk, int x, int y, int z, MeshData meshData)
    {
        meshData.vertices.Add(new Vector3(x - 0.5f, y - 0.5f, z - 0.5f));
        meshData.vertices.Add(new Vector3(x + 0.5f, y - 0.5f, z - 0.5f));
        meshData.vertices.Add(new Vector3(x + 0.5f, y - 0.5f, z + 0.5f));
        meshData.vertices.Add(new Vector3(x - 0.5f, y - 0.5f, z + 0.5f));

        meshData.AddQuadTriangles();

        meshData.uv.AddRange(FaceUVs(Direction.D));
        return meshData;
    }

    protected virtual MeshData FaceDataNorth
        (Chunk chunk, int x, int y, int z, MeshData meshData)
    {
        meshData.vertices.Add(new Vector3(x + 0.5f, y - 0.5f, z + 0.5f));
        meshData.vertices.Add(new Vector3(x + 0.5f, y + 0.5f, z + 0.5f));
        meshData.vertices.Add(new Vector3(x - 0.5f, y + 0.5f, z + 0.5f));
        meshData.vertices.Add(new Vector3(x - 0.5f, y - 0.5f, z + 0.5f));

        meshData.AddQuadTriangles();
        meshData.uv.AddRange(FaceUVs(Direction.N));
        return meshData;
    }

    protected virtual MeshData FaceDataEast
        (Chunk chunk, int x, int y, int z, MeshData meshData)
    {
        meshData.vertices.Add(new Vector3(x + 0.5f, y - 0.5f, z - 0.5f));
        meshData.vertices.Add(new Vector3(x + 0.5f, y + 0.5f, z - 0.5f));
        meshData.vertices.Add(new Vector3(x + 0.5f, y + 0.5f, z + 0.5f));
        meshData.vertices.Add(new Vector3(x + 0.5f, y - 0.5f, z + 0.5f));

        meshData.AddQuadTriangles();
        meshData.uv.AddRange(FaceUVs(Direction.E));
        return meshData;
    }

    protected virtual MeshData FaceDataSouth
        (Chunk chunk, int x, int y, int z, MeshData meshData)
    {
        meshData.vertices.Add(new Vector3(x - 0.5f, y - 0.5f, z - 0.5f));
        meshData.vertices.Add(new Vector3(x - 0.5f, y + 0.5f, z - 0.5f));
        meshData.vertices.Add(new Vector3(x + 0.5f, y + 0.5f, z - 0.5f));
        meshData.vertices.Add(new Vector3(x + 0.5f, y - 0.5f, z - 0.5f));

        meshData.AddQuadTriangles();
        meshData.uv.AddRange(FaceUVs(Direction.S));
        return meshData;
    }

    protected virtual MeshData FaceDataWest
        (Chunk chunk, int x, int y, int z, MeshData meshData)
    {
        meshData.vertices.Add(new Vector3(x - 0.5f, y - 0.5f, z + 0.5f));
        meshData.vertices.Add(new Vector3(x - 0.5f, y + 0.5f, z + 0.5f));
        meshData.vertices.Add(new Vector3(x - 0.5f, y + 0.5f, z - 0.5f));
        meshData.vertices.Add(new Vector3(x - 0.5f, y - 0.5f, z - 0.5f));

        meshData.AddQuadTriangles();
        meshData.uv.AddRange(FaceUVs(Direction.W));
        return meshData;
    }

    public virtual Tile TexturePosition(Direction direction)
    {
        if (TextureTilePosition == null)
            TextureTilePosition = Vector2Int.zero;

        Tile tile = new Tile();
        tile.x = TextureTilePosition.x;
        tile.y = TextureTilePosition.y;

        return tile;
    }

    public virtual Vector2[] FaceUVs(Direction direction)
    {
        Vector2[] uvs = new Vector2[4];
        Tile tilePos = TexturePosition(direction);

        uvs[0] = new Vector2(tileSize * tilePos.x + tileSize, tileSize * tilePos.y);
        uvs[1] = new Vector2(tileSize * tilePos.x + tileSize, tileSize * tilePos.y + tileSize);
        uvs[2] = new Vector2(tileSize * tilePos.x, tileSize * tilePos.y + tileSize);
        uvs[3] = new Vector2(tileSize * tilePos.x, tileSize * tilePos.y);

        return uvs;
    }
}