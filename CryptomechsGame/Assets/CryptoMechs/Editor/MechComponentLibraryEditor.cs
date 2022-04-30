using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.IO;

[CustomEditor (typeof(MechComponentLibrary))]
public class MechComponentLibraryEditor : Editor
{
    public override void OnInspectorGUI()
    {
        if (GUILayout.Button("Process Part Classes"))
        {
            ProcessPartClasses();
        }
        base.OnInspectorGUI();

        DrawComponentParts();
    }

    MechPartPlacement selectedPlacement = MechPartPlacement.LEG;

    private MechComponentLibrary lib;
    public string newPartID;
    public GameObject newPartPrefab;
    public string partSearch = "";
    public string partClass = "";
    public MechPartComponent selectedPart = null;
    public string json = "";

    public void DrawComponentParts()
    {
        lib = target as MechComponentLibrary;
        
        GUILayout.Space(10f);

        int l = System.Enum.GetValues(typeof(MechPartPlacement)).Length;
        
        int cellsPerRow = 3;
        int counter = 0;
        Color oCol = GUI.backgroundColor;

        GUILayout.BeginHorizontal();
        for (int i = 0; i < l; i++)
        {
            MechPartPlacement placement = ((MechPartPlacement)i);

            GUI.backgroundColor = selectedPlacement == placement ? Color.green : Color.gray;
            if (GUILayout.Button(((MechPartPlacement)i).ToString().ToUpper()))
            {
                selectedPlacement = placement;
                selectedPart = null;
                partSearch = placement.ToString() + "_";
            }
            counter += 1;
            if (counter >= cellsPerRow)
            {
                GUILayout.EndHorizontal();
                GUILayout.BeginHorizontal();
                counter = 0;
            }
        }
        GUILayout.EndHorizontal();

        GUI.backgroundColor = oCol;

        //if (selectedPart == null)
        {
            DrawMechPartEntries();
        }
        //else
        //{
        //    DrawSelectedPart();
        //}
        GUILayout.Space(10f);
        
    }

    public PartAttribute newAttribute;
    public float newAttributeVal = 0;
    public string newAttributeValStr = "";

