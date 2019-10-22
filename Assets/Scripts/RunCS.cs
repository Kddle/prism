using SimplexNoise;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RunCS : MonoBehaviour
{
    public ComputeShader computeShader;
    Texture2D texture;

    MeshRenderer meshRenderer;
    // Start is called before the first frame update
    void Start()
    {
        GenerateNoiseGPU();
        GenerateNoiseCPU();
    }

    void GenerateNoiseGPU()
    {
        float last_gpu_time = Time.realtimeSinceStartup;
        float[] noiseMap = new float[1024 * 1024];

        ComputeBuffer noiseMapBuffer = new ComputeBuffer(1024 * 1024, sizeof(float));
        noiseMapBuffer.SetData(noiseMap);

        int kernel = computeShader.FindKernel("CSMain");
        computeShader.SetBuffer(kernel, "NoiseMap", noiseMapBuffer);
        computeShader.Dispatch(kernel, 32, 32, 1);

        noiseMapBuffer.GetData(noiseMap);
        noiseMapBuffer.Dispose();

        float total_gpu_time = Time.realtimeSinceStartup - last_gpu_time;

        Debug.Log($"[GPU] Done in {total_gpu_time} seconds");
    }


    void GenerateNoiseCPU()
    {
        float last_cpu_time = Time.realtimeSinceStartup;
        float[] noiseMap = new float[1024 * 1024];

        for (int x = 0; x < 1024; x++)
            for (int y = 0; y < 1024; y++)
            {
                noiseMap[x + y * 1024] = Noise.Generate(x, y);
            }
        
        float total_cpu_time = Time.realtimeSinceStartup - last_cpu_time;

        Debug.Log($"[CPU] Done in {total_cpu_time} seconds");
    }
}
