using UnityEngine;
using System.Collections;
using BehaviourMachine;

public class LeaderObjectiveCondition : ConditionNode
{
    public string objective;
    EnemyLeader leader;

    public override void OnEnable()
    {
        base.OnEnable();
        leader = this.self.gameObject.GetComponent<EnemyLeader>();
    }

    public override Status Update()
    {
        if (leader.objective == objective)
        {
            // Send event?
            if (onSuccess.id != 0)
                owner.root.SendEvent(onSuccess.id);

            // Update status
            return Status.Success;
        }
        else
        {
            // Update status
            return Status.Failure;
        }
    }
}