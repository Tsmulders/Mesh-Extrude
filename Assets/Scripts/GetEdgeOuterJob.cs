using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Jobs;
using Unity.Collections;
using Unity.Burst;
using Unity.Mathematics;

[BurstCompile]
public struct GetEdgeOuterJob : IJobFor
{
    public NativeArray<float3> vertices;
    public NativeArray<int> triangles;

    public NativeList<float3> positionA;
    public NativeList<float3> positionB;
    public NativeList<int> indexA;
    public NativeList<int> indexB;

    public List<Edge> edges; //kan geen custom class door sturen. moet omvormen naar rouwe data

    public void Execute(int i)
    {

        Edge edge = new Edge(vertices[triangles[i]], vertices[triangles[i + 1]], triangles[i], triangles[i + 1]);
        Edge edge1 = new Edge(vertices[triangles[i + 1]], vertices[triangles[i + 2]], triangles[i + 1], triangles[i + 2]);
        Edge edge2 = new Edge(vertices[triangles[i + 2]], vertices[triangles[i]], triangles[i + 2], triangles[i]);

        bool found = false;
        bool found1 = false;
        bool found2 = false;
        foreach (Edge e in edges)
        {
            if (e.AlmostEqual(edge))
            {
                found = true;

                edges.Remove(e);
                break;
            }
        }
        foreach (Edge e in edges)
        {
            if (e.AlmostEqual(edge1))
            {
                found1 = true;
                edges.Remove(e);
                break;
            }
        }
        foreach (Edge e in edges)
        {
            if (e.AlmostEqual(edge2))
            {
                found2 = true;
                edges.Remove(e);
                break;
            }
        }
        if (!found) edges.Add(edge);
        if (!found1) edges.Add(edge1);
        if (!found2) edges.Add(edge2);

        //if (!found)
        //{
        //    positionA.Add(edge.A);
        //    positionB.Add(edge.B);
        //    indexA.Add(edge.indexA);
        //    indexB.Add(edge.indexB);
        //}
        //if (found1)
        //{
        //    positionA.Add(edge1.A);
        //    positionB.Add(edge1.B);
        //    indexA.Add(edge1.indexA);
        //    indexB.Add(edge1.indexB);
        //}
        //if (found2)
        //{
        //    positionA.Add(edge2.A);
        //    positionB.Add(edge2.B);
        //    indexA.Add(edge2.indexA);
        //    indexB.Add(edge2.indexB);
        //}
    }
}
