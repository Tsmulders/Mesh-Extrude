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

    public float threshold = 0.0001f;
    public float extrudeStrength = 0;

    // Start is called before the first frame update
    void Start()
    {

        listMech = new List<Mesh>();
        mesh = GetComponent<MeshFilter>().mesh;
        listMech.Add(mesh);

        float furthestDistance = furtherPoint(mesh);
        threshold = furthestDistance / 500;
        extrudeStrength = furthestDistance / 50;

        MechVerticesMerge2_0.AutoWeld(mesh, threshold);

        mesh = GetComponent<MeshFilter>().mesh;

        Debug.Log(mesh.vertices);

        mesh2 = clonemesh(mesh, extrudeStrength);

        edges = GetEdgesOfMesh.GetEdge(mesh);

        mesh = GetComponent<MeshFilter>().mesh = CombinerMesh(mesh, mesh2);
        ar3 = GetComponent<MeshFilter>().mesh.vertices;


        triangle = GetComponent<MeshFilter>().mesh.triangles;

        triangle = gameObject.GetComponent<MeshFilter>().mesh.triangles = CennectMeshes(edges).ToArray();

        mesh = GetComponent<MeshFilter>().mesh;

        RecalculateMesh(mesh);
    }

    void Update()
    {
        //gameObject.GetComponent<MeshFilter>().mesh.triangles = triangle;
        gameObject.GetComponent<MeshFilter>().mesh.vertices = ar3;

    }

    void RecalculateMesh(Mesh meshRecalculate)
    {
        meshRecalculate.RecalculateNormals();
        meshRecalculate.RecalculateTangents();
        meshRecalculate.RecalculateBounds();
        meshRecalculate.RecalculateUVDistributionMetrics();
    }

    private Mesh clonemesh(Mesh original, float extrudeStrength)
    {
        Mesh clone = new Mesh();
        Vector3[] vertices = original.vertices;
        clone.vertices = new Vector3[original.vertices.Length];

        for (int i = 0; i < clone.vertices.Length; i++)
        {
            vertices[i] = original.vertices[i] + -original.normals[i] * extrudeStrength;
        }

        clone.vertices = vertices;
        clone.uv = original.uv;
        clone.triangles = original.triangles.Reverse().ToArray();

        ar1 = original.vertices;
        ar2 = clone.vertices;

        clone.RecalculateNormals();

        return clone;
    }

    Mesh CombinerMesh(in Mesh original, in Mesh addition)
    {
        //MeshFilter[] meshFilters = new MeshFilter[meshes.Length];
        //for (int m = 0; m < meshes.Length; m++)
        //{
        //    meshFilters = meshes[m].GetComponentsInChildren<MeshFilter>();
        //}
        CombineInstance[] combine = new CombineInstance[2];
        combine[0].mesh = original;
        combine[0].transform = transform.localToWorldMatrix;
        combine[1].mesh = addition;
        combine[1].transform = transform.localToWorldMatrix;

        Mesh mesh = new Mesh();
        mesh.CombineMeshes(combine, true, false, false);

        return mesh;
    }

    List<int> CennectMeshes(List<Edge> edgePoints)
    {
        List<int> trianglesList = new List<int>();
        int[] oneTriangel = new int[3];
        trianglesList.AddRange(mesh.triangles);
        //calculate the new triangles
        for (int i = 0; i < edgePoints.Count; i++)
        {
            if (edgePoints[i].indexA == edgePoints[i].indexB)
            {
                //i++;
            }

            oneTriangel[0] = edgePoints[i].indexB + ar1.Length;
            oneTriangel[1] = edgePoints[i].indexB;
            oneTriangel[2] = edgePoints[i].indexA;
            trianglesList.AddRange(oneTriangel);

            oneTriangel[0] = edgePoints[i].indexB + ar1.Length;
            oneTriangel[1] = edgePoints[i].indexA;
            oneTriangel[2] = edgePoints[i].indexA + ar1.Length;
            trianglesList.AddRange(oneTriangel);

            //oneTriangel[0] = edgePoints[i].indexA + ar1.Length - 2;
            //oneTriangel[1] = edgePoints[i].indexA;
            //oneTriangel[2] = edgePoints[i].indexB;
            //trianglesList.AddRange(oneTriangel);

            //oneTriangel[0] = edgePoints[i].indexA + ar1.Length - 2;
            //oneTriangel[1] = edgePoints[i].indexB;
            //oneTriangel[2] = edgePoints[i].indexB + ar1.Length - 2;
            //trianglesList.AddRange(oneTriangel);
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

    float furtherPoint(Mesh mesh)
    {
        float furthestDistance = 0;
        Vector3[] vertices = mesh.vertices;
        for (int i = 0; i < vertices.Length; i++)
        {
            for (int j = 0; j < vertices.Length; j++)
            {
                float distance = Vector3.Distance(vertices[i], vertices[j]);
                if (furthestDistance < distance)
                {
                    furthestDistance = distance;
                }
            }
        }
        return furthestDistance;
    }



    private void OnDrawGizmos()
    {
        if (mesh)
        {
            for (int i = 0; i < edges.Count; i++)
            {

                if (edges[i].indexA == edges[i].indexB)
                {
                    Gizmos.color = Color.yellow;
                    Gizmos.DrawWireSphere(transform.TransformPoint(mesh.vertices[edges[i].indexB]), 0.05f);
                }
                else 
                {

                Gizmos.color = Color.green;
                Gizmos.DrawWireSphere(transform.TransformPoint(mesh.vertices[edges[i].indexB]), 0.04f);
                }
            }
        }
    }
}
