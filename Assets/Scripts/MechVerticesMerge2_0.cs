using Microsoft.Win32.SafeHandles;
using System.Collections.Generic;
using System.Linq;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;

public class MechVerticesMerge2_0 : MonoBehaviour
{
    //weld close vertices. that it wil be 1 index.
    public static void AutoWeld(Mesh mesh, float threshold)
    {
        //Vector3[] normals = mesh.normals;
        //List<int> tris = new List<int>();
        //tris.AddRange(mesh.triangles.ToList());

        //List<Vector3> verts = new List<Vector3>();
        //verts = mesh.vertices.ToList();

        //NativeList<Vector3> vertices = new NativeList<Vector3>(Allocator.TempJob);
        //NativeList<ContainerA> newVertsNative = new NativeList<ContainerA>(Allocator.TempJob);


        //for (int i = 0; i < verts.Count; i++)
        //{
        //    vertices.Add(verts[i]);
        //}



        ////List<List<int>> newVerts = new List<List<int>>();
        //List<int> dellVerts = new List<int>();


        //VerticesWeldingJob verticesWeldingJob = new VerticesWeldingJob()
        //{
        //    vertices = vertices,
        //    newVerts = newVertsNative,
        //    threshold = threshold,
        //};


        //JobHandle dependency = new JobHandle();
        //JobHandle sheduleJobHandle = verticesWeldingJob.Schedule(verts.Count, dependency);
        //JobHandle sheduleParralelJobHandle = verticesWeldingJob.ScheduleParallel(verts.Count, 1, sheduleJobHandle);

        //sheduleParralelJobHandle.Complete();

        //List<List<int>> newVerts = new List<List<int>>();

        //for (int i = 0; i < newVertsNative.Length; i++)
        //{
        //    newVerts[0].Add(newVertsNative[0].subContainers[i]);
        //}


        //for (int i = 0; i < newVertsNative.Length; i++)
        //{
        //    for (int j = 0; j < newVerts.Count; j++)
        //    {
        //        if (!newVertsNative[j].subContainers.Contains(j))
        //        {
        //            List<int> nv = new List<int>();
        //            for (int k = 0; k < newVertsNative[i].subContainers.Length; k++)
        //            {
        //                nv.Add(newVertsNative[i].subContainers[k]);
        //            }
        //            newVerts.Add(nv);
        //        }
        //    }
        //}

        //vertices.Dispose();
        //newVertsNative.Dispose();



        //test 2

        Vector3[] normals = mesh.normals;
        List<int> tris = new List<int>();
        tris.AddRange(mesh.triangles.ToList());
        List<Vector3> verts = new List<Vector3>();
        verts = mesh.vertices.ToList();

        List<List<int>> newVerts = new List<List<int>>();

        for (int i = 0; i < verts.Count; i++)
        {


            NativeList<Vector3> vertices = new NativeList<Vector3>(Allocator.TempJob);
            NativeList<int> newVertsNative = new NativeList<int>(Allocator.TempJob);

            NativeArray<bool> foundVertices = new NativeArray<bool>(verts.Count, Allocator.TempJob);



            for (int j = 0; j < verts.Count; j++)
            {
                vertices.Add(verts[j]);
            }

            for (int j = 0; j < newVerts.Count; j++)
            {
                for (int k = 0; k < newVerts[j].Count; k++)
                {
                    newVertsNative.Add(newVerts[j][k]);
                }
            }

            VerticesWeldingJob verticesWeldingJob = new VerticesWeldingJob()
            {
                vertices = vertices,
                newVertsNative = newVertsNative,
                threshold = threshold,
                firstVertices = i,
                //addToList = true,
                foundVertices = foundVertices,
            };


            JobHandle dependency = new JobHandle();
            JobHandle sheduleJobHandle = verticesWeldingJob.Schedule(verts.Count, dependency);
            JobHandle scheduleParallelJobHandle = verticesWeldingJob.ScheduleParallel(verts.Count, 1, sheduleJobHandle);

            scheduleParallelJobHandle.Complete();

            List<int> nv = new List<int>();

            for (int j = 0; j < verts.Count; j++)
            {
                if (verticesWeldingJob.foundVertices[j])
                {
                    nv.Add(j);
                }
            }

            vertices.Dispose();
            newVertsNative.Dispose();
            foundVertices.Dispose();

            if (nv.Count > 0)
            {
                nv.Add(i);
                nv.Sort();
                newVerts.Add(nv);
                Debug.Log("done");
            }

        }




        //old code
        //List<Vector3> verts = new List<Vector3>();
        //verts = mesh.vertices.ToList();
        //List<int> tris = new List<int>();
        //tris.AddRange(mesh.triangles.ToList());
        //List<List<int>> newVerts = new List<List<int>>();
        //List<int> dellVerts = new List<int>();
        //Vector3[] normals = mesh.normals;
        ////get close verticies
        //for (int i = 0; i < verts.Count; i++)
        //{
        //    // Has vertex already been added to newVerts list?
        //    bool addToList = true;
        //    List<int> v = new List<int>();
        //    for (int j = 0; j < verts.Count; j++)
        //    {
        //        float distance = Vector3.Distance(verts[i], verts[j]);
        //        if (distance <= threshold)
        //        {
        //            if (newVerts.Count == 0)
        //            {
        //                addToList = true;
        //                goto addToList;
        //            }
        //            for (int k = 0; k < newVerts.Count; k++)
        //            {
        //                if (newVerts[k].Contains(j))
        //                {
        //                    addToList = false;
        //                }
        //            }
        //        addToList:
        //            if (addToList)
        //            {

        //                if (!v.Contains(i) && i != j) v.Add(j);
        //            }
        //        }
        //    }
        //    if (v.Count > 0)
        //    {
        //        v.Add(i);
        //        v.Sort();
        //        newVerts.Add(v);
        //    }
        //}


        //Debug.Log("newVerts done");
        //return if 0 where found  
        if (newVerts.Count == 0) return;
        Debug.Log(newVerts.Count);
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

        //recalculate normals
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
                    //normals[k] = Vector3.Lerp(normals[newVerts[i][fadsfa1]], normals[newVerts[i][fadsfa2]], 0.5f) * 2;
                }
            }
        }


        //Vector3 normal = Vector3.Lerp(v1, V2, 0.5f).normalized;

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
        mesh.normals = normals;
        mesh.RecalculateTangents();
        mesh.RecalculateBounds();
        //mesh.RecalculateNormals();
        mesh.RecalculateUVDistributionMetrics();

    }
}
