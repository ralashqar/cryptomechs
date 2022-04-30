using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class State
{
    protected CharacterAgentController CharacterController;

    protected State(CharacterAgentController characterController)
    {
        CharacterController = characterController;
    }

    public virtual IEnumerator MainState()
    {
        yield break;
    }

    public virtual IEnumerator Main()
    {
        yield break;
    }

    public virtual IEnumerator End()
    {
        yield break;
    }

}
