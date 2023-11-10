using System.Collections;
using System.Collections.Generic;
using System.Xml.Serialization;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.UIElements;



public struct mesh_data
{
    public float3 vertex;
}

public class ComputeShaderTestC : MonoBehaviour
{

    [Header("compute shader")]
    [SerializeField] private ComputeShader compute;
    [SerializeField] ComputeBuffer _meshPropertiesBuffer;


    [Header(" texture")]
    [SerializeField] private Color coler = Color.red;

    [SerializeField] private RenderTexture result;

    [SerializeField] private Mesh mesh;

    private int _kernel;

    

    Vector3[] vertices;

    int _count;

    mesh_data[] _data;
    [Header("wave")]
    [SerializeField] private bool reverse;

    // Start is called before the first frame update
    void Start()
    {
        _kernel = compute.FindKernel("CSMain");

        UpdateBuffers();
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

    // Update is called once per frame
    void Update()
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

        int xGroup = (int)(_count / 64.0f);

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
}

