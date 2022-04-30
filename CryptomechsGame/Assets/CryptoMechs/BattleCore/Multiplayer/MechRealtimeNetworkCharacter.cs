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


public class MechRealtimeNetworkCharacter : MonoBehaviourPunCallbacks, IPunObservable
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
    CharacterAgentController mech;

    private Vector3[] positions;
    private Quaternion[] quats;

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

        GeneratePlayerMech();
    }

    static bool playerMechsGenerated = false;
    static bool opponentMechsGenerated = false;

    public void GeneratePlayerMech()
    {
        mechs = new List<FullMech>();
        playerMechs = new List<FullMech>();
        opponentMechs = new List<FullMech>();

        if (photonView.IsMine)
        {
            mech = GenerateMech(MechPlacement.PLAYER_1, BattleTeam.PLAYER);

            ExitGames.Client.Photon.Hashtable hashtable = new ExitGames.Client.Photon.Hashtable();
            hashtable.Add("mech1", mechs[0].mechDef.ToArray());
            photonView.Controller.SetCustomProperties(hashtable);
            playerMechsGenerated = true;

            /*
            //mech.gameObject.AddComponent<PhotonTransformView>();
            PhotonView mview = mech.gameObject.AddComponent<PhotonView>();

            PhotonTransformView view = mech.gameObject.AddComponent<PhotonTransformView>();
            view.m_SynchronizePosition = true;
            view.m_SynchronizeRotation = true;

            mview.ObservedComponents = new List<Component>();
            mview.ObservedComponents.Add(view);
            
            foreach (var caster in mech.mech.casters)
            {
                PhotonTransformView cview = caster.gameObject.AddComponent<PhotonTransformView>();
                cview.m_SynchronizePosition = true;
                cview.m_SynchronizeRotation = true;
                
                photonView.ObservedComponents = new List<Component>();
                photonView.ObservedComponents.Add(view);

                //PhotonAnimatorView aview = caster.gameObject.AddComponent<PhotonAnimatorView>();

            }
            */
            GameManager.Instance.SetupCam();
        }
        else
        {
            StartCoroutine(LoadOpponentsRoutine());
        }
    }

    [PunRPC]
    public void UpdateTransforms(Vector3[] positions, Quaternion[] quats)
    {
        int l = positions.Length;

        if (this.mech == null || positions.Length == 0) return;
        int i = 0;

        this.mech.transform.localPosition = Vector3.Lerp(this.mech.transform.localPosition, positions[i], Time.deltaTime * 5f);
        this.mech.transform.localRotation = Quaternion.Lerp(this.mech.transform.localRotation , quats[i], Time.deltaTime * 5f);

        i++;
        if (i >= l) return;

        foreach (var c in mech.mech.casters)
        {
            c.transform.localPosition = Vector3.Lerp(c.transform.localPosition, positions[i], Time.deltaTime * 5f);
            c.transform.localRotation = Quaternion.Lerp(c.transform.localRotation, quats[i], Time.deltaTime * 5f);
            //c.transform.localRotation = quats[i];
            i++;
            if (i >= l) return;
        }
    }

    public IEnumerator LoadOpponentsRoutine()
    {
        while (!photonView.Controller.CustomProperties.ContainsKey("mech1"))
        {
            yield return null;
        }
        var m1 = photonView.Controller.CustomProperties["mech1"] as int[];

        this.mech = GenerateMech(MechPlacement.OPPONENT_1, BattleTeam.OPPONENT, m1.ToList());
        opponentMechsGenerated = true;

        //if (playerMechsGenerated && opponentMechsGenerated)
        //{
        //    BattleManager.Instance.SetupAndStartMatch(aiOpponent: false);
        //}
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

    public CharacterAgentController GenerateMech(MechPlacement placement, BattleTeam team, List<int> mechDef = null)
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

        if (!photonView.IsMine)
        {
            mechAgent.remoteControl = true;
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

        return mechAgent;
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

    public static void SendMove(int mechCasterIndex, int casterSlot, int casterAbilityIndex, int mechTargetIndex)
    {
        object[] content = new object[] { mechCasterIndex, casterSlot, casterAbilityIndex, mechTargetIndex }; // Array contains the target position and the IDs of the selected units
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
            
            TurnBasedAgent target = null;
            var mechs = FindObjectsOfType<CharacterAgentController>();

            switch(targetIndex)
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

            if (casterIndex > 0 && casterIndex <= opponentMechs.Count)
            {
                FullMech caster = mechs.FirstOrDefault(m => m.placement == MechPlacement.OPPONENT_1).mech;
                if (casterIndex == 2)
                {
                    caster = mechs.FirstOrDefault(m => m.placement == MechPlacement.OPPONENT_2).mech;
                }

                MechSlot slot = (MechSlot)casterSlot;
                var part = caster.GetCasterBySlot(slot);
                AbilityCard ability = part.partAbilities.GetAbilities()[abilityIndex];
                caster.character.battleTurnManager.PlayCard(ability, target);
            }
        }
    }

    public (Vector3[], Quaternion[]) GetTransforms()
    {
        if (this.mech == null) return (new Vector3[] { Vector3.zero }, new Quaternion[] { Quaternion.identity });
        List<Vector3> positions = new List<Vector3>();
        List<Quaternion> rotations = new List<Quaternion>();
        positions.Add(mech.transform.localPosition);
        rotations.Add(mech.transform.localRotation);

        foreach(var c in mech.mech.casters)
        {
            positions.Add(c.transform.localPosition);
            rotations.Add(c.transform.localRotation);
        }
        return (positions.ToArray(), rotations.ToArray());
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

            //var posquats = GetTransforms();
            //photonView.RPC("UpdateTransforms", RpcTarget.AllBuffered, posquats.Item1, posquats.Item2);

            if (this.Health <= 0f)
            {
                GameNetworkManager.Instance.LeaveRoom();
            }
        }

        if (!photonView.IsMine)
        {
            UpdateTransforms(this.positions, this.quats);
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

            (positions, quats) = GetTransforms();
            stream.SendNext(positions);
            stream.SendNext(quats);
        }
        else
        {
            // Network player, receive data
            this.IsFiring = (bool)stream.ReceiveNext();
            this.Health = (float)stream.ReceiveNext();

            this.positions = (Vector3[])stream.ReceiveNext();
            this.quats= (Quaternion[])stream.ReceiveNext();
        }
    }

    #endregion

}
