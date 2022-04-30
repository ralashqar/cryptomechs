using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public interface IMoveAgent
{
    Vector3 GetCurrentPosition();
    bool GetCanMove();
    void SetCanMove(bool canMove);
    bool IsMoving();

    void SetSpeed(float newSpeed);
    float GetSpeed();

    void SetVelocity(Vector3 newVelocity);
    Vector3 GetVelocity();

    void SetTarget(Vector3 target, MoveMode mode);
    Vector3 GetTarget();

    void MoveToTarget();
}

public enum MoveMode
{
    DEFAULT,
    BACKWARDS
}

[System.Serializable]
public class AgentMover : IMoveAgent
{
    [HideInInspector]
    public NavMeshAgent agent;
    [HideInInspector]
    public Transform target;
    [HideInInspector]
    public CharacterAgentController cachedCharacter;

    private Vector3 currentPosition;
    private bool isMoving = false;
    private bool canMove = true;
    private Vector3 targetPosition;
    private MoveMode mode;

    private Coroutine bashRoutine;

    public bool IsEncumbered { get; private set; } = false;
    
    public void ForcePosition(Vector3 newTarget)
    {
        agent.SetDestination(newTarget);
        agent.transform.position = newTarget;
        this.targetPosition = newTarget;
    }

    public IEnumerator BashRoutine(Vector3 target, float time = 1f)
    {
        IsEncumbered = true;
        float elapsedTime = 0;
        agent.updateRotation = true;
        float fraction = 0;
        yield return null;
        Vector3 initPos = agent.transform.position;
        while (elapsedTime < time)
        {
            fraction = Mathf.Clamp01(elapsedTime / time);
            elapsedTime += Time.deltaTime;
            Vector3 newTarget = initPos * (1 - fraction) + target * fraction;
            newTarget.y = agent.transform.position.y;
            agent.SetDestination(newTarget);
            agent.transform.position = newTarget;
            this.targetPosition = newTarget;
            yield return null;
        }
        IsEncumbered = false;
    }

    public IEnumerator BashRoutine(Vector3 direction, float moveRate, float time = 1f)
    {
        IsEncumbered = true;
        float elapsedTime = 0;
        agent.updateRotation = true;
        while(elapsedTime < time)
        {
            elapsedTime += Time.deltaTime;
            Vector3 newTarget = agent.transform.position + direction.normalized * moveRate * Time.deltaTime;
            agent.SetDestination(newTarget);
            agent.transform.position = newTarget;
            this.targetPosition = newTarget;
            yield return null;
        }
        IsEncumbered = false;
    }

    public void BashToTarget(Vector3 target, float time)
    {
        if (bashRoutine != null) cachedCharacter.StopCoroutine(bashRoutine);
        bashRoutine = cachedCharacter.StartCoroutine(BashRoutine(target, time));
    }

    public void Bash(Vector3 direction, ImpactDefinition impact)
    {
        float moveRate = impact.impactValue;
        float time = impact.timeOfEffect;
        //if (!IsEncumbered)
        if (bashRoutine != null) cachedCharacter.StopCoroutine(bashRoutine);
        bashRoutine = cachedCharacter.StartCoroutine(BashRoutine(direction, moveRate, time));
    }

    public void SetSpeed(float speed)
    {
        agent.speed = speed;
    }

    public float GetSpeed()
    {
        return GetVelocity().magnitude;
    }

    public void SetVelocity(Vector3 vel)
    {
        agent.velocity = vel;
    }

    public Vector3 GetVelocity()
    {
        return agent.velocity;
    }

    public bool GetCanMove()
    {
        return agent.updatePosition;
    }

    public void SetCanMove(bool canMove)
    {
        agent.updatePosition = canMove;
    }

    public void SetCanRotate(bool canRotate)
    {
        agent.updateRotation = canRotate;
    }

    public bool IsAtTarget()
    {
        return Vector3.Distance(this.targetPosition, this.cachedCharacter.GetPosition())
                < this.agent.stoppingDistance * 1.2f;
    }
    
    public bool IsMoving()
    {
        return agent.speed > 0.1f;
        return true;
    }

    public void SetTarget(Vector3 target, MoveMode mode = MoveMode.DEFAULT)
    {
        if (mode == MoveMode.BACKWARDS && Vector3.Distance(target, this.agent.transform.position) < 0.1f)
            return;

        mode = MoveMode.DEFAULT;
        this.mode = mode;
        this.agent.updateRotation = mode == MoveMode.DEFAULT;

        var legsAnimator = this.cachedCharacter.mech.GetCasterBySlot(MechSlot.LEGS).animator;
        if (legsAnimator != null)
        {
            float speed = mode == MoveMode.BACKWARDS ? -1f : 1f;
            legsAnimator.SetFloat("speed", speed);
        }

        this.agent.updateRotation = mode == MoveMode.DEFAULT;

        if (IsEncumbered)
            return;

        agent.SetDestination(target);
        this.targetPosition = target;
    }

    public Vector3 GetTarget()
    {
        if (target != null)
            return target.position;
        return targetPosition;
    }

    public void MoveToTarget()
    {
        if (target != null)
        {
            agent.SetDestination(target.position);
        }
    }

    public Vector3 GetCurrentPosition()
    {
        return agent.transform.position;
    }

}
