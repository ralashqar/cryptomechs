using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using static MechAttributes;

public enum MechPartPlacement
{
    LEG,
    ARM,
    RIGHT_ARM,
    SHOULDERS,
    COCKPIT,
    WEAPON_MAIN,
    WEAPON,
    WEAPON_TOP,
    BACKPACK,
    GADGET,
    SHOULDERS_HALF_L,
    SHOULDERS_HALF_R,
    ANTENNA,
    SHIELD,
    CUSTOM
}

public enum EVMechPartType
{
    UNDEFINED = 0,
    LEGS = 1,
    SHOULDERS = 2,
    COCKPIT = 3,
    ARM = 4,
    WEAPON = 5,
    GADGET = 6,
    SHIELD = 7,
    BACKPACK = 8
}

public enum SocketType
{
    COCKPIT,
    SHOULDERS,
    RIGHT_ARM,
    LEFT_ARM,
    BACKPACK,
    WEAPON_SHIELD,
    WEAPON_SHIELD_TOP,
    GADGET,
    ENCHANTMENT_SLOT,
    CUSTOM
}

[System.Serializable]
public class MechPartComponent
{
    public static MechPartPlacement GetPartType(EVMechPartType t)
    {
        switch (t)
        {
            case EVMechPartType.LEGS:
                return MechPartPlacement.LEG;
            case EVMechPartType.SHOULDERS:
                return MechPartPlacement.SHOULDERS;
            case EVMechPartType.COCKPIT:
                return MechPartPlacement.COCKPIT;
            case EVMechPartType.WEAPON:
                return MechPartPlacement.WEAPON;
            case EVMechPartType.ARM:
                return MechPartPlacement.ARM;
            case EVMechPartType.SHIELD:
                return MechPartPlacement.SHIELD;
            case EVMechPartType.GADGET:
                //return MechPartPlacement.GADGET;
                return MechPartPlacement.WEAPON;
            case EVMechPartType.BACKPACK:
                return MechPartPlacement.BACKPACK;
            default:
                return MechPartPlacement.CUSTOM;
        }
    }

    public EVMechPartType GetEVMPartType()
    {
        switch (this.placement)
        {
            case MechPartPlacement.LEG:
                return EVMechPartType.LEGS;
            case MechPartPlacement.SHOULDERS:
                return EVMechPartType.SHOULDERS;
            case MechPartPlacement.COCKPIT:
                return EVMechPartType.COCKPIT;
            case MechPartPlacement.WEAPON:
                return EVMechPartType.WEAPON;
            case MechPartPlacement.ARM:
                return EVMechPartType.ARM;
            case MechPartPlacement.SHIELD:
                return EVMechPartType.SHIELD;
            case MechPartPlacement.GADGET:
                return EVMechPartType.GADGET;
            case MechPartPlacement.BACKPACK:
                return EVMechPartType.BACKPACK;
            default:
                return EVMechPartType.UNDEFINED;
        }
    }

    public static MechPartPlacement GetRandomPartTypeFromSlot(MechSlot slot)
    {
        switch (slot)
        {
            case MechSlot.LEGS:
                return MechPartPlacement.LEG;
            case MechSlot.SHOULDERS:
                return MechPartPlacement.SHOULDERS;
            case MechSlot.COCKPIT:
                return MechPartPlacement.COCKPIT;
            case MechSlot.LEFT_ARM:
                return MechPartPlacement.ARM;
            case MechSlot.RIGHT_ARM:
                return MechPartPlacement.ARM;
            case MechSlot.RIGHT_WEAPON_SHIELD:
                return MechPartPlacement.WEAPON;
                //return MechPartPlacement.GADGET;
            case MechSlot.LEFT_WEAPON_SHIELD:
                return MechPartPlacement.SHIELD;
            case MechSlot.HEAD_WEAPON_SHIELD:
                return MechPartPlacement.WEAPON;
            case MechSlot.BACKPACK:
                return MechPartPlacement.BACKPACK;
        }
        return MechPartPlacement.WEAPON;
    }

    public MechPartComponent(string partID, GameObject prefab, MechPartPlacement placement)
    {
        this.partID = partID;
        this.prefab = prefab;
        this.placement = placement;
    }

