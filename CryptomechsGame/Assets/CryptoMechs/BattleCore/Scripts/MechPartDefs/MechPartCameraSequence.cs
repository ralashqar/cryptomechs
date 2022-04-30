using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum CameraTargetType
{
    PLAYER,
    OPPONENT,
    CUSTOM,
    MECH_SLOT
}

[System.Serializable]
public class CameraNoiseProfile
{
    public List<NoiseLayer> positionXNoise;
    public List<NoiseLayer> positionYNoise;
    public List<NoiseLayer> positionZNoise;

    public List<NoiseLayer> rotationXNoise;
    public List<NoiseLayer> rotationYNoise;
    public List<NoiseLayer> rotationZNoise;
}

[System.Serializable]
public class NoiseLayer
{
    public float frequency;
    public float amplitude;
    public bool addPerlinNoise;
}

[System.Serializable]
public class CameraKeyFrame
{
    public string keyID = "";
    public CameraTargetType target;
    public Vector3 localPos;
    public Vector3 lookatOffset;
    public Transform followTr;
    public Transform lookatTr;
    public float time = 1f;

    public bool addNoise = false;
    public CameraNoiseProfile noiseProfile;

    public float EvaluateNoise(float time, List<NoiseLayer> layers)
    {
        float val = 0;
        float perlin = Mathf.PerlinNoise(time, 0);
        if (layers != null)
        {
            foreach (var n in layers)
            {
                float freq = n.frequency * (n.addPerlinNoise ? perlin : 1f);
                float amp = n.amplitude * (n.addPerlinNoise ? perlin : 1f);
                float noise = Mathf.Sin(freq * time) * amp;
                val += noise;
            }
        }
        return val;
    }

    public Vector3 GetNoisePositionDelta(float time)
    {
        float xDelta = EvaluateNoise(time, noiseProfile.positionXNoise);
        float yDelta = EvaluateNoise(time, noiseProfile.positionYNoise);
        float zDelta = EvaluateNoise(time, noiseProfile.positionZNoise);
        
        return new Vector3(xDelta, yDelta, zDelta);
    }

    public Quaternion GetNoiseEulerDelta(float time)
    {
        float xDelta = EvaluateNoise(time, noiseProfile.rotationXNoise);
        float yDelta = EvaluateNoise(time, noiseProfile.rotationYNoise);
        float zDelta = EvaluateNoise(time, noiseProfile.rotationZNoise);
        return Quaternion.Euler(xDelta, yDelta, zDelta);
    }

    public Quaternion GetFollowPivotQuat()
    {
        return followTr != null ? followTr.rotation : Quaternion.identity;
    }
    public Quaternion GetLookatPivotQuat()
    {
        return lookatTr != null ? lookatTr.rotation : Quaternion.identity;
    }

    public Vector3 GetFollowPivotPos()
    {
        return followTr != null ? followTr.position : Vector3.zero;
    }
    public Vector3 GetLookatPivotPos()
    {
        return lookatTr != null ? lookatTr.position : Vector3.zero;
    }

    public Vector3 GetLookatTarget()
    {
        return lookatTr.TransformPoint(lookatOffset);
    }


    public CameraKeyFrame()
    {

    }

    public CameraKeyFrame(Vector3 pos, Quaternion rot, Transform pivot, float time = 1f)
    {
        Encode(pos, rot, pivot, time);
    }

    public void Encode(Vector3 pos, Quaternion rot, Transform pivot, float time = 1f)
    {
        this.time = time;
        this.followTr = pivot;
        this.lookatTr = pivot;
        this.lookatOffset = Vector3.zero;
        this.localPos = pivot != null ? pivot.InverseTransformPoint(pos) : pos;

        float d = (pivot.position - pos).magnitude;
        this.lookatOffset = pivot.InverseTransformPoint(pos + rot * Vector3.forward * d);
    }

    public void ReEncodePos(Vector3 newWorldPos)
    {
        this.localPos = this.followTr.InverseTransformPoint(newWorldPos);
    }

    public void ReEncode(Vector3 newWorldPos, Quaternion newRot)
    {
        this.localPos = this.followTr.InverseTransformPoint(newWorldPos);
        float d = (lookatTr.position - newWorldPos).magnitude;
        this.lookatOffset = lookatTr.InverseTransformPoint(newWorldPos + newRot * Vector3.forward * d);
    }

