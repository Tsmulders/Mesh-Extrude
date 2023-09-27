using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UIElements;

public class MeshExtrude : MonoBehaviour
{

    private Mesh mesh; 
    private Mesh mesh2;

    private List<Mesh> listMech;
    private Vector3[] normals;

    //debug test 
    public Vector3[] ar1;
    public Vector3[] ar2;
    public Vector3[] ar3;
    public int[] triangle;
    public List<Edge> edges = new List<Edge>();

    // Start is called before the first frame update
    void Start()
    {
        listMech = new List<Mesh>();
        mesh = GetComponent<MeshFilter>().mesh;
        listMech.Add(mesh);
        //objectMesh2.GetComponent<MeshFilter>().mesh = mesh;
        mesh2 = new Mesh();

        Vector3[] vertices = mesh.vertices;

        normals = mesh.normals;

        mesh2.vertices = new Vector3[mesh.vertices.Length];

        for (int i = 0; i < mesh2.vertices.Length; i++)
        {
            vertices[i] = mesh.vertices[i] + -mesh.normals[i] * 0.2f;
        }

        mesh2.vertices = vertices;
        mesh2.uv = mesh.uv;
        mesh2.triangles = mesh.triangles.Reverse().ToArray();

        ar1 = mesh.vertices;
        ar2 = mesh2.vertices;

        //mesh2.triangles = triangles;
        mesh2.RecalculateNormals();

        edges = GetEdgesOfMesh.GetEdge(mesh);

        mesh = GetComponent<MeshFilter>().mesh = CombinerMesh(mesh, mesh2);
        ar3 = GetComponent<MeshFilter>().mesh.vertices;


        triangle = GetComponent<MeshFilter>().mesh.triangles;

        //edges = GetEdges(triangle, ar1).ToArray(); //ar3 gebruiken voor de hele mech ar1 voor eerste mech.
        
        triangle = gameObject.GetComponent<MeshFilter>().mesh.triangles = CennectMeshes(edges).ToArray();

        RecalculateMesh();
    }

    void Update()
    {
        gameObject.GetComponent<MeshFilter>().mesh.triangles = triangle;
        gameObject.GetComponent<MeshFilter>().mesh.vertices = ar3;
    }

    void RecalculateMesh()
    {
        mesh2.RecalculateNormals();
        mesh2.RecalculateTangents();
        mesh2.RecalculateBounds();
    }

    Mesh CombinerMesh(in Mesh origin, in Mesh addition)
    {
        //MeshFilter[] meshFilters = new MeshFilter[meshes.Length];
        //for (int m = 0; m < meshes.Length; m++)
        //{
        //    meshFilters = meshes[m].GetComponentsInChildren<MeshFilter>();
        //}
        CombineInstance[] combine = new CombineInstance[2];
        combine[0].mesh = origin;
        combine[0].transform = transform.localToWorldMatrix;
        combine[1].mesh = addition;
        combine[1].transform = transform.localToWorldMatrix;

        Mesh mesh = new Mesh();
        mesh.CombineMeshes(combine, true, false, false);

        //gameObject.GetComponent<MeshFilter>().mesh = new Mesh();
        //gameObject.GetComponent<MeshFilter>().mesh.CombineMeshes(combine, true, true);
        //transform.gameObject.active = true;
        return mesh;
    }


    List<int> GetEdges(int[] triangle, Vector3[] vertices)
    {
        List<int> edges = new List<int>();
        
        for (int i = 0; i < vertices.Length -2; i++)
        {
            int occurrences = triangle.Count(x => x == i);
            if ((occurrences > 0 && occurrences < 5) || (occurrences > 8 && occurrences < 20))
            {
                edges.Add(i);
            }
        }
        return edges;
    }

    List<int> CennectMeshes(List<Edge> edgePoints)
    {
        List<int> trianglesList = new List<int>();
        int[] oneTriangel = new int[3];
        trianglesList.AddRange(mesh.triangles);

        //oneTriangel[0] = 440;
        //oneTriangel[1] = 3;
        //oneTriangel[2] = 2;
        //trianglesList.AddRange(oneTriangel);

        //oneTriangel[0] = 440;
        //oneTriangel[1] = 2;
        //oneTriangel[2] = 439;
        //trianglesList.AddRange(oneTriangel);

        //oneTriangel[0] = 5 + ar1.Length - 2;
        //oneTriangel[1] = 5;
        //oneTriangel[2] = 3;
        //trianglesList.AddRange(oneTriangel);

        //oneTriangel[0] = 5 + ar1.Length - 2;
        //oneTriangel[1] = 3;
        //oneTriangel[2] = 3 + ar1.Length - 2;
        //trianglesList.AddRange(oneTriangel);

        //oneTriangel[0] = 124 + ar1.Length - 2;
        //oneTriangel[1] = 124;
        //oneTriangel[2] = 115;
        //trianglesList.AddRange(oneTriangel);

        //oneTriangel[0] = 124 + ar1.Length - 2;
        //oneTriangel[1] = 115;
        //oneTriangel[2] = 115 + ar1.Length - 2;
        //trianglesList.AddRange(oneTriangel);

        /*569
        769
            767*/

        for (int i = 0; i < edgePoints.Count - 1; i++)
        {
            oneTriangel[0] = edgePoints[i].indexB + ar1.Length - 2;
            oneTriangel[1] = edgePoints[i].indexB;
            oneTriangel[2] = edgePoints[i].indexA;
            trianglesList.AddRange(oneTriangel);

            oneTriangel[0] = edgePoints[i].indexB + ar1.Length - 2;
            oneTriangel[1] = edgePoints[i].indexA;
            oneTriangel[2] = edgePoints[i].indexA + ar1.Length - 2;
            trianglesList.AddRange(oneTriangel);
        }
        return trianglesList;
    }

    /*
    punten kontroleren welke moeten aan elkaar moet worden gezet.
    die controleer je met de array van waar je een clone hebt van gemaakt en pakt die indsex
    die tel je er bij de array wat als eerste obejct toe je nog geen mesh was toe gevoegt

    array first object              dit kan ook een int zijn van de lengte van de array.
    array flat serves
    array flat serves clone 
    array end object                dit kan ook een int zijn van de lengte van de array.

    zet je bij de triangels 3 niewe int 
    [0] index flat serves
    [1] om de 2 point te zoeken moet ik nog even over na denken
    [2] index flat serves clone + first obect - 2

    -2 is voor de 0 van de array af te tellen

 */

    private void OnDrawGizmos()
    {
        //if (mesh)
        //{
        //    for (int i = 0; i < edges.Length; i++)
        //    {
        //        Gizmos.color = Color.green;
        //        Gizmos.DrawWireSphere(transform.TransformPoint(mesh.vertices[edges[i]]), 0.04f);
                

        //    }

        //}
    }
}
