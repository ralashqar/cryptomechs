using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AINeutralState : State
{
    public AINeutralState(CharacterAgentController characterController) : base(characterController)
    {

    }
    public override IEnumerator MainState()
    {
        while (true)
        {            
            //CharacterController.data.aiBehaviour.AISequences.ForEach(x => { if (x.CanExecute()) x.Execute(); });
            yield return null;
        }
    }
}
