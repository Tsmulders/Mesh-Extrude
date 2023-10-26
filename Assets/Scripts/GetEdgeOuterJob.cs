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
    //public NativeArray<Vector3> vertices;
    //public NativeArray<int> triangles;

    [ReadOnly]
    public NativeList<Vector3> positionA;
    [ReadOnly]
    public NativeList<Vector3> positionB;
    //[ReadOnly]
    //public NativeList<int> indexA;
    //[ReadOnly]
    //public NativeList<int> indexB;

    public NativeArray<bool> foundOne;
    public NativeArray<int> indexFound;

    [ReadOnly]
    public NativeList<Vector3> positionACheck;
    [ReadOnly]
    public NativeList<Vector3> positionBCheck;
    [ReadOnly]
    public NativeList<int> indexACheck;
    [ReadOnly] 
    public NativeList<int> indexBCheck;
    public void Execute(int i)
    {
        foundOne[i] = false;
        for (int j = 0; j < positionA.Length; j++)
        {
            if (check(positionA[j], positionACheck[i]) &&
                check(positionB[j], positionBCheck[i]) ||
                check(positionA[j], positionBCheck[i]) &&
                check(positionB[j], positionACheck[i]))
            {
                foundOne[i] = true;
                indexFound[i] = j;
                break;
            }
        }
    }
    public bool check(Vector3 v1, Vector3 v2)
    {
        return Mathf.Abs(Vector3.Distance(v1, v2)) <= Mathf.Epsilon;
    }
}
