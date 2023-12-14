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
        
        List<Edge> edges = new List<Edge>();

        //check if is compatible for witch compute shader
        //30 is the number of available treats if the workload than is above 65535 that the compute shader wil not work.
        //than it will take the code that wil splits it up smaller tasks.
        if (allEdges.Count / 30 > 65535)
        {
            //get all outer edges of the mesh. 
            edges = GetEdgesOfMesh.GetEdges2(mesh, allEdges);
        }
        else
        {
            //get all outer edges of the mesh. 
            edges = GetEdgesOfMesh.GetEdges(mesh, allEdges);
        }
                                                            //edges = GetEdgesOfMesh.GetEdge(mesh, allEdges);

        //check if data is not null
        if (edges == null || edges.Count == 0 || edges.Count == allEdges.Count) return new ExtrudeData[0];

        //this wil make look if a outer edges is a Circle
        List<Edge[]> CircleEdges = new List<Edge[]>();
        CircleEdges = GetEdgeCircle(edges);

        //check if it is a circle not a wrong triangle
        for (int i = 0; i < CircleEdges.Count; i++)
        {
            if (CircleEdges[i].Length <= 3)
            {
                CircleEdges.RemoveAt(i);
            }
        }
        //check if data is not null
        if (CircleEdges.Count == 0) return new ExtrudeData[0];

        //check if is compatible for witch compute shader
        //32 is the number of available treats if the workload than is above 65535 that the compute shader wil not work.
        //than it will take the code that wil splits it up smaller tasks.
        if (allEdges.Count / 32 > 65535)
        {
            //will get all vertices that needs to be extruded
            return GetVertices91(CircleEdges, allEdges, mesh);
        }
        else
        {
            //will get all vertices that needs to be extruded
            return GetVertices(CircleEdges, allEdges, mesh);
        }
    }


    private static List<Edge[]> GetEdgeCircle(List<Edge> edges)
    {
        List<Edge[]> circleEdges = new List<Edge[]>();

        List<Edge> edgesCircle = new List<Edge>();

        edgesCircle.Add(edges[0]);
        List<int> notChosen = new List<int>();
        int j = 0;

        List<Edge> edgesDone = new List<Edge>();
        edgesDone.AddRange(edges);
    //back point
    _l2:
        //loop throw edges that needs to be checked
        for (int i = 0; i < edgesDone.Count; i++)
        {
            //check if index is not the same
            if (edges[i].indexA == edgesDone[i].indexB)
            {
                i++;
                continue;
            }
            //check if index b is the same than index a 
            if (edgesCircle[j].indexB == edgesDone[i].indexA)
            {
                //check if not already contains
                if (!edgesCircle.Contains(edgesDone[i]))
                {
                    edgesCircle.Add(edgesDone[i]);
                    j++;
                    goto _l2;
                }
            }
            //add if is not connected to edge
            if (!edgesCircle.Contains(edgesDone[i]))
            {
                notChosen.Add(i);
            }
        }
        //check if index a of the first index is the same as index b of the last index.
        if (edgesCircle[0].indexA != edgesCircle[edgesCircle.Count - 1].indexB)
        {
            for (int i = 0; i < edgesCircle.Count; i++)
            {
                edgesDone.Remove(edgesCircle[i]);
            }
            //reset if there are more flat polygons.
            if (edgesDone.Count > 0)
            {
                edgesCircle.Clear();
                edgesCircle.Add(edgesDone[0]);
                notChosen.Clear();
                j = 0;
                goto _l2;
            }
        }
        //if is flatpolygon add to list
        circleEdges.Add(edgesCircle.ToArray());
        //reset if there are more flat polygons.
        if (notChosen.Count != 0)
        {
            for (int i = 0; i < edgesCircle.Count; i++)
            {
                edgesDone.Remove(edgesCircle[i]);
            }
            if (edgesDone.Count > 0)
            {
                edgesCircle.Clear();
                edgesCircle.Add(edgesDone[0]);
                notChosen.Clear();
                j = 0;
                goto _l2;
            }
        }
        //return all outer edges of object
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

        //get compute shader from map
        ComputeShader compute;
        compute = AssetDatabase.LoadAssetAtPath<ComputeShader>("Assets/Scripts/ComputeShader/ExtrudeDataShader.compute");
        int _kernel = compute.FindKernel("CSMain");
        //set y treat group
        yGroup = Mathf.RoundToInt((allEdges.Count / 32.0f));
        Debug.Log(yGroup);

        //create compute buffers
        ComputeBuffer countBuffer = new ComputeBuffer(1, sizeof(int), ComputeBufferType.IndirectArguments);
        ComputeBuffer indexABuffer = new ComputeBuffer(allEdges.Count, sizeof(int));
        ComputeBuffer indexBBuffer = new ComputeBuffer(allEdges.Count, sizeof(int));
        ComputeBuffer InsiteArrayBuffer = new ComputeBuffer(allEdges.Count, sizeof(int));

        //link buffers
        compute.SetBuffer(_kernel, "AIndex", indexABuffer);
        compute.SetBuffer(_kernel, "BIndex", indexBBuffer);
        compute.SetBuffer(_kernel, "InsiteArray", InsiteArrayBuffer);

        //prepare array to sent to compute shader
        for (int j = 0; j < allEdges.Count; j++)
        {
            indexA[j] = allEdges[j].indexA;
            indexB[j] = allEdges[j].indexB;
        }

        //set compute buffer data
        indexABuffer.SetData(indexA);
        indexBBuffer.SetData(indexB);
        compute.SetInt("maxY", allEdges.Count);

    //back point
    _L1:

        int[] InsiteArray = new int[verticesCount];
        InsiteArray[0] = 0;
        //prepare data for compute shader
        for (int j = 0; j < CircleEdges[i].Length; j++)
        {
            nextCheck.Add(CircleEdges[i][j].indexA);
            //nextCheck.Add(CircleEdges[i][j].indexB);
            InsiteArray[CircleEdges[i][j].indexA] = 1;
            InsiteArray[CircleEdges[i][j].indexB] = 1;
        }

        indexEdges.AddRange(nextCheck);
        //set compute buffer data
        InsiteArrayBuffer.SetData(InsiteArray);
        //
        while (loop)
        {
            //set x treat group
            xGroup = Mathf.RoundToInt((nextCheck.Count / 1));
            //Debug.Log(nextCheck.Count);
            //Debug.Log(xGroup);
            //create append buffer
            ComputeBuffer result;
            result = new ComputeBuffer(allEdges.Count * 50, sizeof(float), ComputeBufferType.Append);
            result.SetCounterValue(0);
            //create compute buffers
            ComputeBuffer indexCheck = new ComputeBuffer(nextCheck.Count, sizeof(float));
            //link buffers
            compute.SetBuffer(_kernel, "Result", result);
            compute.SetBuffer(_kernel, "indexCheck", indexCheck);
            //set compute buffer data
            indexCheck.SetData(nextCheck.ToArray());
            compute.SetInt("max", nextCheck.Count);
            //dispatch to compute shaders
            if (xGroup < 65535 && yGroup < 65535)
            {
                compute.Dispatch(_kernel, xGroup, yGroup, 1);
            }
            
            //will copy the count of the append buffer
            ComputeBuffer.CopyCount(result, countBuffer, 0);
            
            int[] counter = new int[1] {0};
            //set the count in a array
            countBuffer.GetData(counter);

            int count = counter[0];
            //we make a array of the size of the append buffer
            int[] data = new int[count];
            //Get de data of the append buffer
            result.GetData(data);
            //set data to list
            indexEdges.AddRange(data.Distinct().ToList());
            //check if data = 0 or 1 that it is complete
            if(data.Length <= 1)
            {
                loop = false;
            }
            //make it ready to next check
            nextCheck.Clear();
            nextCheck.AddRange(data.Distinct().ToList());
            //release compute buffer, append buffer
            result.Release();
            indexCheck.Release();
        }
        

        indexEdges.Sort();
        indexEdges = indexEdges.Distinct().ToList();

        extrude.Add(new ExtrudeData(indexEdges, CircleEdges[i]));
        //reset if there are more edge circle
        if (CircleEdges.Count -1 != i)
        {
            i++;
            loop = true;
            indexEdges.Clear();
            goto _L1;
        }
        //release compute buffer
        countBuffer.Release();
        InsiteArrayBuffer.Release();
        indexABuffer?.Release();
        indexABuffer = null;
        indexBBuffer?.Release();
        indexBBuffer = null;

        return extrude.ToArray();
    }

    private static ExtrudeData[] GetVertices91(List<Edge[]> CircleEdges, List<Edge> allEdges, Mesh mesh)
    {
        int verticesCount = mesh.vertices.Length;

        bool loop = true;

        int xGroup = 1;
        int yGroup = 1;

        int[] indexA = new int[allEdges.Count];
        int[] indexB = new int[allEdges.Count];


        int i = 0;

        List<int> indexEdges = new List<int>();
        List<ExtrudeData> extrude = new List<ExtrudeData>();
        List<int> nextCheck = new List<int>();

        //get compute shader from map
        ComputeShader compute;
        compute = AssetDatabase.LoadAssetAtPath<ComputeShader>("Assets/Scripts/ComputeShader/ExtrudeDataShader.compute");
        int _kernel = compute.FindKernel("CSMain2");
        //set y treat group
        yGroup = Mathf.RoundToInt((allEdges.Count / 91.0f));
        //Debug.Log(yGroup);

        //create compute buffers
        ComputeBuffer countBuffer = new ComputeBuffer(1, sizeof(int), ComputeBufferType.IndirectArguments);
        ComputeBuffer indexABuffer = new ComputeBuffer(allEdges.Count, sizeof(int));
        ComputeBuffer indexBBuffer = new ComputeBuffer(allEdges.Count, sizeof(int));
        ComputeBuffer InsiteArrayBuffer = new ComputeBuffer(allEdges.Count, sizeof(int));

        //link buffers
        compute.SetBuffer(_kernel, "AIndex", indexABuffer);
        compute.SetBuffer(_kernel, "BIndex", indexBBuffer);
        compute.SetBuffer(_kernel, "InsiteArray", InsiteArrayBuffer);

        //prepare array to sent to compute shader
        for (int j = 0; j < allEdges.Count; j++)
        {
            indexA[j] = allEdges[j].indexA;
            indexB[j] = allEdges[j].indexB;
        }

        //set compute buffer data
        indexABuffer.SetData(indexA);
        indexBBuffer.SetData(indexB);
        compute.SetInt("maxY", allEdges.Count);

    //back point
    _L1:

        int[] InsiteArray = new int[verticesCount];
        InsiteArray[0] = 0;
        //prepare data for compute shader
        for (int j = 0; j < CircleEdges[i].Length; j++)
        {
            nextCheck.Add(CircleEdges[i][j].indexA);
            //nextCheck.Add(CircleEdges[i][j].indexB);
            InsiteArray[CircleEdges[i][j].indexA] = 1;
            InsiteArray[CircleEdges[i][j].indexB] = 1;
        }

        indexEdges.AddRange(nextCheck);
        //set compute buffer data
        InsiteArrayBuffer.SetData(InsiteArray);
        //
        while (loop)
        {
            //set x treat group
            xGroup = Mathf.RoundToInt((nextCheck.Count / 1));
            //Debug.Log(nextCheck.Count);
            //Debug.Log(xGroup);
            //create append buffer
            ComputeBuffer result;
            result = new ComputeBuffer(allEdges.Count * 50, sizeof(float), ComputeBufferType.Append);
            result.SetCounterValue(0);
            //create compute buffers
            ComputeBuffer indexCheck = new ComputeBuffer(nextCheck.Count, sizeof(float));
            //link buffers
            compute.SetBuffer(_kernel, "Result", result);
            compute.SetBuffer(_kernel, "indexCheck", indexCheck);
            //set compute buffer data
            indexCheck.SetData(nextCheck.ToArray());
            compute.SetInt("max", nextCheck.Count);
            
            //dispatch to compute shaders
            if (xGroup < 65535 && yGroup < 65535)
            {
                compute.Dispatch(_kernel, xGroup, yGroup, 1);
            }

            //will copy the count of the append buffer
            ComputeBuffer.CopyCount(result, countBuffer, 0);

            int[] counter = new int[1] { 0 };
            //set the count in a array
            countBuffer.GetData(counter);

            int count = counter[0];
            //we make a array of the size of the append buffer
            int[] data = new int[count];
            //Get de data of the append buffer
            result.GetData(data);
            //set data to list
            indexEdges.AddRange(data.Distinct().ToList());
            //check if data = 0 or 1 that it is complete
            if (data.Length <= 1)
            {
                loop = false;
            }
            //make it ready to next check
            nextCheck.Clear();
            nextCheck.AddRange(data.Distinct().ToList());
            //release compute buffer, append buffer
            result.Release();
            indexCheck.Release();
        }


        indexEdges.Sort();
        indexEdges = indexEdges.Distinct().ToList();

        extrude.Add(new ExtrudeData(indexEdges, CircleEdges[i]));
        //reset if there are more edge circle
        if (CircleEdges.Count - 1 != i)
        {
            i++;
            loop = true;
            indexEdges.Clear();
            goto _L1;
        }
        //release compute buffer
        countBuffer.Release();
        InsiteArrayBuffer.Release();
        indexABuffer?.Release();
        indexABuffer = null;
        indexBBuffer?.Release();
        indexBBuffer = null;

        return extrude.ToArray();
    }
}
