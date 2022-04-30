using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System.IO;
using Tamarin.FirebaseX;
using UnityEngine.UI;
using System.Threading.Tasks;

public enum PostProcessRuleType
{
    EXCLUSIVE_BRANCH,
    DELETEBRANCH_IF_EXISTS
}

public enum RarityToAssemble
{
    ALL = 0,
    COMMON,
    UNCOMMON,
    RARE,
    EPIC,
    LEGENDARY
}

[System.Serializable]
public class MechConnectionRule
{
    public PostProcessRuleType ruleType;
    public MechPartPlacement placementEnabled;
    public List<MechPartPlacement> attachedPlacements;
    public MechPartPlacement placementDisabled;
}

[System.Serializable]
public class MechPartRules
{
    public MechPartPlacement placement;
    public int maxOccurences = 5;
    public List<MechPartPlacement> excludes;
}

[System.Serializable]
public class ConnectedPart
{
    public MechPartComponent part;
    public MechPartPlacement placement;
    public GameObject partGO;
    public ConnectedPart (MechPartPlacement placement, MechPartComponent part, GameObject partGO)
    {
        this.placement = placement;
        this.part = part;
        this.partGO = partGO;
    }
}

[System.Serializable]
public class MechPartConnection
{
    public MechPartPlacement parentPlacement;    
    public List<ConnectedPart> connectedParts;
}

/*
public class InvalidCombinations
{
    public MechPartPlacement placement;
    public int maxOccurences = 5;
    public List<MechPartPlacement> excludes;
}
*/
[System.Serializable]
public class RarityHighlightGlows
{
    public RarityToAssemble rarity;
    public HighlightPlus.HighlightProfile profile;
}

public class MechAssembler : MonoBehaviour
{
    public static MechAssembler Instance;

    public TMPro.TMP_InputField nftIDInput;
    public RarityToAssemble assembleMode = RarityToAssemble.ALL;

    public GameObject stagePivot;
    public Dictionary<MechPartPlacement, MechPartComponent> pinnedParts;
    public bool enablePartPinning = false;

    public List<MechPartRules> mechPartRules;
    public List<MechConnectionRule> postProcessRules;

    public Camera snapshotCam;
    public RarityHighlightGlows defaultHighlightProfile;
    public List<RarityHighlightGlows> highlightProfiles;

    public List<Texture2D> patterns;

    [HideInInspector]
    public List<GameObject> assembledParts;

    public FullMech selectedMech = null;
    public FullMech lastSelectedMech = null;

    //[HideInInspector]
    [SerializeField]
    //public List<MechPartPlacement, List<(MechPartPlacement, GameObject)>> partsComposition;
    public List<MechPartConnection> partsComposition;

    public MechPartConnection GetPartConnection(MechPartPlacement parent)
    {
        var parts = partsComposition.Find(c => c.parentPlacement == parent);
        return parts;
    }

    public List<ConnectedPart> GetConnectedParts(MechPartPlacement parent)
    {
        var parts = partsComposition.Find(c => c.parentPlacement == parent);
        if (parts == null) return null;
        return parts.connectedParts;
    }

    public void AddConnectedParts(MechPartPlacement parent, List<(MechPartPlacement, MechPartComponent, GameObject)> connectedParts)
    {
        //if (!ContainsPartKey(parent))
        if (partsComposition == null) partsComposition = new List<MechPartConnection>();
        bool containsPart = ContainsPartKey(parent);
        MechPartConnection newConnection = containsPart ? GetPartConnection(parent) : new MechPartConnection();
        
        if (!containsPart) partsComposition.Add(newConnection);

        newConnection.parentPlacement = parent;
        
        if (newConnection.connectedParts == null)
            newConnection.connectedParts = new List<ConnectedPart>();
        
        foreach((MechPartPlacement, MechPartComponent, GameObject) p in connectedParts)
        {
            newConnection.connectedParts.Add(new ConnectedPart(p.Item1, p.Item2, p.Item3));
        }

        //newConnection.connectedParts = connectedParts;
    }

    public bool ContainsPartKey(MechPartPlacement parent)
    {
        var parts = partsComposition.Find(c => c.parentPlacement == parent);
        return (parts != null);
    }

    public async void BuildRandomMech(FullMech m, int id = 1)
    {
        var web3 = Web3Manager.Instance;        
        await web3.GetBagParts(id);

        //MechFXManager.Instance?.CLearFX();
        MechComponentLibrary lib = GameObject.FindObjectOfType<MechComponentLibrary>();

        m.ResetParts();
        if (assembledParts == null) assembledParts = new List<GameObject>();
        for (int i = 0; i < assembledParts.Count; ++i)
        {
            GameObject.DestroyImmediate(assembledParts[i]);
        }
        assembledParts = new List<GameObject>();
        partsComposition = new List<MechPartConnection>(); // Dictionary<MechPartPlacement, List<(MechPartPlacement, GameObject)>>();

        // build base

        MechPartPlacement placement = MechPartPlacement.LEG;
        RandomBuildPart(lib, MechSlot.LEGS, placement, new List<MechPartPlacement>() { placement }, null, web3.partIDs, true);
        CameraFocuser camFocus = GameObject.FindObjectOfType<CameraFocuser>();
        var rootGO = GetConnectedParts(MechPartPlacement.LEG)[0].partGO;
        //rootGO.transform.Rotate(Vector3.up, 27f);
        //camFocus?.FocusCam(rootGO);

        PostProcessMechAssembly();
        AssignToMech(m, rootGO, web3.partIDs);
    }

