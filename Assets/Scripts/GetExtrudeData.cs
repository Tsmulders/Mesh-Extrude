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
        List<Edge> allEdges = new List<Edge>();

        //get all edges of the triangles 
        allEdges.AddRange(GetEdgesOfMesh.GetAllEdge(mesh));

        //check if data is not null
        if (allEdges.Count == 0)
        {
            return new ExtrudeData[0];
        }
        //get all outer edges of the mesh. 
        List<Edge> edges = new List<Edge>();
        edges = GetEdgesOfMesh.GetEdges(mesh, allEdges);
                                                                            //edges = GetEdgesOfMesh.GetEdge(mesh, allEdges);

        //check if data is not null
        if (edges == null || edges.Count == 0) return new ExtrudeData[0];

        //this wil make look if a outer edges is a Circle
        List<Edge[]> CircleEdges = new List<Edge[]>();
        CircleEdges = GetEdgeCircle(edges);

        //check if it is a circle not a wrong triangle 
        for (int i = 0; i < CircleEdges.Count; i++)
        {
            if (CircleEdges[i].Length <= 2)
            {
                CircleEdges.RemoveAt(i);
            }
        }
        //check if data is not null
        if (CircleEdges.Count == 0) return new ExtrudeData[0];

        //will get all vertices that needs to be extruded
        return GetVertices(CircleEdges, allEdges, mesh);
    }


    private static List<Edge[]> GetEdgeCircle(List<Edge> edges)
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


    private static ExtrudeData[] GetVertices(List<Edge[]> CircleEdges , List<Edge> allEdges, Mesh mesh)
    {
        int verticesCount = mesh.vertices.Length;

        bool loop = true;

        int xGroup = 1;
        int yGroup = 1;

        int[] indexA = new int[allEdges.Count];
        int[] indexB= new int[allEdges.Count];
        

        int i = 0;

        List<int> indexEdges = new List<int>();
        List<ExtrudeData> extrude = new List<ExtrudeData>();
        List<int> nextCheck = new List<int>();

        ComputeShader compute;
        compute = AssetDatabase.LoadAssetAtPath<ComputeShader>("Assets/Scripts/ComputeShader/ExtrudeDataShader.compute");
        int _kernel = compute.FindKernel("CSMain");
        yGroup = Mathf.RoundToInt((allEdges.Count / 32.0f));
        Debug.Log(yGroup);

        
        ComputeBuffer countBuffer = new ComputeBuffer(1, sizeof(int), ComputeBufferType.IndirectArguments);
        ComputeBuffer indexABuffer = new ComputeBuffer(allEdges.Count, sizeof(int));
        ComputeBuffer indexBBuffer = new ComputeBuffer(allEdges.Count, sizeof(int));
        ComputeBuffer InsiteArrayBuffer = new ComputeBuffer(allEdges.Count, sizeof(int));


        compute.SetBuffer(_kernel, "AIndex", indexABuffer);
        compute.SetBuffer(_kernel, "BIndex", indexBBuffer);
        compute.SetBuffer(_kernel, "InsiteArray", InsiteArrayBuffer);

        for (int j = 0; j < allEdges.Count; j++)
        {
            indexA[j] = allEdges[j].indexA;
            indexB[j] = allEdges[j].indexB;
        }


        indexABuffer.SetData(indexA);
        indexBBuffer.SetData(indexB);

    _L1:
        int[] InsiteArray = new int[verticesCount];
        InsiteArray[0] = 0;
        for (int j = 0; j < CircleEdges[i].Length; j++)
        {
            nextCheck.Add(CircleEdges[i][j].indexA);
            //nextCheck.Add(CircleEdges[i][j].indexB);
            InsiteArray[CircleEdges[i][j].indexA] = 1;
            InsiteArray[CircleEdges[i][j].indexB] = 1;
        }
        indexEdges.AddRange(nextCheck);
        InsiteArrayBuffer.SetData(InsiteArray);

        while (loop)
        {
            xGroup = Mathf.RoundToInt((nextCheck.Count / 1));
            Debug.Log(nextCheck.Count);
            Debug.Log(xGroup);
            ComputeBuffer result;
            result = new ComputeBuffer(allEdges.Count * 50, sizeof(float), ComputeBufferType.Append);
            result.SetCounterValue(0);
            ComputeBuffer indexCheck = new ComputeBuffer(nextCheck.Count, sizeof(float));

            compute.SetBuffer(_kernel, "Result", result);
            compute.SetBuffer(_kernel, "indexCheck", indexCheck);

            indexCheck.SetData(nextCheck.ToArray());
            compute.SetInt("max", nextCheck.Count);

            compute.Dispatch(_kernel, xGroup, yGroup, 1);
            ComputeBuffer.CopyCount(result, countBuffer, 0);

            int[] counter = new int[1] {0};
            countBuffer.GetData(counter);

            int count = counter[0];

            int[] data = new int[count];

            result.GetData(data);

            indexEdges.AddRange(data.Distinct().ToList());

            if(data.Length <= 1)
            {
                loop = false;
            }
            nextCheck.Clear();
            nextCheck.AddRange(data.Distinct().ToList());
            result.Release();
            indexCheck.Release();
        }
        

        indexEdges.Sort();
        indexEdges = indexEdges.Distinct().ToList();

        extrude.Add(new ExtrudeData(indexEdges, CircleEdges[i]));

        if (CircleEdges.Count -1 != i)
        {
            i++;
            loop = true;
            indexEdges.Clear();
            goto _L1;
        }
        countBuffer.Release();
        InsiteArrayBuffer.Release();
        indexABuffer?.Release();
        indexABuffer = null;
        indexBBuffer?.Release();
        indexBBuffer = null;

        return extrude.ToArray();
    }
}
