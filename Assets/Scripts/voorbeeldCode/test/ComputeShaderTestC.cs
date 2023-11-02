using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public struct Vertex
{
    public Vector3 position;
}

public class ComputeShaderTestC : MonoBehaviour
{
    public ComputeShader compute;
    public Color coler = Color.red;

    public RenderTexture result;

    //public Mesh mesh;
    
    
    // Start is called before the first frame update
    void Start()
    {
        //mesh = GetComponent<MeshFilter>().mesh;
    }

    // Update is called once per frame
    void Update()
    {
        int kernel = compute.FindKernel("MeshWave");

        compute.SetTexture(kernel, "Result", result);

        compute.SetFloat("_Time", Time.time);
        //compute("_vertices", mesh.vertices);
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
