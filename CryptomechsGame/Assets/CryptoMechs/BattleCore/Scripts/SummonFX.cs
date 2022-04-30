using System.Collections;
using System.Collections.Generic;
using UnityEngine;
//using AdvancedDissolve_Example;
using System.Linq;

[System.Serializable]
public class DissolveMaskPlaneDefinition
{
    public GameObject rootObj;
    public List<MeshRenderer> meshesToDissolve;
    public List<Material> materials;
    //public Controller_Mask_Plane maskPlane;

    public void Initialize()
    {
        if (meshesToDissolve == null || meshesToDissolve.Count == 0)
        {
            meshesToDissolve = rootObj.gameObject.GetComponentsInChildren<MeshRenderer>().ToList();
        }

        materials = new List<Material>();
        if (meshesToDissolve != null)
        {
            foreach (MeshRenderer mesh in meshesToDissolve)
            {
                materials.AddRange(mesh.materials);
                var shader = Shader.Find("VacuumShaders/Advanced Dissolve/Legacy Shaders/Diffuse");
                foreach (Material mat in mesh.materials)
                {
                    mat.shader = shader;
                    mat.SetFloat("_DissolveMask", 2);
                    mat.SetFloat("_DissolveGlobalControl", 3);
                }
            }
        }

        //if (matCollector == null) matCollector = GetComponentInChildren<CollectDissolveMaterials>();
        //if (maskPlane == null) maskPlane = maskPlane = GetComponentInChildren<Controller_Mask_Plane>();

        //if (maskPlane)
        //    maskPlane.materials = materials.ToArray();

        //maskPlane?.UpdateMaskKeywords();
    }
}

public class SummonFX : MonoBehaviour
{
    //public List<MeshRenderer> meshesToDissolve;
    public List<DissolveMaskPlaneDefinition> maskPlaneDefs;

    public GameObject maskGO;
    public Vector3 maskDirection = Vector3.up;
    public float maskMotionDistance = 5f;
    public float summonTime = 3f;
    private Coroutine summonRoutine;

    //public Controller_Mask_Plane maskPlane;
    //public Controller_Cutout controllerCutout;
    //public Controller_Edge controllerEdge;

    private Vector3 currentMaskPos;
    private Vector3 summonedPos;
    private Vector3 unSummonedPos;
    //public CollectDissolveMaterials matCollector;

    //public List<Material> materials;

    // Start is called before the first frame update
    void Start()
    {
        unSummonedPos = currentMaskPos = maskGO.transform.localEulerAngles;
        summonedPos = maskGO.transform.localPosition + maskDirection * maskMotionDistance;

        List<Material> allMats = new List<Material>();
        foreach (DissolveMaskPlaneDefinition mask in maskPlaneDefs)
        {
            mask.Initialize();
            allMats.AddRange(mask.materials);
        }

        //if (controllerCutout)
        //    controllerCutout.materials = allMats.ToArray();
        //if (controllerEdge)
        //    controllerEdge.materials = allMats.ToArray();

        if (maskGO == null) maskGO = this.gameObject;
        TriggerSummon();
    }

    public void TriggerMine(float fraction, float time = 0)
    {
        float amount = fraction * maskMotionDistance;
        Vector3 mineToPosition = currentMaskPos - (amount * maskDirection);

        if (time > 0)
        {
            if (summonRoutine != null) StopCoroutine(summonRoutine);
            summonRoutine = StartCoroutine(AnimateSummoningMask(mineToPosition, time, true));
        }
        else
        {
            SetMaskToPosition(mineToPosition);
        }
    }

    public void TriggerSummon()
    {
        currentMaskPos = unSummonedPos;
        if (summonRoutine != null) StopCoroutine(summonRoutine);
        summonRoutine = StartCoroutine(AnimateSummoningMask(summonedPos, summonTime));
    }

    public void SetMaskToPosition(Vector3 toPosition)
    {
        maskGO.transform.localPosition = toPosition;
        currentMaskPos = maskGO.transform.localPosition;
    }

    public IEnumerator AnimateSummoningMask(Vector3 toPosition, float time, bool reverse = false)
    {
        yield return null;

        float elapsedTime = 0;
        Vector3 fromPos = currentMaskPos;

        Vector3 range = toPosition - fromPos;

        while (elapsedTime < time)
        {
            float fraction = elapsedTime / time;
            elapsedTime += Time.deltaTime;
            SetMaskToPosition(fromPos + (fraction * range));
            //maskGO.transform.localPosition += maskDirection * rate * Time.deltaTime * (reverse ? -1f : 1f);
            yield return null;
        }

    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
