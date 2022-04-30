using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class AttackTarget : IBehaviourTask
{
    public CharacterAgentController AICharacterAgent { get; set; }

    public IBehaviourTask Clone()
    {
        AttackTarget clone = new AttackTarget();
        clone.rotateToTargetRate = this.rotateToTargetRate;
        return clone;
    }

    public float rotateToTargetRate = 4f;
    private IAttackable cachedAttackTarget;

    public bool CanExecute()
    {
        cachedAttackTarget = AICharacterAgent.GetCurrentAttackTarget();
        return cachedAttackTarget != null &&
            cachedAttackTarget.IsAlive() &&
            !AICharacterAgent.IsCasting &&
            Vector3.Distance(AICharacterAgent.GetPosition(), cachedAttackTarget.GetPosition()) < AICharacterAgent.attackRange;
    }

    public void TriggerTask()
    {
        if (AICharacterAgent.IsCasting) return;
        cachedAttackTarget = AICharacterAgent.GetCurrentAttackTarget();
        AICharacterAgent.TryWeaponAttack();
    }

    public void Execute()
    {
        if (AICharacterAgent.IsCasting || AICharacterAgent.attackCooldown > 0) return;
        cachedAttackTarget = AICharacterAgent.GetCurrentAttackTarget();
        if (cachedAttackTarget != null && !AICharacterAgent.IsChannelingAbility && !AICharacterAgent.IsAimingAbility)
        {
            AICharacterAgent.RotateTowardsTarget(cachedAttackTarget.GetPosition(), rotateToTargetRate);
            AICharacterAgent.TryWeaponAttack();
        }
    }

    public bool IsCompleted()
    {
        if (cachedAttackTarget == null || !cachedAttackTarget.IsAlive()) return true;
        else
        {
            return Vector3.Distance(AICharacterAgent.GetPosition(), cachedAttackTarget.GetPosition()) > AICharacterAgent.attackRange;
        }
    }

    public void OnSuccess()
    {
        throw new System.NotImplementedException();
    }
}
