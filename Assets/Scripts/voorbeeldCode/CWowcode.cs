using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CWowcode : MonoBehaviour
{
    public static void Dilate(in Texture2D texture, in ComputeShader cs, int itteration = 25)
    {
        Vector2Int size = new Vector2Int(texture.width, texture.height);

        // Create new render texture for the compute shader to write the result in.
        RenderTexture renderSource = new RenderTexture(size.x, size.y, 0, RenderTextureFormat.ARGB32);
        renderSource.enableRandomWrite = true;
        renderSource.Create();

        RenderTexture renderTarget = new RenderTexture(size.x, size.y, 0, RenderTextureFormat.ARGB32);
        renderTarget.enableRandomWrite = true;
        renderTarget.Create();

        Graphics.Blit(texture, renderSource);

        // Gather kernal information and.
        // Threadpool 64 | AMD warpsize 64 threads, NVidea 32 threads (2 warps).
        int kernel = cs.FindKernel("CSMain");
        int workgroupsX = Mathf.CeilToInt(renderSource.width / 8.0f);
        int workgroupsy = Mathf.CeilToInt(renderSource.height / 8.0f);

        // Set buffers.
        cs.SetTexture(kernel, "Original", renderSource);
        cs.SetTexture(kernel, "Result", renderTarget);

        for (int i = 0; i < itteration; i++)
        {
            cs.Dispatch(kernel, workgroupsX, workgroupsy, 1);
        }

        RenderTexture.active = renderTarget;

        texture.ReadPixels(new Rect(0, 0, size.x, size.y), 0, 0);
        texture.Apply();

        RenderTexture.active = null;
        renderTarget.Release();
        renderSource.Release();
    }
}
