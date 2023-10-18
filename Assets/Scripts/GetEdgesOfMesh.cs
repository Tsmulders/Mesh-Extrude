using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Reflection;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.UIElements;

public class GetEdgesOfMesh : MonoBehaviour
{
    

    public static List<Edge> GetEdge(Mesh mesh)
    {
        /*
         * Multithread  Unity's JOB SYSTEM
         */

        Vector3[] points = mesh.vertices; // The mesh’s vertices
        int[] indicies = mesh.triangles; // The mesh’s triangle indicies

        NativeArray<float3> vertices = new NativeArray<float3>(points.Length, Allocator.TempJob);
        NativeArray<int> triangles = new NativeArray<int>(indicies.Length, Allocator.TempJob);

        List<Edge> edges = new List<Edge>();

        for (int i = 0; i < points.Length; i++)
        {
            vertices[i] = points[i];
        }

        for (int i = 0; i < indicies.Length; i++)
        {
            triangles[i] = indicies[i];
        }



        var edgesJob = new GetEdgeOuterJob()
        {
            vertices = vertices,
            triangles = triangles,
        };

        edgesJob.Run(indicies.Length);

        JobHandle sheduleJobDependency = new JobHandle();
        JobHandle sheduleJobHandle = edgesJob.Schedule(indicies.Length, sheduleJobDependency);
        JobHandle sheduleParralelJobHandle = edgesJob.ScheduleParallel(indicies.Length, 1, sheduleJobHandle);

        sheduleParralelJobHandle.Complete();

        vertices.Dispose();


        for (int i = 0; i < indicies.Length - 1; i += 3)
        {
            Edge[] edge = new Edge[3];
            
            edge[0] = new Edge(vertices[triangles[i]], vertices[triangles[i + 1]], triangles[i], triangles[i + 1]);
            edge[1] = new Edge(vertices[triangles[i + 1]], vertices[triangles[i + 2]], triangles[i + 1], triangles[i + 2]);
            edge[2] = new Edge(vertices[triangles[i + 2]], vertices[triangles[i]], triangles[i + 2], triangles[i]);




            for (int j = 0; j < edgesJob.foundOne.Length; j++)
            {
                if (edgesJob.foundOne[j])
                {
                    edges.Remove(edge[j]);
                }
                else if (!edgesJob.foundOne[j])
                {
                    edges.Add(edge[j]);
                }
            }

        }




        //oude manier doet paar uur over
        //for every two triangle indicies
        //    for (int i = 0; i < indicies.Length - 1; i += 3)
        //    {
        //        // Create a new edge with the corresponding points
        //        // and add it to edge list
        //        Edge edge = new Edge(points[indicies[i]], points[indicies[i + 1]], indicies[i], indicies[i + 1]);
        //        Edge edge1 = new Edge(points[indicies[i + 1]], points[indicies[i + 2]], indicies[i + 1], indicies[i + 2]);
        //        Edge edge2 = new Edge(points[indicies[i + 2]], points[indicies[i]], indicies[i + 2], indicies[i]);

        //        bool found = false;
        //        bool found1 = false;
        //        bool found2 = false;
        //        foreach (Edge e in edges)
        //        {
        //            if (e.AlmostEqual(edge) && !found)
        //            {
        //                found = true;

        //                edges.Remove(e);
        //                break;
        //            }

        //        }
        //        foreach (Edge e in edges)
        //        {
        //            if (e.AlmostEqual(edge1))
        //            {
        //                found1 = true;
        //                edges.Remove(e);
        //                break;
        //            }
        //        }
        //        foreach (Edge e in edges)
        //        {
        //            if (e.AlmostEqual(edge2))
        //            {
        //                found2 = true;
        //                edges.Remove(e);
        //                break;
        //            }
        //        }
        //        if (!found) edges.Add(edge);
        //        if (!found1) edges.Add(edge1);
        //        if (!found2) edges.Add(edge2);
        //    }


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