    public async void BuildRandomMech(FullMech m, bool fromAssembly = true)
    {
        var web3 = Web3Manager.Instance;
        int id;
        if (int.TryParse(nftIDInput.text, out id))
        {
            if (id > 0)
            {
                await web3.GetBagParts(id);
            }
        }

        //MechFXManager.Instance?.CLearFX();
        MechComponentLibrary lib = GameObject.FindObjectOfType<MechComponentLibrary>();

        m.ResetParts();
        if (assembledParts == null) assembledParts = new List<GameObject>();
        for (int i = 0; i < assembledParts.Count; ++i)
        {
            GameObject.DestroyImmediate(assembledParts[i]);
        }
        assembledParts = new List<GameObject>();
        partsComposition = new List<MechPartConnection>(); // Dictionary<MechPartPlacement, List<(MechPartPlacement, GameObject)>>();

        // build base

        MechPartPlacement placement = MechPartPlacement.LEG;
        RandomBuildPart(lib, MechSlot.LEGS, placement, new List<MechPartPlacement>() { placement }, null, web3.partIDs, fromAssembly);
        CameraFocuser camFocus = GameObject.FindObjectOfType<CameraFocuser>();
        var rootGO = GetConnectedParts(MechPartPlacement.LEG)[0].partGO;
        //rootGO.transform.Rotate(Vector3.up, 27f);
        //camFocus?.FocusCam(rootGO);

        PostProcessMechAssembly();
        AssignToMech(m, rootGO, web3.partIDs);
    }

    public void BuildFullyRandomMech(FullMech m)
    {
        if (m.mechDef != null)
        {
            BuildMech(m, m.mechDef);
            return;
        }

        MechComponentLibrary lib = GameObject.FindObjectOfType<MechComponentLibrary>();
        List<int> randomParts = new List<int>();
        //var web3 = Web3Manager.Instance;   

        for (int i = 0; i < 9; ++i)
        {

            MechSlot s = (MechSlot)i;
            MechPartPlacement pl = MechPartComponent.GetRandomPartTypeFromSlot(s);
            var parts = lib.MechPartDefinitions.FindAll(p => p.placement == pl);
            int rand = Random.Range(0, parts.Count);
            int id = parts[rand].partIndex;
            randomParts.Add(id);
        }

        BuildMech(m, randomParts);
    }

    public void BuildMech(FullMech m, List<int> mechDef)
    {
        MechComponentLibrary lib = GameObject.FindObjectOfType<MechComponentLibrary>();
        
        //MechFXManager.Instance?.CLearFX();

        m.ResetParts();
        if (assembledParts == null) assembledParts = new List<GameObject>();
        for (int i = 0; i < assembledParts.Count; ++i)
        {
            GameObject.DestroyImmediate(assembledParts[i]);
        }
        assembledParts = new List<GameObject>();
        partsComposition = new List<MechPartConnection>(); // Dictionary<MechPartPlacement, List<(MechPartPlacement, GameObject)>>();

        // build base

        MechPartPlacement placement = MechPartPlacement.LEG;
        RandomBuildPart(lib, MechSlot.LEGS, placement, new List<MechPartPlacement>() { placement }, null, mechDef, true);
        //CameraFocuser camFocus = GameObject.FindObjectOfType<CameraFocuser>();
        var rootGO = GetConnectedParts(MechPartPlacement.LEG)[0].partGO;
        //rootGO.transform.Rotate(Vector3.up, 27f);
        //camFocus?.FocusCam(rootGO);

        PostProcessMechAssembly();
        AssignToMech(m, rootGO, mechDef);
    }

    void OnGUI()
    {
        return;
        if (GUI.Button(new Rect(10, 10, 150, 100), "RANDOMIZE"))
        {
            ResetParts();
            MechPartPlacement placement = MechPartPlacement.LEG;
            RandomBuildPart(MechComponentLibrary.Instance, MechSlot.LEGS, placement, new List<MechPartPlacement>() { placement }, null);
            //CameraFocuser camFocus = GameObject.FindObjectOfType<CameraFocuser>();
            //camFocus?.FocusCam(partsComposition[MechPartPlacement.LEG][0].Item2);
            //PostProcessMechAssembly();
        }
    }

