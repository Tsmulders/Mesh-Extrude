using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;

public class MechVerticesMerge3_0 : MonoBehaviour
{
    // Start is called before the first frame update
    public static Mesh AutoWeld(Mesh mesh, float threshold)
    {

        List<int> tris = mesh.triangles.ToList();
        Vector3[] normals = mesh.normals;

        //checks for vertices need to be connected
        List<List<int>> newVerts = CloseVertices(mesh, threshold);
        if (newVerts.Count == 0) return mesh;

        Debug.Log(newVerts.Count);

        //Recalculate Triangles
        tris = ReassignTriangles3(newVerts, tris).ToList();
        //Recalculate Normals
        normals = RecalculateNormals(newVerts, normals);

        //set new triangles
        mesh.triangles = tris.ToArray();
        //set new normals
        mesh.normals = normals;
        mesh.RecalculateTangents();
        mesh.RecalculateBounds();
        mesh.RecalculateUVDistributionMetrics();
        return mesh;
    }

    private static List<List<int>> CloseVertices(Mesh mesh, float threshold)
    {
        Vector3[] _vertices = mesh.vertices;

        List<List<int>> newVerts = new List<List<int>>();
        //get compute shader from map
        ComputeShader compute = AssetDatabase.LoadAssetAtPath<ComputeShader>("Assets/Scripts/ComputeShader/VerticesWeldingShader.compute");
        int _kernel = compute.FindKernel("CSMain");
        //create compute buffer
        ComputeBuffer countBuffer = new ComputeBuffer(1, sizeof(int), ComputeBufferType.IndirectArguments);
        ComputeBuffer _verticesBuffer = new ComputeBuffer(_vertices.Length, sizeof(float) * 3);
        //link buffer
        compute.SetBuffer(_kernel, "_vertices", _verticesBuffer);

        //set data to compute buffers
        _verticesBuffer.SetData(_vertices);
        compute.SetFloat("threshold", threshold);

        //loop throw all vertices
        for (int i = 0; i < _vertices.Length; i++)
        {
            //if already contains skip
            bool skip = false;
            for (int j = 0; j < newVerts.Count; j++)
            {
                if (newVerts[j].Contains(i))
                {
                    skip = true;
                    break;
                }
            }

            if (!skip)
            {
                //create Append buffer
                ComputeBuffer result = new ComputeBuffer(mesh.vertices.Length, sizeof(int), ComputeBufferType.Append);
                //set append buffer to 0 this needs to be have in the code to use append buffer consisted.
                result.SetCounterValue(0);
                //link buffer
                compute.SetBuffer(_kernel, "Result", result);
                //set data to compute buffers
                compute.SetInt("firstVertices", i);
                //dispatch to compute shader
                compute.Dispatch(_kernel, _vertices.Length/ 32, 1, 1);
                //will copy the count of the append buffer
                ComputeBuffer.CopyCount(result, countBuffer, 0);

                int[] counter = new int[1] { 0 };
                //set the count in a array
                countBuffer.GetData(counter);
                
                int count = counter[0];
                //we make a array of the size of the append buffer
                int[] data = new int[count];
                //set de data of the append buffer in the array
                result.GetData(data);
                //check if data = not 0
                if (data.Length > 0)
                {   //wil add to new Verts list
                    List<int> indices = new List<int>();
                    indices.AddRange(data);
                    indices.Add(i);
                    indices.Sort();
                    newVerts.Add(indices.Distinct().ToList());
                    Debug.Log("done");
                }
                //release append buffer
                result.Release();
            }
        }
        //release compute buffer
        countBuffer.Release();
        _verticesBuffer.Release();
        //return
        return newVerts;
    }

    private static List<int> ReassignTriangles(List<List<int>> newVerts, List<int> tris)
    {
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
        return tris;
    }
    private static int[] ReassignTriangles2(List<List<int>> newVerts, List<int> tris)
    {
        ComputeShader compute = AssetDatabase.LoadAssetAtPath<ComputeShader>("Assets/Scripts/ComputeShader/VerticesWeldingShader.compute");
        int _kernel = compute.FindKernel("reassignTria");
        ComputeBuffer trianglesBuffer = new ComputeBuffer(tris.Count, sizeof(int));

        compute.SetBuffer(_kernel, "triangles", trianglesBuffer);

        trianglesBuffer.SetData(tris);
        int[] data = new int[tris.Count];
        for (int i = 0; i < newVerts.Count; i++)
        {
            int xGroup = newVerts[i].Count / 1;
            int yGroup = tris.Count / 32;

            ComputeBuffer newVertsBuffer = new ComputeBuffer(newVerts[i].Count, sizeof(int));

            compute.SetBuffer(_kernel, "newVerts", newVertsBuffer);

            trianglesBuffer.SetData(newVerts[i]);

            compute.Dispatch(_kernel, xGroup, yGroup, 1);
            
            trianglesBuffer.GetData(data);

            newVertsBuffer.Release();
        }

        trianglesBuffer.Release();
        return data;
    }

    private static int[] ReassignTriangles3(List<List<int>> newVerts, List<int> tris)
    {
        //get compute shader from map
        ComputeShader compute = AssetDatabase.LoadAssetAtPath<ComputeShader>("Assets/Scripts/ComputeShader/VerticesWeldingShader.compute");
        int _kernel = compute.FindKernel("reassignTria3");
        //create compute buffer
        ComputeBuffer trianglesBuffer = new ComputeBuffer(tris.Count, sizeof(int));
        //link buffer
        compute.SetBuffer(_kernel, "triangles", trianglesBuffer);
        //set data for compute buffer
        trianglesBuffer.SetData(tris);

        int[] newTriangles = new int[tris.Count];

        //set x treat group
        int xGroup = tris.Count / 32;

        for (int i = 0; i < newVerts.Count; i++)
        {
            for (int j = 1; j < newVerts[i].Count; j++)
            {
                //set data for compute shader
                compute.SetInt("check", newVerts[i][j]);
                compute.SetInt("setTo", newVerts[i][0]);
                //dispatch to compute shader
                compute.Dispatch(_kernel, xGroup, 1, 1);
                //get data from compute shader
                trianglesBuffer.GetData(newTriangles);
            }
        }
        //release Compute buffers
        trianglesBuffer.Release();
        return newTriangles;
    }

    private static Vector3[] RecalculateNormals(List<List<int>> newVerts, Vector3[] normals)
    {
        for (int i = 0; i < newVerts.Count; i++)
        {
            int bigX = newVerts[i][0];
            int bigY = newVerts[i][0];
            int bigZ = newVerts[i][0];
            for (int j = 0; j < newVerts[i].Count; j++)
            {
                //check if it is bigger if is change
                if (Mathf.Abs(normals[bigX].x) < Mathf.Abs(normals[newVerts[i][j]].x))
                {
                    bigX = newVerts[i][j];
                }
                if (Mathf.Abs(normals[bigY].y) < Mathf.Abs(normals[newVerts[i][j]].y))
                {
                    bigY = newVerts[i][j];
                }
                if (Mathf.Abs(normals[bigZ].z) < Mathf.Abs(normals[newVerts[i][j]].z))
                {
                    bigZ = newVerts[i][j];
                }
            }
            //set normal
            //moet kijken of die for loop wel nodig is
            for (int k = 0; k < normals.Length; k++)
            {
                if (k == newVerts[i][0])
                {
                    normals[k] = new Vector3(normals[bigX].x, normals[bigY].y, normals[bigZ].z);
                }
            }
        }
        return normals;
    }
}
