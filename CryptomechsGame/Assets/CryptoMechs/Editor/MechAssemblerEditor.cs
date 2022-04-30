using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.Linq;

[CustomEditor(typeof(MechAssembler))]
public class MechAssemblerEditor : Editor
{
    public override void OnInspectorGUI()
    {
        MechComponentLibrary lib = GameObject.FindObjectOfType<MechComponentLibrary>();
        MechAssembler assembler = target as MechAssembler;

        var assembledParts = assembler.assembledParts;

        GUILayout.BeginHorizontal();
        assembler.enablePartPinning = EditorGUILayout.Toggle("Enable Pinning", assembler.enablePartPinning);
        if (GUILayout.Button("Clear Pins"))
        {
            assembler.ClearPinnedParts();
        }
        GUILayout.EndHorizontal();

        List<FullMech> mechSlots = assembler.transform.GetComponentsInChildren<FullMech>().ToList();
        int index = 1;
        GUILayout.BeginHorizontal();
        foreach(var m in mechSlots)
        {
            if (GUILayout.Button("Build Mech " + index.ToString()))
            {
                //MechFXManager.Instance?.CLearFX();
                assembler.BuildRandomMech(m, false);

                GameObject.FindObjectOfType<MechMintUIManager>()?.SetFullMechUI();
            }
            index++;
        }

        if (GUILayout.Button("Build Mech Army"))
        {
            List<FullMech> allMechs = GameObject.FindObjectsOfType<FullMech>().ToList();

            foreach (var m in allMechs)
            {
                if (mechSlots.Contains(m)) continue;
                assembler.BuildRandomMech(m, true);
            }

            //GameObject.FindObjectOfType<MechMintUIManager>().SetFullMechUI();
        }
        GUILayout.EndHorizontal();

        GUILayout.Space(10f);
        base.OnInspectorGUI();

        //if (assembler.postProcessRules == null) assembler.postProcessRules = new List<MechConnectionRule>();
        //GUILayout.Label("POST PROCESS RULES", EditorStyles.boldLabel);
        //GUILayout.BeginHorizontal();
        //GUILayout.EndHorizontal();

    }

    
}
