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
public enum SpecialAnimationType
{
    MELEE_STRIKE,
    RANGED_STRIKE,
    CAST
}

[System.Serializable]
public class AnimationDescriptor
{
    public Animation animation;
    public float blendTime;
    public float playbackSpeed = 1;
}

public class AnimationClipOverrides : List<KeyValuePair<AnimationClip, AnimationClip>>
{
    public AnimationClipOverrides(int capacity) : base(capacity) { }

    public AnimationClip this[string name]
    {
        get { return this.Find(x => x.Key.name.Equals(name)).Value; }
        set
        {
            int index = this.FindIndex(x => x.Key.name.Equals(name));
            if (index != -1)
                this[index] = new KeyValuePair<AnimationClip, AnimationClip>(this[index].Key, value);
        }
    }
}

[System.Serializable]
public class CustomMotion
{
    public string motionTag = "";
    public List<Motion> animations;
}

[System.Serializable]
public class AnimationDictionary : UnitySerializedDictionary<SpecialAnimationType, List<Animation>> { }

public abstract class UnitySerializedDictionary<TKey, TValue> : Dictionary<TKey, TValue>, ISerializationCallbackReceiver
{
    [SerializeField, HideInInspector]
    private List<TKey> keyData = new List<TKey>();

    [SerializeField, HideInInspector]
    private List<TValue> valueData = new List<TValue>();

    void ISerializationCallbackReceiver.OnAfterDeserialize()
    {
        this.Clear();
        for (int i = 0; i < this.keyData.Count && i < this.valueData.Count; i++)
        {
            this[this.keyData[i]] = this.valueData[i];
        }
    }

    void ISerializationCallbackReceiver.OnBeforeSerialize()
    {
        this.keyData.Clear();
        this.valueData.Clear();

        foreach (var item in this)
        {
            this.keyData.Add(item.Key);
            this.valueData.Add(item.Value);
        }
    }
}

[System.Serializable]
public class AgentAnimationController
{
    //[HorizontalGroup("Split")]
    [HorizontalGroup("Split")]

    [BoxGroup("Split/Left")]
    public Motion IdleAnimation;

    //[BoxGroup("Split/Left")]
    //public Animation WalkAnimation;

    [BoxGroup("Split/Left")]
    public Motion RunAnimation;

    [BoxGroup("Split/Left")]
    public List<Motion> MeleeAttackAnimations;

    [BoxGroup("Split/Right")]
    public List<Motion> RangeAttackAnimations;

    [BoxGroup("Split/Right")]
    public List<Motion> CastAnimations;

    [BoxGroup("Split/Right")]
    public List<Motion> DeathAnimations;

    [BoxGroup("Split/Right")]
    public List<CustomMotion> CustomAnimations;


    //[BoxGroup("Split/Left")]
    //public AnimationDictionary specialAnimations;