    public int partIndex = 0;
    public string partDNA;
    public string partClass;
    public List<AttributeDefinition> attributes;
    public MechPartPlacement placement;
    public List<SocketType> connectsTo;
    public List<SocketType> connectors;
    public string partID;
    public GameObject prefab;
    public int quantity;
    public int rarity;
    public float hueShift = 0;
    public float hueShiftSecondary = 0;

    public bool isPatternEnabled = false;
    public int selectedPattern = 0;
    public int selectedPatternMode = 0;

    public string fx;

    public string attachmentNodeName = "";

    public bool HasMultiAttachments()
    {
        switch (placement)
        {
            //case MechPartPlacement.WEAPON_BASE:
            //    return true;
            default:
                return false;
        }
    }

    public void CyclePatternMode()
    {
        this.selectedPatternMode++;
        if (this.selectedPatternMode >= 4)
        {
            this.selectedPatternMode = 0;
        }
    }

    public void CyclePattern()
    {
        this.selectedPattern++;
        if (this.selectedPattern >= MechAssembler.Instance.patterns.Count)
        {
            this.selectedPattern = 0;
        }
    }

    public void ProcessSocketsAndConnectors()
    {
        this.connectsTo = ConnectsTo(this.placement);
        this.connectors = MechAssembler.GetSockets(this, this.prefab);
    }

    public static List<SocketType> ConnectsTo(MechPartPlacement placement)
    {
        
        switch(placement)
        {
            case MechPartPlacement.SHIELD:
                return new List<SocketType>() { SocketType.WEAPON_SHIELD, SocketType.WEAPON_SHIELD_TOP};
            case MechPartPlacement.SHOULDERS:
                return new List<SocketType>() { SocketType.SHOULDERS };
            case MechPartPlacement.ARM:
                return new List<SocketType>() { SocketType.LEFT_ARM, SocketType.RIGHT_ARM };
            case MechPartPlacement.WEAPON:
                return new List<SocketType>() { SocketType.WEAPON_SHIELD, SocketType.WEAPON_SHIELD_TOP };
            case MechPartPlacement.BACKPACK:
                return new List<SocketType>() { SocketType.BACKPACK };
            case MechPartPlacement.GADGET:
                return new List<SocketType>() { SocketType.GADGET };
            case MechPartPlacement.COCKPIT:
                return new List<SocketType>() { SocketType.COCKPIT };
            default:
                return new List<SocketType>() { SocketType.COCKPIT };
        }
    }


    public static MechSlot GetSlot(GameObject connector)
    {
        string s = connector.name;
        switch (s.ToLower())
        {
            case "mount_top":
                return MechSlot.SHOULDERS;
            case "mount_cockpit":
                return MechSlot.COCKPIT;
            case "mount_arm_l":
                return MechSlot.LEFT_ARM;
            case "mount_arm_r":
                return MechSlot.RIGHT_ARM;
            //return MechPartPlacement.WEAPON_BASE;
            case "mount_weapon_l":
                return MechSlot.LEFT_ARM;
            case "mount_weapon_r":
                return MechSlot.RIGHT_ARM;
            case "mount_weapon":
            case "mount_gadget":
                //case "mount_hand_l":
                //case "mount_hand_r":
                var p = connector;
                while(p.transform.parent != null)
                {
                    p = p.transform.parent.gameObject;
                    if (p.name.ToLower() == "mount_weapon_l")
                        return MechSlot.LEFT_WEAPON_SHIELD;
                    if (p.name.ToLower() == "mount_weapon_r")
                        return MechSlot.RIGHT_WEAPON_SHIELD;
                }
                return MechSlot.UNDEFINED;
            case "mount_weapon_top":
                return MechSlot.HEAD_WEAPON_SHIELD;
            //case "mount_gadget":
            //    return SocketType.GADGET;
            case "mount_back":
            case "mount_backpack":
                return MechSlot.BACKPACK;
            default:
                return MechSlot.UNDEFINED;
        }
    }

