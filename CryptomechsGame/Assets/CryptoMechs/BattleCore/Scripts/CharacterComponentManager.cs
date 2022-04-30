using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector.Demos.RPGEditor;

public class CharacterComponentManager
{
    public CharacterAgentController character;

    public CharacterComponentManager(CharacterAgentController character)
    {
        this.character = character;
    }

    public virtual void OnDestroy()
    {

    }
}
