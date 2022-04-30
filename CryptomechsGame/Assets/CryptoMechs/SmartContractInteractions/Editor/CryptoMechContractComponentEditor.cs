using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(CryptoMechContractComponent))]
public class CryptoMechContractComponentEditor : Editor
{
    public string contract = "0x72f597E452495C9b929B5Ea0CF9Ffa35Ee815c44";
    public string account = "0xbE519500010b11035423A252a9aCA32247230F59";

    public int tokenID = 1;
    public int cachedBalance;

    public override async void OnInspectorGUI()
    {
        var contractInteraction = target as CryptoMechContractComponent;
        contract = EditorGUILayout.TextField("Contract", contract);
        account = EditorGUILayout.TextField("Account Address", account);
        GUILayout.Space(5f);

        if (GUILayout.Button("Reset"))
        {
            contractInteraction.balance = 0;
            contractInteraction.isComputingBalance = false;
        }
        GUILayout.Space(5f);

        GUILayout.BeginHorizontal();
        tokenID = EditorGUILayout.IntField("Token ID", tokenID);
        if (!contractInteraction.isComputingBalance)
        { 
            if (GUILayout.Button("Get Balance"))
            {
                contractInteraction.GetBalanceOf(account, tokenID.ToString());
            }
        }
        else
        {
            GUILayout.Label("Retrieving Balance...");
        }
        GUILayout.EndHorizontal();
        
        if (!contractInteraction.isComputingBalance)
            GUILayout.Label("Balance Computed: " + contractInteraction.balance);

        GUILayout.Space(10f);
        GUILayout.Label("PART CREATION", EditorStyles.boldLabel);
        GUILayout.Space(10f);
        if (GUILayout.Button("Create Part"))
        {
            contractInteraction.CreatePart("p5", 5);
        }

    }
}
