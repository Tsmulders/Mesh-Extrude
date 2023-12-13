using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Reflection;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;
using UnityEngine.tvOS;
using UnityEngine.UIElements;
using static UnityEditor.FilePathAttribute;

public class GetEdgesOfMesh : MonoBehaviour
{
    

    public static List<Edge> GetEdge(Mesh mesh, List<Edge> alledges)
    {
        /*
         * Multithread  Unity's JOB SYSTEM
         * de triagle array opsplitsen in klijneren array
         * check all edeges
         */

        //test 2
        //List<Edge> alledgesCoppy = new List<Edge>();

        //alledgesCoppy.AddRange(alledges);

        //int coreCount = SystemInfo.processorCount - 2;

        //Vector3[] points = mesh.vertices; // The mesh’s vertices
        //int[] indicies = mesh.triangles; // The mesh’s triangle indicies

        //NativeArray<float3> vertices = new NativeArray<float3>(points.Length, Allocator.TempJob);
        //NativeArray<int> triangles = new NativeArray<int>(indicies.Length, Allocator.TempJob);

        //List<Edge> edges = new List<Edge>();

        //NativeList<JobHandle> jobs = new NativeList<JobHandle>(Allocator.TempJob);

        //int arraysplitscount = alledgesCoppy.Count / coreCount;

        //for (int i = 0; i < indicies.Length; i++)
        //{
            
        //}



        //test 1
        Vector3[] points = mesh.vertices; // The mesh’s vertices
        int[] indicies = mesh.triangles; // The mesh’s triangle indicies

        //NativeArray<Vector3> vertices = new NativeArray<Vector3>(points.Length, Allocator.TempJob);

        List<Edge> edges = new List<Edge>();

        //for (int i = 0; i < points.Length; i++)
        //{
        //    vertices[i] = points[i];
        //}

        //for (int i = 0; i < indicies.Length; i++)
        //{
        //    triangles[i] = indicies[i];
        //}

        

        for (int i = 0; i < indicies.Length - 1; i += 3)
        {

            NativeList<Vector3> positionA = new NativeList<Vector3>(Allocator.Persistent);
            NativeList<Vector3> positionB = new NativeList<Vector3>(Allocator.Persistent);
            //NativeList<int> indexA = new NativeList<int>();
            //NativeList<int> indexB = new NativeList<int>();

            NativeList<Vector3> positionACheck = new NativeList<Vector3>(Allocator.Persistent);
            NativeList<Vector3> positionBCheck = new NativeList<Vector3>(Allocator.Persistent);
            NativeList<int> indexACheck = new NativeList<int>(3, Allocator.Persistent);
            NativeList<int> indexBCheck = new NativeList<int>(3, Allocator.Persistent);

            NativeArray<bool> foundOne = new NativeArray<bool>(3, Allocator.Persistent);
            NativeArray<int> indexFound = new NativeArray<int>(3, Allocator.Persistent);

            Edge[] edge = new Edge[3];

            start:

            edge[0] = new Edge(points[indicies[i]], points[indicies[i + 1]], indicies[i], indicies[i + 1]);
            edge[1] = new Edge(points[indicies[i + 1]], points[indicies[i + 2]], indicies[i + 1], indicies[i + 2]);
            edge[2] = new Edge(points[indicies[i + 2]], points[indicies[i]], indicies[i + 2], indicies[i]);

            for (int j = 0; j < edges.Count; j++)
            {
                positionA.Add(edges[j].A);
                positionB.Add(edges[j].B);
            }

            if (positionA.Length < 0)
            {
                for (int j = 0; j < edge.Length; j++)
                {
                    edges.Add(edge[j]);
                    positionA.Add(edge[j].A);
                    positionB.Add(edge[j].B);
                    //indexA.Add(edge[j].indexA);
                    //indexB.Add(edge[j].indexB);
                }
                i += 3;
                goto start;
            }

            for (int j = 0; j < edge.Length; j++)
            {
                positionACheck.Add(edge[j].A);
                positionBCheck.Add(edge[j].B);
                indexACheck.Add(edge[j].indexA);
                indexBCheck.Add(edge[j].indexB);
                foundOne[j] = false;
                indexFound[j] = -1;
            }

            GetEdgeOuterJob edgesJob = new GetEdgeOuterJob()
            {
                positionA = positionA,
                positionB = positionB,
                //indexA = indexA,
                //indexB = indexB,
                foundOne = foundOne,
                indexFound = indexFound,
                positionACheck = positionACheck,
                positionBCheck = positionBCheck,
                indexACheck = indexACheck,
                indexBCheck = indexBCheck,
            };
            

            JobHandle dependency = new JobHandle();
            JobHandle sheduleJobHandle = edgesJob.Schedule(edge.Length, dependency);
            JobHandle sheduleParralelJobHandle = edgesJob.ScheduleParallel(edge.Length, 1, sheduleJobHandle);

            sheduleParralelJobHandle.Complete();

            List<int> removeindex = new List<int>();

            for (int j = 0; j < edgesJob.foundOne.Length; j++)
            {
                if (edgesJob.foundOne[j])
                {

                    //edges.RemoveAt(edgesJob.indexFound[j]);
                    if (!removeindex.Contains(edgesJob.indexFound[j]))
                    {
                        removeindex.Add(edgesJob.indexFound[j]);
                    }

                }
                else if (!edgesJob.foundOne[j])
                {
                    edges.Add(edge[j]);
                }
            }

            positionACheck.Dispose();
            positionBCheck.Dispose();
            indexACheck.Dispose();
            indexBCheck.Dispose();
            positionA.Dispose();
            positionB.Dispose();
            foundOne.Dispose();
            indexFound.Dispose();
            //indexA.Dispose();
            //indexB.Dispose();

            removeindex.Sort();
            removeindex.Reverse();

            for (int j = 0; j < removeindex.Count; j++)
            {
                edges.RemoveAt(removeindex[j]);
            }
        }

        //oude manier doet paar uur over
        //for every two triangle indicies

        //List<Edge> edges = new List<Edge>();

        //Vector3[] points = mesh.vertices; // The mesh’s vertices
        //int[] indicies = mesh.triangles; // The mesh’s triangle indicies

        //for (int i = 0; i < indicies.Length - 1; i += 3)
        //{
        //    // Create a new edge with the corresponding points
        //    // and add it to edge list
        //    Edge edge = new Edge(points[indicies[i]], points[indicies[i + 1]], indicies[i], indicies[i + 1]);
        //    Edge edge1 = new Edge(points[indicies[i + 1]], points[indicies[i + 2]], indicies[i + 1], indicies[i + 2]);
        //    Edge edge2 = new Edge(points[indicies[i + 2]], points[indicies[i]], indicies[i + 2], indicies[i]);

        //    bool found = false;
        //    bool found1 = false;
        //    bool found2 = false;


        //    foreach (Edge e in edges)
        //    {
        //        if (e.AlmostEqual(edge) && !found)
        //        {
        //            found = true;

        //            edges.Remove(e);
        //            break;
        //        }

        //    }
        //    foreach (Edge e in edges)
        //    {
        //        if (e.AlmostEqual(edge1))
        //        {
        //            found1 = true;
        //            edges.Remove(e);
        //            break;
        //        }
        //    }
        //    foreach (Edge e in edges)
        //    {
        //        if (e.AlmostEqual(edge2))
        //        {
        //            found2 = true;
        //            edges.Remove(e);
        //            break;
        //        }
        //    }
        //    if (!found) edges.Add(edge);
        //    if (!found1) edges.Add(edge1);
        //    if (!found2) edges.Add(edge2);
        //}


        foreach (Edge edge in edges)
        {
            edge.Draw();
        }
        return edges;
    }

