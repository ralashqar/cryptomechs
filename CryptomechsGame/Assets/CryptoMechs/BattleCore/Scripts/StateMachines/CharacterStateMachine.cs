using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterStateMachine : BaseUnit
{
    protected State State;
    private Coroutine stateRoutine;

    protected List<Coroutine> AdditiveStatesRoutines;

    public void SetState(State state)
    {
        this.State = state;
        if (stateRoutine != null) StopCoroutine(stateRoutine);
        stateRoutine = StartCoroutine(State.MainState());
    }

    public void AddState(State state)
    {
        StartCoroutine(state.MainState());
    }

    public void TerminateStateMachine()
    {
        if (stateRoutine != null) StopCoroutine(stateRoutine);
    }
}
