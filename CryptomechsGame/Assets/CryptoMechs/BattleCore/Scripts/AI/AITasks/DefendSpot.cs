using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class DefendSpot : IBehaviourTask
{
    public CharacterAgentController AICharacterAgent { get; set; }
    private ITargettable cachedClosestTarget;

    private Transform spotTransform;
    Vector3 spotPosition;
    public float aggroRangeIn = 10f;
    public float maxDistanceOut = 15f;
    public bool useAbilities = true;
    public float abilityCooldownTimer = 3f;
    public float abilityAimTimer = 1f;
    private bool isAggroed = false;

    private MoveToTarget moveTask;
    private AttackTarget attackTask;
    private TriggerRandomAbility abilityTask;

    public bool IsAggro { get { return isAggroed; } }

    public IBehaviourTask Clone()
    {
        DefendSpot clone = new DefendSpot();
        clone.aggroRangeIn = this.aggroRangeIn;
        clone.maxDistanceOut = this.maxDistanceOut;
        return clone;
    }

    public bool CanExecute()
    {
        return true;
    }

    private bool initialized = false;
    public void TriggerTask()
    {
        if (spotTransform != null) spotPosition = spotTransform.position;
        else if (!initialized) spotPosition = AICharacterAgent.GetPosition();

        initialized = true;

        if (moveTask == null)
        {
            moveTask = new MoveToTarget();
            moveTask.AICharacterAgent = AICharacterAgent;
        }
        if (attackTask == null)
        {
            attackTask = new AttackTarget();
            attackTask.AICharacterAgent = AICharacterAgent;
        }
        if (abilityTask == null)
        {
            abilityTask = new TriggerRandomAbility();
            abilityTask.AICharacterAgent = AICharacterAgent;
            abilityTask.taskCooldownTimer = abilityCooldownTimer;
            abilityTask.aimTimer = abilityAimTimer;
        }
        UpdateAggro();
    }

    private void UpdateAggro()
    {
        if (spotTransform != null) spotPosition = spotTransform.position;
        cachedClosestTarget = CharacterAgentsManager.FindClosestAttackableEnemy(AICharacterAgent.GetPosition(), AICharacterAgent.GetTeam()) as ITargettable;
        if (cachedClosestTarget != null)
        {
            //AICharacterAgent.SetMoveToTarget(cachedClosestTarget);
            float distance = Vector3.Distance(spotPosition, cachedClosestTarget.GetPosition());
            if (distance <= aggroRangeIn)
            {
                //AICharacterAgent.SetMoveToTarget(cachedClosestTarget);
                isAggroed = true;
            }
            else
            {
                if (isAggroed && distance >= maxDistanceOut)
                    isAggroed = false;
            }
        }
        else
        {
            isAggroed = false;
        }
        //AttackLoop();
        
        if (isAggroed)
        {
            AttackLoop();
            //AICharacterAgent.SetMoveToTarget(cachedClosestTarget);
        }
        else
        {
            if (!AICharacterAgent.IsCasting)
                AICharacterAgent.SetMoveToTarget(spotPosition);
        }
        
    }

    public void AttackLoop()
    {
        if (useAbilities)
        {
            abilityTask.Execute();
        }
        //if (!abilityTask.IsCasting())
        {
            if (attackTask.CanExecute())
            {
                attackTask.Execute();
            }
            if (moveTask.CanExecute())
            {
                moveTask.Execute();
            }
        }
        //return;
        //if (!abilityTask.IsCasting())
        {
            //if (attackTask.CanExecute())
           // {
            //    attackTask.Execute();
            //}
            //else if (moveTask.CanExecute())
            //{
            //    moveTask.Execute();
            //}
        }
    }

    public void Execute()
    {
        UpdateAggro();
    }

    public bool IsCompleted()
    {
        return false;
    }

    public void OnSuccess()
    {
        //throw new System.NotImplementedException();
    }
}
