using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;

public class MechVerticesMerge : MonoBehaviour
{
    public static void AutoWeld(Mesh mesh, float threshold)
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
                    if(!newVerts.Contains(j) && i != j)
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
        List <List<int>> merchCluster = new List<List<int>>();
        
        //test 1
        for (int i = 0; i < newVerts.Count; i++)
        {
            List<int> Temp = new List<int>();
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
                            
                            if(!Temp.Contains(newVerts[i])) Temp.Add(newVerts[i]);
                            
                            if(!vertexdel.Contains(newVerts[i])) vertexdel.Add(newVerts[i]);
                        }
                    }
                }
            }

            if (Temp.Count > 0)
            {
                Temp.RemoveAt(Temp.Count - 1);
                merchCluster.Add(Temp);
            }

        }

        List<Vector3> niewverts = verts.ToList();

        for (int i = 0; i < merchCluster.Count - 1; i++)
        {
            for (int j = 0; j < merchCluster[i].Count; j++)
            {
            if (!vertexdel.Contains(merchCluster[i][j]))
            {
                vertexdel.Add(merchCluster[i][j]);
            }

            }
        }
        vertexdel.Sort();

            for (int j = vertexdel.Count -1; j >= 0; j--)
            {
            niewverts.RemoveAt(vertexdel[j]);
                
                for (int k = 0; k < tris.Length; k++)
                {
                    if (tris[k] >= vertexdel[j])
                    {
                        tris[k] = tris[k] - 1;
                    }
                }
            }

        //vertexdel.Sort();
        //for (int i = vertexdel.Count -1; i > 0; i--)
        //{
        //    int ti = vertexdel[i];
        //    if(niewverts.Count<= ti|| 0 > ti) { Debug.Log(ti+" "+ niewverts.Count); }



        //    niewverts.RemoveAt(ti);
        //    for (int j = 0; j < tris.Length; j++)
        //    {
        //        if (tris[j] >= ti)
        //        {
        //            tris[j] = tris[j] - 1;
        //        }
        //    }
        //}


        //test 2
        for (int i = 0; i < newVerts.Count; i++)
        {
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
                        }
                        if (newVerts[i] != newVerts[j])
                        {

                        }
                    }
                }
            }
        }

        // Update mesh!
        mesh.triangles = tris;
        mesh.vertices = niewverts.ToArray();
        //mesh.uv = newUVs.ToArray();
        mesh.RecalculateTangents();
        mesh.RecalculateBounds();
        //mesh.RecalculateNormals();
        mesh.RecalculateUVDistributionMetrics();
    }

    public static void AutoWeld1(Mesh mesh, float threshold)
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
                    if (!newVerts.Contains(j) && i != j)
                    {
                        // Accept new vertex!
                        newVerts.Add(j);
                        newUVs.Add(mesh.uv[i]);
                    }

                }

            }
        }

        // Rebuild triangles using new verticies
        int[] tris = mesh.triangles;
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
                        }
                    }
                }
            }

        }
        // Update mesh!
        //mesh.vertices = newVerts.ToArray();
        mesh.triangles = tris;
        mesh.uv = newUVs.ToArray();
        mesh.RecalculateTangents();
        mesh.RecalculateBounds();
        mesh.RecalculateNormals();
    }
}