    public static SocketType GetSocket(string s)
    {
        switch (s.ToLower())
        {
            case "mount_top":
                return SocketType.SHOULDERS;
            case "mount_cockpit":
                return SocketType.COCKPIT;
            case "mount_arm_l":
                return SocketType.LEFT_ARM;
            case "mount_arm_r":
                return SocketType.RIGHT_ARM;
            //return MechPartPlacement.WEAPON_BASE;
            case "mount_weapon_l":
            case "mount_weapon_r":
            case "mount_weapon":
            case "mount_gadget":
                //case "mount_hand_l":
                //case "mount_hand_r":
                return SocketType.WEAPON_SHIELD;
            case "mount_weapon_top":
                return SocketType.WEAPON_SHIELD_TOP;
            //case "mount_gadget":
            //    return SocketType.GADGET;
            case "mount_backpack":
                return SocketType.BACKPACK;
            default:
                return SocketType.CUSTOM;
        }
    }
    public static List<MechPartPlacement> GetPlacementFromString(string placement)
    {
        switch(placement.ToLower())
        {
            case "mount_top":
                return new List<MechPartPlacement>() { MechPartPlacement.SHOULDERS };
            case "mount_cockpit":
                return new List<MechPartPlacement>() { MechPartPlacement.COCKPIT };
            //case "mount_arm_l":
            //    return new List<MechPartPlacement>() { MechPartPlacement.LEFT_ARM };
            //case "mount_arm_r":
            //    return new List<MechPartPlacement>() { MechPartPlacement.RIGHT_ARM };
                //return MechPartPlacement.WEAPON_BASE;
            case "mount_weapon_top":
                return new List<MechPartPlacement>() { MechPartPlacement.WEAPON, MechPartPlacement.SHIELD };
            case "mount_weapon_l":
                return new List<MechPartPlacement>() { MechPartPlacement.ARM, MechPartPlacement.WEAPON, MechPartPlacement.SHIELD };
            case "mount_weapon_r":
                return new List<MechPartPlacement>() { MechPartPlacement.ARM, MechPartPlacement.WEAPON, MechPartPlacement.SHIELD };
            case "mount_weapon":
            //case "mount_hand_l":
            //case "mount_hand_r":
                //return new List<MechPartPlacement>() { MechPartPlacement.WEAPON, MechPartPlacement.SHIELD };
            case "mount_gadget":
                return new List<MechPartPlacement>() { MechPartPlacement.WEAPON, MechPartPlacement.SHIELD, MechPartPlacement.GADGET };
            case "mount_backpack":
                return new List<MechPartPlacement>() { MechPartPlacement.BACKPACK };
            case "mount_halfshoulder_r":
                return new List<MechPartPlacement>() { MechPartPlacement.SHOULDERS_HALF_R };
            case "mount_halfshoulder_l":
                return new List<MechPartPlacement>() { MechPartPlacement.SHOULDERS_HALF_L };
            case "mount_weapon_main":
                return new List<MechPartPlacement>() { MechPartPlacement.WEAPON_MAIN };
            case "mount_antenna":
                return new List<MechPartPlacement>() { MechPartPlacement.ANTENNA };
            default:
                return new List<MechPartPlacement>() { MechPartPlacement.CUSTOM };
        }
    }

    public string GetAttachmentNodeName()
    {
        switch (this.placement)
        {
            case MechPartPlacement.SHOULDERS:
                return "Mount_Top";
            case MechPartPlacement.COCKPIT:
                return "Mount_Cockpit";
            case MechPartPlacement.ARM:
                return "Mount_Arm_L";
            case MechPartPlacement.RIGHT_ARM:
                return "Mount_Arm_R";
            //case MechPartPlacement.WEAPON_BASE:
            //    return "Mount_Weapon_";
            case MechPartPlacement.WEAPON:
                return "Mount_Weapon_";
            case MechPartPlacement.BACKPACK:
                return "Mount_Backpack";
            case MechPartPlacement.GADGET:
                return "Mount_Gadget";
            default:
                return attachmentNodeName;
        }
    }
}

[System.Serializable]
public class PartClassDefinition
{
    public string materialPrefix = "";
    public string partClass = "";
    public List<Material> materials;
}

public class MechComponentLibrary : MonoBehaviour
{
    public static MechComponentLibrary Instance;
    
