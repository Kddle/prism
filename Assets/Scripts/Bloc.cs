using UnityEngine;
using Newtonsoft.Json;

public struct BlocData
{
    [JsonProperty("isSolid")]
    public int IsSolid;

    [JsonProperty("upTextureCoordinates")]
    public Vector2Int UpTextureCoordinates;

    [JsonProperty("downTextureCoordinates")]
    public Vector2Int DownTextureCoordinates;

    [JsonProperty("restTextureCoordinates")]
    public Vector2Int RestTextureCoordinates;
}