using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ComputeShaderTests : MonoBehaviour
{
    public ComputeShader computeShader;
    List<int[]> data = new List<int[]>();

    void Start()
    {
        for(int i = 0; i < 10; i++)
        {
            StartCoroutine(Calculate());
            Debug.Log("CoroutineDone");
        }

        Debug.Log("Done !");
    }

    IEnumerator Calculate()
    {
        var x = new int[4 * 4 * 4];
        ComputeBuffer buffer = new ComputeBuffer(4 * 4 * 4, sizeof(int));
        buffer.SetData(x);

        int kernel = computeShader.FindKernel("CSMain");

        computeShader.SetBuffer(kernel, "Result", buffer);

        computeShader.Dispatch(kernel, 2, 2, 2);

        buffer.GetData(x);

        data.Add(x);

        yield return null;
    }
}