    public List<PartClassDefinition> partClassDefinitions;

    [HideInInspector]
    public List<MechPartComponent> MechPartDefinitions;
    // Start is called before the first frame update

    void Start()
    {
        Instance = this;    
    }

    Coroutine activeRoutine;
    public void ExportImageSequence(int numImages)
    {
        if (activeRoutine != null) StopCoroutine(activeRoutine);
        activeRoutine = StartCoroutine(ExportImageSequenceRoutine(numImages));
    }

    public void ExportGIFSequence(int numImages)
    {
        if (activeRoutine != null) StopCoroutine(activeRoutine);
        activeRoutine = StartCoroutine(ExportGIFSequenceRoutine(numImages));
    }

    public IEnumerator ExportImageSequenceRoutine(int numParts)
    {
        int imageID = 0;
        yield return null;

        for (int i = 0; i < Mathf.Min(numParts, MechPartDefinitions.Count); ++i)
        {
            var part = MechPartDefinitions[i];
            var assembler = this.gameObject.GetComponent<MechAssembler>();
            assembler?.PreviewPart(part);
            if (assembler.assembledParts[0].GetComponent<Animator>() != null)
                assembler.assembledParts[0].GetComponent<Animator>().enabled = false;
            yield return null;
            GameObject.FindObjectOfType<MechMintUIManager>().SetPartUI(part);

            imageID++;
            yield return new WaitForSeconds(0.3f);

            //GameObject camGO = GameObject.Find("NFTCamera");
            var camGO = GameObject.FindObjectOfType<ExportImageCamera>();
            Camera Cam = camGO.GetComponent<Camera>();

            RenderTexture currentRT = RenderTexture.active;
            RenderTexture.active = Cam.targetTexture;

            Cam.Render();

            Texture2D Image = new Texture2D(Cam.targetTexture.width, Cam.targetTexture.height);
            Image.ReadPixels(new Rect(0, 0, Cam.targetTexture.width, Cam.targetTexture.height), 0, 0);
            Image.Apply();
            RenderTexture.active = currentRT;

            var Bytes = Image.EncodeToPNG();
            DestroyImmediate(Image);
            string testFile = imageID + ".png";
            string pathToFile = Path.Combine(Application.dataPath, "Renders", testFile);
            File.WriteAllBytes(pathToFile, Bytes);

            yield return null;
        }
    }

    public IEnumerator ExportGIFSequenceRoutine(int numParts)
    {
        int imageID = 0;
        yield return null;

        for (int i = 0; i < Mathf.Min(numParts, MechPartDefinitions.Count); ++i)
        {
            var part = MechPartDefinitions[i];
            var assembler = this.gameObject.GetComponent<MechAssembler>();
            assembler?.PreviewPart(part);
            if (assembler.assembledParts[0].GetComponent<Animator>() != null)
                assembler.assembledParts[0].GetComponent<Animator>().enabled = false;
            yield return null;
            GameObject.FindObjectOfType<MechMintUIManager>().SetPartUI(part);

            imageID++;
            yield return null;

            string testFile = imageID.ToString();// + ".gif";
            string pathToFile = Path.Combine(Application.dataPath, "Renders");
            yield return RecordGIF(testFile, pathToFile, 2);
            yield return null;
        }
    }

    public IEnumerator RecordGIF(string filename, string filepath, float time = 2f)
    {
        var e = GameObject.FindObjectOfType<ExportImageCamera>();
        var rec = e.gameObject.GetComponent<Record>();
        
        //Record rec = GameObject.FindObjectOfType<Record>();
        rec.Setup(true, 1024, 1024, 24, 3, 0, 15);
        //Coroutine r = rec.StartCoroutine(rec.RecordAndSave(filename, filepath, time));

        yield return rec.StartCoroutine(rec.RecordAndSave(filename, filepath, time));
        yield return null;
        yield return null;
        string file = filename + ".gif";
        //S3Manager s3Manager = GameObject.FindObjectsOfType<S3Manager>()[0];
        //s3Manager.Upload(file, filepath + file);
        //string pathToFile = Path.Combine(Application.dataPath, "Renders", testFile);
        //File.WriteAllBytes(pathToFile, Bytes);
    }
}
