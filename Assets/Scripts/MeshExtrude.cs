using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using Unity.VisualScripting.Antlr3.Runtime;
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

    int[] extrudevertex;

    // Start is called before the first frame update
    void Awake()
    {

        listMech = new List<Mesh>();
        mesh = GetComponent<MeshFilter>().mesh;



        float furthestDistance = furtherPoint(mesh);
        threshold = furthestDistance / 500;
        extrudeStrength = furthestDistance / 50;

        MechVerticesMerge2_0.AutoWeld(mesh, threshold);

        mesh = GetComponent<MeshFilter>().mesh;
        ar3 = GetComponent<MeshFilter>().mesh.vertices;

        extrudevertex = flatpolygonalseeker.LooseSurface(mesh).ToArray();
        if (extrudevertex == null)
        {
            Debug.Log("there are no lose polygons");
            return;
        }

        edges = GetEdgesOfMesh.GetEdge(mesh);

        mesh2 = clonemesh(mesh, extrudevertex, extrudeStrength);


       mesh = GetComponent<MeshFilter>().mesh = CombinerMesh(mesh, mesh2);
        
        triangle = GetComponent<MeshFilter>().mesh.triangles;
        ar3 = GetComponent<MeshFilter>().mesh.vertices;

        triangle = gameObject.GetComponent<MeshFilter>().mesh.triangles = CennectMeshes(edges, extrudevertex).ToArray();

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

    private Mesh clonemesh(Mesh original, int[] verticesIndex, float extrudeStrength)
    {
        //om de triangles te krijgen sla ze al op in get edges of mesh
        Mesh clone = new Mesh();
        List<Vector3> vertices = new List<Vector3>();
        List<Vector2> uv = new List<Vector2>();
        List<int> triangles = new List<int>();
        //for (int i = 0; i < verticesIndex.Length; i++)
        //{
        //    vertices.Add(original.vertices[verticesIndex[i]]);
        //    uv.Add(original.uv[verticesIndex[i]]);
        //}

        for (int i = 0; i < verticesIndex.Length; i++)
        {
            vertices.Add(original.vertices[verticesIndex[i]] + -original.normals[verticesIndex[i]] * extrudeStrength);
            uv.Add(original.uv[verticesIndex[i]]);
        }

        Triangles[] triangles1;
        triangles1 = GetTriangles.getTrianglesGivenIndex(verticesIndex, original).ToArray();

        for (int i = 0; i < triangles1.Length; i++)
        {
            triangles.AddRange(triangles1[i].triangleIndex);
        }

        //index zit niet meer op de zelfde plaats. daarom kan de triagles het niet vinden. eerst te info finden hoe die moet veranderen en daarna het aanpassen

        List<List<int>> trianglesIndexchange = new List<List<int>>();
        List<int> Indexchange = new List<int>();
        for (int i = 0; i < verticesIndex.Length; i++)
        {
            for (int j = 0; j < triangles.Count; j++)
            {
                if (verticesIndex[i] == triangles[j])
                {
                    triangles[j] = i;
                }
            }
        }

        triangles.Reverse();
        clone.vertices = vertices.ToArray();
        clone.uv = uv.ToArray();
        clone.triangles = triangles.ToArray();

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

    List<int> CennectMeshes(List<Edge> edgePoints, int[] verticesIndex)
    {
        List<int> trianglesList = new List<int>();
        int[] oneTriangel = new int[3];
        trianglesList.AddRange(mesh.triangles);
        List<Edge> edgePoints2 = new List<Edge>();
        

        for (int i = 0; i < edgePoints.Count; i++)
        {
            edgePoints2.Add(new Edge(edgePoints[i].A, edgePoints[i].B, edgePoints[i].indexA, edgePoints[i].indexB));
        }

        for (int i = 0; i < edgePoints2.Count; i++)
        {
            for (int j = 0; j < verticesIndex.Length; j++)
            {
                if (edgePoints2[i].indexA == verticesIndex[j])
                {
                    edgePoints2[i].indexA = j;
                }
                if (edgePoints2[i].indexB == verticesIndex[j])
                {
                    edgePoints2[i].indexB = j;
                }
            }
        }

        //calculate the new triangles
        for (int i = 0; i < edgePoints.Count; i++)
        {
            oneTriangel[0] = edgePoints2[i].indexB + ar1.Length;
            oneTriangel[1] = edgePoints[i].indexB;
            oneTriangel[2] = edgePoints[i].indexA;
            trianglesList.AddRange(oneTriangel);

            oneTriangel[0] = edgePoints2[i].indexB + ar1.Length;
            oneTriangel[1] = edgePoints[i].indexA;
            oneTriangel[2] = edgePoints2[i].indexA + ar1.Length;
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
            //for (int i = 0; i < edges.Count; i++)
            //{

            //    if (edges[i].indexA == edges[i].indexB)
            //    {
            //        Gizmos.color = Color.yellow;
            //        Gizmos.DrawWireSphere(transform.TransformPoint(mesh.vertices[edges[i].indexB]), 0.05f);
            //    }
            //    else
            //    {

            //        Gizmos.color = Color.green;
            //        Gizmos.DrawWireSphere(transform.TransformPoint(mesh.vertices[edges[i].indexB]), 0.04f);
            //    }
            //}
            if (extrudevertex != null)
            {
                for (int i = 0; i < extrudevertex.Length; i++)
                {
                    Gizmos.color = Color.green;
                    Gizmos.DrawWireSphere(transform.TransformPoint(mesh.vertices[extrudevertex[i]]), 0.005f);
                }
            }
        }
    }
}
