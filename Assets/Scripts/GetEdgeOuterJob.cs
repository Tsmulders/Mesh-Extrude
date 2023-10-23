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

    public NativeList<Vector3> positionA;
    public NativeList<Vector3> positionB;

    public NativeList<int> indexA;
    public NativeList<int> indexB;

    public NativeArray<bool> foundOne;

    Vector3 positionACheck;
    Vector3 positionBCheck;
    int indexACheck;
    int indexBCheck;
    public void Execute(int i)
    {

        //for (int j = 0; j < positionA.Length; j++)
        //{
            if (check(positionA[i], positionACheck) &&
                check(positionB[i], positionBCheck) ||
                check(positionA[i], positionBCheck) &&
                check(positionB[i], positionACheck))
            {
                foundOne[i] = true;
                //positionA.RemoveAt(j);
                //positionB.RemoveAt(j);
                //indexA.RemoveAt(indexACheck);
                //indexB.RemoveAt(indexBCheck);
            }
        //}
        if (!foundOne[i])
        {
            positionA.Add(positionACheck);
            positionB.Add(positionBCheck);
            indexA.Add(indexACheck);
            indexB.Add(indexBCheck);
        }
    }
    public bool check(Vector3 v1, Vector3 v2)
    {
        return Mathf.Abs(Vector3.Distance(v1, v2)) <= Mathf.Epsilon;
    }
}