    public static List<Edge> GetEdges(Mesh mesh, List<Edge> allEdges)
    {

        List<Edge> edges = new List<Edge>();

        List<int> remove = new List<int>();
        //set x & y tread groups can not be higher than 65535
        int xGroup = Mathf.RoundToInt((allEdges.Count / 30.0f));
        int yGroup = Mathf.RoundToInt((allEdges.Count / 30.0f));
       
        int[] indexA = new int[allEdges.Count];
        int[] indexB = new int[allEdges.Count];
        Vector3[] positionA = new Vector3[allEdges.Count];
        Vector3[] positionB = new Vector3[allEdges.Count];
        int[] foundOne = new int[allEdges.Count];
        //splits all Edges to different array
        for (int i = 0; i < allEdges.Count; i++)
        {
            //indexA[i] = allEdges[i].indexA;
            //indexB[i] = allEdges[i].indexB;
            positionA[i] = allEdges[i].A;
            positionB[i] = allEdges[i].B;
            foundOne[i] = 0;
        }
        //get compute shader from map
        ComputeShader compute = AssetDatabase.LoadAssetAtPath<ComputeShader>("Assets/Scripts/ComputeShader/GetEdgeOuterShader.compute");
        int _kernel = compute.FindKernel("CSMain3");

        //ComputeBuffer result = new ComputeBuffer(allEdges.Count, sizeof(int), ComputeBufferType.Append);
        //result.SetCounterValue(0);

        //create compute buffer
        //ComputeBuffer countBuffer = new ComputeBuffer(1, sizeof(int), ComputeBufferType.IndirectArguments);
        //ComputeBuffer indexABuffer = new ComputeBuffer(allEdges.Count, sizeof(int));
        //ComputeBuffer indexBBuffer = new ComputeBuffer(allEdges.Count, sizeof(int));
        ComputeBuffer positionABuffer = new ComputeBuffer(allEdges.Count, sizeof(float) * 3);
        ComputeBuffer positionBBuffer = new ComputeBuffer(allEdges.Count, sizeof(float) * 3);
        ComputeBuffer foundOneBuffer = new ComputeBuffer(allEdges.Count, sizeof(int));

        //link buffer
        //compute.SetBuffer(_kernel, "result", result);1`1
        //compute.SetBuffer(_kernel, "indexA", indexABuffer);
        //compute.SetBuffer(_kernel, "indexB", indexBBuffer);
        compute.SetBuffer(_kernel, "positionA", positionABuffer);
        compute.SetBuffer(_kernel, "positionB", positionBBuffer);
        compute.SetBuffer(_kernel, "foundOne", foundOneBuffer);

        //set data to compute buffers
        //indexABuffer.SetData(indexA);
        //indexBBuffer.SetData(indexB);
        positionABuffer.SetData(positionA);
        positionBBuffer.SetData(positionB);
        foundOneBuffer.SetData(foundOne);
        compute.SetInt("count", allEdges.Count);

        //dispatch to compute shader
        compute.Dispatch(_kernel, xGroup, yGroup, 1);

        //ComputeBuffer.CopyCount(result, countBuffer, 0);

        //int[] counter = new int[1] { 0 };
        ////countBuffer.GetData(counter);

        //int count = counter[0];

        //int[] data = new int[count];

        //result.GetData(data);

        //get data
        foundOneBuffer.GetData(foundOne);

        //release all compute buffers
        //result.Release();
        //countBuffer.Release();
        //indexABuffer.Release();
        //indexBBuffer.Release();
        positionABuffer.Release();
        positionBBuffer.Release();
        foundOneBuffer.Release();

        ///remove.AddRange(data);

        //remove = remove.Distinct().ToList();

        //remove.Sort();
        //remove.Reverse();

        //for (int i = 0; i < remove.Count; i++)
        //{
        //    allEdges.RemoveAt(remove[i]);
        //}

        //please all outer edge in site a list
        for (int i = 0; i < foundOne.Length; i++)
        {
            if (foundOne[i] == 0)
            {
                edges.Add(allEdges[i]);
            }
        }
        //draw outer edge
        foreach (Edge edge in edges)
        {
            edge.Draw();
        }
        //return outer edge
        return edges;
    }

