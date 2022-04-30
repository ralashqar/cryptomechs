using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NarrativeInteractionState : State
{
    public NarrativeInteractionState (CharacterAgentController character) : base(character)
    {

    }

    public override IEnumerator MainState()
    {
        yield return null;
        yield break;
        bool isActionSequenceRunning = CharacterController.narrativesManager.activeSequence != null;

        if (!isActionSequenceRunning) CharacterController.mover.SetTarget(CharacterController.GetPosition());

        while (CharacterController.narrativesManager.IsNarrativeInteractionActive)
        {
            isActionSequenceRunning = CharacterController.narrativesManager.activeSequence != null;
            if (!isActionSequenceRunning && CharacterController.targetObject != null)
            {
                // aim towards target
                CharacterController.RotateTowardsTarget(CharacterController.targetObject.GetPosition(), 4f);

                //TODO: do this in a new NPC state
                CharacterController.narrativesManager.InteractingNPC?.RotateTowardsTarget(CharacterController.GetPosition(), 4f);
            }
            
            CharacterController.narrativesManager?.Tick();

            yield return null;
        }

        this.CharacterController.SetState(new PlayerControlState(CharacterController));
        yield break;
    }
}
