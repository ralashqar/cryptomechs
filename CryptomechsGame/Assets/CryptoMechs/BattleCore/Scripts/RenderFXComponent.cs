using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;

public class RenderFXComponent
{
    public List<GameObject> meshFXObjects;
    public List<Material> meshFXMaterials;
    public List<ImpactFXInstance> impactFXs;
    IAttackable character;

    public RenderFXComponent(IAttackable character)
    {
        this.character = character;
    }

    public void ClearAllFX()
    {
        foreach (var go in meshFXObjects)
        {
            if (go != null)
                GameObject.Destroy(go);
        }

        meshFXObjects?.Clear();
        linkedFX.Clear();
    }

    public List<GameObject> linkedFX;

    public void ClearFX(MechSlot slot)
    {
        if (linkedFX == null) return;
        linkedFX.ForEach(x => GameObject.Destroy(x));
        linkedFX.Clear();
    }
    
    public void ReceiveImpact(ImpactFXDefinition impactFX)
    {
        if (impactFX.renderEffectPrefab != null)
        {
            SetupFX(character.GetGameObject(), impactFX.renderEffectPrefab, impactFX);
        }
    }
    
    public void SetupFX(GameObject key, GameObject fxPrefab, ImpactFXDefinition impactFX)
    {
        if (fxPrefab == null) return;

        //character.mech.GetCasterBySlot(slot).
        GameObject target = key;

        if (meshFXObjects == null) meshFXObjects = new List<GameObject>();
        if (linkedFX == null) linkedFX = new List<GameObject>();

        GameObject go = GameObject.Instantiate(fxPrefab, MechFXManager.Instance.transform);
        //go.transform.parent = target.transform;

        PSMeshRendererUpdater psMesh = go.GetComponent<PSMeshRendererUpdater>();
        if (psMesh == null) return;

        List<Light> lights = go.GetComponentsInChildren<Light>().ToList();
        //lights?.ForEach(l => l.enabled = false);

        var renderers = key.GetComponentsInChildren<MeshRenderer>();
        int originalNumMats = renderers[0].materials.Length;

        foreach (var renderer in renderers)
        {
            psMesh.UpdateMeshEffect(renderer.gameObject);
            break;
        }

        numAddedMats = renderers[0].materials.Length - originalNumMats;
        /*
        if (linkedFX.ContainsKey(key))
        {
            if (linkedFX[key] != null)
                linkedFX[key].Add(go);
            else
                linkedFX[key] = new List<GameObject>() { go };
        }
        else
        {
            linkedFX.Add(key, new List<GameObject>() { go });
        }
        meshFXObjects.Add(go);
        */
        //GameObject.Destroy(psMesh, duration);
        AddImpactFX(go, impactFX);
        //GameObject.Destroy(go, duration);
    }

    int numAddedMats = 0;
    public void AddImpactFX(GameObject impactGO, ImpactFXDefinition impactFX)
    {
        if (impactFXs == null)
            impactFXs = new List<ImpactFXInstance>();
        impactFXs.Add(new ImpactFXInstance(impactGO, impactFX.impactDuration, impactFX.impactMovesDuration, impactFX.useDeterministicTime));
    }

    public void UpdateImpactFXs()
    {
        if (impactFXs == null) return;

        float time = Time.time;
        int move = BattleManager.Instance.moveIndex;

        for (int i = 0; i < impactFXs.Count; ++i)
        {
            var impact = impactFXs[i];
            int movesElapsed = move - impact.moveStamp;
            float timeElapsed = time - impact.timestamp;
            bool canApply = impact.useDeterministicTime ?
                movesElapsed > impact.moveDuration
                : timeElapsed > impact.duration;

            if (canApply)
            {
                //impact.caster.GetAbilitiesManager()?.ApplyImpact(impact.caster, this.character, impact.effect, impact.abilityFraction);
                GameObject.Destroy(impact.fx);
                impactFXs.Remove(impact);

                //var renderers = character.GetGameObject().GetComponentsInChildren<MeshRenderer>();
                //foreach (var renderer in renderers)
                //{
                //    if (numAddedMats > 0)
                //    {
                //        int matCount = renderer.materials.Length - 1;
                //        for (int j = 0; j < numAddedMats; ++j)
                //            renderer.materials[matCount - j] = null;
                //    }
                //}

                i--;
            }
        }
    }
}
