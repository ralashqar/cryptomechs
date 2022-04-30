using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IBehaviourCondition
{
    string ConditionName { get; set; }
    bool ConditionSatisfied();
}