    public static List<Edge> GetEdges2(Mesh mesh, List<Edge> allEdges)
    {

        List<Edge> edges = new List<Edge>();

        List<int> remove = new List<int>();
        //set x & y tread groups can not be higher than 65535
        int xGroup = Mathf.RoundToInt((allEdges.Count / 11.0f));
        int yGroup = Mathf.RoundToInt((allEdges.Count / 92.0f));


        Vector3[] positionA = new Vector3[allEdges.Count];
        Vector3[] positionB = new Vector3[allEdges.Count];
        int[] foundOne = new int[allEdges.Count];
        //splits all Edges to different array
        for (int i = 0; i < allEdges.Count; i++)
        {
            positionA[i] = allEdges[i].A;
            positionB[i] = allEdges[i].B;
            foundOne[i] = 0;
        }
        //get compute shader from map
        ComputeShader compute = AssetDatabase.LoadAssetAtPath<ComputeShader>("Assets/Scripts/ComputeShader/GetEdgeOuterShader.compute");
        int _kernel = compute.FindKernel("CSMain4");

        //create compute buffer
        ComputeBuffer positionABuffer = new ComputeBuffer(allEdges.Count, sizeof(float) * 3);
        ComputeBuffer positionBBuffer = new ComputeBuffer(allEdges.Count, sizeof(float) * 3);
        ComputeBuffer foundOneBuffer = new ComputeBuffer(allEdges.Count, sizeof(int));

        //link buffer
        compute.SetBuffer(_kernel, "positionA", positionABuffer);
        compute.SetBuffer(_kernel, "positionB", positionBBuffer);
        compute.SetBuffer(_kernel, "foundOne", foundOneBuffer);

        //set data to compute buffers
        positionABuffer.SetData(positionA);
        positionBBuffer.SetData(positionB);
        foundOneBuffer.SetData(foundOne);
        compute.SetInt("count", allEdges.Count);

        int allEdgesSplitsCount = allEdges.Count / 1000;
        xGroup = Mathf.RoundToInt((allEdgesSplitsCount / 11.0f) + 0.5f);
        for (int i = 0; i < allEdgesSplitsCount; i++)
        {
            compute.SetInt("startPoint", allEdgesSplitsCount * i);
            //dispatch to compute shader
            compute.Dispatch(_kernel, xGroup, yGroup, 1);
        }
        
        //get data
        foundOneBuffer.GetData(foundOne);

        //release all compute buffers
        positionABuffer.Release();
        positionBBuffer.Release();
        foundOneBuffer.Release();

        //please all outer edge in site a list
        for (int i = 0; i < foundOne.Length; i++)
        {
            if (foundOne[i] == 0)
            {
                edges.Add(allEdges[i]);
            }
        }
        //draw outer edge
        foreach (Edge edge in edges)
        {
            edge.Draw();
        }
        //return outer edge
        return edges;
    }