    //[VerticalGroup("Split")]
    //[BoxGroup("Split/Right")]
    [HideInInspector]
    public Animator animator;

#if UNITY_EDITOR
    public AnimatorController CreateController(string controllerName)
    {
        // Creates the controller
        var controller = AnimatorController.CreateAnimatorControllerAtPath("Assets/Resources/AnimationControllers/" + controllerName + ".controller");
        controller.RemoveLayer(0);
        controller.AddLayer("Base");
        // Add parameters
        controller.AddParameter("Idle", AnimatorControllerParameterType.Bool);
        controller.AddParameter("Run", AnimatorControllerParameterType.Bool);

        // Add StateMachines
        var rootStateMachine = controller.layers[0].stateMachine;

        // Add States
        var stateRun = rootStateMachine.AddState("Run");
        var stateIdle = rootStateMachine.AddState("Idle");

        rootStateMachine.defaultState = stateIdle;

        var runToIdle = stateRun.AddTransition(stateIdle);
        runToIdle.AddCondition(UnityEditor.Animations.AnimatorConditionMode.If, 0, "Idle");
        runToIdle.AddCondition(UnityEditor.Animations.AnimatorConditionMode.IfNot, 0, "Run");
        runToIdle.hasFixedDuration = true;
        runToIdle.duration = 0.25f;

        var idleToRun = stateIdle.AddTransition(stateRun);
        //idleToRun.AddCondition(UnityEditor.Animations.AnimatorConditionMode.IfNot, 0, "Idle");
        idleToRun.AddCondition(UnityEditor.Animations.AnimatorConditionMode.If, 0, "Run");
        idleToRun.hasFixedDuration = true;
        idleToRun.duration = 0.25f;

        // Populate Animation Clips
        //controller.AddMotion(IdleAnimation);
        //controller.AddMotion(RunAnimation);
        controller.SetStateEffectiveMotion(stateIdle, IdleAnimation);
        controller.SetStateEffectiveMotion(stateRun, RunAnimation);

        AddAnimationTypeToController(controller, MeleeAttackAnimations, "Melee", rootStateMachine, stateRun, stateIdle);
        AddAnimationTypeToController(controller, CastAnimations, "Cast", rootStateMachine, stateRun, stateIdle);

        foreach (CustomMotion m in CustomAnimations)
        {
            AddAnimationTypeToController(controller, m.animations, m.motionTag, rootStateMachine, stateRun, stateIdle);
        }

        return controller;
    }

    public void AddMotionToController(AnimatorController controller, Motion motion, int index, string motionTag, AnimatorStateMachine rootStateMachine, AnimatorState stateRun, AnimatorState stateIdle)
    {
        var name = motionTag + (index++).ToString();
        var mState = rootStateMachine.AddState(motionTag + index.ToString());
        mState.tag = motionTag;
        mState.speed = 1.3f;
        controller.AddParameter(name, AnimatorControllerParameterType.Trigger);

        var mTrans = rootStateMachine.AddAnyStateTransition(mState);
        mTrans.AddCondition(AnimatorConditionMode.If, 0, name);
        mTrans.hasExitTime = false; mTrans.hasFixedDuration = true; mTrans.duration = 0.25f;

        var mToRun = mState.AddTransition(stateRun, true);
        mToRun.hasExitTime = false;
        mToRun.AddCondition(AnimatorConditionMode.IfNot, 0, "Idle");
        mToRun.AddCondition(AnimatorConditionMode.If, 0, "Run");
        mToRun.hasFixedDuration = true; mToRun.duration = 0.25f;

        var mToIdle = mState.AddTransition(stateIdle, true);
        mToIdle.hasExitTime = true;
        mToIdle.AddCondition(AnimatorConditionMode.If, 0, "Idle");
        mToIdle.AddCondition(AnimatorConditionMode.IfNot, 0, "Run");
        mToIdle.hasFixedDuration = true; mToIdle.duration = 0.25f;

        controller.SetStateEffectiveMotion(mState, motion);
    }

    public void AddAnimationTypeToController(AnimatorController controller, List<Motion> motions, string motionTag, AnimatorStateMachine rootStateMachine, AnimatorState stateRun, AnimatorState stateIdle)
    {
        if (motions != null && motions.Count > 0)
        {
            int c = 1;
            foreach (Motion m in motions)
            {
                AddMotionToController(controller, m, c, motionTag, rootStateMachine, stateRun, stateIdle);
                c++;
            }
        }
    }
#endif
}

[System.Serializable]
public class AgentAnimationControllerRuntime
{
    public AgentAnimationController data;
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

    [HideInInspector]
    public int IdleState = Animator.StringToHash("Base.Idle");
    [HideInInspector]
    public int RunState = Animator.StringToHash("Base.Run");
    [HideInInspector]
    public int CastState = Animator.StringToHash("Base.Cast");
    [HideInInspector]
    public int StrikeState = Animator.StringToHash("Base.Melee");

    public AgentAnimationControllerRuntime(AgentAnimationController data)
    {
        this.data = data;
    }

