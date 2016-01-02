using UnityEngine;
using System.Collections;
using BehaviourMachine;

public class Retrete : ActionNode
{
    EnemyLeader leader;
    public override void OnEnable()
    {
        leader = this.self.gameObject.GetComponent<EnemyLeader>();
    }

    public override Status Update()
    {
        if(leader.currentTactic == null || leader.currentTactic.targetElement == null)
        {
            leader.currentStrategy.plan.Pop();
            return Status.Success;
        }

        return Status.Failure;
    }
}
