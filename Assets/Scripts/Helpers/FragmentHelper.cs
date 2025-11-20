using System.Runtime.InteropServices;
using System.Collections.Generic; 
using UnityEngine;

public class FragmentHelper
{
    Material material;  
    List<ComputeBuffer> computeBuffers;
    List<string> bufferNames; 

    public FragmentHelper(Material material) 
    { 
        this.material = material;
        computeBuffers = new();
        bufferNames = new(); 
    }

    ComputeBuffer getUsedBuffer(string nameInShader, int count)
    {
        
        for (int i = 0; i < bufferNames.Count; i++)
        {
            if (bufferNames[i] != nameInShader)
                continue; 

            ComputeBuffer buffer = computeBuffers[i];

            if (buffer.count != count)
                continue;

            return buffer; 
        }

        return null; 
    }

    public int addbuffer<T>(T[] data, string nameInShader, bool clearIfNew = false) where T : struct
    {
        int size = Marshal.SizeOf(typeof(T));
        var buffer = getUsedBuffer(nameInShader, data.Length);

        if (buffer == null)
        {
            if (clearIfNew)
                Release(); 
            buffer = new ComputeBuffer(data.Length, size);
            computeBuffers.Add(buffer);
            bufferNames.Add(nameInShader);
        }

        buffer.SetData(data);
        material.SetBuffer(nameInShader, buffer);

        return computeBuffers.Count - 1;
    }

    public void setVariable<T>(T variable, string nameInShader)
    {
        if (variable is int i)
        {
            material.SetInteger(nameInShader, i);
        }
        else if (variable is float f)
        {
            material.SetFloat(nameInShader, f);
        }
        else if (variable is Vector3 v3)
        {
            material.SetVector(nameInShader, v3);
        }
        else if (variable is Matrix4x4 m4)
        {
            material.SetMatrix(nameInShader, m4);
        }
        else if (variable is Color color)
        {
            material.SetColor(nameInShader, color);
        }
        else if (variable is RenderTexture rt)
        {
            material.SetTexture(nameInShader, rt);
        }
    }

    public void Release()
    {
        while (computeBuffers.Count > 0)
        {
            computeBuffers[0].Release();
            computeBuffers[0].Dispose();
            computeBuffers[0] = null;
            computeBuffers.RemoveAt(0);
            bufferNames.RemoveAt(0); 
        }
    }

    public RenderTexture createRenderTexture(bool mipMapped)
    {
        RenderTexture rt = new RenderTexture(2048, 2048, 0, RenderTextureFormat.ARGB32);
        rt.useMipMap = false;
        rt.autoGenerateMips = false;
        rt.wrapMode = TextureWrapMode.Repeat;
        rt.filterMode = FilterMode.Bilinear; 
        rt.Create();
        return rt;
    }
}
