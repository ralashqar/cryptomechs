using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using Sirenix.OdinInspector.Demos.RPGEditor;

public class IdleState : State
{
    private float cachedTargetDistance;
    private Vector3 targetPos;
    private bool hasCastAbility = false;
    public MechPartCaster caster;

    public IdleState(CharacterAgentController characterController) : base(characterController)
    {
    }

    public override IEnumerator MainState()
    {
        float idleTime = 2;
        float timeElapsed = 0;

        while (CharacterController.IsCasting)
        {
            yield return null;
        }

        if (CharacterController.battleTurnManager.GetOccupiedTile() != null)
            CharacterController.mover.SetTarget(CharacterController.battleTurnManager.GetOccupiedTile().GetPosition(), MoveMode.BACKWARDS);
        //CharacterController.mover.SetTarget(CharacterController.battleTurnManager.GetBasePoint(), MoveMode.BACKWARDS);

        Vector3 forwardTarget = CharacterController.battleTurnManager.GetCastFromPoint() - CharacterController.battleTurnManager.GetBasePoint();

        /*
        while (!CharacterController.mover.IsAtTarget())
        {
            CharacterController.RotateTowardsTarget(CharacterController.GetPosition() + forwardTarget.normalized, 1f);
            foreach(var c in CharacterController.mech.casters)
            {
                if (c.slot != MechSlot.LEGS)
                    c.RotateToDefaultOrientation(4f);
            }
            yield return null;
        }
        */
        while (timeElapsed < idleTime)
        {
            timeElapsed += Time.deltaTime;
            foreach (var c in CharacterController.mech.casters)
            {
                if (c.slot != MechSlot.LEGS)
                    c.RotateToDefaultOrientation(4f);
            }
            yield return null;
        }

        //while (timeElapsed < idleTime)
        //{
        //    timeElapsed += Time.deltaTime;
        //    CharacterController.RotateTowardsTarget(CharacterController.battleTurnManager.GetCastFromPoint(), 1f);
        //    yield return null;
        //}
        yield break;
    }
}
