using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Edge
{
    public Vector3 A;
    public Vector3 B;
    public int indexA;
    public int indexB;

    // Creates an object that represents
    // a line from A to B (an edge)
    public Edge(Vector3 a, Vector3 b, int inA, int inB)
    {
        A = a;
        B = b;
        indexA = inA;
        indexB = inB;
    }
    public void Draw()
    {
        Debug.DrawLine(A, B, UnityEngine.Color.red, 1000);
    }
    public bool AlmostEqual(Edge b)
    {
        //vergelijken met epsilon   Mathf.Epsilon  
        bool equal = false;

        if (check(this.A, b.A) &&
            check(this.B, b.B) ||
            check(this.A, b.B) &&
            check(this.B, b.A)) return true;

        return equal;
    }

    public bool check(Vector3 v1, Vector3 v2)
    {
        return Mathf.Abs(Vector3.Distance(v1, v2)) <= Mathf.Epsilon;
    }



}
