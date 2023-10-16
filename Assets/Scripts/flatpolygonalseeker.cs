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



    public static ExtrudeData[] LooseSurface(Mesh mesh)
    {
        List<ExtrudeData> extrude = new List<ExtrudeData>();

        List<Edge> edges = new List<Edge>();
        
        edges = GetEdgesOfMesh.GetEdge(mesh);
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
            if (!notChosen.Contains(i))
            {
                notChosen.Add(i);
            }
        }
        //if (edgesCircle[0].indexA != edgesCircle[edgesCircle.Count - 1].indexB)
        //{
        //    return null;
        //}

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
            edgesCircle.Clear();
            edgesCircle.Add(edgesdone[notChosen[0]]);
            notChosen.Clear();
            j = 0;
             goto _l2;
        }

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
        indexEdges.Sort();

        extrude.Add(new ExtrudeData(indexEdges, edgesCircle));

        if (notChosen.Count != 0)
        {
            for (int i = 0; i < edgesCircle.Count; i++)
            {
                edgesdone.Remove(edgesCircle[i]);
            }
            edgesCircle.Clear();
            edgesCircle.Add(edges[notChosen[0]]);
            notChosen.Clear();
            indexEdges.Clear();
            j = 0;
            goto _l2;
        }

        return extrude.ToArray();
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
