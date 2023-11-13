using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

public class GetExtrudeData : MonoBehaviour
{

    public static ExtrudeData[] GetData(Mesh mesh)
    {
        List<Edge> alledges = new List<Edge>();
        alledges.AddRange(GetEdgesOfMesh.GetAllEdge(mesh));

        List<Edge> edges = new List<Edge>();
        edges = GetEdgesOfMesh.GetEdge(mesh, alledges);

        if (edges == null || edges.Count == 0) return null;
        List<Edge[]> CircleEdges = new List<Edge[]>();
        CircleEdges = GetEdgeCircle(edges);

        return GetVertices(CircleEdges, alledges);
    }


    public static List<Edge[]> GetEdgeCircle(List<Edge> edges)
    {
        List<Edge[]> circleEdges = new List<Edge[]>();

        List<Edge> edgesCircle = new List<Edge>();

        edgesCircle.Add(edges[0]);
        List<int> notChosen = new List<int>();
        int j = 0;

        List<Edge> edgesdone = new List<Edge>();
        edgesdone.AddRange(edges);

    _l2:
        for (int i = 0; i < edgesdone.Count; i++)
        {
            if (edges[i].indexA == edgesdone[i].indexB)
            {
                i++;
            }
            if (edgesCircle[j].indexB == edgesdone[i].indexA)
            {
                if (!edgesCircle.Contains(edgesdone[i]))
                {
                    edgesCircle.Add(edgesdone[i]);
                    j++;
                    goto _l2;
                }
            }
            if (!edgesCircle.Contains(edgesdone[i]))
            {
                notChosen.Add(i);
            }
        }
        if (edgesCircle[0].indexA != edgesCircle[edgesCircle.Count - 1].indexB)
        {
            for (int i = 0; i < edgesCircle.Count; i++)
            {
                edgesdone.Remove(edgesCircle[i]);
            }
            if (edgesdone.Count > 0)
            {
                edgesCircle.Clear();
                edgesCircle.Add(edgesdone[0]);
                notChosen.Clear();
                j = 0;
                goto _l2;
            }
        }
        
        circleEdges.Add(edgesCircle.ToArray());

        if (notChosen.Count != 0)
        {
            for (int i = 0; i < edgesCircle.Count; i++)
            {
                edgesdone.Remove(edgesCircle[i]);
            }
            if (edgesdone.Count > 0)
            {
                edgesCircle.Clear();
                edgesCircle.Add(edgesdone[0]);
                notChosen.Clear();
                j = 0;
                goto _l2;
            }
        }
        return circleEdges;
    }


    public static ExtrudeData[] GetVertices(List<Edge[]> CircleEdges , List<Edge> allEdges)
    {
        bool loop = false;
        int[] data;

        int xGroup = 1;
        int yGroup = 1;

        int[] indexA = new int[allEdges.Count];
        int[] indexB= new int[allEdges.Count];
        bool[] bools = new bool[allEdges.Count];

        int i = 0;

        List<int> indexEdges = new List<int>();
        List<ExtrudeData> extrude = new List<ExtrudeData>();
        List<int> nextCheck = new List<int>();

        ComputeShader compute;
        compute = compute = AssetDatabase.LoadAssetAtPath<ComputeShader>("Assets/Scripts/ComputeShader/ExtrudeDataShader.compute");
        int _kernel = compute.FindKernel("CSMain");
        yGroup = (int)(allEdges.Count / 8.0f);

        
        ComputeBuffer countBuffer = new ComputeBuffer(1, 3 * sizeof(float));

        for (int j = 0; j < allEdges.Count; j++)
        {
            indexA[j] = allEdges[j].indexA;
            indexB[j] = allEdges[j].indexB;
            bools[allEdges[j].indexA] = true;
            bools[allEdges[j].indexB] = true;
        }

    _L1:
        bool[] boolsListed = bools;

        for (int j = 0; j < CircleEdges[i].Length; j++)
        {
            nextCheck.Add(CircleEdges[i][j].indexA);
            nextCheck.Add(CircleEdges[i][j].indexB);
            bools[CircleEdges[i][j].indexA] = true;
            bools[CircleEdges[i][j].indexB] = true;
        }

        ComputeBuffer boolArray = new ComputeBuffer(allEdges.Count, 3 * sizeof(float));
        compute.SetBuffer(_kernel, "boolInsiteArray", boolArray);
        boolArray.SetData(boolsListed);

        while (loop)
        {
            //compute shader

            xGroup = (int)(nextCheck.Count / 8.0f);

            ComputeBuffer result;
            result = new ComputeBuffer(0, 3 * sizeof(float));

            compute.SetBuffer(_kernel, "Result", result);

            compute.SetInts("indexA", indexA);
            compute.SetInts("indexB", indexB);
            compute.SetInts("indexCheck", nextCheck.ToArray());

            compute.Dispatch(_kernel, xGroup, yGroup, 1);

            ComputeBuffer.CopyCount(result, countBuffer, 0);

            int[] counter = new int[1] {0};
            countBuffer.GetData(counter);

            int count = counter[0];

            data = new int[count];

            result.GetData(data);

            indexEdges.AddRange(data);

            xGroup = (int)(data.Length / 8.0f);

            if(data.Length !> 0)
            {
                loop = true;
            }
            nextCheck.Clear();
            nextCheck.AddRange(data);

            result.Release();
            
        }
        countBuffer.Release();
        boolArray.Release();

        extrude.Add(new ExtrudeData(indexEdges, CircleEdges[i]));

        if (CircleEdges.Count == i)
        {
            i++;
            loop = false;
            indexEdges.Clear();
            goto _L1;
        }
        
        return extrude.ToArray();
    }
}
