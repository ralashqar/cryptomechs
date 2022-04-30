using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using Sirenix.OdinInspector.Demos.RPGEditor;

public class ChannelingAbilityState : State
{
    private float cachedTargetDistance;
    private Vector3 targetPos;
    private bool hasCastAbility = false;
    public MechPartCaster caster;

    public ChannelingAbilityState(CharacterAgentController characterController, MechPartCaster caster) : base(characterController)
    {
        targetPos = caster.GetPosition();
        this.caster = caster;
    }

    private float channelTime = 0;
    private float moveWhileChannelDelayTime = 0;
    private float channelTimer = 0;
    bool cancelAbility = false;

    public void OnCancelAbility(MechPartCaster part)
    {
        if (part == caster)
            this.cancelAbility = true;
    }

    public void ChannelingAbilityUpdate()
    {
        //bool isChanneling = selectedAbility != null && animationController.IsCurrentAnimationTaggedAs(selectedAbility.CustomChannelAnimationTrigger);
        AbilityProjectileItem ability = caster.selectedAbility;
        int maxRounds = AbilitiesManager.GetMaxRounds(ability);
        int roundsUsed = 0;

        if (caster.IsAimingAbility) return;
        
        caster.IsChannelStateActive = true;

        if (caster.selectedAbility.AbilityMode == AbilityFXType.LASER)
        {
            if (CharacterController.manualControl && !caster.AutoCasting)
            {
                RaycastHit hit;
                if (Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out hit))
                {
                    NavMeshHit navMeshHit = new NavMeshHit();
                    NavMesh.SamplePosition(hit.point, out navMeshHit, 1, 1);
                    Vector3 hitPoint = hit.point;
                    hitPoint.y = caster.GetPosition().y;
                    caster.OnUpdateCastTarget(hitPoint);
                    if (caster.hasIndependentAim)
                    {
                        caster.AimSpellAtTarget(hitPoint);
                    }
                    caster.lastAbilityPoint = hitPoint;
                }
            }

            if (!caster.hasIndependentAim)
                caster.AimSpellAtTarget(caster.lastAbilityPoint);
        }

        if (caster.selectedAbility.ChannelMode == AbilityChannelingMode.REPEATED_INTERVALS)
        {
            RaycastHit hit;
            if (CharacterController.manualControl && !caster.AutoCasting)
            {
                if (Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out hit))
                {
                    NavMeshHit navMeshHit = new NavMeshHit();
                    NavMesh.SamplePosition(hit.point, out navMeshHit, 1, 1);
                    Vector3 hitPoint = hit.point;
                    hitPoint.y = caster.GetPosition().y;
                    caster.AimSpellAtTarget(hitPoint);
                    caster.lastAbilityPoint = hitPoint;
                }
            }

            channelTimer -= Time.deltaTime;
            if (channelTimer <= 0 && roundsUsed < maxRounds)
            {
                //channelTimer = 1f / caster.selectedAbility.ChannelRate;
                channelTimer = caster.selectedAbility.ChannelTime / (float)caster.selectedAbility.NumImpacts;
                caster.CastAbility(false);
                roundsUsed++;
            }
        }

        if (moveWhileChannelDelayTime > 0) moveWhileChannelDelayTime -= Time.deltaTime;

        if (caster.selectedAbility.MoveWhileChanneling && moveWhileChannelDelayTime <= 0)// && animationController.IsCurrentAnimationTaggedAs(selectedAbility.CustomChannelAnimationTrigger))
        {
            if (caster.selectedAbility.AimType == AbilityAimType.POINT)
            {
                float distance = Vector3.Distance(caster.positionAtCastTime, caster.lastAbilityPoint);
                CharacterController.transform.position = Vector3.MoveTowards(CharacterController.transform.position, 
                    caster.lastAbilityPoint, Time.deltaTime * caster.selectedAbility.ChannelRate * distance);
                caster.targetObject = null;
                CharacterController.mover.SetTarget(CharacterController.GetPosition());
                CharacterController.transform.forward = Vector3.RotateTowards(CharacterController.transform.forward, (caster.lastAbilityPoint - CharacterController.GetPosition()).normalized, 0.1f, 0.1f);
            }
            else
            {
                float distance = caster.selectedAbility.MoveDistance;
                Vector3 forwardDelta = caster.selectedAbility.ChannelRate * CharacterController.GetForward() * Time.deltaTime * distance;
                CharacterController.transform.position += forwardDelta;
                CharacterController.mover.SetTarget(CharacterController.GetPosition());
                if (caster.targetObject != null)
                {
                    CharacterController.RotateTowardsTarget(caster.targetObject.GetPosition(), 4f);
                }
            }
        }

        if (ability.ApplyImpactWhileChanneling || ability.ChannelMode == AbilityChannelingMode.CONTINUOUS)
        {
            if (hasCastAbility || ability.ChannelMode != AbilityChannelingMode.CONTINUOUS)
            {
                caster.abilitiesManager.ChannelAbility(CharacterController, caster.lastAbilityPoint, caster.selectedAbility);
            }
        }
    }

    public override IEnumerator MainState()
    {
        while (caster.selectedAbility == null)
        {
            yield return null;
        }

        channelTime = caster.selectedAbility.ChannelTime;
        //channelTimer = 1f / caster.selectedAbility.ChannelRate;
        channelTimer = channelTime / (float)caster.selectedAbility.NumImpacts;
        moveWhileChannelDelayTime = caster.selectedAbility.MoveDelay;

        AbilityProjectileItem ability = caster.selectedAbility;

        caster.OnCastAbility += ()=> { hasCastAbility = true; };
        CharacterController.OnCancelPartAbility += OnCancelAbility;
        caster.IsChannelStateActive = true;

        while (caster.selectedAbility != null && !cancelAbility)
        {
            if (!CharacterController.IsIncapacitated())
            {
                ChannelingAbilityUpdate();
                if (ability.ChannelMode != AbilityChannelingMode.SINGLE && channelTime > 0 && hasCastAbility)
                {
                    channelTime -= Time.deltaTime;
                    if (channelTime <= 0)
                    {
                        caster.ClearActiveAbility();
                        yield break;
                    }
                }
            }
            yield return null;
        }

        CharacterController.OnCancelPartAbility -= OnCancelAbility;
        caster.IsChannelStateActive = false;
        yield break;
    }
}