    public void GenerateRandomFullMech()
    {
        ResetParts();
        MechPartPlacement placement = MechPartPlacement.LEG;

        RandomBuildPart(MechComponentLibrary.Instance, MechSlot.LEGS, placement, new List<MechPartPlacement>() { placement }, null);

        CameraFocuser camFocus = GameObject.FindObjectOfType<CameraFocuser>();
        var rootGO = GetConnectedParts(MechPartPlacement.LEG)[0].partGO;
        //camFocus.
        //rootGO.transform.Rotate(Vector3.up, 27f);
        //camFocus?.FocusCam(rootGO);

        PostProcessMechAssembly();
    }

    public async Task<bool> SnapshotMech()
    {
        int mechID = Web3Manager.Instance.nftID;

        Camera Cam = snapshotCam;
        RenderTexture currentRT = RenderTexture.active;
        RenderTexture.active = Cam.targetTexture;
        Cam.Render();
        Texture2D Image = new Texture2D(Cam.targetTexture.width, Cam.targetTexture.height);
        Image.ReadPixels(new Rect(0, 0, Cam.targetTexture.width, Cam.targetTexture.height), 0, 0);
        Image.Apply();
        RenderTexture.active = currentRT;
        var Bytes = Image.EncodeToPNG();
        
        string testFile = mechID.ToString() + ".png";
        var s3Manager = GameObject.FindObjectOfType<S3Manager>();
        s3Manager.Upload(testFile, Bytes);
        
        //FirebaseManager fbManager = GameObject.FindObjectsOfType<FirebaseManager>()[0];
        //fbManager.UploadFile(testFile, Bytes);
        //var success = await Upload(testFile, Bytes);
        
        Destroy(Image);
        return true;
    }

    public Button uploadBtn;

    public async Task<bool> Upload(string testFile, byte[] Bytes)
    {
        var upload = await FirebaseAPI.Instance.StorageUpload("gs://cryptomechs-1e6a4.appspot.com", testFile, Bytes, "image/png");
        return upload;
    }

    public void ClearPinnedParts()
    {
        pinnedParts?.Clear();
    }

    public void PinPart(MechPartComponent part)
    {
        if (!enablePartPinning) return;
        if (pinnedParts == null) pinnedParts = new Dictionary<MechPartPlacement, MechPartComponent>();
        if (!pinnedParts.ContainsKey(part.placement))
        {
            pinnedParts.Add(part.placement, part);
        }
        else
        {
            pinnedParts[part.placement] = part;
        }
    }

    public void ResetParts()
    {
        //if (MechFXManager.Instance != null) MechFXManager.Instance.CLearFX();

        if (assembledParts == null) assembledParts = new List<GameObject>();
        for (int i = 0; i < assembledParts.Count; ++i)
        {
            //MechFXManager.Instance.ClearMechFX(assembledParts[i]);
            GameObject.DestroyImmediate(assembledParts[i]);
        }
        assembledParts = new List<GameObject>();
        //partsComposition = new Dictionary<MechPartPlacement, List<(MechPartPlacement, GameObject)>>();
        partsComposition = new List<MechPartConnection>();

    }

    public void PostProcessMechAssembly()
    {
        return;
        foreach (MechConnectionRule rule in postProcessRules)
        {
            if (!ContainsPartKey(rule.placementEnabled)) continue;
            if (!ContainsPartKey(rule.placementDisabled)) continue;
 
            var a = GetConnectedParts(rule.placementEnabled);
            var b = GetConnectedParts(rule.placementDisabled);
            
            switch (rule.ruleType)
            {
                case PostProcessRuleType.EXCLUSIVE_BRANCH:
                    if (a.Exists(p => rule.attachedPlacements.Contains(p.placement)))
                    {
                        for (int i = 0; i < b.Count; ++i)
                        {
                            var c = b[i];
                            if (rule.attachedPlacements.Contains(c.placement))
                            {
                                GameObject.DestroyImmediate(c.partGO);
                                b.Remove(c);
                                i--;
                            }
                        }
                    }
                    break;
                case PostProcessRuleType.DELETEBRANCH_IF_EXISTS:
                    {
                        if (ContainsPartKey(rule.placementEnabled))
                        {
                            for (int i = 0; i < b.Count; ++i)
                            {
                                var c = b[i];
                                if (rule.attachedPlacements.Contains(c.placement))
                                {
                                    GameObject.DestroyImmediate(c.partGO);
                                    b.Remove(c);
                                    i--;
                                }
                            }
                        }
                    }
                    break;

            }
            
        }
    }

    public bool CanBuildPart(MechPartPlacement partType)
    {
        if (partType == MechPartPlacement.ANTENNA) return false;
        return true;
        MechPartRules rule = mechPartRules.Find(r => r.placement == partType);
        if (rule != null)
        {
            if (ContainsPartKey(rule.placement))
            {
                var a = GetConnectedParts(rule.placement);
                if (a.Count >= rule.maxOccurences)
                {
                    return false;
                }
            }
        }

        if (mechPartRules.Exists(r => r.excludes != null && r.excludes.Contains(partType) && ContainsPartKey(r.placement)))
        {
            return false;
        }

        return true;
    }

