using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WaitAction : CharacterActionBase
{
    private float waitTimer = 1f;

    public WaitAction() { }

    public WaitAction(float waitTime = 1f)
    {
        this.waitTime = waitTime;
    }

    public override void Complete()
    {
    }

    public override bool IsComplete()
    {
        if (!base.IsTriggered()) return false;
        return waitTimer < 0;
    }

    public override void ExecuteFrame()
    {
        waitTimer -= Time.deltaTime;
    }

    public override void CommitAction()
    {
        waitTimer = waitTime;
    }
}
