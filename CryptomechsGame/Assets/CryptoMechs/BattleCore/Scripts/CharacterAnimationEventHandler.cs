using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterAnimationEventHandler : MonoBehaviour
{
    public void ApplyMeleeImpact()
    {
        SendMessageUpwards("MeleeStrikeImpact");
    }

    public void ApplyDamage()
    {
        SendMessageUpwards("MeleeStrikeImpact");
    }

    public void TriggerCastEffect()
    {
        SendMessageUpwards("TriggerCast");
    }
}
