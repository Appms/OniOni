using UnityEngine;
using System.Collections;
using BehaviourMachine;

public class RequiresLeaderCondition : ConditionNode {

    public override void OnEnable()
    {
        base.OnEnable();
    }

    public override Status Update()
    {
        // Update status
        return Status.Success;
    }
}