    public void Setup(CharacterAgentController character, Animator animator)
    {
        this.character = character;
        this.animator = animator;
        animator.GetCurrentAnimatorStateInfo(0);
        currentBaseState = animator.GetCurrentAnimatorStateInfo(0);

        animatorOverrideController = new AnimatorOverrideController(animator.runtimeAnimatorController);
        animator.runtimeAnimatorController = animatorOverrideController;

        clipOverrides = new AnimationClipOverrides(animatorOverrideController.overridesCount);
        animatorOverrideController.GetOverrides(clipOverrides);

        //var clips = Resources.LoadAll("Animations/", typeof(AnimationClip)).Cast<AnimationClip>();
        //allClips = new List<AnimationClip>(clips);
    }

    public bool IsMeleeAttacking()
    {
        currentBaseState = animator.GetCurrentAnimatorStateInfo(0);
        return currentBaseState.IsTag("Melee");
    }

    public bool IsCasting()
    {
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

    public bool IsRunning()
    {
        return currentBaseState.fullPathHash == RunState;
    }

    public bool IsAttackingTarget()
    {
        return IsMeleeAttacking() || IsCasting();
    }

    public void TriggerDeath()
    {
        TriggerCustom("Death");
        animator.SetBool("Idle", false);
        animator.SetBool("Run", false);
    }

    public void TriggerCustom(string motionTag)
    {
        CustomMotion motions = data.CustomAnimations.Find(m => m.motionTag == motionTag);
        if (motions != null)
        {
            int index = Random.Range(1, motions.animations.Count + 1);
            animator.SetTrigger(motionTag + index.ToString());
        }
    }

    public void TriggerMelee()
    {
        int index = Random.Range(1, data.MeleeAttackAnimations.Count + 1);
        animator.SetTrigger("Melee" + index.ToString());
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
        if (!IsAnimationPaused)
        {
            cachedPausedAnimSpeed = animator.speed;
            animator.speed = 0;
        }
    }

    public void ChannelLoopActiveAnimation()
    {
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
        if (loopingAnim != null)
            character.StopCoroutine(loopingAnim);
        animator.speed = cachedPausedAnimSpeed;
    }

    public void TriggerCast(AbilityProjectileItem ability)
    {
        if (ability == null) return;
        if (ability.UseCustomChannelAnimation)
        {
            TriggerCustom(ability.CustomChannelAnimationTrigger);
        }
        else
        {
            int index = Random.Range(1, data.CastAnimations.Count + 1);
            animator.SetTrigger("Cast" + index.ToString());
        }
    }

    public void SwitchAnimation(Animation newAnim)
    {
        string newState = animator.GetCurrentAnimatorStateInfo(0).IsName("Dance1") ? "Dance2" : "Dance1";
        AnimationClip clipToSwapIn = newAnim.clip;
        lastAnimStateIndex = 1 - lastAnimStateIndex;
        string clipName = lastAnimStateIndex == 0 ? Anim1ClipKey : Anim2ClipKey;
        clipOverrides[clipName] = clipToSwapIn;
        animatorOverrideController.ApplyOverrides(clipOverrides);
        SwitchAnimation(0.01f);
    }

    public void SwitchAnimationInternal()
    {
        animator.SetTrigger("SwitchAnimation");
    }

    public void SwitchAnimation(float delay)
    {
        SwitchAnimationInternal();
        //Invoke("SwitchDanceInternal", delay);
    }

    public void UpdateMoveAnimationState(float speed, bool canMove, float idleSpeedThreshold)
    {
        animator.GetCurrentAnimatorStateInfo(0);
        currentBaseState = animator.GetCurrentAnimatorStateInfo(0);

        if (speed > idleSpeedThreshold)
        {
            if (currentBaseState.fullPathHash != RunState && canMove)
            {
                animator.SetBool("Idle", false);
                animator.SetBool("Run", true);
            }
            //currentPos = Vector3.MoveTowards(currentPos, targetPos, Time.deltaTime * speed);
        }
        else
        {
            if (currentBaseState.fullPathHash != IdleState && canMove)
            {
                animator.SetBool("Idle", true);
                animator.SetBool("Run", false);
            }
        }
    }
}
