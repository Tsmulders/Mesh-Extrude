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
        List<List<int>> newVerts = CloseVertices(mesh, threshold);
        if (newVerts.Count == 0) return mesh;
        Debug.Log(newVerts.Count);
        tris = ReassignTriangles(newVerts, tris);
        normals = RecalculateNormals(newVerts, normals);

        mesh.triangles = tris.ToArray();
        mesh.normals = normals;
        mesh.RecalculateTangents();
        mesh.RecalculateBounds();
        //mesh.RecalculateNormals();
        mesh.RecalculateUVDistributionMetrics();
        return mesh;
    }

    private static List<List<int>> CloseVertices(Mesh mesh, float threshold)
    {
        Vector3[] _vertices = mesh.vertices;

        List<List<int>> newVerts = new List<List<int>>();
        ComputeShader compute = AssetDatabase.LoadAssetAtPath<ComputeShader>("Assets/Scripts/ComputeShader/VerticesWeldingShader.compute");
        int _kernel = compute.FindKernel("CSMain");

        ComputeBuffer countBuffer = new ComputeBuffer(1, sizeof(int), ComputeBufferType.IndirectArguments);
        ComputeBuffer _verticesBuffer = new ComputeBuffer(_vertices.Length, sizeof(float) * 3);

        compute.SetBuffer(_kernel, "_vertices", _verticesBuffer);

        _verticesBuffer.SetData(_vertices);
        compute.SetFloat("threshold", threshold);

        for (int i = 0; i < _vertices.Length; i++)
        {
            //bool skip = false;
            //for (int j = 0; j < newVerts.Count; j++)
            //{
            //    if (newVerts[j].Contains(i))
            //    {
            //        skip = true;
            //    }
            //}

            //if (!skip)
            //{
                ComputeBuffer result = new ComputeBuffer(mesh.vertices.Length, sizeof(int), ComputeBufferType.Append);
                result.SetCounterValue(0);

                compute.SetBuffer(_kernel, "Result", result);

                compute.SetInt("firstVertices", i);
                compute.Dispatch(_kernel, _vertices.Length/ 16, 1, 1);

                ComputeBuffer.CopyCount(result, countBuffer, 0);

                int[] counter = new int[1] { 0 };
                countBuffer.GetData(counter);

                int count = counter[0];

                int[] data = new int[count];

                result.GetData(data);

                if (data.Length > 0)
                {
                    List<int> indices = new List<int>();
                    indices.AddRange(data);
                    indices.Add(i);
                    indices.Sort();
                    newVerts.Add(indices.Distinct().ToList());
                    Debug.Log("done");
                }
                result.Release();
            }
        //}
        countBuffer.Release();
        _verticesBuffer.Release();
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

    private static Vector3[] RecalculateNormals(List<List<int>> newVerts, Vector3[] normals)
    {
        for (int i = 0; i < newVerts.Count; i++)
        {
            int bigX = newVerts[i][0];
            int bigY = newVerts[i][0];
            int bigZ = newVerts[i][0];
            for (int j = 0; j < newVerts[i].Count; j++)
            {
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
