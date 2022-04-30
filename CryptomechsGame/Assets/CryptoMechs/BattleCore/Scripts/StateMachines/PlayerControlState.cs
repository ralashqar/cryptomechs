using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class PlayerControlState : State
{
    private float cachedTargetDistance;
    private Vector3 targetPos;
    private ITargettable closestTargettable;
    private float clickPointDistance = 100;

    public PlayerControlState(CharacterAgentController characterController) : base(characterController)
    {
        targetPos = CharacterController.GetPosition();
        CharacterController.targetObject = null;
        CharacterController.cachedClosestEnemy = null;
    }

    public void ProcessClickMoveTarget()
    {
        if (Input.GetMouseButtonDown(0))
        {
            RaycastHit hit;
            if (Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out hit))
            {
                NavMeshHit navMeshHit = new NavMeshHit();
                NavMesh.SamplePosition(hit.point, out navMeshHit, 1, 1);
                Vector3 hitPoint = hit.point;
                hitPoint.y = CharacterController.transform.position.y;
                CharacterController.mover.SetTarget(hitPoint);
                targetPos = hitPoint;

                if (CharacterController.IsChannelingAbility && CharacterController.selectedAbility != null && CharacterController.selectedAbility.CanCancelAbility)
                {
                    CharacterController.ClearActiveAbility();
                }

                closestTargettable = CharacterAgentsManager.FindClosestTargettable(CharacterController, hitPoint);
                IAttackable closestAttackable = closestTargettable as IAttackable;
                if (closestAttackable != null && closestAttackable.GetTeam() != CharacterController.GetTeam()
                    && closestAttackable.GetTeam() != CharacterTeam.NEUTRAL)
                {
                    CharacterController.cachedClosestEnemy = closestAttackable;
                }
                else
                {
                    CharacterController.cachedClosestEnemy = null;
                }

                if (closestTargettable != null)
                    clickPointDistance = Vector3.Distance(hitPoint, closestTargettable.GetGameObject().transform.position);
                else
                    clickPointDistance = 0;

                //CharacterController.cachedClosestEnemy = CharacterAgentsManager.FindClosestAttackableEnemy(hitPoint, CharacterController.GetTeam());

                float closestDistance = closestTargettable != null ? clickPointDistance : -1;

                if (closestDistance > 0 && closestDistance < 2.0f)
                {
                    CharacterController.targetObject = closestTargettable;
                    targetPos = closestTargettable.GetPosition();

                    CharacterController.agent.stoppingDistance = CharacterController.attackRange * 0.75f;
                }
                else
                {
                    CharacterController.targetObject = null;
                    CharacterController.cachedClosestEnemy = null;
                    CharacterController.agent.stoppingDistance = 1.0f;
                }
            }
        }
    }

    private void ProcessClosestEnemyTarget()
    {
        if (CharacterController.targetObject == null)
        {
            if (Vector3.Distance(targetPos, CharacterController.GetPosition()) < 2.0f)
            {
                CharacterController.cachedClosestEnemy = CharacterAgentsManager.FindClosestAttackableEnemy(CharacterController.GetPosition(), CharacterController.GetTeam());
                CharacterController.targetObject = CharacterController.cachedClosestEnemy != null ? CharacterController.cachedClosestEnemy : null;
                CharacterController.agent.stoppingDistance = CharacterController.attackRange * 0.75f;

                if (CharacterController.targetObject != null && Vector3.Distance(CharacterController.GetPosition(), CharacterController.targetObject.GetPosition()) > CharacterController.attackRange)
                {
                    CharacterController.targetObject = null;
                    CharacterController.cachedClosestEnemy = null;
                    CharacterController.agent.stoppingDistance = 1.0f;
                }
            }
        }

        cachedTargetDistance = 0;
        if (CharacterController.targetObject != null)
        {
            targetPos = CharacterController.targetObject.GetPosition();
            cachedTargetDistance = Vector3.Distance(CharacterController.GetPosition(), targetPos);
        }
     
        CharacterController.agent.destination = targetPos;
        
    }

    public void ProcessAttacks()
    {
        if (CharacterController.agent.velocity.magnitude < CharacterController.idleSpeedThreshold)
        {
            //if (!IsCastingAbility && !IsSelectingTarget)
            {
                if (CharacterController.targetObject != null)
                {
                    // aim towards target
                    CharacterController.RotateTowardsTarget(CharacterController.targetObject.GetPosition(), 4f);
                }
            }
            if (CharacterController.targetObject == null)
            {
                CharacterController.agent.updatePosition = true;
            }
            if (CharacterController.cachedClosestEnemy != null && CharacterController.cachedClosestEnemy.IsAlive())
            {
                CharacterController.TryWeaponAttack();
            }
        }

        //if (!CharacterController.animationController.IsAttackingTarget())
        {
            CharacterController.IsWeaponStriking = false;
        }
    }

    public Vector3 Get3DMousePosition()
    {
        RaycastHit hit;

        //if (Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out hit, 300.0f, MouseRaycast))
        if (Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out hit))
            return hit.point;
        else
            return Vector3.zero;
    }

    public void ProcessActiveAbility()
    {
        /*
        if (CharacterController.IsAimingAbility)
        {
            CharacterController.AimSpellAtTarget(CharacterController.Splats.GetSpellCursorPosition());
            if (Input.GetMouseButtonUp(0))
            {
                CharacterController.CommitAbility(CharacterController.selectedAbility);
                CharacterController.CancelAllSplats();
            }
        }
        */
        foreach(var caster in CharacterController.mech.casters)
        {
            if (!caster.AutoCasting && caster.IsAimingAbility)
            {
                //caster.AimSpellAtTarget(CharacterController.Splats.GetSpellCursorPosition());
                caster.AimSpellAtTarget(Get3DMousePosition());
                if (Input.GetMouseButtonUp(0))
                {
                    caster.CommitAbility(caster.selectedAbility);
                    //CharacterController.CancelAllSplats();
                }
            }
        }
    }

    public void EvaluateNPCInteractable()
    {
        //Check for character interaction
        if (closestTargettable is CharacterAgentController)
        {
            CharacterAgentController targetAgent = closestTargettable as CharacterAgentController;
            if (targetAgent.interactionNarrative != null && clickPointDistance < 1f &&
                Vector3.Distance(targetAgent.GetPosition(), CharacterController.GetPosition()) < 4f)
            {
                bool canTriggerNarrative = CharacterController.narrativesManager.CanTriggerNarrative(targetAgent.interactionNarrative, targetAgent);
                if (canTriggerNarrative)
                {
                    CharacterController.narrativesManager.TriggerNarrative(targetAgent.interactionNarrative, targetAgent);
                    CharacterController.SetState(new NarrativeInteractionState(CharacterController));
                    CharacterController.targetObject = closestTargettable;
                }
                return;
            }
        }
    }

    public override IEnumerator MainState()
    {
        //CharacterController.LoadSplats();

        while (true)
        {
            while (GameManager.Instance.mode != GameMode.REALTIME_BATTLE)
            {
                yield return null;
            }

            if (!CharacterController.IsIncapacitated())
            {
                bool canCancelAbility = (CharacterController.selectedAbility != null && CharacterController.selectedAbility.CanCancelAbility);
                //if (!CharacterController.IsAimingAbility && (!CharacterController.IsChannelingAbility
                //    || canCancelAbility))
                if (CharacterController.CanMove)
                {
                    ProcessClickMoveTarget();
                    //if (!CharacterController.IsChannelingAbility)
                    //{
                    //    ProcessClosestEnemyTarget();
                        //ProcessAttacks();
                    //}
                }
                else
                {
                    this.targetPos = CharacterController.GetPosition();
                }

                ProcessActiveAbility();

                //EvaluateNPCInteractable();
            }
            yield return null;
        }
    }
}
