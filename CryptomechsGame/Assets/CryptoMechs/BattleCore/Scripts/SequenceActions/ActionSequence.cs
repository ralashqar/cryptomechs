using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;
using System.Linq;

[Sirenix.OdinInspector.ShowOdinSerializedPropertiesInInspector]
public class ActionSequence
{
    public List<ICharacterAction> actions;
    private int activeActionIndex = 0;
    private ICharacterAction activeAction;
    private float sequenceStartTime = 0;

    private List<List<ICharacterAction>> splitSequences;
    private List<ICharacterAction> activeActions;

    // Update is called once per frame
    public void Trigger()
    {
        /*
        activeAction = actions[0];
        activeActionIndex = 0;
        sequenceStartTime = Time.time;
        activeAction?.Trigger(sequenceStartTime);
        */
        sequenceStartTime = Time.time;

        splitSequences = new List<List<ICharacterAction>>();
        List<ICharacterAction> seq = new List<ICharacterAction>();
        for (int i = 0; i < actions.Count; ++i)
        {
            CharacterActionBase a = actions[i] as CharacterActionBase;
            if (a.waitMode != CharacterActionWaitType.TimeFromStart)
            {
                seq.Add(a);
            }
            else
            {
                splitSequences.Add(seq);
                seq = new List<ICharacterAction>();
                seq.Add(a);
            }
        }
        splitSequences.Add(seq);

        activeActions = new List<ICharacterAction>();

        for (int i = 0; i < splitSequences.Count; ++i)
        {
            var a = splitSequences[i][0];
            activeActions.Add(a);
            a?.Trigger(sequenceStartTime);
        }
    }

    public bool Execute()
    {
        for (int i = 0; i < splitSequences.Count; ++i)
        {
            var a = activeActions[i];
            a?.Tick();
            if (a.IsComplete())
            {
                int index = splitSequences[i].IndexOf(a);
                if (index + 1 >= splitSequences[i].Count)
                {
                    splitSequences.RemoveAt(i);
                    i--;
                }
                else
                {
                    activeActions[i] = splitSequences[i][index + 1];
                    activeActions[i]?.Trigger(sequenceStartTime);
                }
            }
        }

        if (splitSequences.Count == 0)
        {
            Complete();
            return true;
        }
        
        return false;

        /*
        activeAction?.Tick();
        if (activeAction.IsComplete())
        {
            activeActionIndex++;
            if (activeActionIndex >= actions.Count)
            {
                Complete();
            }
            else
            {
                activeAction = actions[activeActionIndex];
                activeAction?.Trigger(sequenceStartTime);
            }
        }
        */
    }

    public void Complete()
    {

    }
}
