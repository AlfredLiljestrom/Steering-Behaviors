using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;

public class ComputeHelper
{
    ComputeShader shader;
    List<ComputeBuffer> computeBuffers;
    int kernel;

    public Vector3Int numOfThreads;

    public ComputeHelper(ComputeShader shader) 
    {
        numOfThreads = new Vector3Int(8, 8, 8);     
        this.shader = shader;
        kernel = shader.FindKernel(shader.name);
        computeBuffers = new List<ComputeBuffer>();
    }

    public int addBuffer<T>(T[] data, string nameInShader) where T : struct
    {
        int size = Marshal.SizeOf(typeof(T));
        var buffer = new ComputeBuffer(data.Length, size);
        buffer.SetData(data);

        computeBuffers.Add(buffer);
        shader.SetBuffer(kernel, nameInShader, buffer);


        return computeBuffers.Count - 1;
    }

    public void addtexture(RenderTexture rt, string nameInShader)
    {
        shader.SetTexture(kernel, nameInShader, rt);
    }

    public void setThreads(int count, int threadsPerGroup)
    {
        int group = Mathf.CeilToInt((float)count / threadsPerGroup); 
        numOfThreads = new Vector3Int(group, 1, 1);
    }

    public void setThreads(Vector2Int threads, int threadsPerGroup)
    {
        int t1 = Mathf.CeilToInt((float)threads.x / threadsPerGroup);
        int t2 = Mathf.CeilToInt((float)threads.y / threadsPerGroup);
        numOfThreads = new Vector3Int(t1, t2, 1);
    }

    public void setVariable<T>(T variable, string nameInShader)
    {
        if (variable is int i)
        {
            shader.SetInt(nameInShader, i);
        }
        else if (variable is float f)
        {
            shader.SetFloat(nameInShader, f);
        }
        else if (variable is bool b)
        {
            shader.SetBool(nameInShader, b);
        }
        else if (variable is Vector3 v3)
        {
            shader.SetFloats(nameInShader, v3.x, v3.y, v3.z); 
        }
        else if (variable is Vector3Int v3i)
        {
            shader.SetFloats(nameInShader, v3i.x, v3i.y, v3i.z);
        }
    }
    public void Dispatch()
    {
        shader.Dispatch(kernel, numOfThreads.x, numOfThreads.y, numOfThreads.z); 
    }

    public void getData<T>(ref List<T> data, int bufferID) where T : struct
    {
        T[] arr = new T[data.Count];
        computeBuffers[bufferID].GetData(arr);
        data.Clear();
        data.AddRange(arr);
    }

    public void getData<T>(ref T[] data, int bufferID) where T : struct
    {
        computeBuffers[bufferID].GetData(data);
    }

    public void Release()
    {
        while(computeBuffers.Count > 0)
        {
            computeBuffers[0].Release();
            computeBuffers[0].Dispose();
            computeBuffers[0] = null;
            computeBuffers.RemoveAt(0);
        }
    }
}
