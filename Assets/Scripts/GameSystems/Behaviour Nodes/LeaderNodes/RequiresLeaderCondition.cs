using UnityEngine;
using System.Collections;
using BehaviourMachine;

public class RequiresLeaderCondition : ConditionNode {

    EnemyLeader leader;

    public override void OnEnable()
    {
        base.OnEnable();
        leader = this.self.gameObject.GetComponent<EnemyLeader>();
    }

    public override Status Update()
    {
        if (leader.currentTactic.requiresLeader) return Status.Success;
        else return Status.Failure;
    }
}
