using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using Sirenix.OdinInspector.Demos.RPGEditor;

public class AutoCastState : State
{
    public MechPartCaster caster;
    
    public AutoCastState(CharacterAgentController characterController, MechPartCaster caster) : base(characterController)
    {
        this.caster = caster;
        this.castAI = new AutoCastAI(characterController, caster);
    }

    private AutoCastAI castAI;
    
    public override IEnumerator MainState()
    {
        while (caster != null)// && caster.canAutoCast)
        {
            //if (this.castAI.CanExecute())
            //{
            this.castAI.Execute();
            //}
            yield return null;
        }
        //CharacterController.IsCasting = false;
        yield break;
    }
}
