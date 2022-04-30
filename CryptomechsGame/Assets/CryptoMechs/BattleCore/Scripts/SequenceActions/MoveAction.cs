using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MoveAction : CharacterActionBase
{
    public enum MoveType
    {
        TRANSLATE_TO,
        ROTATE_TOWARDS
    }

    public MoveType moveType;
    public MoveMode moveMode;

    public float rotateRate = 4f;

    private bool useTag = true;
    public string moveToTargetWithTag;
    Transform targetTr;
    Vector3 targetPos;
    private CharacterAgentController character;
    private float stoppingDistance = 0;

    private bool useWaypoints = false;
    int currentWaypointTarget = 0;
    private List<Vector3> waypoints;
    public void SetTarget(List<Vector3> waypoints)
    {
        this.useWaypoints = true;
        this.waypoints = waypoints;
        this.currentWaypointTarget = 0;
        targetPos = waypoints[currentWaypointTarget];
    }

    public void SetTarget(Transform tr)
    {
        this.targetTr = tr;
        this.targetPos = tr.position;
        this.useTag = false;
    }

    public override ICharacterAction Clone()
    {
        MoveAction clone = new MoveAction();
        clone.targetCharacter = this.targetCharacter;
        clone.targetSlot = this.targetSlot;
        clone.characterID = this.characterID;
        clone.waitMode = this.waitMode;
        clone.waitTime = this.waitTime;

        clone.rotateRate = this.rotateRate;
        clone.moveType = this.moveType;
        clone.moveMode = this.moveMode;
        clone.moveToTargetWithTag = this.moveToTargetWithTag;

        return clone;
    }

    public MoveAction() { }

    public override void Complete()
    {
        character.mover.agent.stoppingDistance = stoppingDistance;
    }

    public override bool IsComplete()
    {
        if (base.IsTriggered() == false || !base.IsActionCommited()) return false;
        if (useWaypoints && moveType == MoveType.TRANSLATE_TO)
        {
            if (currentWaypointTarget < waypoints.Count - 1)
                return false;
        }

        if (moveType == MoveType.TRANSLATE_TO)
        {
            return Vector3.Distance(targetPos, character.GetPosition())
                < character.mover.agent.stoppingDistance * 1.2f;
        }
        else if (moveType == MoveType.ROTATE_TOWARDS)
        {
            Vector3 lookVec = character.GetForward();
            lookVec.y = 0;
            Vector3 targetVec = targetTr.position - character.GetPosition();
            targetVec.y = 0;

            return Vector3.Angle(lookVec, targetVec) < 5f;
        }
        return false;
    }

    public override void ExecuteFrame()
    {
        if (IsActionCommited() && moveType == MoveType.ROTATE_TOWARDS)
        {
            character.RotateTowardsTarget(targetTr.position, rotateRate);
        }

        if (useWaypoints)
        {
            bool arrived = Vector3.Distance(targetPos, character.GetPosition())
                < character.mover.agent.stoppingDistance * 1.2f;

            if (arrived)
            {
                currentWaypointTarget++;
                if (currentWaypointTarget < waypoints.Count)
                {
                    targetPos = waypoints[currentWaypointTarget];
                    character.mover.SetTarget(targetPos, this.moveMode);
                }
            }
        }
    }

    public override void CommitAction()
    {
        character = base.GetTargetCharacter();
        stoppingDistance = character.mover.agent.stoppingDistance;
        character.mover.agent.stoppingDistance = 0.5f;

        if (useTag && !useWaypoints)
        {
            switch (moveToTargetWithTag)
            {
                case "cast":
                    targetPos = character.battleTurnManager.GetCastFromPoint();
                    break;
                case "base":
                    targetPos = character.battleTurnManager.GetBasePoint();
                    break;
                default:
                    {
                        GameObject target = GameObject.Find(moveToTargetWithTag);

                        if (target == null)
                        {
                            Complete();
                        }
                        else
                        {
                            targetTr = target.transform;
                            targetPos = targetTr.position;
                        }
                        break;
                    }
            }
        }

        if (moveType == MoveType.TRANSLATE_TO)
        {
            //CharacterAgentsManager.Instance.player.SetState(new AINeutralState(CharacterAgentsManager.Instance.player));
            character.mover.SetTarget(targetPos, this.moveMode);
        }
        else
        {

        }
    }
}
