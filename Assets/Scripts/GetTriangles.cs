using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class GetTriangles : MonoBehaviour
{

    public static List<Triangles> getAllTiangles(Mesh mesh)
    {
        int[] triangles = mesh.triangles;
        List<Triangles> trianglesList = new List<Triangles>();

        for (int i = 0; i < triangles.Length; i += 3)
        {
            Triangles tria = new Triangles(triangles[i], triangles[i + 1], triangles[i + 2]);
            //if (!trianglesList.Contains(tria))
            //{
                trianglesList.Add(tria);
            //}
        }
        return trianglesList;
    }

    public static List<Triangles> getTrianglesGivenIndex(int[] vertex, Mesh mesh)
    {
        List<Triangles> alltrianglesList = new List<Triangles>();
        //getall the triangles of the object
        alltrianglesList = getAllTiangles(mesh);
        List<Triangles> trianglesList = new List<Triangles>();
        //
        for (int i = 0;i < vertex.Length;i++)
        {
            for (int j = 0; j < alltrianglesList.Count; j++)
            {
                if (alltrianglesList[j].triangleIndex.Contains(vertex[i]))
                {
                    if (!trianglesList.Contains(alltrianglesList[j]))
                    {
                        trianglesList.Add(alltrianglesList[j]);
                    }
                }
            }
        }
        return trianglesList;
    }

}
