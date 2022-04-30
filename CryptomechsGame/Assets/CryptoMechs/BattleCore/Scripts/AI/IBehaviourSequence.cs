using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;
using System.Linq;

[Sirenix.OdinInspector.ShowOdinSerializedPropertiesInInspector]
public class IBehaviourSequence
{
    public CharacterAgentController AICharacterAgent;

    public static IBehaviourTask GetAITask<T>(CharacterAgentController AICharacterAgent) where T : IBehaviourTask
    {
        var seq = AICharacterAgent.aiBehavior.AISequences.Find(s=>s.tasks.Exists(t=> t is T));
        if (seq != null)
        {
            var a = seq.tasks.Find(t => t is T);
            return a;
        }
        return null;
    }

    public IBehaviourSequence Clone()
    {
        IBehaviourSequence clone = new IBehaviourSequence();
        clone.conditions = this.conditions;
        clone.tasks = new List<IBehaviourTask>();
        foreach (IBehaviourTask t in tasks)
        {
            clone.tasks.Add(t.Clone());
        }
        clone.priority = this.priority;
        return clone;
    }

    public void SetCharacterAgentController(CharacterAgentController AICharacterAgent)
    {
        this.AICharacterAgent = AICharacterAgent;
        tasks.ForEach(t => { t.AICharacterAgent = AICharacterAgent; });
    }

    public List<IBehaviourCondition> conditions;
    public List<IBehaviourTask> tasks;
    public int priority;

    private IBehaviourTask activeTask = null;
    private bool IsExecuting { get { return activeTask != null; } }

    int activeTaskID = 0;

    public void TriggerTask (IBehaviourTask task)
    {
        activeTask = task;
        task?.TriggerTask();
    }

    public bool CanExecute()
    {
        bool canExecute = true;
        if (conditions == null) return true;
        foreach(IBehaviourCondition condition in conditions)
        {
            if (!condition.ConditionSatisfied())
            {
                canExecute = false;
                break;
            }
        }

        return canExecute;
    }

    public void ExitSequence()
    {
        activeTask = null;
    }

    public void Execute()
    {
        if (!IsExecuting && tasks != null && tasks.Count > 0)
        {
            if (CanExecute())
            {
                activeTaskID = 0;
                TriggerTask(tasks[0]);
            }
        }
        activeTask?.Execute();
        return;
        if (activeTask != null)
        {
            activeTask?.Execute();
            if (activeTask.IsCompleted() || !activeTask.CanExecute())
            {
                activeTaskID++;
                if (activeTaskID < tasks.Count)
                {
                    TriggerTask(tasks[activeTaskID]);
                }
                else
                {
                    activeTask = null;
                }
            }
        }
    }
}
