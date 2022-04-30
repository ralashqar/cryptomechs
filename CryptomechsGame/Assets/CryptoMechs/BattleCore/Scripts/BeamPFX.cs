using Sirenix.OdinInspector.Demos.RPGEditor;
using System.Collections.Generic;
using System.Collections;
using UnityEngine;

[System.Serializable]
public class BeamPFX : MonoBehaviour, AbilityVisualFXBehavior
{
    [SerializeField]
    public GameObject beamStartPrefab;
    [SerializeField]
    public GameObject beamEndPrefab;
    [SerializeField]
    public GameObject beamLineRendererPrefab;

    GameObject beamStart;
    GameObject beamEnd;
    GameObject beam;
    LineRenderer line;
    Transform sourceMuzzle;

    [Header("Adjustable Variables")]
    public float beamEndOffset = 1f; //How far from the raycast hit point the end effect is positioned
    public float textureScrollSpeed = 8f; //How fast the texture scrolls along the beam
    public float textureLengthScale = 3; //Length of the beam texture

    public Vector3 GetTargetPosition()
    {
        return this.sourceMuzzle.transform.position;
    }

    public void DestroyFX(GameObject go)
    {
        Destroy(beamStart);
        Destroy(beamEnd);
        Destroy(beam);
    }

    public void OnChannel(Vector3 position)
    {
        line.positionCount = 2;

        Vector3 start = sourceMuzzle != null ? sourceMuzzle.position : transform.position;
        Vector3 dir = position - start;

        line.SetPosition(0, start);
        beamStart.transform.position = start;

        Vector3 end = position;
        
        //RaycastHit hit;
        //if (Physics.Raycast(start, dir.normalized, out hit, dir.magnitude + beamEndOffset))
        //    end = hit.point - (dir.normalized * beamEndOffset);
        //else
        //    end = transform.position + (dir * 100);

        beamEnd.transform.position = end;
        line.SetPosition(1, end);

        beamStart.transform.LookAt(beamEnd.transform.position);
        beamEnd.transform.LookAt(beamStart.transform.position);

        float distance = Vector3.Distance(start, end);
        line.sharedMaterial.mainTextureScale = new Vector2(distance / textureLengthScale, 1);
        line.sharedMaterial.mainTextureOffset -= new Vector2(Time.deltaTime * textureScrollSpeed, 0);
    }

    public void OnFire(IAbilityCaster caster, Transform sourceMuzzle, AbilityProjectileItem ability)
    {
        beamStart = Instantiate(beamStartPrefab, new Vector3(0, 0, 0), Quaternion.identity) as GameObject;
        beamEnd = Instantiate(beamEndPrefab, new Vector3(0, 0, 0), Quaternion.identity) as GameObject;
        beam = Instantiate(beamLineRendererPrefab, new Vector3(0, 0, 0), Quaternion.identity) as GameObject;
        line = beam.GetComponent<LineRenderer>();
        this.sourceMuzzle = sourceMuzzle;
    }

    public void OnHitTarget(Vector3 target)
    {
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
