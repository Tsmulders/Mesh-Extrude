using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Triangles
{
    public int indexA;
    public int indexB;
    public int indexC;
    public List<int> triangleIndex = new List<int>();

    public Triangles( int inA, int inB, int inC)
    {
        triangleIndex.Add(inA);
        triangleIndex.Add(inB);
        triangleIndex.Add(inC);
        indexA = inA;
        indexB = inB;
        indexC = inC;
    }  
}
