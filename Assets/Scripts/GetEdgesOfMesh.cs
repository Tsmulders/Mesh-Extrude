using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using UnityEngine;

public class GetEdgesOfMesh : MonoBehaviour
{
    public static List<int> GetEdges(int[] triangle)
    {
        List<int> edges = new List<int>();

        for (int i = 0; i < triangle.Length; i++)
        {
            int occurrences = triangle.Count(x => x == i);
            if (occurrences < 5)
            {
                edges.Add(i);
            }
        }
        return edges;
    }
}



