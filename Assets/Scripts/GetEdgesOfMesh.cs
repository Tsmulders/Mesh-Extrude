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
using UnityEngine;
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
                    //positionA.RemoveAt(edgesJob.indexFound[j]);
                    //positionB.RemoveAt(edgesJob.indexFound[j]);
                    //indexA.RemoveAt(edgesJob.indexFound[j]);
                    //indexB.RemoveAt(edgesJob.indexFound[j]);
                }
                else if (!edgesJob.foundOne[j])
                {
                    edges.Add(edge[j]);
                    //positionA.Add(edgesJob.positionACheck[j]);
                    //positionB.Add(edgesJob.positionBCheck[j]);
                    //indexA.Add(edgesJob.indexACheck[j]);
                    //indexB.Add(edgesJob.indexBCheck[j]);
                }
            }

            positionACheck.Dispose();
            positionBCheck.Dispose();
            indexACheck.Dispose();
            indexBCheck.Dispose();
            foundOne.Dispose();
            indexFound.Dispose();
            positionA.Dispose();
            positionB.Dispose();
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


    public static List<Edge> GetAllEdge(Mesh mesh)
    {
        Vector3[] points = mesh.vertices; // The mesh’s vertices
        int[] indicies = mesh.triangles; // The mesh’s triangle indicies

        List<Edge> edges = new List<Edge>();

        for (int i = 0; i < indicies.Length - 1; i += 3)
        {
            Edge edge = new Edge(points[indicies[i]], points[indicies[i + 1]], indicies[i], indicies[i + 1]);
            Edge edge1 = new Edge(points[indicies[i + 1]], points[indicies[i + 2]], indicies[i + 1], indicies[i + 2]);
            Edge edge2 = new Edge(points[indicies[i + 2]], points[indicies[i]], indicies[i + 2], indicies[i]);

            if (!edges.Contains(edge))
            {
                edges.Add(edge);
            }
            if (!edges.Contains(edge1))
            {
                edges.Add(edge1);
            }
            if (!edges.Contains(edge2))
            {
                edges.Add(edge2);
            }
        }

        return edges;
    }
}


