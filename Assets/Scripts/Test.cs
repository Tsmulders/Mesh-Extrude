using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Test : MonoBehaviour
{
    public Mesh mesh;

    // Start is called before the first frame update
    void Start()
    {
        mesh = GetComponent<MeshFilter>().mesh;

    }
    // Update is called once per frame
    void Update()
    {
        //Debug.Log( HasFlatshadedSurface(mesh));
    }

    private void CreateCube()
    {
        Vector3[] vertices = {
            new Vector3 (0, 0, 0),
            new Vector3 (1, 0, 0),
            new Vector3 (1, 1, 0),
            new Vector3 (0, 1, 0),
            new Vector3 (0, 1, 1),
            new Vector3 (1, 1, 1),
            new Vector3 (1, 0, 1),
            new Vector3 (0, 0, 1),
        };

        int[] triangles = {
            0, 2, 1, //face front
			0, 3, 2,
            2, 3, 4, //face top
			2, 4, 5,
            1, 2, 5, //face right
			1, 5, 6,
            0, 7, 4, //face left
			0, 4, 3,
            5, 4, 7, //face back
			5, 7, 6,
            0, 6, 7, //face bottom
			0, 1, 6
        };

        Mesh mesh = GetComponent<MeshFilter>().mesh;
        mesh.Clear();
        mesh.vertices = vertices;
        mesh.triangles = triangles;
        mesh.Optimize();
        mesh.RecalculateNormals();
    }

    //deze code kijkt of dat er een platte kant is op de mesh.
    public bool HasFlatshadedSurface(Mesh m)
    {
        Vector3[] normals = m.normals;
        int[] indices = m.triangles;
        int triangleCount = indices.Length / 3;
        for (int i = 0; i < triangleCount; i++)
        {
            Vector3 n1 = normals[indices[i * 3]];
            Vector3 n2 = normals[indices[i * 3 + 1]];
            Vector3 n3 = normals[indices[i * 3 + 2]];
            if (n1 == n2 && n1 == n3)
                return true;
        }
        return false;
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
