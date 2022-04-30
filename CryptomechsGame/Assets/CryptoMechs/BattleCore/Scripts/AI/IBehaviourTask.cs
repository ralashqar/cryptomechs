using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IBehaviourTask
{
    CharacterAgentController AICharacterAgent { get; set; }
    bool CanExecute();
    void Execute();
    void OnSuccess();
    void TriggerTask();
    bool IsCompleted();
    IBehaviourTask Clone();

}