    public static List<Edge> GetEdges3(Mesh mesh, List<Edge> allEdges)
    {

        List<Edge> edges = new List<Edge>();

        List<int> remove = new List<int>();
        //set x & y tread groups can not be higher than 65535
        int xGroup;
        int yGroup = Mathf.RoundToInt((allEdges.Count / 92.0f));


        Vector3[] positionA = new Vector3[allEdges.Count];
        Vector3[] positionB = new Vector3[allEdges.Count];
        int[] foundOne = new int[allEdges.Count];
        //splits all Edges to different array
        for (int i = 0; i < allEdges.Count; i++)
        {
            positionA[i] = allEdges[i].A;
            positionB[i] = allEdges[i].B;
            foundOne[i] = 0;
        }
        //get compute shader from map
        ComputeShader compute = AssetDatabase.LoadAssetAtPath<ComputeShader>("Assets/Scripts/ComputeShader/GetEdgeOuterShader.compute");
        int _kernel = compute.FindKernel("CSMain5");

        //create compute buffer
        ComputeBuffer positionABuffer = new ComputeBuffer(allEdges.Count, sizeof(float) * 3);
        ComputeBuffer positionBBuffer = new ComputeBuffer(allEdges.Count, sizeof(float) * 3);
        ComputeBuffer foundOneBuffer = new ComputeBuffer(allEdges.Count, sizeof(int));

        //link buffer
        compute.SetBuffer(_kernel, "positionA", positionABuffer);
        compute.SetBuffer(_kernel, "positionB", positionBBuffer);
        compute.SetBuffer(_kernel, "foundOne", foundOneBuffer);

        //set data to compute buffers
        positionABuffer.SetData(positionA);
        positionBBuffer.SetData(positionB);
        foundOneBuffer.SetData(foundOne);
        compute.SetInt("count", allEdges.Count);

        int allEdgesSplitsCount = allEdges.Count / 1000;
        xGroup = Mathf.RoundToInt((allEdgesSplitsCount / 32.0f) + 0.5f);
        yGroup = Mathf.RoundToInt((allEdgesSplitsCount / 32.0f) + 0.5f);

        for (int i = 0; i < allEdgesSplitsCount; i++)
        {
            for (int j = 0; j < allEdgesSplitsCount; j++)
            {
                compute.SetInt("startPoint", allEdgesSplitsCount * i);
                compute.SetInt("startPointY", allEdgesSplitsCount * j);

                //dispatch to compute shader
                compute.Dispatch(_kernel, xGroup, yGroup, 1);
            }
        }

        //get data
        foundOneBuffer.GetData(foundOne);

        //release all compute buffers
        positionABuffer.Release();
        positionBBuffer.Release();
        foundOneBuffer.Release();

        //please all outer edge in site a list
        for (int i = 0; i < foundOne.Length; i++)
        {
            if (foundOne[i] == 0)
            {
                edges.Add(allEdges[i]);
            }
        }
        //draw outer edge
        foreach (Edge edge in edges)
        {
            edge.Draw();
        }
        //return outer edge
        return edges;
    }

