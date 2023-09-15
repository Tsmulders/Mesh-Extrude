using System.Collections;
using System.Collections.Generic;
using System.Linq;
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
    public int[] edges;

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
        //CreateFlatServeseMesh(mesh2);


        GetComponent<MeshFilter>().mesh = CombinerMesh(mesh, mesh2);
        ar3 = GetComponent<MeshFilter>().mesh.vertices;
        triangle = GetComponent<MeshFilter>().mesh.triangles;

        edges = GetEdges(triangle, ar3).ToArray();


    }
    void Update()
    {
        gameObject.GetComponent<MeshFilter>().mesh.triangles = triangle;
        gameObject.GetComponent<MeshFilter>().mesh.vertices = ar3;
        
    }

    void CreateFlatServeseMesh(Mesh mech)
    {
        //Mesh cloneMech = new Mesh();

        //listGameObjects.Add(cloneMech);
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
        
        for (int i = 0; i < vertices.Length; i++)
        {
            int occurrences = triangle.Count(x => x == i);
            if (occurrences < 5)
            {
                edges.Add(i);
            }
        }
        return edges;
    }
    void CennectMeshes(int[] edgePoints)
    {
        List<int> trianglesList = new List<int>();
        int[] oneTriangel = new int[3];

        for (int i = 0; i < oneTriangel.Length; i++)
        {

        }
        trianglesList.AddRange(oneTriangel);
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
    [1] om de buer point te zoeken moet ik nog even over na denken
    [2] index flat serves clone + first obect

 */

}
