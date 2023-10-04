using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

public class flatpolygonalseeker : MonoBehaviour
{
    Mesh mesh;
    void Start()
    {
        mesh = GetComponent<MeshFilter>().mesh;
        LooseSurface(mesh);
    }

    void LooseSurface(Mesh mesh)
    {
        MechVerticesMerge2_0.AutoWeld(mesh, 0.0001f);
        List<Edge> edges = new List<Edge>();
        edges = GetEdgesOfMesh.GetEdge(mesh);
        List<Edge> edgesCircle = new List<Edge>();
        edgesCircle.Add(edges[0]);

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

                edgesCircle.Add(edges[i]);
                j++;
                goto _l2;
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
        for (int i = 0; i < edgesCircle.Count; i++)
        {
            if (!indexEdges.Contains(edgesCircle[i].indexA))
            {
                indexEdges.Add(edgesCircle[i].indexA);
            }
        }
        Debug.Log("afadfe");

        Vector3[] verts = mesh.vertices;

        for (int i = 0; i < verts.Length; i++)
        {
            for (int k = 0; k < edgesCircle.Count; k++)
            {

            }
        }
    }
}
