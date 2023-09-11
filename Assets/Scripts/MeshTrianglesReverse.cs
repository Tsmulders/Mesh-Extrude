using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class MeshTrianglesReverse : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        Mesh mesh = GetComponent<MeshFilter>().mesh;
        mesh.triangles = mesh.triangles.Reverse().ToArray();
    }
}
