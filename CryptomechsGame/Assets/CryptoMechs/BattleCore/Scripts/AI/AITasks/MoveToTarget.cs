using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class MoveToTarget : IBehaviourTask
{
    public CharacterAgentController AICharacterAgent { get; set; }

    private ITargettable cachedClosestTarget;

    public IBehaviourTask Clone()
    {
        MoveToTarget clone = new MoveToTarget();
        return clone;
    }

    public bool CanExecute()
    {
        return true;
    }

    public void TriggerTask()
    {
        cachedClosestTarget = CharacterAgentsManager.FindClosestAttackableEnemy(AICharacterAgent.GetPosition(), AICharacterAgent.GetTeam()) as ITargettable;
        //if (cachedClosestTarget != null && !AICharacterAgent.IsCasting)
        //    AICharacterAgent.SetMoveToTarget(cachedClosestTarget);
    }

    public void Execute()
    {
        /*
        var seq = AICharacterAgent.aiBehavior.AISequences.Find(s=>s.tasks.Exists(t=> t is TriggerRandomAbility));
        if (seq != null)
        {
            var a = seq.tasks.Find(t => t is TriggerRandomAbility);
            if (!(a as TriggerRandomAbility).IsOnCooldown)
            {
                return;
            }
        }
        */
        if (AICharacterAgent.IsCasting)
        {
            //AICharacterAgent.ForceStandStill();
            //AICharacterAgent.SetMoveToTarget(AICharacterAgent.GetPosition());
            return;
        }

        if (cachedClosestTarget == null)
        {
            cachedClosestTarget = CharacterAgentsManager.FindClosestAttackableEnemy(AICharacterAgent.GetPosition(), AICharacterAgent.GetTeam()) as ITargettable;
        }

        if (cachedClosestTarget != null && !AICharacterAgent.IsCasting)
        {
            Vector3 delta = cachedClosestTarget.GetPosition() - AICharacterAgent.GetPosition();
            float d = delta.magnitude;
            if (d >= AICharacterAgent.attackRange)
            {
                Vector3 target = AICharacterAgent.GetPosition() + (delta - (delta.normalized * (AICharacterAgent.attackRange) * 0.8f));
                
                //AICharacterAgent.SetMoveToTarget(cachedClosestTarget);

                AICharacterAgent.SetMoveToTarget(target);
            }
        }
    }

    public bool IsCompleted()
    {
        if (cachedClosestTarget != null)
        {
            return Vector3.Distance(AICharacterAgent.GetPosition(), cachedClosestTarget.GetPosition()) < AICharacterAgent.attackRange;
        }

        return false;

    }

    public void OnSuccess()
    {
        throw new System.NotImplementedException();
    }
}
