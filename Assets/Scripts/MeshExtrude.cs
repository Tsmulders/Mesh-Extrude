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
    public Edge[] edge;

    //debug test 
    public Vector3[] ar1;
    public Vector3[] ar2;
    public Vector3[] ar3;
    public int[] triangles;

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
        triangles = GetComponent<MeshFilter>().mesh.triangles;
        ar3 = GetComponent<MeshFilter>().mesh.vertices;

        CennectMeshes();
        FindEdge();
    }

    void CreateFlatServeseMesh(Mesh mech)
    {
        //Mesh cloneMech = new Mesh();

        //listGameObjects.Add(cloneMech);
    }

    void CombinerMesh(GameObject[] meshes)
    {
        MeshFilter[] meshFilters = new MeshFilter[meshes.Length];
        for (int m = 0; m < meshes.Length; m++)
        {
            meshFilters = meshes[m].GetComponentsInChildren<MeshFilter>();
        }
        CombineInstance[] combine = new CombineInstance[meshFilters.Length];
        int i = 0;
        while (i < meshFilters.Length)
        {
            combine[i].mesh = meshFilters[i].sharedMesh;
            combine[i].transform = meshFilters[i].transform.localToWorldMatrix;
            //meshFilters[i].gameObject.SetActive(false);
            i++;
        }
        gameObject.GetComponent<MeshFilter>().mesh = new Mesh();
        gameObject.GetComponent<MeshFilter>().mesh.CombineMeshes(combine, true, true);
        //transform.gameObject.active = true;
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

    void CennectMeshes()
    {
        List<int> triangles;




    }

    void FindEdge()
    {
       GetEdgesOfMesh.BuildManifoldEdges(gameObject.GetComponent<MeshFilter>().mesh);
    }
}
