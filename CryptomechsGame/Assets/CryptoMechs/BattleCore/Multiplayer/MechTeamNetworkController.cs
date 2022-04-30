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

[System.Serializable]
public class MechNetworkSetup
{
    public MechSlot slot;
    public AbilityCard ability;
}

public class MechTeamNetworkController : MonoBehaviourPunCallbacks, IPunObservable
{
    #region Public Fields

    [Tooltip("The current Health of our player")]
    public float Health = 1f;

    [Tooltip("The local player instance. Use this to know if the local player is represented in the Scene")]
    public static GameObject LocalPlayerInstance;

    #endregion

    #region Private Fields

    [Tooltip("The Player's UI GameObject Prefab")]
    [SerializeField]
    private GameObject playerUiPrefab;

    [SerializeField]
    private GameObject mechTest;

    public List<MechNetworkSetup> mechDefs;
    public List<FullMech> mechs;

    public List<FullMech> playerMechs;
    public List<FullMech> opponentMechs;


    //True, when the user is firing
    bool IsFiring;

    #endregion

    #region MonoBehaviour CallBacks

    /// <summary>
    /// MonoBehaviour method called on GameObject by Unity during early initialization phase.
    /// </summary>
    public void Awake()
    {
        // #Important
        // used in GameManager.cs: we keep track of the localPlayer instance to prevent instanciation when levels are synchronized
        if (photonView.IsMine)
        {
            LocalPlayerInstance = gameObject;
        }
        
        // #Critical
        // we flag as don't destroy on load so that instance survives level synchronization, thus giving a seamless experience when levels load.
        DontDestroyOnLoad(gameObject);
    }

    /// <summary>
    /// MonoBehaviour method called on GameObject by Unity during initialization phase.
    /// </summary>
    public void Start()
    {
#if UNITY_5_4_OR_NEWER
        // Unity 5.4 has a new scene management. register a method to call CalledOnLevelWasLoaded.
        UnityEngine.SceneManagement.SceneManager.sceneLoaded += OnSceneLoaded;
#endif
        GameNetworkManager.Instance.OnOtherPlayerConnected += OnOtherPlayerConnected;

        GeneratePlayerMechs();
    }

    static bool playerMechsGenerated = false;
    static bool opponentMechsGenerated = false;

    static int team1Index = 0;
    static int team2Index = 0;

    public void GeneratePlayerMechs()
    {
        mechs = new List<FullMech>();
        playerMechs = new List<FullMech>();
        opponentMechs = new List<FullMech>();

        if (photonView.IsMine)
        {
            GenerateMech(MechPlacement.PLAYER_1, BattleTeam.PLAYER);
            GenerateMech(MechPlacement.PLAYER_2, BattleTeam.PLAYER);

            team1Index = CharacterAgentsManager.Instance.playerIndex;
            Debug.Log("T1: " + team1Index.ToString());

            ExitGames.Client.Photon.Hashtable hashtable = new ExitGames.Client.Photon.Hashtable();
            hashtable.Add("mech1", mechs[0].mechDef.ToArray());
            hashtable.Add("mech2", mechs[1].mechDef.ToArray());
            photonView.Controller.SetCustomProperties(hashtable);
            playerMechsGenerated = true;

            GameManager.Instance.SetupCam();
        }
        else
        {
            StartCoroutine(LoadOpponentsRoutine());
        }
    }

    public IEnumerator LoadOpponentsRoutine()
    {
        while (!photonView.Controller.CustomProperties.ContainsKey("mech1"))
        {
            yield return null;
        }
        var m1 = photonView.Controller.CustomProperties["mech1"] as int[];
        var m2 = photonView.Controller.CustomProperties["mech2"] as int[];

        GenerateMech(MechPlacement.OPPONENT_1, BattleTeam.OPPONENT, m1.ToList());
        GenerateMech(MechPlacement.OPPONENT_2, BattleTeam.OPPONENT, m2.ToList());
        opponentMechsGenerated = true;

        team2Index = CharacterAgentsManager.Instance.playerIndex;
        Debug.Log("T2: " + team2Index.ToString());

        if (playerMechsGenerated && opponentMechsGenerated)
        {
            BattleManager.Instance.SetupAndStartMatch(aiOpponent: false);
        }
    }

