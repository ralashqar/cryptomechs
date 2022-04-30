using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(MechPartCameraSequence))]
public class MechPartCameraSequenceEditor : Editor
{
    public override void OnInspectorGUI()
    {
        MechPartCameraSequence seq = target as MechPartCameraSequence;
        if (seq.keyFrames == null)
            seq.keyFrames = new List<CameraKeyFrame>();

        //base.DrawDefaultInspector();

        seq.isEnabled = EditorGUILayout.Toggle("Enabled", seq.isEnabled);
        seq.autoSetLinkedSequence = EditorGUILayout.Toggle("Auto Link To Mouse", seq.autoSetLinkedSequence);
        seq.allowDragPivot = EditorGUILayout.Toggle("Allow Drag Pivot", seq.allowDragPivot);
        seq.dragPivot = EditorGUILayout.ObjectField("root", seq.dragPivot, typeof(Transform), true) as Transform;
        //var pivotProperty = serializedObject.FindProperty("dragPivot");
        //EditorGUILayout.PropertyField(pivotProperty);//, new GUIContent("Drag Pivot"));

        seq.blendTime = EditorGUILayout.FloatField("Blend Time", seq.blendTime);

        DrawAddNew();
        GUILayout.Space(15f);

        for (int i = 0; i < seq.keyFrames.Count; ++i)
        {
            var k = seq.keyFrames[i];
            DrawCamEntry(k);
            if (selectedCamKey == k)
            {
                var m_keyframe = serializedObject.FindProperty("keyFrames").GetArrayElementAtIndex(i);
                EditorGUILayout.PropertyField(m_keyframe, new GUIContent("Modify Cam Key"));
            }
            GUILayout.Space(10f);
        }

        GUILayout.Space(10f);
        if (selectedCamKey != null)
        {
            if (GUILayout.Button("Preview Selected"))
            {
                var (pos, rot) = selectedCamKey.EvaluateCamera();
                Vector3 f = rot * Vector3.forward;
                SceneView.lastActiveSceneView.pivot = pos + f * SceneView.lastActiveSceneView.cameraDistance;
                SceneView.lastActiveSceneView.rotation = rot;
                SceneView.lastActiveSceneView.Repaint();
                prevTime = time;
            }

            if (GUILayout.Button("Update Selected"))
            {
                selectedCamKey.ReEncode(SceneView.lastActiveSceneView.camera.transform.position, SceneView.lastActiveSceneView.camera.transform.rotation);
            }
        }

        GUILayout.Space(10f);

        float totaleTime = seq.GetTotalTime();
        time = EditorGUILayout.Slider(time, 0, totaleTime);
        
        if (time != prevTime)
        {
            var (pos, rot) = seq.EvaluateCamera(time);
            Vector3 f = rot * Vector3.forward;
            SceneView.lastActiveSceneView.pivot = pos + f * SceneView.lastActiveSceneView.cameraDistance;
            SceneView.lastActiveSceneView.rotation = rot;
            SceneView.lastActiveSceneView.Repaint();
            prevTime = time;
        }
    }

    // Inspector GUI
    float time = 0;
    float prevTime = 0;
    float duration = 1f;
    GameObject pivot;

    //Scene GUI
    int selectedCamIndex = 0;
    CameraKeyFrame selectedCamKey;

    public void DrawAddNew()
    {
        MechPartCameraSequence lib = target as MechPartCameraSequence;
        GUILayout.BeginHorizontal();
        //newPartID = EditorGUILayout.TextField("New Part ID", newPartID);
        if (GUILayout.Button("ADD NEW"))
        {
            var cam = SceneView.lastActiveSceneView.camera;
            var selectedPivot = Selection.activeTransform;
            duration = EditorGUILayout.FloatField("Duration", duration);
            CameraKeyFrame newCam = new CameraKeyFrame(cam.transform.position, cam.transform.rotation, selectedPivot, duration);
            lib.keyFrames.Add(newCam);
        }
        GUILayout.EndHorizontal();
        pivot = EditorGUILayout.ObjectField("pivot", pivot, typeof(Object), true) as GameObject;

    }

    public void DrawCamEntry(CameraKeyFrame k)
    {
        float lw = EditorGUIUtility.labelWidth;
        float fw = EditorGUIUtility.fieldWidth;
        
        EditorGUIUtility.labelWidth = 50f;
        //EditorGUIUtility.fieldWidth = 4f;

        GUILayout.Label("Cam", EditorStyles.boldLabel);
        MechPartCameraSequence lib = target as MechPartCameraSequence;

        GUILayout.BeginHorizontal();
        k.keyID = EditorGUILayout.TextField("ID", k.keyID);
        GUILayout.EndHorizontal();
        GUILayout.BeginHorizontal();
        k.time = EditorGUILayout.FloatField("Duration", k.time);
        k.target = (CameraTargetType)EditorGUILayout.EnumPopup("TargetType", k.target);
        if (GUILayout.Button("Select"))
        {
            selectedCamKey = k;
        }
        GUILayout.EndHorizontal();
        GUILayout.BeginHorizontal();
        k.followTr = EditorGUILayout.ObjectField("Follow", k.followTr, typeof(Object), true) as Transform;
        k.lookatTr = EditorGUILayout.ObjectField("Lookat", k.followTr, typeof(Object), true) as Transform;
        if (GUILayout.Button("Delete"))
        {
            lib.keyFrames.Remove(k);
        }
        GUILayout.EndHorizontal();

        EditorGUIUtility.labelWidth = lw;
        //EditorGUIUtility.fieldWidth = fw;
    }

    void OnSceneGUI()
    {
        MechPartCameraSequence lib = target as MechPartCameraSequence;

        Color oCol = Handles.color;
        Vector3 prevPos = Vector3.zero;

        for (int i = 0; i < lib.keyFrames.Count; ++i) 
        {
            var k = lib.keyFrames[i];
            var (pos, rot) = k.EvaluateCamera();

            Handles.color = k == selectedCamKey ? Color.green : Color.yellow;
            if (Handles.Button(pos, Quaternion.identity, 0.5f, 0.5f, Handles.SphereHandleCap))
            {
                selectedCamKey = k;
            }

            if (selectedCamKey == k)
            {
                Vector3 nPos = Handles.PositionHandle(pos, Quaternion.identity);
                if (nPos != pos)
                    k.ReEncodePos(nPos);
            }

            if (selectedCamKey == k &&  k.followTr != null)
                Handles.DrawLine(pos, k.GetLookatTarget());

            if (i > 0)
                Handles.DrawLine(pos, prevPos);

            prevPos = pos;
        }

        Handles.color = oCol;
    }

}
