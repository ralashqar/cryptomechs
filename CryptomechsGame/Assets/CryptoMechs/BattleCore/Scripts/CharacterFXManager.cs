using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;

public class CharacterFXManager
{
    public List<GameObject> meshFXObjects;

    CharacterAgentController character;

    public CharacterFXManager(CharacterAgentController character)
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

    public Dictionary<MechSlot, List<GameObject>> linkedFX;

    public void ClearMechSlotFX(MechSlot slot)
    {
        if (linkedFX == null) return;
        if (linkedFX.ContainsKey(slot))
        {
            linkedFX[slot].ForEach(x => GameObject.Destroy(x));
            linkedFX.Remove(slot);
        }
    }
    
    public void ReceiveImpact(ImpactFXDefinition impactFX)
    {
        if (impactFX.renderEffectPrefab != null)
        {
            SetupMechFX(MechSlot.COCKPIT, impactFX.renderEffectPrefab, impactFX);
            SetupMechFX(MechSlot.SHOULDERS, impactFX.renderEffectPrefab, impactFX);
            SetupMechFX(MechSlot.LEGS, impactFX.renderEffectPrefab, impactFX);
        }
        //SetupMechFX(MechSlot.COCKPIT, impactFX.renderEffect, impactFX.impactDuration);
        //SetupMechFX(MechSlot.SHOULDERS, impactFX.renderEffect, impactFX.impactDuration);
        //SetupMechFX(MechSlot.LEGS, impactFX.renderEffect, impactFX.impactDuration);
    }
    
    public void RemoveEffect(MechSlot slot)
    {

    }

    public void SetupMechFX(MechSlot key, GameObject fxPrefab, ImpactFXDefinition impactFX)
    {
        if (fxPrefab == null) return;

        //character.mech.GetCasterBySlot(slot).
        GameObject target = character.mech.GetSlotGameObject(key);
        if (target == null) return;
        if (meshFXObjects == null) meshFXObjects = new List<GameObject>();
        if (linkedFX == null) linkedFX = new Dictionary<MechSlot, List<GameObject>>();

        GameObject go = GameObject.Instantiate(fxPrefab, MechFXManager.Instance.transform);
        //go.transform.parent = target.transform;

        PSMeshRendererUpdater psMesh = go.GetComponent<PSMeshRendererUpdater>();
        if (psMesh == null) return;

        List<Light> lights = go.GetComponentsInChildren<Light>().ToList();
        //lights?.ForEach(l => l.enabled = false);

        var renderers = character.mech.GetSlotRenderers(key);

        foreach (var renderer in renderers)
        {
            psMesh.UpdateMeshEffect(renderer.gameObject);
        }

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
        this.character.buffsManager.timedImpacts.AddImpactFX(go, impactFX);
        //GameObject.Destroy(go, duration);
    }

    public void SetupMechFX(MechSlot key, string fxID, float duration)
    {
        if (string.IsNullOrEmpty(fxID)) return;

        //character.mech.GetCasterBySlot(slot).
        GameObject target = character.mech.GetSlotGameObject(key);

        if (meshFXObjects == null) meshFXObjects = new List<GameObject>();
        if (linkedFX == null) linkedFX = new Dictionary<MechSlot, List<GameObject>>();

        MechFX fx = MechFXManager.Instance.mechEffects.Find(x => x.ID == fxID);
        if (fx == null) return;

        GameObject go = GameObject.Instantiate(fx.fxPrefab, MechFXManager.Instance.transform);
        //go.transform.parent = target.transform;

        PSMeshRendererUpdater psMesh = go.GetComponent<PSMeshRendererUpdater>();
        if (psMesh == null) return;

        List<Light> lights = go.GetComponentsInChildren<Light>().ToList();
        //lights?.ForEach(l => l.enabled = false);
        
        var renderers = character.mech.GetSlotRenderers(key);

        foreach (var renderer in renderers)
        {
            psMesh.UpdateMeshEffect(renderer.gameObject);
        }

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
        GameObject.Destroy(go, duration);
    }

}
