using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AICombatState : State
{
    public AICombatState(CharacterAgentController characterController) : base(characterController)
    {

    }

    public override IEnumerator MainState()
    {
        yield return null; // wait a frame first;

        while (true)
        {            
            while (GameManager.Instance.mode != GameMode.REALTIME_BATTLE)
            {
                yield return null;
            }

            if (CharacterController.IsAlive() && !CharacterController.IsIncapacitated())
                CharacterController.aiBehavior.AISequences.ForEach(x => { if (x.CanExecute()) x.Execute(); });
            yield return null;
        }
    }
}