    public override void OnDisable()
    {
        // Always call the base to remove callbacks
        base.OnDisable();

#if UNITY_5_4_OR_NEWER
        UnityEngine.SceneManagement.SceneManager.sceneLoaded -= OnSceneLoaded;
#endif
        PhotonNetwork.NetworkingClient.EventReceived -= OnEvent;
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

    public void OnOtherPlayerConnected(Player player)
    {
        //GenerateMech(MechPlacement.OPPONENT_1, BattleTeam.OPPONENT);
        //GenerateMech(MechPlacement.OPPONENT_2, BattleTeam.OPPONENT);

        //BattleManager.Instance.SetupAndStartMatch(aiOpponent: false);
    }

    public void OnOtherPlayerDisconnected(Player player)
    {

    }

    public const byte PlayAbilityCardEventCode = 1;
    public const byte PlayAbilityMove = 2;

    public static void SendMove(int mechCasterIndex, int tileIndex = -1)
    {
        object[] content = new object[] { mechCasterIndex, tileIndex }; // Array contains the target position and the IDs of the selected units
        RaiseEventOptions raiseEventOptions = new RaiseEventOptions { Receivers = ReceiverGroup.Others }; // You would have to set the Receivers to All in order to receive this event on the local client as well
        PhotonNetwork.RaiseEvent(PlayAbilityMove, content, raiseEventOptions, SendOptions.SendReliable);
    }

    public static void SendNetworkAbility(int mechCasterIndex, int casterSlot, int casterAbilityIndex, int mechTargetIndex, int tileIndex = -1)
    {
        object[] content = new object[] { mechCasterIndex, casterSlot, casterAbilityIndex, mechTargetIndex, tileIndex }; // Array contains the target position and the IDs of the selected units
        RaiseEventOptions raiseEventOptions = new RaiseEventOptions { Receivers = ReceiverGroup.Others }; // You would have to set the Receivers to All in order to receive this event on the local client as well
        PhotonNetwork.RaiseEvent(PlayAbilityCardEventCode, content, raiseEventOptions, SendOptions.SendReliable);
    }

    public override void OnEnable()
    {
        base.OnEnable();
        PhotonNetwork.NetworkingClient.EventReceived += OnEvent;
    }

    private void OnEvent(EventData photonEvent)
    {
        byte eventCode = photonEvent.Code;
        if (eventCode == PlayAbilityCardEventCode)
        {
            object[] data = (object[])photonEvent.CustomData;
            int casterIndex = (int)data[0];
            int casterSlot = (int)data[1];
            int abilityIndex = (int)data[2];
            int targetIndex = (int)data[3];
            int tileID = (int)data[4];

            TurnBasedAgent caster = null;
            TurnBasedAgent target = null;


            var mechs = FindObjectsOfType<CharacterAgentController>();
            
            /*
            switch (targetIndex)
            {
                case 1:
                    target = mechs.FirstOrDefault(m => m.placement == MechPlacement.OPPONENT_1).battleTurnManager;
                    break;
                case 2:
                    target = mechs.FirstOrDefault(m => m.placement == MechPlacement.OPPONENT_2).battleTurnManager;
                    break;
                case 3:
                    target = mechs.FirstOrDefault(m => m.placement == MechPlacement.PLAYER_1).battleTurnManager;
                    break;
                case 4:
                    target = mechs.FirstOrDefault(m => m.placement == MechPlacement.PLAYER_2).battleTurnManager;
                    break;
                case 0:
                default:
                    break;
            }
            */

            Debug.Log("caster : " + casterIndex.ToString() + " : target : " + targetIndex.ToString());

            /*
            if (casterIndex < team1Index)
                casterIndex += (team1Index - 1);
            if (targetIndex < team1Index)
                targetIndex += (team1Index - 1);

            if (casterIndex < team2Index && casterIndex >= team1Index)
                casterIndex -= (team1Index - 1);
            if (targetIndex < team2Index && targetIndex >= team1Index)
                targetIndex -= (team1Index - 1);
            */
            if (casterIndex == 1)
                casterIndex = 3;
            else if (casterIndex == 2)
                casterIndex = 4;
            else if (casterIndex == 3)
                casterIndex = 1;
            else if (casterIndex == 4)
                casterIndex = 2;

            if (targetIndex == 1)
                targetIndex = 3;
            else if (targetIndex == 2)
                targetIndex = 4;
            else if (targetIndex == 3)
                targetIndex = 1;
            else if (targetIndex == 4)
                targetIndex = 2;

            CharacterAgentController casterCharacter = casterIndex >= 0 ? CharacterAgentsManager.Instance.GetCharacterByIndex(casterIndex) as CharacterAgentController : null;
            caster = casterCharacter != null ? casterCharacter.battleTurnManager : null;

            CharacterAgentController targetCharacter = targetIndex >= 0 ? CharacterAgentsManager.Instance.GetCharacterByIndex(targetIndex) as CharacterAgentController : null;
            target = targetCharacter != null ? targetCharacter.battleTurnManager : null;


            if (casterIndex > 0)
            {
                MechSlot slot = (MechSlot)casterSlot;
                var part = casterCharacter.mech.GetCasterBySlot(slot);
                AbilityCard ability = part.partAbilities.GetAbilities()[abilityIndex];
                caster.PlayCard(ability, target, tileID);
            }
        }
        else if (eventCode == PlayAbilityMove)
        {
            object[] data = (object[])photonEvent.CustomData;
            int casterIndex = (int)data[0];
            int tileID = (int)data[1];

            TurnBasedAgent caster = null;
            
            if (casterIndex == 1)
                casterIndex = 3;
            else if (casterIndex == 2)
                casterIndex = 4;
            else if (casterIndex == 3)
                casterIndex = 1;
            else if (casterIndex == 4)
                casterIndex = 2;

            if (casterIndex > 0)
            {
                CharacterAgentController casterCharacter = casterIndex >= 0 ? CharacterAgentsManager.Instance.GetCharacterByIndex(casterIndex) as CharacterAgentController : null;
                caster = casterCharacter != null ? casterCharacter.battleTurnManager : null;
                caster?.PlayCard(BattleTileManager.Instance.moveAction, null, tileID);
            }
        }
    }

        /// <summary>
        /// MonoBehaviour method called on GameObject by Unity on every frame.
        /// Process Inputs if local player.
        /// Show and hide the beams
        /// Watch for end of game, when local player health is 0.
        /// </summary>
    public void Update()
    {
        // we only process Inputs and check health if we are the local player
        if (photonView.IsMine)
        {
            this.ProcessInputs();

            if (this.Health <= 0f)
            {
                GameNetworkManager.Instance.LeaveRoom();
            }
        }

    }


#if !UNITY_5_4_OR_NEWER
        /// <summary>See CalledOnLevelWasLoaded. Outdated in Unity 5.4.</summary>
        void OnLevelWasLoaded(int level)
        {
            this.CalledOnLevelWasLoaded(level);
        }
#endif


    /// <summary>
    /// MonoBehaviour method called after a new level of index 'level' was loaded.
    /// We recreate the Player UI because it was destroy when we switched level.
    /// Also reposition the player if outside the current arena.
    /// </summary>
    /// <param name="level">Level index loaded</param>
    void CalledOnLevelWasLoaded(int level)
    {
        // check if we are outside the Arena and if it's the case, spawn around the center of the arena in a safe zone
        //if (!Physics.Raycast(transform.position, -Vector3.up, 5f))
        //{
        //    transform.position = new Vector3(0f, 5f, 0f);
        //}
    }

    #endregion

    #region Private Methods


#if UNITY_5_4_OR_NEWER
    void OnSceneLoaded(UnityEngine.SceneManagement.Scene scene, UnityEngine.SceneManagement.LoadSceneMode loadingMode)
    {
        this.CalledOnLevelWasLoaded(scene.buildIndex);
    }
#endif

    /// <summary>
    /// Processes the inputs. This MUST ONLY BE USED when the player has authority over this Networked GameObject (photonView.isMine == true)
    /// </summary>
    void ProcessInputs()
    {
        
    }

    #endregion

    #region IPunObservable implementation

    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.IsWriting)
        {
            // We own this player: send the others our data
            stream.SendNext(this.IsFiring);
            stream.SendNext(this.Health);
        }
        else
        {
            // Network player, receive data
            this.IsFiring = (bool)stream.ReceiveNext();
            this.Health = (float)stream.ReceiveNext();
        }
    }

    #endregion

}
