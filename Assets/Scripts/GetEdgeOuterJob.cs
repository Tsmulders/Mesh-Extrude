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

    public NativeArray<bool> foundOne;

    float3 positionACheck;
    float3 positionBCheck;
    int indexACheck;
    int indexBCheck;


    public List<Edge> edges; //kan geen custom class door sturen. moet omvormen naar rouwe data

    public void Execute(int i)
    {
        bool found = false;
        foreach (Edge e in edges)
        {
            if (e.AlmostEqual(edges[1]))
            {
                found = true;
                //positionA.Remove(positionACheck);
                //positionB.RemoveAt(positionBCheck);
                //indexA.RemoveAt(indexACheck);
                //indexB.RemoveAt(indexBCheck);
                break;
            }
        }
        if (!found)
        {
            positionA.Add(positionACheck);
            positionB.Add(positionBCheck);
            indexA.Add(indexACheck);
            indexB.Add(indexBCheck);
        }
    }
}
