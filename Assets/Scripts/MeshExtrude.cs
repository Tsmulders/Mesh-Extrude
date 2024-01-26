using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using Unity.VisualScripting.Antlr3.Runtime;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

public class MeshExtrude : MonoBehaviour
{
    private List<Mesh> listMech;
    private Vector3[] normals;


    private int[] triangle;

    float threshold = 0.0001f;
    float extrudeStrength = 0;

    ExtrudeData[] extrudeVertex;
    int countVer1;

    Vector3[] ar3;

    [SerializeField] private bool welding;
    // Start is called before the first frame update
    private void Start()
    {

        ExtrudeStart();
        ar3 = GetComponent<MeshFilter>().mesh.vertices;
    }

    void ExtrudeStart()
    {
        //wil also check child if need it be extruded 
        if (transform.childCount > 0)
        {
            for (int i = 0; i < transform.childCount; i++)
            {
                Extrude(transform.GetChild(i).gameObject);
            }
        }
        //check if main object has a mech filter
        if (GetComponent<MeshFilter>() != null)
        {
            Extrude(gameObject);
        }
    }

    //private void Update()
    //{
    //    GetComponent<MeshFilter>().mesh.vertices = ar3;
    //}

    void Extrude(GameObject gObject)
    {
        //get mech
        Mesh mesh;
        listMech = new List<Mesh>();
        mesh = gObject.GetComponent<MeshFilter>().mesh;
        MeshRenderer renderer = gObject.GetComponent<MeshRenderer>();

        //get extrude strength
        Vector3 size = renderer.bounds.size;
        float furthestDistance = size.x;
                                                                    //float furthestDistance = furtherPoint(mesh); // die duurt steet langer hoe meer vertices er zijn
        threshold = float.Epsilon;
        extrudeStrength = furthestDistance / 50;

        //make sure that all triangles are connected to each other.
        if (welding)
        {
            mesh =  MechVerticesMerge3_0.AutoWeld(mesh, threshold);
        }

        //get all extrude data 
                                                                    //extrudeVertex = flatpolygonalseeker.LooseSurface(mesh).ToArray();
        extrudeVertex = GetExtrudeData.GetData(mesh).ToArray();

        //check if something can be extruded.
        if (extrudeVertex.Length == 0)
        {
            Debug.Log("there are no lose polygons");
            return;
        }

        Debug.Log(extrudeVertex[0].indexEdges.Length);

        //this wil extrude flat polygon also if there are more flat polygons.
        for (int i = 0; i < extrudeVertex.Length; i++)
        {
            Mesh mesh2;
            //whil make a clone out of the flat polygons vertices
            mesh2 = cloneMesh(mesh, extrudeVertex[i].indexEdges, extrudeStrength);
            //combines clone and main mech
            mesh = gObject.GetComponent<MeshFilter>().mesh = CombinerMesh(mesh, mesh2);

            triangle = gObject.GetComponent<MeshFilter>().mesh.triangles;

            //connect the main mech and clone to each other with new triangles.
            triangle = gObject.GetComponent<MeshFilter>().mesh.triangles = ConnectMeshes(extrudeVertex[i].edgesCircle, extrudeVertex[i].indexEdges, mesh).ToArray();

            mesh = gObject.GetComponent<MeshFilter>().mesh;
        }
        //Recalculate the Mesh (Normals, Tangents, Bounds, UVDistributionMetrics)
        RecalculateMesh(mesh);

    }


    void RecalculateMesh(Mesh meshRecalculate)
    {
        //Recalculate the Mesh
        //meshRecalculate.RecalculateNormals();
        //meshRecalculate.RecalculateTangents();

        //meshRecalculate.RecalculateBounds();
        //meshRecalculate.RecalculateUVDistributionMetrics();
        TB.NormalSolver.RecalculateNormals(meshRecalculate, 45.0f);
    }

    private Mesh cloneMesh(Mesh original, int[] verticesIndex, float extrudeStrength)
    {
        //om de triangles te krijgen sla ze al op in get edges of mesh
        Mesh clone = new Mesh();

        List<Vector3> vertices = new List<Vector3>();
        List<Vector2> uv = new List<Vector2>();
        List<int> triangles = new List<int>();
        //create new vertices 
        for (int i = 0; i < verticesIndex.Length; i++)
        {
            vertices.Add(original.vertices[verticesIndex[i]] + -original.normals[verticesIndex[i]] * extrudeStrength);
            uv.Add(original.uv[verticesIndex[i]]);
        }
        
        Triangles[] triangles1;
        //get the triangles are need to be connected
        triangles1 = GetTriangles.getTrianglesGivenIndex(verticesIndex, original).ToArray();
        //add triangles to the list
        for (int i = 0; i < triangles1.Length; i++)
        {
            triangles.AddRange(triangles1[i].triangleIndex);
        }

        //reassigned triangles to vertices index
        for (int i = 0; i < verticesIndex.Length; i++)
        {
            for (int j = 0; j < triangles.Count; j++)
            {
                if (verticesIndex[i] == triangles[j])
                {
                    triangles[j] = i;
                }
            }
        }
        //set to clone mesh 
        triangles.Reverse();
        clone.vertices = vertices.ToArray();
        clone.uv = uv.ToArray();
        clone.triangles = triangles.ToArray();

        clone.RecalculateNormals();
        //return clone of that part of the mesh.
        return clone;
    }

