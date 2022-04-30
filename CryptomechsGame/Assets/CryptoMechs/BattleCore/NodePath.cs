using System.Collections;
using System.Collections.Generic;
using UnityEngine;
//using Dreamteck.Splines;
using Sirenix.OdinInspector.Demos.RPGEditor;

[System.Serializable]
public struct PossibleEncounter
{
    public string NarrativeBlock;
    public float probability;
}

public class NodePath : MonoBehaviour
{
    public MapConnectorPath pathData;

    public MapNodeBase NodeA;
    public MapNodeBase NodeB;

    public int numSplinePoints = 2;
    //public SplineComputer pathSpline;

    public TerrainPreset terrainPreset;

    public float GetLinearLength()
    {
        return Vector3.Distance(NodeA.transform.transform.position, NodeB.transform.position);
    }

    /*
    public float distance = 0;
    
    public List<PossibleEncounter> possibleEncounters;

    public int maxEncounters = 1;


    public float GetJourneyTimeFromTerrainType()
    {
        return Vector3.Distance (NodeA.transform.position, NodeB.transform.position) * terrainPreset.TimeMultiplier;
    }
    */
    public float journeyTime;

    public void Initialize()
    {
        //if (pathSpline != null)
        //{
        //    pathSpline.SetPointPosition(0, NodeA.transform.position);
        //    pathSpline.SetPointPosition(pathSpline.pointCount - 1, NodeB.transform.position);
        //}
    }
    // Start is called before the first frame update
    void Start()
    {
        //journeyTime = GetJourneyTimeFromTerrainType();
    }

    public void UpdateNodePath()
    {
        /*
        transform.position = (NodeA.transform.position + NodeB.transform.position) / 2;
        float distance = GetLinearLength();

        float segmentLength = distance / (float)numSplinePoints;

        Vector3 nodeAPosO = pathSpline.GetPointPosition(0);
        Vector3 nodeBPosO = pathSpline.GetPointPosition(pathSpline.pointCount - 1);

        pathSpline.SetPointPosition(0, NodeA.transform.position);
        pathSpline.SetPointPosition(pathSpline.pointCount - 1, NodeB.transform.position);

        Vector3 deltaA = pathSpline.GetPointPosition(0) - nodeAPosO;
        Vector3 deltaB = pathSpline.GetPointPosition(pathSpline.pointCount - 1) - nodeBPosO;


        for (int i = 1; i < pathSpline.pointCount - 1; ++i)
        {
            float dA = Vector3.Distance(pathSpline.GetPointPosition(i), nodeAPosO);
            float dB = Vector3.Distance(pathSpline.GetPointPosition(i), nodeBPosO);

            float weight = dA / (dA + dB);
            Vector3 newPos = pathSpline.GetPointPosition(i) + deltaA * weight + deltaB * (1 - weight);
            //pathSpline.SetPointPosition(i, newPos);
        }
        */
    }

    // Update is called once per frame
    void Update()
    {
    }
}
