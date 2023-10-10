using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;

public class MechVerticesMerge2_0 : MonoBehaviour
{
    // Start is called before the first frame update
    public static void AutoWeld(Mesh mesh, float threshold)
    {
        List<Vector3> verts = new List<Vector3>();
        verts = mesh.vertices.ToList();

        List<int> tris = new List<int>();
        tris.AddRange(mesh.triangles.ToList());

        List<List<int>> newVerts = new List<List<int>>();
        List<int> dellVerts = new List<int>();


        //get close verticies
        for (int i = 0; i < verts.Count; i++)
        {
            // Has vertex already been added to newVerts list?
            bool addToList = true;
            List<int> v = new List<int>();
            for (int j = 0; j < verts.Count; j++)
            {
                float distance = Vector3.Distance(verts[i], verts[j]);
                if (distance <= threshold)
                {
                    if (newVerts.Count == 0)
                    {
                        addToList = true;
                        goto addToList;
                    }
                    for (int k = 0; k < newVerts.Count; k++)
                    {
                        if (newVerts[k].Contains(j))
                        {
                            addToList = false;
                        }
                    }
                    addToList:
                    if (addToList)
                    {
                        
                        if (!v.Contains(i) && i != j) v.Add(j);
                    }
                }
            }
             if (v.Count > 0) 
            {
                v.Add(i);
                v.Sort();
                newVerts.Add(v);
            }
        }

        //Debug.Log("newVerts done");
        //return if 0 where found  
        if (newVerts.Count == 0) return;

        //reassign triangles 
        for (int i = 0; i < newVerts.Count; i++)
        {
            for (int j = 0; j < newVerts[i].Count; j++)
            {
                for (int k = 0; k < tris.Count; k++)
                {
                    if (tris[k] == newVerts[i][j])
                    {
                        tris[k] = newVerts[i][0];
                    }
                }
            }
        }
        //Debug.Log("triangles done");



        ////sort 
        //for (int i = 0; i < newVerts.Count; i++)
        //{
        //    for (int j = 1; j < newVerts[i].Count; j++)
        //    {
        //        dellVerts.Add(newVerts[i][j]);
        //    }
        //}
        //dellVerts.Sort();
        //dellVerts.Reverse();

        //Debug.Log("sort done");

        ////dell reassign vertices

        //for (int i = 0; i < dellVerts.Count; i++)
        //{
        //    for (int k = 0; k < tris.Count; k++)
        //    {
        //        if (tris[k] >= dellVerts[i])
        //        {
        //            tris[k] = tris[k] - 1;
        //        }
        //    }
        //}

        ////dell verts

        //for (int i = 0; i < dellVerts.Count; i++)
        //{
        //    verts.RemoveAt(dellVerts[i]);
        //}

        mesh.triangles = tris.ToArray();
        mesh.vertices = verts.ToArray();
        //mesh.uv = newUVs.ToArray();
        mesh.RecalculateTangents();
        mesh.RecalculateBounds();
        //mesh.RecalculateNormals();
        mesh.RecalculateUVDistributionMetrics();

    }
}