    Mesh CombinerMesh(in Mesh original, in Mesh addition)
    {
        //set original and addition in a array
        CombineInstance[] combine = new CombineInstance[2];
        combine[0].mesh = original;
        combine[0].transform = transform.localToWorldMatrix;
        combine[1].mesh = addition;
        combine[1].transform = transform.localToWorldMatrix;

        Mesh mesh = new Mesh();
        // Combine the tho meshes
        mesh.CombineMeshes(combine, true, false, false);
        //saves the count of the mesh - additional
        countVer1 = mesh.vertices.Length - addition.vertices.Length;
        //return combine mesh
        return mesh;
    }

    List<int> ConnectMeshes(Edge[] edgePoints, int[] verticesIndex, Mesh mesh)
    {
        List<int> trianglesList = new List<int>();
        int[] oneTriangel = new int[3];
        trianglesList.AddRange(mesh.triangles);
        List<Edge> edgePoints2 = new List<Edge>();

        //create new edge point
        for (int i = 0; i < edgePoints.Length; i++)
        {
            edgePoints2.Add(new Edge(edgePoints[i].A, edgePoints[i].B, edgePoints[i].indexA, edgePoints[i].indexB));
        }

        //set new edge point to the correct index
        for (int i = 0; i < edgePoints2.Count; i++)
        {
            for (int j = 0; j < verticesIndex.Length; j++)
            {
                if (edgePoints2[i].indexA == verticesIndex[j])
                {
                    edgePoints2[i].indexA = j;
                }
                if (edgePoints2[i].indexB == verticesIndex[j])
                {
                    edgePoints2[i].indexB = j;
                }
            }
        }

        //calculate the new triangles
        for (int i = 0; i < edgePoints.Length; i++)
        {
            oneTriangel[0] = edgePoints2[i].indexB + countVer1;
            oneTriangel[1] = edgePoints[i].indexB;
            oneTriangel[2] = edgePoints[i].indexA;
            trianglesList.AddRange(oneTriangel);

            oneTriangel[0] = edgePoints2[i].indexB + countVer1;
            oneTriangel[1] = edgePoints[i].indexA;
            oneTriangel[2] = edgePoints2[i].indexA + countVer1;
            trianglesList.AddRange(oneTriangel);
        }
        //return new triangles
        return trianglesList;
    }

    //float furtherPoint(Mesh mesh)
    //{
    //    float furthestDistance = 0;
    //    Vector3[] vertices = mesh.vertices;
    //    for (int i = 0; i < vertices.Length; i++)
    //    {
    //        for (int j = 0; j < vertices.Length; j++)
    //        {
    //            float distance = Vector3.Distance(vertices[i], vertices[j]);
    //            if (furthestDistance < distance)
    //            {
    //                furthestDistance = distance;
    //            }
    //        }
    //    }
    //    return furthestDistance;
    //}



    private void OnDrawGizmos()
    {
        //for (int i = 0; i < edges.Count; i++)
        //{

        //    if (edges[i].indexA == edges[i].indexB)
        //    {
        //        Gizmos.color = Color.yellow;
        //        Gizmos.DrawWireSphere(transform.TransformPoint(mesh.vertices[edges[i].indexB]), 0.05f);
        //    }
        //    else
        //    {

        //        Gizmos.color = Color.green;
        //        Gizmos.DrawWireSphere(transform.TransformPoint(mesh.vertices[edges[i].indexB]), 0.04f);
        //    }
        //}
        //if (extrudevertex != null)
        //{
        //    for (int i = 0; i < extrudevertex.Length; i++)
        //    {
        //        Gizmos.color = Color.green;
        //        Gizmos.DrawWireSphere(transform.TransformPoint(mesh.vertices[extrudevertex[i]]), 0.005f);
        //    }
        //}

        //if (SceneView.lastActiveSceneView.drawGizmos)
        //{

        //}
    }
}
