using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

public class flatpolygonalseeker : MonoBehaviour
{
    public static ExtrudeData[] LooseSurface(Mesh mesh)
    {
        List<ExtrudeData> extrude = new List<ExtrudeData>();

        List<Edge> Alledges = new List<Edge>();
        Alledges.AddRange(GetEdgesOfMesh.GetAllEdge(mesh));

        List<Edge> edges = new List<Edge>();
        edges = GetEdgesOfMesh.GetEdge(mesh, Alledges);
        List<Edge> edgesCircle = new List<Edge>();

        if (edges == null || edges.Count == 0) return extrude.ToArray();

        edgesCircle.Add(edges[0]);
        List<int> notChosen = new List<int>();
        int j = 0;

        List<Edge> edgesdone = new List<Edge>();
        edgesdone.AddRange(edges);

        _l2:
        for (int i = 0; i < edgesdone.Count; i++)
        {
            if (edges[i].indexA == edgesdone[i].indexB)
            {
                i++;
            }
            if (edgesCircle[j].indexB == edgesdone[i].indexA)
            {
                if (!edgesCircle.Contains(edgesdone[i]))
                {
                    edgesCircle.Add(edgesdone[i]);
                    j++;
                    goto _l2;
                }
            }
            if (!edgesCircle.Contains(edgesdone[i]))
            {
                notChosen.Add(i);
            }
        }

        if (notChosen.Count == 0 && edgesCircle[0].indexA != edgesCircle[edgesCircle.Count - 1].indexB)
        {
            return extrude.ToArray();
        }
        if (edgesCircle[0].indexA != edgesCircle[edgesCircle.Count - 1].indexB) 
        {
            for (int i = 0; i < edgesCircle.Count; i++)
            {
                edgesdone.Remove(edgesCircle[i]);
            }
            if (edgesdone.Count > 0)
            {
            edgesCircle.Clear();
            edgesCircle.Add(edgesdone[0]);
            notChosen.Clear();
            j = 0;
            goto _l2;
            }
        }

        Debug.Log("lose edge");

        //extract all vertices from list
        List<int> indexEdges = new List<int>();

        Vector3[] verts = mesh.vertices;
        List<int> allvertsExtrude = new List<int>();


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
        indexEdges.Sort();

        extrude.Add(new ExtrudeData(indexEdges, edgesCircle));

        if (notChosen.Count != 0)
        {
            for (int i = 0; i < edgesCircle.Count; i++)
            {
                edgesdone.Remove(edgesCircle[i]);
            }
            if (edgesdone.Count > 0)
            {
                edgesCircle.Clear();
                edgesCircle.Add(edgesdone[0]);
                notChosen.Clear();
                j = 0;
                goto _l2;
            }
        }

        return extrude.ToArray();
    }
}