    public void AddPart(MechPartPlacement parentPlacement, MechPartPlacement placement, MechPartComponent part, GameObject go)
    {
        if (assembledParts == null) assembledParts = new List<GameObject>();
        if (partsComposition == null) partsComposition = new List<MechPartConnection>();
        assembledParts.Add(go);
        AddConnectedParts(parentPlacement, new List<(MechPartPlacement, MechPartComponent, GameObject)>() { (placement, part, go) });
        /*
        if (!ContainsPartKey(parentPlacement))
        {
            AddConnectedParts(parentPlacement, new List<(MechPartPlacement, GameObject)>() { (placement, go) });
        }
        else
        {
            partsComposition[parentPlacement].Add((placement, go));
        }
        */
    }

    // Start is called before the first frame update
    void Start()
    {
        Debug.Log("MechAssembler reporting");
        Instance = this;
        uploadBtn.onClick.AddListener(async () => { await SnapshotMech(); });
    }

    public void SelectMech(FullMech mech)
    {
        if (selectedMech == mech) return;
        lastSelectedMech = selectedMech;
        selectedMech = mech;
        if (mech.GetTeam() == CharacterTeam.PLAYER)
        {
            //CustomisationCameraSystem.Instance?.TriggerCamera(mech.transform, Vector3.up * 2.5f, 1f, 12f, 45f, false);
            //CustomisationCameraSystem.Instance?.TriggerCamera(mech.transform, Vector3.up * 8.5f, Vector3.up * 1f, 1f, 18f, 45f, false);

            //CustomisationCameraSystem.Instance?.TriggerCamera(mech.transform, Vector3.up * 20f, Vector3.up * 1.5f, 1f, 22f, 45f, false);
            CustomisationCameraSystem.Instance?.TriggerCamera(CustomisationCameraSystem.Instance.topDownCamDef, mech.transform, 1f, true);
        }
    }

