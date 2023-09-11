using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;

public class MeshExtrude : MonoBehaviour
{

    private Mesh mesh;
    private Mesh mesh2;

    private List<GameObject> listGameObjects;
    private List<int> triangles;
    private Vector3[] normals;


    private int Clonei = 0;

    // Start is called before the first frame update
    void Start()
    {
        listGameObjects = new List<GameObject>();
        listGameObjects.Add(gameObject);
        mesh = GetComponent<MeshFilter>().mesh;
        //objectMesh2.GetComponent<MeshFilter>().mesh = mesh;
        mesh2 = mesh;
        Vector3[] vertices = mesh.vertices;
        normals = mesh.normals;
        int[] triangles = mesh.triangles;


        mesh2.vertices = vertices;
        mesh2.triangles = triangles;
        mesh2.RecalculateNormals();
        CreateFlatServeseMesh(mesh2);

        CombinerMesh(listGameObjects.ToArray());
    }

    void CreateFlatServeseMesh(Mesh mech)
    {
        GameObject cloneMech = new GameObject();
        cloneMech.name = "Clone flat polygons" + Clonei++;
        cloneMech.AddComponent<MeshFilter>();
        cloneMech.AddComponent<MeshRenderer>().material.SetColor("White", Color.white);
        cloneMech.GetComponent<MeshFilter>().mesh = mesh2;
        cloneMech.AddComponent<MeshTrianglesReverse>();

        listGameObjects.Add(cloneMech);
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
}
