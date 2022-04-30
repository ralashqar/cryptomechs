using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface ICharacterAction
{
    ICharacterAction Clone();
    void Trigger(float sequenceStartTime);
    void Complete();
    void Tick();
    bool IsComplete();
    void CommitAction();
}

public enum CharacterActionWaitType
{
    Immediate,
    TimeFromStart,
    TimeRelativeToLastAction,
}

public enum TargetCharacter
{
    PLAYER,
    NPC,
    CUSTOM_OBJECT,
}

public class CharacterActionBase : ICharacterAction
{
    public TargetCharacter targetCharacter;
    public MechSlot targetSlot;
    public string characterID;
    public CharacterActionWaitType waitMode;
    public float waitTime = 0;
    private float elapsedTime;
    private float sequenceStartTime = 0;
    private float actionStartTime = 0;
    private bool isTriggered = false;
    private bool actionCommitted = false;

    private CharacterAgentController character;
    private MechPartCaster mCaster;

    public virtual ICharacterAction Clone()
    {
        CharacterActionBase clone = new CharacterActionBase();
        clone.targetCharacter = this.targetCharacter;
        clone.targetSlot = this.targetSlot;
        clone.characterID = this.characterID;
        clone.waitMode = this.waitMode;
        clone.waitTime = this.waitTime;
        return clone;
    }

    public void SetCaster(MechPartCaster caster)
    {
        this.character = caster.character;
        this.mCaster = caster;
    }

    public virtual void Complete()
    {
    }

    public bool IsTriggered()
    {
        return isTriggered;
    }

    public MechPartCaster GetTargetCaster()
    {
        return mCaster;
    }

    public CharacterAgentController GetTargetCharacter()
    {
        return character;
    }

    public virtual bool IsComplete()
    {
        return false;
    }

    public virtual void CommitAction()
    {
        actionCommitted = true;
    }

    public virtual void ExecuteFrame()
    {

    }

    public bool IsActionCommited()
    {
        return actionCommitted;
    }

    public bool IsWaitTimeComplete()
    {
        switch(waitMode)
        {
            case CharacterActionWaitType.TimeFromStart:
                return ((actionStartTime + elapsedTime) - sequenceStartTime) > waitTime;
            case CharacterActionWaitType.TimeRelativeToLastAction:
                return elapsedTime > waitTime;
            case CharacterActionWaitType.Immediate:
            default:
                return true;
        }
    }

    public virtual void Tick()
    {
        if (isTriggered)
        {
            elapsedTime += Time.deltaTime;
            if (!actionCommitted && IsWaitTimeComplete())
            {
                CommitAction();
                actionCommitted = true;
            }

            if (actionCommitted)
            {
                ExecuteFrame();
            }
        }
    }

    public virtual void Trigger(float sequenceStartTime)
    {
        this.sequenceStartTime = sequenceStartTime;
        this.actionStartTime = Time.time;
        isTriggered = true;

        if (character == null)
        {
            switch (targetCharacter)
            {
                case TargetCharacter.PLAYER:
                    character = CharacterAgentsManager.Instance.player;
                    break;
                case TargetCharacter.NPC:
                    character = CharacterAgentsManager.Instance.charactersByID.ContainsKey(characterID) ?
                        CharacterAgentsManager.Instance.charactersByID[characterID] as CharacterAgentController
                            : null;
                    break;
                default:
                    character = null;
                    break;
            }
        }
        if (mCaster == null)
        {
            var mech = character.GetComponentInChildren<FullMech>();
            if (mech != null)
            {
                mCaster = mech.GetCasterBySlot(targetSlot);
            }
        }

        if (IsWaitTimeComplete())
        {
            CommitAction();
        }
    }

}
/*
public class CharacterAction : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
*/