    /*
    public (Vector3, Vector3) EvaluateCameraVec()
    {
        Vector3 pos = GetFollowPivotPos() + (GetFollowPivotQuat() * localPos);
        Vector3 lookat = GetLookatPivotPos() + (GetLookatPivotQuat() * lookatOffset);

        return (pos, lookat - pos);
    }
    */
    
    public (Vector3, Quaternion) EvaluateCamera(float timeOverride = -1)
    {
        Vector3 pos = GetFollowPivotPos() + (GetFollowPivotQuat() * localPos);
        Vector3 lookat = GetLookatPivotPos() + (GetLookatPivotQuat() * lookatOffset);
        Quaternion camRot = Quaternion.LookRotation(lookat - pos, Vector3.up);

        float t = timeOverride >= 0 ? timeOverride : Time.time;
        pos += camRot * GetNoisePositionDelta(t);
        camRot *= GetNoiseEulerDelta(t);
        return (pos, camRot);
    }

    public void SetupTargets()
    {

    }
}

public class MechPartCameraSequence : MonoBehaviour
{
    public bool isEnabled = true;
    public bool autoSetLinkedSequence = false;
    public bool allowDragPivot = false;
    public Transform dragPivot;
    public float time;
    public float blendTime;
    public List<CameraKeyFrame> keyFrames;

    public void AddCamKey(Vector3 pos, Quaternion rot, Transform pivot, float time = 1f)
    {
        CameraKeyFrame k = new CameraKeyFrame(pos, rot, pivot, time);
    }

    public float GetTotalTime()
    {
        float totalTime = 0;
        foreach (var k in keyFrames)
        {
            totalTime += k.time;
        }
        return totalTime;
    }

    public (Vector3, Quaternion) EvaluateCamera(float time)
    {
        float totalTime = 0;
        for (int i = 0; i < keyFrames.Count; ++i)
        {
            if (time >= totalTime && time < (totalTime + keyFrames[i].time))
            {
                if (i + 1 < keyFrames.Count)
                {
                    float fraction = (time - totalTime) / (keyFrames[i].time); 
                    return BlendKeys(keyFrames[i], keyFrames[i + 1], fraction, time);
                }
                else
                {
                    return EvaluateCamera(keyFrames[i], time);
                }
            }
            if (i + 1 >= keyFrames.Count)
                return EvaluateCamera(keyFrames[i], time);

            totalTime += keyFrames[i].time;
        }

        return (Vector3.zero, Quaternion.identity);
    }

    public (Vector3, Quaternion) EvaluateCamera(CameraKeyFrame a, float timeOverride = -1)
    {
        return a.EvaluateCamera(timeOverride);
    }

    public (Vector3, Quaternion) BlendKeys(CameraKeyFrame a, CameraKeyFrame b, float fraction, float timeOverride = -1)
    {
        Vector3 followPos = Vector3.Lerp(a.GetFollowPivotPos(), b.GetFollowPivotPos(), fraction);
        Vector3 lookatPos = Vector3.Lerp(a.GetLookatPivotPos(), b.GetLookatPivotPos(), fraction);

        Quaternion followRot = Quaternion.Slerp(a.GetFollowPivotQuat(), b.GetFollowPivotQuat(), fraction);
        Quaternion lookatRot = Quaternion.Slerp(a.GetLookatPivotQuat(), b.GetLookatPivotQuat(), fraction);

        Vector3 localPos = Vector3.Slerp(a.localPos, b.localPos, fraction);
        Vector3 lookatOffset = Vector3.Lerp(a.lookatOffset, b.lookatOffset, fraction);

        Vector3 pos = followPos + followRot * localPos;
        Vector3 lookat = lookatPos + lookatRot * lookatOffset;
        Quaternion camRot = Quaternion.LookRotation(lookat - pos, Vector3.up);

        Vector3 noisePos = Vector3.Lerp(camRot * a.GetNoisePositionDelta(timeOverride), camRot * b.GetNoisePositionDelta(timeOverride), fraction);
        Quaternion noiseQuat = Quaternion.Slerp(a.GetNoiseEulerDelta(timeOverride), b.GetNoiseEulerDelta(timeOverride), fraction);

        pos +=  noisePos;
        camRot *= noiseQuat;
        return (pos, camRot);
    }

    public void Start()
    {
        if (autoSetLinkedSequence)
        {
            CustomisationCameraSystem.Instance?.LinkDragToSequence(this);
        }
    }
}