    public void DrawSelectedPart()
    {
        GUILayout.Space(15f);
        if (GUILayout.Button("Back to Entries"))
        {
            selectedPart = null;
            return;
        }
        
        float lWidth = EditorGUIUtility.labelWidth;
;
        GUILayout.Space(10f);
        if (selectedPart.attributes == null) selectedPart.attributes = new List<AttributeDefinition>();
        selectedPart.partID = EditorGUILayout.TextField("PartID", selectedPart.partID);
        selectedPart.placement = (MechPartPlacement)EditorGUILayout.EnumPopup("Placement", selectedPart.placement);
        selectedPart.prefab = EditorGUILayout.ObjectField(selectedPart.prefab, typeof(Object), true) as GameObject;

        EditorGUIUtility.labelWidth = 40f;
        GUILayout.Space(10f);
        GUILayout.Label("PART ATTRIBUTES", EditorStyles.boldLabel);
        GUILayout.BeginHorizontal();
        newAttribute = (PartAttribute)EditorGUILayout.EnumPopup("Type", newAttribute);
        newAttributeVal = EditorGUILayout.FloatField("Value", newAttributeVal);
        if (AttributeDefinition.HasStringVal(newAttribute))
        {
            GUILayout.EndHorizontal();
            GUILayout.BeginHorizontal();
            newAttributeValStr = EditorGUILayout.TextField("StrValue", newAttributeValStr);
        }
        
        if (GUILayout.Button("ADD NEW"))
        {
            selectedPart.attributes.Add(new AttributeDefinition(newAttribute, newAttributeVal, AttributeDefinition.HasStringVal(newAttribute) ? newAttributeValStr : ""));
        }
        GUILayout.EndHorizontal();

        GUILayout.Space(15f);
        GUILayout.Label("MODIFY ENTRIES", EditorStyles.boldLabel);
        foreach(AttributeDefinition attr in selectedPart.attributes)
        {
            GUILayout.BeginHorizontal();
            attr.attributeType = (PartAttribute)EditorGUILayout.EnumPopup("Type", attr.attributeType);
            attr.attributeVal= EditorGUILayout.FloatField("Value", attr.attributeVal);
            if (AttributeDefinition.HasStringVal(attr.attributeType))
            {
                GUILayout.EndHorizontal();
                GUILayout.BeginHorizontal();
                attr.attributeValStr = EditorGUILayout.TextField("StrValue", attr.attributeValStr);
            }
            if (GUILayout.Button("DELETE"))
            {
                selectedPart.attributes.Remove(attr);
                GUILayout.EndHorizontal();
                EditorGUIUtility.labelWidth = lWidth;
                break;
            }
            GUILayout.EndHorizontal();

        }

        GUILayout.Space(10f);
        if (GUILayout.Button("Generate Part"))
        {
            var assembler = lib.gameObject.GetComponent<MechAssembler>();
            assembler?.GeneratePart(selectedPart);
            var camFocus = GameObject.FindObjectOfType<CameraFocuser>();
            camFocus.focusObject = assembler.assembledParts[0];
            camFocus?.FocusCam(assembler.assembledParts[0]);
        }

        GUILayout.BeginHorizontal();
        if (GUILayout.Button("InitializeS3"))
        {
            S3Manager s3Manager = GameObject.FindObjectsOfType<S3Manager>()[0];
            s3Manager.Initialize();
        }
        if (GUILayout.Button("ExportJson"))
        {
            json = JsonUtility.ToJson(selectedPart);
            S3Manager s3Manager = GameObject.FindObjectsOfType<S3Manager>()[0];
            string testFile = selectedPart.partID + ".json";
            string pathToFile = Path.Combine(Application.dataPath, "AWS", "Services", "S3", "Example", "TestFiles", testFile);
            File.WriteAllText(pathToFile, json);
            //s3Manager.Initialize();
            s3Manager.Upload(testFile, pathToFile);

            // Take a snapshot image
        }
        if (GUILayout.Button("Export Image"))
        {
            var camGO = GameObject.FindObjectOfType<ExportImageCamera>();
            Camera Cam = null;
            if (camGO != null)
            {
                Cam = camGO.gameObject.GetComponent<Camera>();
            }

            //Camera Cam = GameObject.FindObjectOfType<Camera>();
            S3Manager s3Manager = GameObject.FindObjectsOfType<S3Manager>()[0];

            RenderTexture currentRT = RenderTexture.active;
            RenderTexture.active = Cam.targetTexture;

            Cam.Render();

            Texture2D Image = new Texture2D(Cam.targetTexture.width, Cam.targetTexture.height);
            Image.ReadPixels(new Rect(0, 0, Cam.targetTexture.width, Cam.targetTexture.height), 0, 0);
            Image.Apply();
            RenderTexture.active = currentRT;

            var Bytes = Image.EncodeToPNG();
            Destroy(Image);
            string testFile = selectedPart.partID + ".png";
            string pathToFile = Path.Combine(Application.dataPath, "AWS", "Services", "S3", "Example", "TestFiles", testFile);
            File.WriteAllBytes(pathToFile, Bytes);
            s3Manager.Upload(testFile, pathToFile);
        }
        if (GUILayout.Button("Save GIF"))
        {
            string testFile = selectedPart.partID;
            string pathToFile = Path.Combine(Application.dataPath, "AWS", "Services", "S3", "Example", "TestFiles");
            S3Manager s3Manager = GameObject.FindObjectsOfType<S3Manager>()[0];
            s3Manager.StartCoroutine(RecordGIF(testFile, pathToFile + "/", 2f));
        }

        GUILayout.EndHorizontal();
        if (GUILayout.Button("FocusCam"))
        {
            //var camFocus = GameObject.FindObjectOfType<CameraFocuser>();
            //camFocus?.FocusCam();
        }

        GUILayout.Space(10f);
        GUILayout.Label("Last JSON: " + json.ToString());

        EditorGUIUtility.labelWidth = lWidth;

    }

