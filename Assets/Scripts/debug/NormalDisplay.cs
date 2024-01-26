using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NormalDisplay : MonoBehaviour
{
    public float fadsf = 0.1f;

    // Start is called before the first frame update
    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnDrawGizmosSelected()
    {
        Mesh mesh = GetComponent<MeshFilter>().mesh;

        List<Vector3> normals = new List<Vector3>();
        List<Vector3> vertices = new List<Vector3>();

        mesh.GetNormals(normals);
        mesh.GetVertices(vertices);

        for (int i = 0; i < vertices.Count; i++)
        {
            Gizmos.DrawLine(vertices[i], vertices[i] + normals[i] * fadsf);
        }


        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;

        if(Physics.Raycast(ray, out hit, 1000000000))
        {
            Gizmos.color = Color.red;
            Gizmos.DrawLine(hit.point, hit.point + hit.normal);
            Gizmos.color = Color.white;
        }
    }
}
