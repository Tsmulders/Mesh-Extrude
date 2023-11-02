using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;
using UnityEngine.Rendering;

public class flatpolygonalseeker : MonoBehaviour
{
    public static ExtrudeData[] LooseSurface(Mesh mesh)
    {
        List<ExtrudeData> extrude = new List<ExtrudeData>();

        List<Edge> Alledges = new List<Edge>();
        Alledges.AddRange(GetEdgesOfMesh.GetAllEdge(mesh));

        List<Edge> edges = new List<Edge>();
        edges = GetEdgesOfMesh.GetEdge(mesh, Alledges);
        List<Edge> edgesCircle = new List<Edge>();

        if (edges == null || edges.Count == 0) return extrude.ToArray();



        /*
         * eerst alle edge cirkel uit elkaar hallen.
         * 
         * daar na alle edge cirkel apart controleren welke fast zitten.
         * ook kijken naar sub mechen om de de vertices te controleeren te verminderen.
         * 
         * 
         * 
         */

        List<List<Edge>> list = new List<List<Edge>>();
     

        edgesCircle.Add(edges[0]);
        List<int> notChosen = new List<int>();
        int j = 0;

        List<Edge> edgesdone = new List<Edge>();
        edgesdone.AddRange(edges);

        _l2:
        for (int i = 0; i < edgesdone.Count; i++)
        {
            if (edges[i].indexA == edgesdone[i].indexB)
            {
                i++;
            }
            if (edgesCircle[j].indexB == edgesdone[i].indexA)
            {
                if (!edgesCircle.Contains(edgesdone[i]))
                {
                    edgesCircle.Add(edgesdone[i]);
                    j++;
                    goto _l2;
                }
            }
            if (!edgesCircle.Contains(edgesdone[i]))
            {
                notChosen.Add(i);
            }
        }        

        //if (notChosen.Count == 0 && edgesCircle[0].indexA != edgesCircle[edgesCircle.Count - 1].indexB)
        //{
        //    return extrude.ToArray();
        //}
        if (edgesCircle[0].indexA != edgesCircle[edgesCircle.Count - 1].indexB) 
        {
            for (int i = 0; i < edgesCircle.Count; i++)
            {
                edgesdone.Remove(edgesCircle[i]);
            }
            if (edgesdone.Count > 0)
            {
            edgesCircle.Clear();
            edgesCircle.Add(edgesdone[0]);
            notChosen.Clear();
            j = 0;
            goto _l2;
            }
        }
        list.Add(edgesCircle);

        //if (edgesdone.Count! > 0)
        //{
        //    for (int i = 0; i < edgesCircle.Count; i++)
        //    {
        //        edgesdone.Remove(edgesCircle[i]);
        //    }
        //    if (edgesdone.Count > 0)
        //    {
        //        edgesCircle.Clear();
        //        edgesCircle.Add(edgesdone[0]);
        //        notChosen.Clear();
        //        j = 0;
        //        goto _l2;
        //    }
        //}

        Debug.Log("lose edge");

        /*oposing 1
         * als ik meedere listen terug krijg
         * weer met een bool list werken als het heeft gevonden zet het op true.
         * daar na er door heen lopen en alles aan de indexedges toevoegen
         * 
         * 
         * NativeQueue bij deze kan je wel parallel schrijven
         * 
         * kan helaas niet Multithread van wegen de array te groot is om te zetten naar nativearray te zetten
         */

        //NativeArray<int> indexA = new NativeArray<int>(Alledges.Count, Allocator.Persistent);
        //NativeArray<int> indexB = new NativeArray<int>(Alledges.Count, Allocator.Persistent);

        //for (int i = 0; i < Alledges.Count; i++)
        //{
        //    indexA[i] = edgesCircle[i].indexA;
        //    indexB[i] = edgesCircle[i].indexB;
        //}

        //NativeList<int> indexextrude = new NativeList<int>(Allocator.TempJob);

        //GetExtrudeDataJob getExtrudeDataJob = new GetExtrudeDataJob()
        //{
        //    indexA = indexA,
        //    indexB = indexB,
        //    indexextrude = indexextrude
        //};

        //JobHandle dependency = new JobHandle();
        //JobHandle sheduleJobHandle = getExtrudeDataJob.Schedule(Alledges.Count, dependency);
        //JobHandle scheduleParallelJobHandle = getExtrudeDataJob.ScheduleParallel(Alledges.Count, 1, sheduleJobHandle);

        //indexA.Dispose();
        //indexB.Dispose();
        //indexextrude.Dispose();


        //extract all vertices from list
        List<int> indexEdges = new List<int>();

        for (int i = 0; i < edgesCircle.Count; i++)
        {
            if (!indexEdges.Contains(edgesCircle[i].indexA))
            {
                indexEdges.Add(edgesCircle[i].indexA);
            }
            if (!indexEdges.Contains(edgesCircle[i].indexB))
            {
                indexEdges.Add(edgesCircle[i].indexB);
            }
        }


        _l3:
        bool fountOne = false;
        for (int i = 0; i < Alledges.Count; i++)
        {
            if (indexEdges.Contains(Alledges[i].indexA) && !indexEdges.Contains(Alledges[i].indexB))
            {
                indexEdges.Add(Alledges[i].indexB);
                fountOne = true;
            }
            if (!indexEdges.Contains(Alledges[i].indexA) && indexEdges.Contains(Alledges[i].indexB))
            {
                indexEdges.Add(Alledges[i].indexA);
                fountOne = true;
            }
        }

        if (fountOne)
        {
            goto _l3;
        }
        indexEdges.Sort();

        extrude.Add(new ExtrudeData(indexEdges, edgesCircle));




        if (notChosen.Count != 0)
        {
            for (int i = 0; i < edgesCircle.Count; i++)
            {
                edgesdone.Remove(edgesCircle[i]);
            }
            if (edgesdone.Count > 0)
            {
                edgesCircle.Clear();
                edgesCircle.Add(edgesdone[0]);
                notChosen.Clear();
                j = 0;
                goto _l2;
            }
        }

        return extrude.ToArray();
    }
}
