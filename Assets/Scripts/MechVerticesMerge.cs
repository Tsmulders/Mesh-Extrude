using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MechVerticesMerge : MonoBehaviour
{
    Mesh mesh;
    

    // Start is called before the first frame update
    void Start()
    {
        mesh = gameObject.GetComponent<MeshFilter>().mesh;
        AutoWeld(mesh, 0.3f);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void VerticesWeld(Mesh mesh, float overflow)
    {
        Vector3[] vertices = mesh.vertices;

        for (int i = 0; i < vertices.Length; i++)
        {
            for (int j = 0; j < vertices.Length; j++)
            {
                if (Vector3.Distance(vertices[i], vertices[j]) <= overflow)
                {
                    vertices[j] = vertices[i];

                }
            }
        }

        mesh.vertices = vertices;
        mesh.RecalculateBounds();
    }

    private void AutoWeld(Mesh mesh, float threshold)
    {
        Vector3[] verts = mesh.vertices;

        // Build new vertex buffer and remove "duplicate" verticies
        // that are within the given threshold.
        List<Vector3> newVerts = new List<Vector3>();
        List<Vector2> newUVs = new List<Vector2>();

        int k = 0;
        newVerts.AddRange(mesh.vertices);
        for (int i = 0; i < verts.Length; i++)
        {
            // Has vertex already been added to newVerts list?

            foreach (Vector3 newVert in newVerts)
                if (Vector3.Distance(newVert, verts[i]) <= threshold)
                    goto skipToNext;
                // Accept new vertex!
                newVerts.Add(verts[i]);
                newUVs.Add(mesh.uv[i]);

            skipToNext:;
        }

        // Rebuild triangles using new verticies
        int[] tris = mesh.triangles;
        for (int i = 0; i < tris.Length; ++i)
        {
            // Find new vertex point from buffer
            for (int j = 0; j < newVerts.Count; ++j)
            {
                if (Vector3.Distance(newVerts[j], verts[i]) <= threshold)
                {
                    tris[j] = i;
                    break; 
                }
            }

        }
        // Update mesh!*
        mesh.Clear();
        mesh.vertices = newVerts.ToArray();
        mesh.triangles = tris;
        mesh.uv = newUVs.ToArray();
        mesh.RecalculateTangents();
        mesh.RecalculateBounds();
    }
}
