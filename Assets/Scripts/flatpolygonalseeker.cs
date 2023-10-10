using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

public class flatpolygonalseeker : MonoBehaviour
{
    Mesh mesh;
    List<int> fadfa = new List<int>();
    Vector3[] verts12345;
    void Start()
    {
        mesh = GetComponent<MeshFilter>().mesh;
        verts12345 = mesh.vertices;
        LooseSurface(mesh);
    }

    public static int[] LooseSurface(Mesh mesh)
    {
        
        List<Edge> edges = new List<Edge>();
        edges = GetEdgesOfMesh.GetEdge(mesh);
        List<Edge> edgesCircle = new List<Edge>();
        if (edges == null || edges.Count == 0) return null;
        edgesCircle.Add(edges[0]);
        List<int> nietgekozen = new List<int>();
        int j = 0;
        _l2:
        for (int i = 0; i < edges.Count; i++)
        {
            if (edges[i].indexA == edges[i].indexB)
            {
                i++;
            }
            if (edgesCircle[j].indexB == edges[i].indexA)
            {
                if (!edgesCircle.Contains(edges[i]))
                {
                    edgesCircle.Add(edges[i]);
                    j++;
                    goto _l2;
                }
            }
            //nietgekozen.Add(i);
        }
        if (edgesCircle[0].indexA != edgesCircle[edgesCircle.Count - 1].indexB)
        {
            return null;
        }

        //if (nietgekozen == null)
        //{
        //    return null;
        //}
        //if (edgesCircle[0].indexA != edgesCircle[edgesCircle.Count - 1].indexB)
        //{
        //    edgesCircle.Clear();
        //    edgesCircle.Add(edges[nietgekozen[0]]);
        //    nietgekozen.Clear();
        //}


        Debug.Log("lose edge");

        //extract all vertices from list
        List<int> indexEdges = new List<int>();

        Vector3[] verts = mesh.vertices;
        List<int> allvertsExtrude = new List<int>();
        List<Edge> Alledges = new List<Edge>();
        Alledges.AddRange(GetEdgesOfMesh.GetAllEdge(mesh));

        for (int i = 0; i < edgesCircle.Count; i++)
        {
            if (!indexEdges.Contains(edgesCircle[i].indexA))
            {
                indexEdges.Add(edgesCircle[i].indexA);
            }
        }
        _l3:
        for (int i = 0; i < Alledges.Count; i++)
        {
            if (indexEdges.Contains(Alledges[i].indexA) && !indexEdges.Contains(Alledges[i].indexB))
            {
                indexEdges.Add(Alledges[i].indexB);
                goto _l3;
            }
        }

        return indexEdges.ToArray();
    }
    private void OnDrawGizmos()
    {
        if (mesh)
        {
            for (int i = 0; i < fadfa.Count; i++)
            {
                Gizmos.color = Color.green;
                Gizmos.DrawWireSphere(transform.TransformPoint(mesh.vertices[fadfa[i]]), 0.1f);
            }
        }
    }

}
