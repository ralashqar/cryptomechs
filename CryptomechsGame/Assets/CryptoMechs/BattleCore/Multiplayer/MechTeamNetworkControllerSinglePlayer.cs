using Photon.Pun;
using Photon.Pun.Demo.PunBasics;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using Photon.Realtime;
using System.Linq;
using Sirenix.OdinInspector.Demos.RPGEditor;
using ExitGames.Client.Photon;

public class MechTeamNetworkControllerSinglePlayer : MonoBehaviour
{

    [SerializeField]
    private GameObject mechTest;

    public List<MechNetworkSetup> mechDefs;
    public List<FullMech> mechs;

    public List<FullMech> playerMechs;
    public List<FullMech> opponentMechs;

    private int playerIndex = 1;
    private int opponentIndex = 1;

    /// <summary>
    /// MonoBehaviour method called on GameObject by Unity during early initialization phase.
    /// </summary>
    public void Awake()
    {
    }

    /// <summary>
    /// MonoBehaviour method called on GameObject by Unity during initialization phase.
    /// </summary>
    public void Start()
    {
        GenerateMechs();
    }

    public void GenerateMechs()
    {
        mechs = new List<FullMech>();
        playerMechs = new List<FullMech>();
        opponentMechs = new List<FullMech>();

        GenerateMech(MechPlacement.PLAYER_1, BattleTeam.PLAYER);
        GenerateMech(MechPlacement.PLAYER_2, BattleTeam.PLAYER);

        GameManager.Instance.SetupCam();

        GenerateMech(MechPlacement.OPPONENT_1, BattleTeam.OPPONENT);
        GenerateMech(MechPlacement.OPPONENT_2, BattleTeam.OPPONENT);

        BattleManager.Instance.SetupAndStartMatch(aiOpponent: true);
    }

    public void GenerateMech(MechPlacement placement, BattleTeam team, List<int> mechDef = null)
    {
        var pl = FindObjectsOfType<MechPlacementNode>().ToList().Find(p => p.placement == placement);
        GameObject mech = Instantiate(Resources.Load("Prefabs/Network/MechTest") as GameObject);
        mech.transform.position = pl.transform.position;
        mech.transform.parent = pl.transform;
        mech.transform.localPosition = Vector3.zero;
        mech.transform.localRotation = Quaternion.identity;
        var mechAgent = mech.GetComponent<CharacterAgentController>();
        
        if (team == BattleTeam.OPPONENT && mechDef != null)
        {
            mechAgent.SetMechDefOverride(mechDef);
        }

        mechAgent.team = team == BattleTeam.PLAYER ? CharacterTeam.PLAYER : CharacterTeam.OPPONENT;
        
        if (placement == MechPlacement.PLAYER_1 && GameManager.Instance.mode == GameMode.REALTIME_BATTLE)
        {
            mechAgent.manualControl = true;
        }

        mechAgent.Initialize();

        foreach(MechNetworkSetup md in mechDefs)
        {
            var partCaster = mechAgent.mech.GetCasterBySlot(md.slot);
            MechPartAbility partAbilities = partCaster.GetComponent<MechPartAbility>();
            if (partAbilities == null)
            {
                partAbilities = partCaster.gameObject.AddComponent<MechPartAbility>();
            }
            if (partAbilities.abilities == null)
                partAbilities.abilities = new List<AbilityCard>();
            partAbilities.abilities.Add(md.ability);
            partCaster.partAbilities = partAbilities;
        }

        if (mechs == null)
            mechs = new List<FullMech>();
        
        mechs.Add(mechAgent.mech);
        
        if (team == BattleTeam.PLAYER)
        {
            this.playerMechs.Add(mechAgent.mech);
        }
        else
        {
            this.opponentMechs.Add(mechAgent.mech);
        }       
    }

}
