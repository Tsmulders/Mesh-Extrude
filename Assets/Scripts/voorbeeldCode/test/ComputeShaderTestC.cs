using System.Collections;
using System.Collections.Generic;
using System.Xml.Serialization;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.UIElements;
using static UnityEngine.Mesh;

public struct Vertex
{
    public Vector3 position;
}

public class ComputeShaderTestC : MonoBehaviour
{
    [SerializeField] private ComputeShader compute;
    [SerializeField] private Color coler = Color.red;

    [SerializeField] private RenderTexture result;

    [SerializeField] private Mesh mesh;

    private int _kernel;

    ComputeBuffer _meshPropertiesBuffer;

    Vector3[] vertices;

    int _count;

    mesh_data[] _data;

    // Start is called before the first frame update
    void Start()
    {
        _kernel = compute.FindKernel("CSMain");

        UpdateBuffers();
    }

    // Update is called once per frame
    void Update()
    {
        compute.SetFloat("_Time", Time.time);
        compute.Dispatch(_kernel, Mathf.CeilToInt(_count / 64), 1, 1); 

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

    private void UpdateBuffers()
    {
        var offset = Vector3.zero;
        var data = new mesh_data[_count];

        for (var i = 0; i < _count; i++)
        {
            data[i] = new mesh_data
            {
                vertex = vertices[i],
            };
        }

        _meshPropertiesBuffer = new ComputeBuffer(_count, 80);
        _meshPropertiesBuffer.SetData(data);

        compute.SetBuffer(_kernel, "data", _meshPropertiesBuffer);
    }


    private struct mesh_data
    {
        public float3 vertex;
    }
}

