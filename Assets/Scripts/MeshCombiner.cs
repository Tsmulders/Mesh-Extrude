using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;

public class MeshCombiner : MonoBehaviour
{

    private Mesh mesh;
    private Mesh mesh2;

    public GameObject objectMesh2;

    int t = 0;
    int v = 0;

    private List<Vector3> vertices;
    private List<int> triangles;

    // Start is called before the first frame update
    void Start()
    {
        mesh = GetComponent<MeshFilter>().mesh;
        objectMesh2.GetComponent<MeshFilter>().mesh = mesh;
        mesh2 = mesh;

        Vector3[] vertices = mesh.vertices;
        Vector3[] normals = mesh.normals;
        int[] triangles = mesh.triangles;


        mesh2.vertices = vertices;
        mesh2.triangles = triangles;
        mesh2.RecalculateNormals();


    }

    // Update is called once per frame
    void Update()
    {

    }


    //void AddBeam(Beam beam)
    //{
    //    int trIndex = vertices.Count;

    //    vertices.Add(beam.direction.from); // Vector3
    //    vertices.Add(beam.direction.to); // Vector3
    //    vertices.Add(beam.dataVertex); // Vector3
    //    mesh.SetVertices(vertices);

    //    triangles.AddRange(Enumerable.Range(trIndex, 3));
    //    mesh.SetTriangles(triangles, 0);
    //}
}
