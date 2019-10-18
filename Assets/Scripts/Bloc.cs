using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Prism.Map;
using Newtonsoft.Json;
using Prism;

public class BlocData
{
    [JsonProperty("id")]
    public byte Id { get; set; }
    [JsonProperty("isSolid")]
    public bool IsSolid { get; set; }
    [JsonProperty("isVisible")]
    public bool IsVisible { get; set; }
    [JsonProperty("textureData")]
    public byte[] TextureData { get; set; }

    public BlocType BlocType => (BlocType)Id;
}

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

    public virtual MeshData FillData(Prism.Map.Chunk chunk, int x, int y, int z, MeshData meshData, List<Bloc> definitions, float blocSize)
    {
        if (!isVisible)
            return meshData;

        if (!definitions[chunk.GetBloc(x, y + 1, z)].IsSolid(Direction.D))
        {
            meshData = FaceDataUp((x * blocSize), (y * blocSize), (z * blocSize), meshData, blocSize / 2f);
        }

        if (!definitions[chunk.GetBloc(x, y - 1, z)].IsSolid(Direction.U))
        {
            meshData = FaceDataDown((x * blocSize), (y * blocSize), (z * blocSize), meshData, blocSize / 2f);
        }

        if (!definitions[chunk.GetBloc(x, y, z + 1)].IsSolid(Direction.S))
        {
            meshData = FaceDataNorth((x * blocSize), (y * blocSize), (z * blocSize), meshData, blocSize / 2f);
        }

        if (!definitions[chunk.GetBloc(x, y, z - 1)].IsSolid(Direction.N))
        {
            meshData = FaceDataSouth((x * blocSize), (y * blocSize), (z * blocSize), meshData, blocSize / 2f);
        }

        if (!definitions[chunk.GetBloc(x + 1, y, z)].IsSolid(Direction.W))
        {
            meshData = FaceDataEast((x * blocSize), (y * blocSize), (z * blocSize), meshData, blocSize / 2f);
        }

        if (!definitions[chunk.GetBloc(x - 1, y, z)].IsSolid(Direction.E))
        {
            meshData = FaceDataWest((x * blocSize), (y * blocSize), (z * blocSize), meshData, blocSize / 2f);
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
         (float x, float y, float z, MeshData meshData, float radius)
    {
        meshData.vertices.Add(new Vector3(x - radius, y + radius, z + radius));
        meshData.vertices.Add(new Vector3(x + radius, y + radius, z + radius));
        meshData.vertices.Add(new Vector3(x + radius, y + radius, z - radius));
        meshData.vertices.Add(new Vector3(x - radius, y + radius, z - radius));

        meshData.AddQuadTriangles();

        meshData.uv.AddRange(FaceUVs(Direction.U));

        return meshData;
    }

    protected virtual MeshData FaceDataDown
        (float x, float y, float z, MeshData meshData, float radius)
    {
        meshData.vertices.Add(new Vector3(x - radius, y - radius, z - radius));
        meshData.vertices.Add(new Vector3(x + radius, y - radius, z - radius));
        meshData.vertices.Add(new Vector3(x + radius, y - radius, z + radius));
        meshData.vertices.Add(new Vector3(x - radius, y - radius, z + radius));

        meshData.AddQuadTriangles();

        meshData.uv.AddRange(FaceUVs(Direction.D));
        return meshData;
    }

    protected virtual MeshData FaceDataNorth
        (float x, float y, float z, MeshData meshData, float radius)
    {
        meshData.vertices.Add(new Vector3(x + radius, y - radius, z + radius));
        meshData.vertices.Add(new Vector3(x + radius, y + radius, z + radius));
        meshData.vertices.Add(new Vector3(x - radius, y + radius, z + radius));
        meshData.vertices.Add(new Vector3(x - radius, y - radius, z + radius));

        meshData.AddQuadTriangles();
        meshData.uv.AddRange(FaceUVs(Direction.N));
        return meshData;
    }

    protected virtual MeshData FaceDataEast
        (float x, float y, float z, MeshData meshData, float radius)
    {
        meshData.vertices.Add(new Vector3(x + radius, y - radius, z - radius));
        meshData.vertices.Add(new Vector3(x + radius, y + radius, z - radius));
        meshData.vertices.Add(new Vector3(x + radius, y + radius, z + radius));
        meshData.vertices.Add(new Vector3(x + radius, y - radius, z + radius));

        meshData.AddQuadTriangles();
        meshData.uv.AddRange(FaceUVs(Direction.E));
        return meshData;
    }

    protected virtual MeshData FaceDataSouth
        (float x, float y, float z, MeshData meshData, float radius)
    {
        meshData.vertices.Add(new Vector3(x - radius, y - radius, z - radius));
        meshData.vertices.Add(new Vector3(x - radius, y + radius, z - radius));
        meshData.vertices.Add(new Vector3(x + radius, y + radius, z - radius));
        meshData.vertices.Add(new Vector3(x + radius, y - radius, z - radius));

        meshData.AddQuadTriangles();
        meshData.uv.AddRange(FaceUVs(Direction.S));
        return meshData;
    }

    protected virtual MeshData FaceDataWest
        (float x, float y, float z, MeshData meshData, float radius)
    {
        meshData.vertices.Add(new Vector3(x - radius, y - radius, z + radius));
        meshData.vertices.Add(new Vector3(x - radius, y + radius, z + radius));
        meshData.vertices.Add(new Vector3(x - radius, y + radius, z - radius));
        meshData.vertices.Add(new Vector3(x - radius, y - radius, z - radius));

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