    // Update is called once per frame
    void Update()
    {
        return;
        if (selectedMech == null)
        {
            List<FullMech> mechSlots = transform.GetComponentsInChildren<FullMech>().ToList();
            SelectMech(mechSlots[0]);
        }

        /*
        if (Input.GetKeyDown(KeyCode.A))
        {
            //lastSelectedMech = selectedMech;
            var fMechs = transform.GetComponentsInChildren<FullMech>();
            if (selectedMech == null || fMechs.Length < 2 || selectedMech == fMechs[1])
                SelectMech(fMechs[0]);
            else if (selectedMech == fMechs[0])
            {
                SelectMech(fMechs[1]);
            }
            else
            {
                SelectMech(fMechs[0]);
            }
            //selectedMech.SelectMechUI();

            return;
        }
        */

        if (Input.GetKeyDown(KeyCode.R))
        {
            List<FullMech> mechSlots = transform.GetComponentsInChildren<FullMech>().ToList();
            if (selectedMech != null)
                BuildRandomMech(selectedMech, false);
        }

        //GameObject.FindObjectOfType<MechMintUIManager>()?.SetFullMechUI();

        if (Input.GetKeyDown(KeyCode.S))
        {
            //var fMech = transform.GetComponentInChildren<FullMech>();
            selectedMech?.SwapSelected();
            return;
        }

        /*
        if (Input.GetKeyDown(KeyCode.S))
        {
            var fMech = transform.GetComponentInChildren<FullMech>();
            fMech?.SwapSelected();
            return;
        }
        */
        /*
        if (Input.GetKeyDown(KeyCode.R))
        {
            ResetParts();
            MechPartPlacement placement = MechPartPlacement.LEG;
            RandomBuildPart(MechComponentLibrary.Instance, placement, new List<MechPartPlacement>() { placement }, null);
        }

        if (Input.GetKeyDown(KeyCode.C))
        {
            RandomizeSwapPart(MechPartPlacement.COCKPIT);
        }
        if (Input.GetKeyDown(KeyCode.D))
        {
            RandomizeSwapPart(MechPartPlacement.SHOULDERS);
        }
        if (Input.GetKeyDown(KeyCode.L))
        {
            RandomizeSwapPart(MechPartPlacement.LEG);
        }
        if (Input.GetKeyDown(KeyCode.W))
        {
            RandomizeSwapPart(MechPartPlacement.WEAPON);
        }
        if (Input.GetKeyDown(KeyCode.B))
        {
            RandomizeSwapPart(MechPartPlacement.BACKPACK);
        }
        if (Input.GetKeyDown(KeyCode.G))
        {
            RandomizeSwapPart(MechPartPlacement.GADGET);
        }
        if (Input.GetKeyDown(KeyCode.S))
        {
            RandomizeSwapPart(MechPartPlacement.SHIELD);
        }
        */
        /*
        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            List<FullMech> mechSlots = transform.GetComponentsInChildren<FullMech>().ToList();
            if (mechSlots.Count > 0)
                BuildRandomMech(mechSlots[0]);
        }

        if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            List<FullMech> mechSlots = transform.GetComponentsInChildren<FullMech>().ToList();
            if (mechSlots.Count > 1)
                BuildRandomMech(mechSlots[1]);
        }
        */
        FullMech m = GetComponentInChildren<FullMech>();
        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            BuildRandomMech(m, 1);
        }
        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            BuildRandomMech(m, 1);
        }
        if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            BuildRandomMech(m, 2);
        }
        if (Input.GetKeyDown(KeyCode.Alpha3))
        {
            BuildRandomMech(m, 3);
        }
        if (Input.GetKeyDown(KeyCode.Alpha4))
        {
            BuildRandomMech(m, 4);
        }
        if (Input.GetKeyDown(KeyCode.Alpha5))
        {
            BuildRandomMech(m, 5);
        }
        if (Input.GetKeyDown(KeyCode.Alpha6))
        {
            BuildRandomMech(m, 6);
        }
        if (Input.GetKeyDown(KeyCode.Alpha7))
        {
            BuildRandomMech(m, 7);
        }
        if (Input.GetKeyDown(KeyCode.Alpha8))
        {
            BuildRandomMech(m, 8);
        }
        if (Input.GetKeyDown(KeyCode.Alpha9))
        {
            BuildRandomMech(m, 9);
        }
        if (Input.GetKeyDown(KeyCode.Alpha0))
        {
            BuildRandomMech(m, 10);
        }

    }

    public void AssignToMech(FullMech mech, GameObject rootGO, List<int> partIDs)
    {
        mech.SetFromAssembly(this, rootGO, partIDs);
        this.assembledParts.Clear();
        this.partsComposition.Clear();
    }

    public void PreviewPart(MechPartComponent part)
    {
        if (MechFXManager.Instance != null) MechFXManager.Instance.ClearAllFX ();
        GeneratePart(part);
        var camFocus = GameObject.FindObjectOfType<CameraFocuser>();
        //if (part.placement == MechPartPlacement.BACKPACK)
        //{
        //    assembledParts[0].transform.Rotate(0, 180, 0);
        //}
        camFocus.focusObject = assembledParts[0];
        camFocus?.FocusCam(assembledParts[0]);
    }

    public void GeneratePart(MechPartComponent part)
    {
        ResetParts();
        GameObject newPart = GameObject.Instantiate(part.prefab, null);
        if (part.placement == MechPartPlacement.SHIELD || part.placement == MechPartPlacement.ARM)
            newPart.transform.Rotate(Vector3.up * 120);
        newPart.transform.Rotate(Vector3.up, -15f);

        var geoms = newPart.GetComponentsInChildren<Renderer>();
        var geomRoot = geoms[0];

        foreach (var geom in geoms)
        {
            //if (geom != geomRoot)
            //{
            //    geom.transform.parent = geomRoot.transform;
            //}
            geom.material = Instantiate<Material>(geom.material);
            geom.material.SetFloat("Vector1_7016e6c5ee73463b8becb8d73f29595d", part.hueShift);
            geom.material.SetFloat("Vector1_b4c2fe157818424089fcb2cced9090fb", part.hueShiftSecondary);
            geom.material.SetFloat("Vector1_e6c478fd46834b87a953d084d4cb7070", 1f);

            if (MechFXManager.Instance == null)
                MechFXManager.Instance = GameObject.FindObjectOfType<MechFXManager>();
            if (MechFXManager.Instance != null)
                MechFXManager.Instance.SetupMechFX(newPart, geom.gameObject, part.fx);
        }

        this.AddPart(part.placement, part.placement, part, newPart);
        if (newPart.GetComponent<Animator>() != null)
        {
            newPart.GetComponent<Animator>().applyRootMotion = false;
        }
    }
    
    public void RandomizeSwapPart(MechPartPlacement partPlacement)
    {
        List<GameObject> objectsToSwap = new List<GameObject>();
        List<GameObject> parents = new List<GameObject>(); ;
        List<MechPartPlacement> keys = new List<MechPartPlacement>();
        List<int> keys2 = new List<int>();

        foreach (MechPartConnection p in partsComposition)
        {
            for (int j = 0; j < p.connectedParts.Count; ++j)
            {
                MechPartPlacement k = p.connectedParts[j].placement;
                if (k == partPlacement)
                {
                    GameObject part = p.connectedParts[j].partGO;
                    objectsToSwap.Add(part);
                    keys.Add(p.parentPlacement);
                    keys2.Add(j);
                    //parents.Add(part.transform.parent.gameObject);
                }
                //GameObject part = partsComposition[p.Key][j].Item2;
            }
        }

        var lib = MechComponentLibrary.Instance;
        int minRarity = 0;
        int maxRarity = 5;
        if (assembleMode != RarityToAssemble.ALL)
        {
            minRarity = (int)assembleMode;
            maxRarity = (int)assembleMode;
        }

        List<MechPartComponent> candidateParts = lib.MechPartDefinitions.FindAll(c => c.placement == partPlacement && CanBuildPart(c.placement)
        && c.rarity >= minRarity && c.rarity <= maxRarity).ToList();
        
        if (candidateParts == null || candidateParts.Count == 0) return;

        int rand = Random.Range(0, candidateParts.Count);

        MechPartComponent selectedPart = candidateParts[rand];

        for (int i = 0; i < objectsToSwap.Count; ++i)
        {
            GameObject p = objectsToSwap[i];

            GameObject newPart = GameObject.Instantiate(selectedPart.prefab, p.transform.parent);
            if (newPart.GetComponent<Animator>() != null)
            {
                newPart.GetComponent<Animator>().applyRootMotion = false;
            }

            var geoms = newPart.GetComponentsInChildren<Renderer>();
            var geomRoot = geoms[0];

            MechFXManager.Instance.ClearMechFX(p);

            foreach (var geom in geoms)
            {
                geom.material = Instantiate<Material>(geom.material);
                geom.material.SetFloat("Vector1_7016e6c5ee73463b8becb8d73f29595d", selectedPart.hueShift);
                geom.material.SetFloat("Vector1_b4c2fe157818424089fcb2cced9090fb", selectedPart.hueShiftSecondary);

                if (MechFXManager.Instance == null)
                    MechFXManager.Instance = GameObject.FindObjectOfType<MechFXManager>();
                if (MechFXManager.Instance != null)
                    MechFXManager.Instance.SetupMechFX(newPart, geom.gameObject, selectedPart.fx);
            }

            newPart.transform.localPosition = Vector3.zero;
            newPart.transform.localRotation = Quaternion.identity;

            var connectorsO = FindDeepChildren(p.transform, "mount_");
            var connectorsN = FindDeepChildren(newPart.transform, "mount_");

            foreach(GameObject o in connectorsO)
            {
                if (o.transform.childCount == 0) continue;
                var child = o.transform.GetChild(0);

                GameObject n = connectorsN.Find(con => con.name.ToLower() == o.name.ToLower());
                if (n != null)
                {
                    child.transform.parent = n.transform;
                    child.transform.localPosition = Vector3.zero;
                    child.transform.localRotation = Quaternion.identity;
                    connectorsN.Remove(n);
                }
            }
            var newVal = GetConnectedParts(keys[i]);
            newVal[keys2[i]].partGO = newPart;
            partsComposition.Find(p=> p.parentPlacement == keys[i]).connectedParts = newVal;
            Destroy(p);
        }
    }

    public void RandomBuildPart(MechComponentLibrary lib, MechSlot socket, MechPartPlacement parentType, List<MechPartPlacement> partTypes, GameObject parent, bool fromAssembly = false)
    {
        RandomBuildPart(lib, socket, parentType, partTypes, parent, null, fromAssembly = false);
    }
    public void RandomBuildPart(MechComponentLibrary lib, MechSlot socket, MechPartPlacement parentType, List<MechPartPlacement> partTypes, GameObject parent, List<int> mechDef, bool fromAssembly = false)
    {
        if (socket == MechSlot.UNDEFINED) return;

        MechAssembler assembler = this;
        if (partTypes.Exists(x => x == MechPartPlacement.SHOULDERS_HALF_L || x == MechPartPlacement.SHOULDERS_HALF_R)) return;

        if (parentType != MechPartPlacement.LEG && partTypes.Exists(p=> p == parentType)) return;

        if (partTypes.All( p=> !assembler.CanBuildPart(p)))
        {
            return;
        }
        //foreach (MechPartPlacement p in partTypes)
        //{
        //    if (!assembler.CanBuildPart(partType))
        //    {
        //        return;
        //    }
        //}
        int minRarity = 0;
        int maxRarity = 5;
        if (assembleMode != RarityToAssemble.ALL)
        {
            minRarity = (int)assembleMode;
            maxRarity = (int)assembleMode;
        }

        List<MechPartComponent> candidateParts = lib.MechPartDefinitions.FindAll(c => partTypes.Contains(c.placement) && CanBuildPart(c.placement)
            && c.rarity >= minRarity && c.rarity <= maxRarity).ToList();

        //List<MechPartComponent> candidateParts = lib.MechPartDefinitions.FindAll(c => partTypes.Contains(c.placement) && CanBuildPart(c.placement)).ToList();
        if (candidateParts == null || candidateParts.Count == 0) return;

        int rand = Random.Range(0, candidateParts.Count);
        
        MechPartComponent selectedPart = candidateParts[rand];
        Web3Manager w3 = GameObject.FindObjectOfType<Web3Manager>();
        if (fromAssembly && mechDef != null)
        {
            if (mechDef != null && mechDef.Count == 9)
            {
                int id = mechDef[(int)socket];
                if (id <= 0) return;
                selectedPart = lib.MechPartDefinitions.Find(m => m.partIndex == id);
            }
        }

        if (enablePartPinning && pinnedParts != null && pinnedParts.ContainsKey(selectedPart.placement))
            selectedPart = pinnedParts[selectedPart.placement];
        
        if (selectedPart.prefab == null)
        {
            Debug.Log("NULLCHECK");
        }
        GameObject newPart = GameObject.Instantiate(selectedPart.prefab, parent == null ? null : parent.transform);
        newPart.transform.localPosition = Vector3.zero;
        
        if (newPart.GetComponent<Animator>() != null)
        {
            newPart.GetComponent<Animator>().applyRootMotion = false;
            //newPart.GetComponent<Animator>().enabled = false;
        }
        var mGeoms = newPart.AddComponent<MechPartGeoms>();

        mGeoms.Setup(selectedPart);
        //mGeoms.PopulateGeoms(selectedPart.placement);

        var geoms = newPart.GetComponentsInChildren<Renderer>();
        var geomRoot = geoms[0];

        foreach (var geom in geoms)
        {
            //if (geom != geomRoot)
            //{
            //    geom.transform.parent = geomRoot.transform;
            //}
            geom.material = Instantiate<Material>(geom.material);
            //geom.material.SetFloat("Vector1_7016e6c5ee73463b8becb8d73f29595d", selectedPart.hueShift);
            geom.material.SetFloat("Vector1_e6c478fd46834b87a953d084d4cb7070", 1f);

            if (MechFXManager.Instance == null)
                MechFXManager.Instance = GameObject.FindObjectOfType<MechFXManager>();
            if (MechFXManager.Instance != null)
                MechFXManager.Instance.SetupMechFX(newPart, geom.gameObject, selectedPart.fx);
        }


        if (highlightProfiles != null && highlightProfiles.Count > 0)
        {
            var profile = highlightProfiles.Find(x => (int)(x.rarity) == selectedPart.rarity);
            if (profile != null)
            {
                var highlight = newPart.AddComponent<HighlightPlus.HighlightEffect>();
                highlight.effectGroup = HighlightPlus.TargetOptions.CustomGeoms;
                highlight.profile = profile.profile;
                highlight.profile.Load(highlight);
                highlight.seeThrough = HighlightPlus.SeeThroughMode.Never;
                highlight.highlighted = false;
                highlight.Refresh();
            }
        }

        /*
        var p = lib.partClassDefinitions.Find(c => c.partClass == candidateParts[rand].partClass);
        if (p != null)
        {
            List<Renderer> renderers = newPart.GetComponentsInChildren<Renderer>().ToList();
            Material mat = p.materials[Random.Range(0, p.materials.Count)];
            foreach(var r in renderers)
            {
                r.material = mat;
            }
            //p.materialPrefix
        }
        */
        MechPartPlacement newPartType = selectedPart.placement;
        if (newPartType == MechPartPlacement.ARM) return;

        assembler.AddPart(parentType, newPartType, selectedPart, newPart);

        var connectors = FindDeepChildren(newPart.transform, "mount_");
        
        if (newPartType == MechPartPlacement.COCKPIT && !connectors.Exists(c => c.name.ToLower() == "mount_weapon_top"))
        {
            var rends = newPart.GetComponentsInChildren<Renderer>();
            float maxY = 0;
            Vector3 center = newPart.transform.position;
            foreach (var r in rends)
            {

                Vector3 c = r.bounds.center;
                Vector3 top = r.bounds.max;
                if (top.y > maxY)
                {
                    maxY = top.y;
                    center = c;
                }
            }

            center.y = maxY;
            var mountTopGO = new GameObject("Mount_Weapon_Top");
            mountTopGO.transform.parent = newPart.transform;
            mountTopGO.transform.position = center;
            mountTopGO.transform.localRotation = Quaternion.Euler(0, 0, -90);
            connectors.Add(mountTopGO);
            // A sphere that fully encloses the bounding box.
        }

        if (newPartType == MechPartPlacement.GADGET)
            newPartType = MechPartPlacement.WEAPON;
        if (newPartType == MechPartPlacement.GADGET)
        {
            if (!newPart.name.ToLower().Contains("gadget_saw") && !newPart.name.ToLower().Contains("gadget_weapon_rockets"))
            {
                var hands = FindDeepChildren(parent.transform.parent, "mech_walker_lefthand");
                if (hands != null && hands.Count > 0)
                {
                    newPart.transform.localPosition = new Vector3(0, 0.2f, 1f);
                }
                else
                {
                    hands = FindDeepChildren(parent.transform.parent, "mech_walker_righthand");
                    if (hands != null && hands.Count > 0)
                    {
                        newPart.transform.localPosition = new Vector3(0, 0.2f, 1f);
                    }
                }
            }
        }

        foreach (GameObject c in connectors)
        {
            //if (newPartType == MechPartPlacement.COCKPIT && c.name.ToLower() == "mount_weapon_l")
            //    c.name = "mount_weapon";
            //if (newPartType == MechPartPlacement.COCKPIT && c.name.ToLower() == "mount_weapon_r")
            //    c.name = "mount_weapon";


            //if (newPartType == MechPartPlacement.ARM && c.name.ToLower() == "mount_weapon")
            //{
            //    c.name = "mount_weapon";
            //}
            //if (newPartType == MechPartPlacement.ARM && c.name.ToLower() == "mount_weapon_r")
            //    c.name = "mount_weapon";

            var newSocket = MechPartComponent.GetSocket(c.name);
            if (newPartType == MechPartPlacement.BACKPACK && newSocket == SocketType.WEAPON_SHIELD_TOP)
                continue;
            if (newPartType == MechPartPlacement.COCKPIT && newSocket == SocketType.WEAPON_SHIELD)
                continue;
            if (newPartType == MechPartPlacement.WEAPON && newSocket == SocketType.WEAPON_SHIELD)
                continue;
            if (newPartType == MechPartPlacement.LEG && newSocket == SocketType.COCKPIT)
                continue;

            if (newSocket == SocketType.LEFT_ARM || newSocket == SocketType.RIGHT_ARM)
                continue;

            var newSlot = MechPartComponent.GetSlot(c);
            if (newSlot == MechSlot.UNDEFINED) continue;

            bool useArms = false;
            if (!useArms || (fromAssembly && w3.partIDs != null && w3.partIDs[(int)newSlot] <= 0))
            {
                if (newSlot == MechSlot.LEFT_ARM)
                    newSlot = MechSlot.LEFT_WEAPON_SHIELD;
                else if (newSlot == MechSlot.RIGHT_ARM)
                    newSlot = MechSlot.RIGHT_WEAPON_SHIELD;
            }

            List<MechPartPlacement> newPlacements = MechPartComponent.GetPlacementFromString(c.name);
            RandomBuildPart(lib, newSlot, selectedPart.placement, newPlacements, c, mechDef, fromAssembly);
        }
    }

    public static List<SocketType> GetSockets(MechPartComponent part, GameObject partGO)
    {
        MechPartPlacement newPartType = part.placement;
        if (part == null || partGO == null)
        {
            return new List<SocketType>();
        }

        var connectors = FindDeepChildren(partGO.transform, "mount_");
        List<SocketType> sockets = new List<SocketType>();
        foreach (GameObject c in connectors)
        {
            if (newPartType == MechPartPlacement.COCKPIT && c.name.ToLower() == "mount_weapon_l")
                c.name = "mount_weapon";
            if (newPartType == MechPartPlacement.COCKPIT && c.name.ToLower() == "mount_weapon_r")
                c.name = "mount_weapon";
            if (newPartType == MechPartPlacement.BACKPACK && MechPartComponent.GetSocket(c.name) == SocketType.WEAPON_SHIELD_TOP)
                continue;
            if (newPartType == MechPartPlacement.COCKPIT && MechPartComponent.GetSocket(c.name) == SocketType.WEAPON_SHIELD)
                continue;
            if (newPartType == MechPartPlacement.WEAPON && MechPartComponent.GetSocket(c.name) == SocketType.WEAPON_SHIELD)
                continue;
            if (newPartType == MechPartPlacement.LEG && MechPartComponent.GetSocket(c.name) == SocketType.COCKPIT)
                continue;
            
            if (newPartType == MechPartPlacement.LEG && MechPartComponent.GetSocket(c.name) == SocketType.RIGHT_ARM)
                continue;
            if (newPartType == MechPartPlacement.LEG && MechPartComponent.GetSocket(c.name) == SocketType.LEFT_ARM)
                continue;

            sockets.Add(MechPartComponent.GetSocket(c.name));
        }

        return sockets;
    }

    public void OnChangeSelectedHueSecondary(float val)
    {
        selectedMech?.OnChangeSelectedHueSecondary(val);
    }

    public void OnTogglePattern(bool toggle)
    {
        selectedMech?.OnTogglePattern(toggle);
    }

    public void OnCyclePattern()
    {
        selectedMech?.OnCyclePattern();
    }

    public void OnCastPreview()
    {
        selectedMech?.OnCastPreview();
    }

    public void OnCyclePatternMode()
    {
        selectedMech?.OnCyclePatternMode();
    }

    public void OnChangeSelectedHue(float val)
    {
        selectedMech?.OnChangeSelectedHue(val);
    }

    public void OnToggleSaturation(float val)
    {
        selectedMech?.OnToggleSaturation(val);
    }

    public void OnChangeSelectedTint(float val)
    {
        selectedMech?.OnChangeSelectedTint(val);
    }

    public static List<GameObject> FindDeepChildren(Transform aParent, string aName)
    {
        List<GameObject> results = new List<GameObject>();
        Queue<Transform> queue = new Queue<Transform>();
        queue.Enqueue(aParent);
        while (queue.Count > 0)
        {
            var c = queue.Dequeue();
            if (c.name.ToLower().StartsWith(aName))
                results.Add(c.gameObject);
            foreach (Transform t in c)
                queue.Enqueue(t);
        }
        return results;
    }
}
