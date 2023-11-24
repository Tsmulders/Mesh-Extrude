using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Serialization;
using Unity.Mathematics;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;



public struct mesh_data
{
    public float3 vertex;
}

public class ComputeShaderTestC : MonoBehaviour
{

    [Header("compute shader")]

    ComputeShader compute;
    [SerializeField] ComputeBuffer _meshPropertiesBuffer;


    [Header(" texture")]
    [SerializeField] private Color coler = Color.red;

    [SerializeField] private RenderTexture result;

    [SerializeField] private Mesh mesh;

    private int _kernel = 0;

    

    Vector3[] vertices;

    int _count;

    mesh_data[] _data;
    [Header("wave")]
    [SerializeField] private bool reverse;

    // Start is called before the first frame update
    void Start()
    {

        compute = AssetDatabase.LoadAssetAtPath<ComputeShader>("Assets/Scripts/voorbeeldCode/test/ComputeShaderTest.compute");
        for (int i = 0; i < 3; i++)
        {
            test2();
        }

        //_kernel = compute.FindKernel("CSMain");
        //UpdateBuffers();
    }

    private void UpdateBuffers()
    {
        _meshPropertiesBuffer = new ComputeBuffer(_count, 3 * sizeof(float));
        

        compute.SetBuffer(_kernel, "_vertices", _meshPropertiesBuffer);

        
    }

    private void OnDisable()
    {
        _meshPropertiesBuffer?.Release();
        _meshPropertiesBuffer = null;

    }

    void OnEnable()
    {
        mesh = GetComponent<MeshFilter>().mesh;
        vertices = mesh.vertices;
        _count = vertices.Length;
        _data = new mesh_data[_count];
    }

    //Update is called once per frame
    void Update()
    {
        //  test1();
    }

    //verandere van color texture
    //    void Update()
    //    {
    //        int kernel = compute.FindKernel("CSMain");

    //        result = new RenderTexture(1032, 1032, 240);
    //        result.enableRandomWrite = true;
    //        result.Create();

    //        compute.SetTexture(kernel, "Result", result);
    //        compute.SetVector("color", coler);
    //        compute.Dispatch(kernel, result.width / 8, result.height / 8, 1);
    //    }

    void test1()
    {

        for (var i = 0; i < _count; i++)
        {
            mesh_data ver = new mesh_data
            {
                vertex = vertices[i],
            };
            _data[i] = ver;
        }
        _meshPropertiesBuffer.SetData(_data);

        int xGroup = (int)(_count / 8.0f) + 1;

        compute.SetFloat("_Time", Time.time);
        compute.SetBool("_reverse", reverse);
        compute.Dispatch(_kernel, xGroup, 1, 1);


        _meshPropertiesBuffer.GetData(_data);

        for (int i = 0; i < _count; i++)
        {
            vertices[i] = _data[i].vertex;
        }

        if (mesh != null)
        {
            mesh.vertices = vertices;
        }
    }

    void test2()
    {


        int _kerneltest2 = compute.FindKernel("test2");

        int threads = 1;
        //doing another * 2 to make sure i have a large enough array.
        int testResults = threads * 8 * 8 * 8 * 2;
        ComputeBuffer results = new ComputeBuffer(1000, sizeof(float), ComputeBufferType.Append);
        ComputeBuffer results2 = new ComputeBuffer(100, sizeof(int));
        results.SetCounterValue(0);

        compute.SetBuffer(_kerneltest2, "result", results);
        compute.SetBuffer(_kerneltest2, "result2", results2);

        compute.Dispatch(_kerneltest2, 10, 10, threads);
        ComputeBuffer countBuffer = new ComputeBuffer(1, sizeof(int), ComputeBufferType.IndirectArguments);

        ComputeBuffer.CopyCount(results, countBuffer, 0);
        int[] counter = new int[1] { 0 };
        countBuffer.GetData(counter);
        int count = counter[0];

        int[] test = new int[count];
        results.GetData(test);
        results.Release();
        //results = null;
        int[] test2 = new int[100];
        results2.GetData(test2);
        results2.Release();

        //GC.Collect();
        
        List<int> result = new List<int>();
        result.AddRange(test);
        result.Sort();
        test = test.Distinct().ToArray();
        Debug.Log("Test results amount : " + test.Length);

        for (int i = 0; i < test.Length; i++)
        {
            if (test[i] == 123)
            {
            Debug.Log("Test result at " + i + " : " + test[i]);
            }
        }
    }
}

