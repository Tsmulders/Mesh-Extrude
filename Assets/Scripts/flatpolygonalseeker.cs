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

    void LooseSurface(Mesh mesh)
    {
        MechVerticesMerge2_0.AutoWeld(mesh, 0.0001f);
        List<Edge> edges = new List<Edge>();
        edges = GetEdgesOfMesh.GetEdge(mesh);
        List<Edge> edgesCircle = new List<Edge>();
        if (edges == null || edges.Count == 0) return;
        edgesCircle.Add(edges[0]);

        int j = 0;
        _l2:
        for (int i = 0; i < edges.Count; i++)
        {
            if (edgesCircle.Count > 2000)
            {
                break;
            }
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
        }

        if (edgesCircle[0].indexA != edgesCircle[edgesCircle.Count -1].indexB)
        {
            Debug.Log("het is niet lose");
            //return;
        }
            Debug.Log("het is een lose edge");

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

        fadfa.AddRange(indexEdges);
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
