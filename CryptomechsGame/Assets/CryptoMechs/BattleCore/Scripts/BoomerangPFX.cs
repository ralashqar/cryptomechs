using Sirenix.OdinInspector.Demos.RPGEditor;
using System.Collections.Generic;
using System.Collections;
using UnityEngine;

[System.Serializable]
public class BoomerangPFX : MonoBehaviour, AbilityVisualFXBehavior
{
    Transform sourceMuzzle;
    public Vector3 initPosLocal;
    public Vector3 initPos;
    public Vector3 dir;
    public Vector3 initForward;
    Quaternion initRotLocal;
    Quaternion initRot;
    Vector3 halfTimePos = Vector3.zero;
    bool pastHalfTime = false;

    AbilityProjectileItem ability;
    MechPartCaster mCaster;
    float channelTime = 1f;
    float elapsedTime = 0;

    public Vector3 GetTargetPosition()
    {
        return sourceMuzzle.position;
    }

    public void DestroyFX(GameObject go)
    {
        this.sourceMuzzle.localRotation = initRotLocal;
        this.sourceMuzzle.transform.localPosition = initPosLocal;
        mCaster.IsBoomeranging = false;
    }

    public void OnChannel(Vector3 position)
    {
        elapsedTime += Time.deltaTime;
        float halfTime = channelTime / 2;

        if (!pastHalfTime && elapsedTime > halfTime)
        {
            halfTimePos = this.sourceMuzzle.transform.position;
            pastHalfTime = true;
        }

        float fraction = elapsedTime <= halfTime ? elapsedTime / halfTime : 1 - ((elapsedTime - halfTime)/ halfTime);

        this.sourceMuzzle.transform.rotation = initRot;
        this.sourceMuzzle.transform.position += sourceMuzzle.transform.forward * Time.deltaTime * 30f;

        float maxD = ability.range;
        if (!pastHalfTime)
        {
            this.sourceMuzzle.transform.position = initPos + dir * (fraction * maxD);
        }
        else
        {
            Vector3 targetPos = sourceMuzzle.transform.parent.TransformPoint(initPosLocal);
            this.sourceMuzzle.transform.position = halfTimePos * (fraction) + targetPos * (1 - fraction);
        }
    }

    public void OnFire(IAbilityCaster caster, Transform sourceMuzzle, AbilityProjectileItem ability)
    {
        this.ability = ability;
        mCaster = sourceMuzzle.GetComponent<MechPartCaster>();
        if (mCaster == null)
            mCaster = sourceMuzzle.GetComponentInParent<MechPartCaster>();
        mCaster.IsBoomeranging = true;
        sourceMuzzle = mCaster.transform;
        this.sourceMuzzle = sourceMuzzle;
        this.initPosLocal = sourceMuzzle.localPosition;
        this.initPos = sourceMuzzle.position;
        this.channelTime = ability.AbilityMode == AbilityFXType.LASER ? ability.ChannelTime : ability.Lifetime;
        initRot = sourceMuzzle.rotation;
        initRotLocal = sourceMuzzle.localRotation;
        this.dir = sourceMuzzle.transform.forward;
    }

    public void OnHitTarget(Vector3 target)
    {
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