    public IEnumerator RecordGIF(string filename, string filepath, float time)
    {
        //var camGO = GameObject.FindObjectOfType<Moments.Recorder>();
        //Camera Cam = camGO.gameObject.GetComponent<Camera>();
        var c = GameObject.FindObjectOfType<ExportImageCamera>();
        Record rec = c.GetComponent<Record>();
        rec.Setup(true, 1024, 1024, 24, 3, 0, 15);
        //Coroutine r = rec.StartCoroutine(rec.RecordAndSave(filename, filepath, time));
        
        yield return rec.StartCoroutine(rec.RecordAndSave(filename, filepath, time));
        yield return null;
        yield return null;
        string file = filename + ".gif";
        S3Manager s3Manager = GameObject.FindObjectsOfType<S3Manager>()[0];
        s3Manager.Upload(file, filepath + file);
    }

    public void DrawMechPartEntries()
    {
        Color oCol = Handles.color;

        if (lib.MechPartDefinitions == null) lib.MechPartDefinitions = new List<MechPartComponent>();

        GUILayout.Space(10f);

        GUILayout.BeginHorizontal();
        newPartID = EditorGUILayout.TextField("New Part ID", newPartID);
        if (GUILayout.Button("ADD NEW"))
        {
            MechPartComponent newPart = new MechPartComponent(newPartID, newPartPrefab, selectedPlacement);
            lib.MechPartDefinitions.Add(newPart);
        }
        GUILayout.EndHorizontal();
        newPartPrefab = EditorGUILayout.ObjectField(newPartPrefab, typeof(Object), true) as GameObject;

        partSearch = EditorGUILayout.TextField("AutoLoadSearch", partSearch);
        GUILayout.BeginHorizontal();
        partClass = EditorGUILayout.TextField("PartClass", partClass);
        if (GUILayout.Button("Auto Load Multi"))
        {
            string[] guids = AssetDatabase.FindAssets("t:Prefab");

            foreach (var guid in guids)
            {
                var path = AssetDatabase.GUIDToAssetPath(guid);
                GameObject go = AssetDatabase.LoadAssetAtPath<GameObject>(path);
                if (go.name.ToLower().StartsWith(partSearch.ToLower()))
                {
                    if (!lib.MechPartDefinitions.Exists(p => p.prefab == go && p.placement == selectedPlacement))
                    {
                        MechPartComponent newPart = new MechPartComponent(go.name.ToLower() + "_" + lib.MechPartDefinitions.Count.ToString(), go, selectedPlacement);
                        newPart.partClass = partClass;
                        lib.MechPartDefinitions.Add(newPart);
                    }
                }
            }
        }
        GUILayout.EndHorizontal();
        float total = 0;
        float oW = EditorGUIUtility.labelWidth;
        EditorGUIUtility.labelWidth = 40f;
        int t1 = 0, t2 = 0, t3 = 0, t4 = 0, t5 = 0;

        foreach (MechPartComponent part in lib.MechPartDefinitions)
        {
            if (part.placement == selectedPlacement)
            {
                GUILayout.Space(15f);
                //part.partID = EditorGUILayout.TextField("Part ID", part.partID);
                var sockets = MechAssembler.GetSockets(part, part.prefab);
                string s = "";
                foreach (SocketType sk in sockets) if (sk != SocketType.CUSTOM) s += sk.ToString() + " | ";
                part.prefab = EditorGUILayout.ObjectField(part.prefab, typeof(Object), true) as GameObject;
                GUILayout.BeginHorizontal();
                part.partIndex = EditorGUILayout.IntField("Part ID", part.partIndex);
                part.hueShift = EditorGUILayout.FloatField("Hue", part.hueShift);
                part.fx = EditorGUILayout.TextField("FX id", part.fx);
                GUILayout.EndHorizontal();
                GUILayout.Label(s);
                GUILayout.BeginHorizontal();
                part.rarity = EditorGUILayout.IntField("Rarity", part.rarity);
                part.quantity = EditorGUILayout.IntField("Amount", part.quantity);
                total += part.quantity;
                GUILayout.EndHorizontal();
                GUILayout.BeginHorizontal();
                if (GUILayout.Button("PREV"))
                {
                    var assembler = lib.gameObject.GetComponent<MechAssembler>();
                    assembler?.PreviewPart(part);
                    assembler.PinPart(part);
                    if (assembler.assembledParts[0].GetComponent<Animator>() != null)
                        assembler.assembledParts[0].GetComponent<Animator>().enabled = false;
                    GameObject.FindObjectOfType<MechMintUIManager>().SetPartUI(part);
                    selectedPart = null;
                    break;
                }
                if (GUILayout.Button("DUPLICATE"))
                {
                    foreach (var p in lib.MechPartDefinitions)
                    {
                        if (p.partIndex > part.partIndex)
                            p.partIndex++;
                    }
                    var assembler = lib.gameObject.GetComponent<MechAssembler>();
                    MechPartComponent c = new MechPartComponent(part.partID, part.prefab, part.placement);
                    c.partIndex = part.partIndex + 1;
                    c.quantity = part.quantity;
                    c.rarity = part.rarity;
                    lib.MechPartDefinitions.Insert(lib.MechPartDefinitions.IndexOf(part) + 1, c);
                    break;
                }

                if (GUILayout.Button("EDIT"))
                {
                    var assembler = lib.gameObject.GetComponent<MechAssembler>();
                    assembler?.PreviewPart(part);
                    selectedPart = part;
                    break;
                }

                if (GUILayout.Button("DELETE"))
                {
                    lib.MechPartDefinitions.Remove(part);
                    break;
                }
                GUILayout.EndHorizontal();
                GUILayout.Space(5f);

                if (selectedPart == part)
                {
                    DrawSelectedPart();
                }

                switch (part.rarity)
                {
                    case 1:
                        t1 += 1;
                        break;
                    case 2:
                        t2 += 1;
                        break;
                    case 3:
                        t3 += 1;
                        break;
                    case 4:
                        t4 += 1;
                        break;
                    case 5:
                        t5 += 1;
                        break;
                }
            }
        }

        GUILayout.Label("TOTAL PART QUANTITIES: " + total.ToString());
        GUILayout.Space(10f);
        EditorGUIUtility.labelWidth = oW;

        r1Num = EditorGUILayout.IntField("Rarity 1 Proportion", r1Num);
        r2Num = EditorGUILayout.IntField("Rarity 2 Proportion", r2Num);
        r3Num = EditorGUILayout.IntField("Rarity 3 Proportion", r3Num);
        r4Num = EditorGUILayout.IntField("Rarity 4 Proportion", r4Num);
        r5Num = EditorGUILayout.IntField("Rarity 5 Proportion", r5Num);
        targetTotal = EditorGUILayout.IntField("Target Total", targetTotal);
        if (GUILayout.Button("Auto Distribute"))
        {
            int pTotal = r1Num + r2Num + r3Num + r4Num + r5Num;
            int r1 = (int)(((float)r1Num / (float)pTotal) * (float)targetTotal);
            int r2 = (int)(((float)r2Num / (float)pTotal) * (float)targetTotal);
            int r3 = (int)(((float)r3Num / (float)pTotal) * (float)targetTotal);
            int r4 = (int)(((float)r4Num / (float)pTotal) * (float)targetTotal);
            int r5 = (int)(((float)r5Num / (float)pTotal) * (float)targetTotal);
            r1 = (int)((float)r1 / (float)t1);
            r2 = (int)((float)r2 / (float)t2);
            r3 = (int)((float)r3 / (float)t3);
            r4 = (int)((float)r4 / (float)t4);
            r5 = (int)((float)r5 / (float)t5);

            foreach (MechPartComponent part in lib.MechPartDefinitions)
            {
                if (part.placement == selectedPlacement)
                {
                    switch (part.rarity)
                    {
                        case 1:
                            part.quantity = r1;
                            break;
                        case 2:
                            part.quantity = r2;
                            break;
                        case 3:
                            part.quantity = r3;
                            break;
                        case 4:
                            part.quantity = r4;
                            break;
                        case 5:
                            part.quantity = r5;
                            break;
                    }
                }
            }
        }


        GUILayout.Space(10f);

        if (GUILayout.Button("RefreshIDs"))
        {
            int id = 1;
            for (int i = 1; i <= 8; ++i)
            {
                EVMechPartType t = (EVMechPartType)i;
                MechPartPlacement ptype = MechPartComponent.GetPartType(t);
                foreach (MechPartComponent part in lib.MechPartDefinitions)
                {
                    if (part.placement == ptype)
                    {
                        part.partIndex = id++;
                    }
                }
            }
            lib.MechPartDefinitions.Sort((x, y) => x.partIndex.CompareTo(y.partIndex));
        }

        if (GUILayout.Button("GenerateCreationArgsO"))
        {
            int startIndex = 0;
            int count = lib.MechPartDefinitions.Count;
            string output = "";
            //count = 256;

            //startIndex = 30;
            //count = 100;

            for (int i = startIndex; i < startIndex + count; ++i)
            {
                MechPartComponent part = lib.MechPartDefinitions[i];
                if (part.GetEVMPartType() == EVMechPartType.UNDEFINED)
                {
                    lib.MechPartDefinitions.Remove(part);
                    i--;
                }
            }

            int id = 1;
            for (int i = 1; i <= 8; ++i)
            {
                EVMechPartType t = (EVMechPartType)i;
                MechPartPlacement ptype = MechPartComponent.GetPartType(t);
                foreach (MechPartComponent part in lib.MechPartDefinitions)
                {
                    if (part.placement == ptype)
                    {
                        part.partIndex = id++;
                    }
                }
            }
            lib.MechPartDefinitions.Sort((x, y) => x.partIndex.CompareTo(y.partIndex));
            /*
            // part ids
            string output = "[";
            for (int i = startIndex; i < startIndex + count; ++i)
            {
                int j = i + 1;
                if (i > startIndex) output += ",";
                output += j.ToString();
            }
            output += "]";

            // type ids
            output += ",[";
            for (int i = startIndex; i < startIndex + count; ++i)
            {
                var part = lib.MechPartDefinitions[i];
                int j = (int)part.GetEVMPartType();
                if (i > startIndex) output += ",";
                output += j.ToString();
            }
            output += "]";

            //Debug.Log(output);
            //return;

            // connectsTo
            output += ",[";
            for (int i = startIndex; i < startIndex + count; ++i)
            {
                if (i > startIndex) output += ",";
                output += "[";
                var part = lib.MechPartDefinitions[i];
                int j = (int)part.placement + 1;
                List<SocketType> sockets = MechPartComponent.ConnectsTo(part.placement);
                bool noFirstComma = true;
                foreach (SocketType type in sockets)
                {
                    if (!noFirstComma) output += ",";
                    output += ((int)type + 1).ToString();
                    noFirstComma = false;
                }
                output += "]";
            }
            output += "]";

            // connectors
            output += ",[";
            for (int i = startIndex; i < startIndex + count; ++i)
            {
                if (i > startIndex) output += ",";
                output += "[";
                var part = lib.MechPartDefinitions[i];
                int j = (int)part.placement + 1;
                List<SocketType> sockets = MechAssembler.GetSockets(part, part.prefab);
                bool noFirstComma = true;
                foreach (SocketType type in sockets)
                {
                    if (!noFirstComma) output += ",";
                    output += ((int)type + 1).ToString();
                    noFirstComma = false;
                }
                output += "]";
            }
            output += "]";
            */
            output += "[0";
            int pType = 1;
            for (int i = startIndex; i < startIndex + count; ++i)
            {
                var part = lib.MechPartDefinitions[i];
                int t = (int)part.GetEVMPartType();
                if (t != pType)
                {
                    pType = t;
                    if (i > startIndex) output += ",";
                    output += i.ToString();
                }
            }
            output += "," + count.ToString() + "]";

            // part quantities
            output += ",[";
            for (int i = startIndex; i < startIndex + count; ++i)
            {
                var part = lib.MechPartDefinitions[i];
                int j = part.quantity;
                if (j == 0) j = 20;
                if (i > startIndex) output += ",";
                output += j.ToString();
            }
            output += "]";

            // part quantities
            output += ",[";
            for (int i = startIndex; i < startIndex + count; ++i)
            {
                var part = lib.MechPartDefinitions[i];
                int j = part.rarity;
                //if (j == 0) j = 20;
                if (i > startIndex) output += ",";
                output += j.ToString();
            }
            output += "]";
            Debug.Log(output);
        }

        if (GUILayout.Button("GenerateCreationArgs"))
        {
            int startIndex = 0;
            int count = lib.MechPartDefinitions.Count;
            string output = "";
            //count = 256;
            
            //startIndex = 30;
            //count = 100;
            
            for (int i = startIndex; i < startIndex + count; ++i)
            {
                MechPartComponent part = lib.MechPartDefinitions[i];
                if (part.GetEVMPartType() == EVMechPartType.UNDEFINED)
                {
                    lib.MechPartDefinitions.Remove(part);
                    i--;
                }
            }

            int id = 1;
            for (int i = 1; i <= 8; ++i)
            {
                EVMechPartType t = (EVMechPartType)i;
                MechPartPlacement ptype = MechPartComponent.GetPartType(t);
                foreach (MechPartComponent part in lib.MechPartDefinitions)
                {
                    if (part.placement == ptype)
                    {
                        part.partIndex = id++;
                    }
                }
            }
            lib.MechPartDefinitions.Sort((x, y) => x.partIndex.CompareTo(y.partIndex));
            /*
            // part ids
            string output = "[";
            for (int i = startIndex; i < startIndex + count; ++i)
            {
                int j = i + 1;
                if (i > startIndex) output += ",";
                output += j.ToString();
            }
            output += "]";

            // type ids
            output += ",[";
            for (int i = startIndex; i < startIndex + count; ++i)
            {
                var part = lib.MechPartDefinitions[i];
                int j = (int)part.GetEVMPartType();
                if (i > startIndex) output += ",";
                output += j.ToString();
            }
            output += "]";

            //Debug.Log(output);
            //return;

            // connectsTo
            output += ",[";
            for (int i = startIndex; i < startIndex + count; ++i)
            {
                if (i > startIndex) output += ",";
                output += "[";
                var part = lib.MechPartDefinitions[i];
                int j = (int)part.placement + 1;
                List<SocketType> sockets = MechPartComponent.ConnectsTo(part.placement);
                bool noFirstComma = true;
                foreach (SocketType type in sockets)
                {
                    if (!noFirstComma) output += ",";
                    output += ((int)type + 1).ToString();
                    noFirstComma = false;
                }
                output += "]";
            }
            output += "]";

            // connectors
            output += ",[";
            for (int i = startIndex; i < startIndex + count; ++i)
            {
                if (i > startIndex) output += ",";
                output += "[";
                var part = lib.MechPartDefinitions[i];
                int j = (int)part.placement + 1;
                List<SocketType> sockets = MechAssembler.GetSockets(part, part.prefab);
                bool noFirstComma = true;
                foreach (SocketType type in sockets)
                {
                    if (!noFirstComma) output += ",";
                    output += ((int)type + 1).ToString();
                    noFirstComma = false;
                }
                output += "]";
            }
            output += "]";
            */

            output = "[";
            for (int i = 0; i < lib.MechPartDefinitions.Count; ++i)
            {
                MechPartComponent p = lib.MechPartDefinitions[i];
                output += "[" + p.partIndex.ToString()
                    + ", " + ((int)p.GetEVMPartType()).ToString()
                    + ", " + p.rarity.ToString()
                    + ", " + p.quantity.ToString() + "]";
                if (i < lib.MechPartDefinitions.Count - 1)
                    output += ", ";
            }
            output += "]";
            Debug.Log(output);
            return;

            output += "[0";
            int pType = 1;
            for (int i = startIndex; i < startIndex + count; ++i)
            {
                var part = lib.MechPartDefinitions[i];
                int t = (int)part.GetEVMPartType();
                if (t != pType)
                {
                    pType = t;
                    if (i > startIndex) output += ",";
                    output += i.ToString();
                }
            }
            output += "," + count.ToString() + "]";

            // part quantities
            output += ",[";
            for (int i = startIndex; i < startIndex + count; ++i)
            {
                var part = lib.MechPartDefinitions[i];
                int j = part.quantity;
                if (j == 0) j = 20;
                if (i > startIndex) output += ",";
                output += j.ToString();
            }
            output += "]";

            // part quantities
            output += ",[";
            for (int i = startIndex; i < startIndex + count; ++i)
            {
                var part = lib.MechPartDefinitions[i];
                int j = part.rarity;
                //if (j == 0) j = 20;
                if (i > startIndex) output += ",";
                output += j.ToString();
            }
            output += "]";
            Debug.Log(output);
        }


        GUILayout.Space(15f);
        if (GUILayout.Button("ExportPartImages"))
        {
            lib.ExportImageSequence(300);
        }
        //if (GUILayout.Button("ExportPartGIFs"))
        //{
        //    lib.ExportGIFSequence(10);
        //}
        GUILayout.Space(15f);

        GUILayout.BeginHorizontal();
        mechIDToLoad = EditorGUILayout.IntField("id", mechIDToLoad);
        if (GUILayout.Button("LoadFromContract"))
        {
            var w3 = GameObject.FindObjectOfType<Web3Manager>();
            w3?.GetBagParts(mechIDToLoad);
        }

        GUILayout.EndHorizontal();

        GUI.backgroundColor = oCol;


    }

    private int mechIDToLoad = 1;

    private int r1Num;
    private int r2Num;
    private int r3Num;
    private int r4Num;
    private int r5Num;
    private int targetTotal = 0;

    public void ProcessPartClasses()
    {
        var lib = (target as MechComponentLibrary);
        foreach (MechPartComponent c in lib.MechPartDefinitions)
        {
            GameObject go = c.prefab;
            var mr = go.GetComponentInChildren<Renderer>();
            if (mr != null)
            {
                foreach (var p in lib.partClassDefinitions)
                {
                    if (mr.sharedMaterial.name.Contains(p.materialPrefix))
                    {
                        c.partClass = p.partClass;
                    }
                }
            }
        }

        string[] guids = AssetDatabase.FindAssets("t:Material");
        List<Material> mats = new List<Material>();
        foreach (var guid in guids)
        {
            var path = AssetDatabase.GUIDToAssetPath(guid);
            Material m = AssetDatabase.LoadAssetAtPath<Material>(path);
            mats.Add(m);
        }

        foreach (var p in lib.partClassDefinitions)
        {
            p.materials = mats.FindAll(m => m.name.Contains(p.materialPrefix));
        }
    }
}
