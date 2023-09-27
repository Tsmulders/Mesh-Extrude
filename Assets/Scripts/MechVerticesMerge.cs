using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;

public class MechVerticesMerge : MonoBehaviour
{
    Mesh _mesh;

    public float threshold = 0.03f;

    // Start is called before the first frame update
    void Start()
    {
        _mesh = gameObject.GetComponent<MeshFilter>().mesh;
        AutoWeld(_mesh, threshold);
    }

    // Update is called once per frame
    void Update()
    {
        
        
    }

    private void AutoWeld(Mesh mesh, float threshold)
    {
        Vector3[] verts = mesh.vertices;

        // Build new vertex buffer and remove "duplicate" verticies
        // that are within the given threshold.
        List<int> newVerts = new List<int>();
        List<Vector2> newUVs = new List<Vector2>();

        //mesh.GetVertices(newVerts); //newVerts.AddRange(mesh.vertices);
        for (int i = 0; i < verts.Length; i++)
        {
            // Has vertex already been added to newVerts list?

            for (int j = 0; j < verts.Length; j++)
            {
                float distance = Vector3.Distance(verts[i], verts[j]);
                if (distance <= threshold)
                {
                    if(!newVerts.Contains(j) && i != j )
                    {
                        //Accept new vertex!
                        newVerts.Add(j);
                        
                        newUVs.Add(mesh.uv[i]);
                    }
                    
                }
                
            }
        }

        // Rebuild triangles using new verticies
        int[] tris = mesh.triangles;
        List<int> vertexdel = new List<int>();

        for (int i = 0; i < newVerts.Count; i++)
        {
            // Find new vertex point from buffer
            for (int j = 0; j < newVerts.Count; j++)
            {
                float distance = Vector3.Distance(verts[newVerts[j]], verts[newVerts[i]]);
                if (distance <= threshold)
                {
                    for (int k = 0; k < tris.Length; k++)
                    {
                        if (tris[k] == newVerts[j])
                        {
                            tris[k] = newVerts[i];
                            if(!vertexdel.Contains(i))
                            vertexdel.Add(i);
                        }
                    }
                }
            }
        }

        List<Vector3> niewverts = verts.ToList();
        vertexdel.Sort();
        for (int i = vertexdel.Count -1; i > 0; i--)
        {

            int ti = vertexdel[i];
            if(niewverts.Count<= ti|| 0 > ti) { Debug.Log(ti+" "+ niewverts.Count); }
            niewverts.RemoveAt(ti);
            
        }

        // Update mesh!
        mesh.vertices = niewverts.ToArray();
        mesh.triangles = tris;
        //mesh.uv = newUVs.ToArray();
        mesh.RecalculateTangents();
        mesh.RecalculateBounds();
        //mesh.RecalculateNormals();
    }
}
