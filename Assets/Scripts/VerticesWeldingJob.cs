using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Xml.Linq;
using Unity.Collections;
using Unity.Jobs;
using Unity.VisualScripting;
using UnityEngine;

public struct VerticesWeldingJob : IJobFor
{
    //public NativeList<Vector3> vertices;

    //public NativeList<ContainerA> newVertsNative;

    //public float threshold;

    //public NativeList<int> v;


    //public void Execute(int index)
    //{
    //    bool addToList = true;



    //    for (int j = 0; j < vertices.Length; j++)
    //    {
    //        float distance = Vector3.Distance(vertices[index], vertices[j]);
    //        if (distance <= threshold)
    //        {
    //            if (newVertsNative.Length == 0)
    //            {
    //                addToList = true;
    //                goto addToList;
    //            }
    //            for (int k = 0; k < newVertsNative.Length; k++)
    //            {
    //                if (newVertsNative[k].subContainers.Contains(j))
    //                {
    //                    addToList = false;
    //                }
    //            }
    //        addToList:
    //            if (addToList)
    //            {
    //                if (!v.Contains(index) && index != j) v.Add(j);
    //            }
    //        }
    //    }
    //    if (v.Length > 0)
    //    {
    //        v.Add(index);
    //        v.Sort();

    //        newVertsNative.Add(new ContainerA());
    //        newVertsNative[newVertsNative.Length].subContainers.AddRange(v.AsArray());
    //    }
    //}

    //test 2

    public NativeList<Vector3> vertices;

    public NativeList<int> newVertsNative;

    public float threshold;

    public int firstVertices;

    public NativeList<int> v;

    public bool addToList;

    public NativeArray<bool> foundVertices;

    public void Execute(int index)
    {
        
        float distance = Vector3.Distance(vertices[firstVertices], vertices[index]);
        if (distance <= threshold)
        {
            if (newVertsNative.Length == 0)
            {
                addToList = true;
                goto addToList;
            }
            for (int k = 0; k < newVertsNative.Length; k++)
            {
                if (newVertsNative.Contains(index))
                {
                    addToList = false;
                }
            }
        addToList:
            if (addToList)
            {
                //if (!v.Contains(firstVertices) && firstVertices != index) v.Add(index);
                if (foundVertices[index])
                {
                    foundVertices[index] = false;
                }  
            }
        }
    }
}

public struct ContainerA
{
    public NativeList<int> subContainers;
}