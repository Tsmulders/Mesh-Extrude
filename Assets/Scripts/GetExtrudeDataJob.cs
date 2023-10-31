using System.Collections;
using System.Collections.Generic;
using System.Threading;
using Unity.Collections;
using Unity.Jobs;
using Unity.VisualScripting;
using UnityEngine;

public struct GetExtrudeDataJob : IJobFor
{
    public NativeQueue<int> queue;
    [ReadOnly]
    public NativeList<int> indexA;
    [ReadOnly]
    public NativeList<int> indexB;

    public NativeList<int> indexextrude;



    public void Execute(int index)
    {

        //Thread;
        //ComputeShader;
        if (indexextrude.Contains(indexA[index]) && !indexextrude.Contains(indexB[index]))
        {
            queue.Enqueue(indexB[index]);
        }
        if (!indexextrude.Contains(indexA[index]) && indexextrude.Contains(indexB[index]))
        {
            queue.Enqueue(indexA[index]);
        }
    }
}
