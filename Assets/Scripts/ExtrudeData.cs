using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ExtrudeData
{
    public int[] indexEdges;
    public Edge[] edgesCircle;

    public ExtrudeData(List<int> index, Edge[] edges)
    {
        indexEdges = index.ToArray();
        edgesCircle = edges;
    }
}
