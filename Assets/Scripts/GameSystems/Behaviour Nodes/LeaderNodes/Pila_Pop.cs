using UnityEngine;
using System.Collections;
using BehaviourMachine;

public class Pila_Pop : ActionNode {

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
        if(leader.currentStrategy.plan.Count == 0)
        {
            leader.currentStrategy = leader.PlanStrategy();
        }
        else
        {
            Strategy potentialBetterStrategy = leader.PlanStrategy();
            if (potentialBetterStrategy.determination > leader.currentStrategy.determination + leader.focusThreshold)
                leader.currentStrategy = potentialBetterStrategy;
        }

        leader.currentTactic = leader.currentStrategy.plan.Pop();
        

        return Status.Success;
    }
}
