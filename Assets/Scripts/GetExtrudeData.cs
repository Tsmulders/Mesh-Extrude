using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GetExtrudeData : MonoBehaviour
{

    public static ExtrudeData[] GetData(Mesh mesh)
    {
        List<Edge> allEdges = new List<Edge>();
        List<Edge> edgesCircle = new List<Edge>();
        return Getvetices(edgesCircle, allEdges);
    }


    public static ExtrudeData[] Getvetices(List<Edge> edgesCircle, List<Edge> allEdges)
    {
        bool loop = false;
        int[] data;

        int xGroup = 1;
        int yGroup = 1;

        bool addEdges = true;

        List<int> indexEdges = new List<int>();
        List<ExtrudeData> extrude = new List<ExtrudeData>();

        ComputeBuffer edgesA = new ComputeBuffer(allEdges.Count, 3 * sizeof(float));
        ComputeBuffer edgesB = new ComputeBuffer(allEdges.Count, 3 * sizeof(float));



        while (loop)
        {
            //compute shader
            ComputeShader compute;
            compute = Resources.Load<ComputeShader>("Assets/Scripts/ComputeShader/ExtrudeDataShader.compute");
            int _kernel = compute.FindKernel("CSMain");
            if (addEdges)
            {
                xGroup = (int)(edgesCircle.Count / 8.0f);
                yGroup = (int)(allEdges.Count / 8.0f);
                addEdges = false;
            }


            ComputeBuffer result;
            result = new ComputeBuffer(0, 3 * sizeof(float));

            compute.SetBuffer(_kernel, "Result", result);

            compute.Dispatch(_kernel, xGroup, yGroup, 1);

            ComputeBuffer countBuffer = new ComputeBuffer(1, 3 * sizeof(float));

            ComputeBuffer.CopyCount(result, countBuffer, 0);

            int[] counter = new int[1] {0};
            countBuffer.GetData(counter);

            int count = counter[0];

            data = new int[count];

            result.GetData(data);

            indexEdges.AddRange(data);

            xGroup = (int)(data.Length / 8.0f);
            yGroup = (int)(data.Length / 8.0f);

            countBuffer.Dispose();
        }
        extrude.Add(new ExtrudeData(indexEdges, edgesCircle));

        return extrude.ToArray();
    }
}
