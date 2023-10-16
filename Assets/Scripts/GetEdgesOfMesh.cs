using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using UnityEngine;

public class GetEdgesOfMesh : MonoBehaviour
{
    

    public static List<Edge> GetEdge(Mesh mesh)
    {
        Vector3[] points = mesh.vertices; // The mesh’s vertices
        int[] indicies = mesh.triangles; // The mesh’s triangle indicies

        List<Edge> edges = new List<Edge>();

        // for every two triangle indicies
        for (int i = 0; i < indicies.Length - 1; i += 3)
        {
            // Create a new edge with the corresponding points
            // and add it to edge list
            Edge edge = new Edge(points[indicies[i]], points[indicies[i + 1]], indicies[i], indicies[i + 1]);
            Edge edge1 = new Edge(points[indicies[i + 1]], points[indicies[i + 2]], indicies[i + 1], indicies[i + 2]);
            Edge edge2 = new Edge(points[indicies[i + 2]], points[indicies[i]], indicies[i + 2], indicies[i]);

            bool found = false;
            bool found1 = false;
            bool found2 = false;
            foreach (Edge e in edges)
            {
                if (e.AlmostEqual(edge))
                {
                    found = true;

                    edges.Remove(e);
                    break;
                }
            }
            foreach (Edge e in edges)
            {
                if (e.AlmostEqual(edge1))
                {
                    found1 = true;
                    edges.Remove(e);
                    break;
                }
            }
            foreach (Edge e in edges)
            {
                if (e.AlmostEqual(edge2))
                {
                    found2 = true;
                    edges.Remove(e);
                    break;
                }
            }
            if (!found) edges.Add(edge);
            if (!found1) edges.Add(edge1);
            if (!found2) edges.Add(edge2);

            // if point a != point b (meaning there is an edge)
            if (points[indicies[i]] != points[indicies[i + 1]])
            {
                
            }
        }

        foreach (Edge edge in edges)
        {
            edge.Draw();
        }
        return edges;
    }

    private static List<Vector3> GetStraightEdges(List<Edge> edges)
    {
        List<Edge> straightEdges = new List<Edge>();

        foreach (Edge edge in edges)
        {
            if (edge.A.x == edge.B.x ||
            edge.A.y == edge.B.y)
            {
                // edge is straight and not diagonal
                straightEdges.Add(edge);
            }
            else
            {
                // edge is diagonal and can be discarded
            }
        }

        List<Vector3> redPoints = GetPoints(straightEdges);
        return redPoints;
    }

    private static List<Vector3> GetPoints(List<Edge> straightEdges)
    {
        List<Vector3> Points = new List<Vector3>();

        foreach (Edge edge in straightEdges)
        {
            Points.Add(edge.A);
            Points.Add(edge.B);
        }

        return Points;
    }

    public static List<Edge> GetAllEdge(Mesh mesh)
    {
        Vector3[] points = mesh.vertices; // The mesh’s vertices
        int[] indicies = mesh.triangles; // The mesh’s triangle indicies

        List<Edge> edges = new List<Edge>();

        for (int i = 0; i < indicies.Length - 1; i += 3)
        {
            Edge edge = new Edge(points[indicies[i]], points[indicies[i + 1]], indicies[i], indicies[i + 1]);
            Edge edge1 = new Edge(points[indicies[i + 1]], points[indicies[i + 2]], indicies[i + 1], indicies[i + 2]);
            Edge edge2 = new Edge(points[indicies[i + 2]], points[indicies[i]], indicies[i + 2], indicies[i]);

            if (!edges.Contains(edge))
            {
                edges.Add(edge);
            }
            if (!edges.Contains(edge1))
            {
                edges.Add(edge1);
            }
            if (!edges.Contains(edge2))
            {
                edges.Add(edge2);
            }
        }

        return edges;
    }
}


