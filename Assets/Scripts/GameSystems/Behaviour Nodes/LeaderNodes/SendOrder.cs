using UnityEngine;
using System.Collections;
using BehaviourMachine;

public class SendOrder : ActionNode
{
    EnemyLeader leader;
    // Called when the owner (BehaviourTree or ActionState) is enabled
    public override void OnEnable()
    {
        base.OnEnable();
        leader = this.self.gameObject.GetComponent<EnemyLeader>();
    }

    // Called when the node starts its execution
    public override void Start()
    {

    }

    // This function is called when the node is in execution
    public override Status Update()
    {
        if (leader.currentTactic == null) leader.PlanStrategy();

        leader.NewOrder(leader.currentTactic.cantMinions, leader.currentTactic.targetElement);
        return Status.Success;
    }
}