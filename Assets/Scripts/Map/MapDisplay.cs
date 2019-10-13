using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public class MapDisplay : MonoBehaviour
{
    public Renderer TextureRenderer;
    public void DrawNoiseMap(float[,] noiseMap)
    {
        int width = noiseMap.GetLength(0);
        int height = noiseMap.GetLength(1);

        Texture2D texture = new Texture2D(width, height);

        Color[] colorMap = new Color[width * height];
        for(int x = 0; x < width; x++)
            for(int y = 0; y < height; y++)
            {
                colorMap[x * width + y] = Color.Lerp(Color.black, Color.white, noiseMap[x, y]);
            }

        texture.SetPixels(colorMap);
        texture.Apply();

        TextureRenderer.sharedMaterial.mainTexture = texture;
        TextureRenderer.transform.localScale = new Vector3(width, 1, height);
    }
}
