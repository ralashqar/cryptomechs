using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.Serialization;
using Sirenix.OdinInspector;
using Sirenix.OdinInspector.Demos.RPGEditor;
using UnityEngine.Playables;
#if UNITY_EDITOR
using UnityEditor.Animations;
#endif

[System.Serializable]
public class MechAnimationController
{
    public CharacterAgentController character;

    [HideInInspector]
    public Animator animator;

    public AnimatorStateInfo currentBaseState;
    //List<AnimationEvent> clipEvents;

    protected AnimatorOverrideController animatorOverrideController;
    protected AnimationClipOverrides clipOverrides;

    private float cachedPausedAnimSpeed = 1;

    int lastAnimStateIndex = 0;
    [BoxGroup("Split/Right")]
    private string Anim1ClipKey;
    [BoxGroup("Split/Right")]
    private string Anim2ClipKey;

    public MechAnimationController()
    {
    }

    public void Setup(CharacterAgentController character)
    {
        this.character = character;
        var legs = character.mech.GetSlotGameObject(MechSlot.LEGS);
        this.animator = legs.GetComponent<Animator>();
        if (animator != null)
            this.animator.enabled = true;
    }

    public bool IsMeleeAttacking()
    {
        currentBaseState = animator.GetCurrentAnimatorStateInfo(0);
        return currentBaseState.IsTag("Melee");
    }

    public bool IsCasting()
    {
        if (animator == null) return false;

        currentBaseState = animator.GetCurrentAnimatorStateInfo(0);
        if (character.selectedAbility != null && character.selectedAbility.UseCustomChannelAnimation)
        {
            if (currentBaseState.IsName(character.selectedAbility.CustomChannelAnimationTrigger))
                return true;
        }
        return currentBaseState.IsTag("Cast");
    }

    public bool IsCurrentAnimationTaggedAs(string motionTag)
    {
        /*
        var transition = animator.GetAnimatorTransitionInfo(0);
        var anim = animator.GetCurrentAnimatorClipInfo(0);

        foreach(AnimatorClipInfo clip in anim)
        {
            if (clip.clip.name.Contains(motionTag))
            {
                return true;
            }
        }
        */
        currentBaseState = animator.GetCurrentAnimatorStateInfo(0);
        return currentBaseState.IsTag(motionTag);
        //return false;
    }

    //public bool IsRunning()
    //{
    //    return currentBaseState.fullPathHash == RunState;
    //}

    public bool IsAttackingTarget()
    {
        return IsMeleeAttacking() || IsCasting();
    }

    public void TriggerDeath()
    {
        if (animator == null) return;

        TriggerCustom("Death");
        animator.SetBool("Idle", false);
        animator.SetBool("Run", false);
    }

    public void TriggerCustom(string motionTag)
    {
        //CustomMotion motions = data.CustomAnimations.Find(m => m.motionTag == motionTag);
        //if (motions != null)
       // {
       //     int index = Random.Range(1, motions.animations.Count + 1);
       //     animator.SetTrigger(motionTag + index.ToString());
       // }
    }

    public void TriggerMelee()
    {
        //int index = Random.Range(1, data.MeleeAttackAnimations.Count + 1);
        //animator.SetTrigger("Melee" + index.ToString());
    }

    Coroutine loopingAnim = null;
    public IEnumerator LoopAnimationSegment(int state, float time, float amplitude, float frequency)
    {
        float startLoopTime = Time.time;
        while (true)
        {
            float t = Time.time - startLoopTime;
            animator.Play(state, 0, time + Mathf.Sin(t * frequency) * amplitude);
            yield return null;
        }
    }
    public IEnumerator LoopAnimationSegment(int state, float time, AnimationSegmentLooper loopSettings)
    {
        float startLoopTime = Time.time;
        while (true)
        {
            float t = Time.time - startLoopTime;
            animator.Play(state, 0, time + Mathf.Sin(t * loopSettings.timeOffsetFrequency) * loopSettings.timeOffsetAmplitudeNormalized);
            yield return null;
        }
    }

    public bool IsAnimationPaused { get { return animator.speed == 0; } }
    public void PauseActiveAnimation()
    {
        if (animator == null) return;

        if (!IsAnimationPaused)
        {
            cachedPausedAnimSpeed = animator.speed;
            animator.speed = 0;
        }
    }

    public void ChannelLoopActiveAnimation()
    {
        if (animator == null) return;

        cachedPausedAnimSpeed = animator.speed;
        animator.speed = 0;
        
        var clipInfo = animator.GetCurrentAnimatorClipInfo(0);
        var s = animator.GetCurrentAnimatorStateInfo(0);
        var time = s.normalizedTime;
        var clipName = clipInfo[0].clip.name;
        loopingAnim = character.StartCoroutine(LoopAnimationSegment(s.fullPathHash, time, character.selectedAbility.channelAnimLooping));
    }

    public void ResumeActiveAnimation()
    {
        if (animator == null) return;

        if (loopingAnim != null)
            character.StopCoroutine(loopingAnim);
        animator.speed = cachedPausedAnimSpeed;
    }

    public void TriggerCast(AbilityProjectileItem ability)
    {
        if (animator == null) return;

        if (ability == null) return;
        if (ability.UseCustomChannelAnimation)
        {
            TriggerCustom(ability.CustomChannelAnimationTrigger);
        }
        else
        {
            //int index = Random.Range(1, data.CastAnimations.Count + 1);
            //animator.SetTrigger("Cast" + index.ToString());
        }
    }

    public bool isRunning = false;

    public void UpdateMoveAnimationState(float speed, bool canMove, float idleSpeedThreshold)
    {
        if (animator == null) return;

        animator.GetCurrentAnimatorStateInfo(0);
        currentBaseState = animator.GetCurrentAnimatorStateInfo(0);

        if (speed > idleSpeedThreshold)
        {
            if (!isRunning && canMove)
            {
                animator.SetTrigger("Walk");
                isRunning = true;
                //animator.SetBool("Idle", false);
                //animator.SetBool("Run", true);
            }
            //currentPos = Vector3.MoveTowards(currentPos, targetPos, Time.deltaTime * speed);
        }
        else
        {
            if (isRunning && canMove)
            {
                animator.SetTrigger("Idle");
                isRunning = false;
                //animator.SetBool("Idle", true);
                //animator.SetBool("Run", false);
            }
        }
    }
}