    public static List<Edge> GetAllEdge(Mesh mesh)
    {
        Vector3[] points = mesh.vertices; // The mesh’s vertices
        int[] triangles = mesh.triangles; // The mesh’s triangle

        List<Edge> edges = new List<Edge>();

        //create new Edge objects
        for (int i = 0; i < triangles.Length - 1; i += 3)
        {
            edges.Add(new Edge(points[triangles[i]], points[triangles[i + 1]], triangles[i], triangles[i + 1]));
            edges.Add(new Edge(points[triangles[i + 1]], points[triangles[i + 2]], triangles[i + 1], triangles[i + 2]));
            edges.Add(new Edge(points[triangles[i + 2]], points[triangles[i]], triangles[i + 2], triangles[i]));
        }
        //return all edges
        return edges;
    }

    public struct data
    {
        public float3 A;
        public float3 B;
        public int indexA;
        public int indexB;
    }

    public static List<data> GetAllEdges(Mesh mesh)
    {
        ComputeShader compute = AssetDatabase.LoadAssetAtPath<ComputeShader>("Assets/Scripts/ComputeShader/GetAllEdgesShader.compute");
        int _kernel = compute.FindKernel("CSMain");

        Vector3[] points = mesh.vertices; // The mesh’s vertices
        int[] triangles = mesh.triangles; // The mesh’s triangle

        List<Edge> edges = new List<Edge>();

        int xGroup = Mathf.RoundToInt((triangles.Length /3 / 32));

        ComputeBuffer vertices = new ComputeBuffer(points.Length, sizeof(float) * 3);
        ComputeBuffer trianglesBuffer = new ComputeBuffer(triangles.Length, sizeof(int));
        ComputeBuffer countBuffer = new ComputeBuffer(1, sizeof(int), ComputeBufferType.IndirectArguments);
        ComputeBuffer result = new ComputeBuffer(triangles.Length * 2, sizeof(float) * 4, ComputeBufferType.Append);
        result.SetCounterValue(0);

        compute.SetBuffer(_kernel, "Result", result);
        compute.SetBuffer(_kernel, "vertices", vertices);
        compute.SetBuffer(_kernel, "triangles", trianglesBuffer);

        vertices.SetData(points);
        trianglesBuffer.SetData(triangles);

        compute.Dispatch(_kernel, xGroup, xGroup, 1);
        
        ComputeBuffer.CopyCount(result, countBuffer, 0);

        int[] counter = new int[1] { 0 };
        countBuffer.GetData(counter);

        int count = counter[0];

        data[] data = new data[count];

        result.GetData(data);

        vertices.Release();
        trianglesBuffer.Release();
        countBuffer.Release();
        result.Release();

        //edges.AddRange(data);

        return data.ToList();
    }
}