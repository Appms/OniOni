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
        /*if(status != Status.Running && (leader.currentStrategy == null || leader.currentStrategy.plan == null || leader.currentStrategy.plan.Count == 0))
        {
            leader.currentStrategy = leader.PlanStrategy();
        }
        else
        {
            Strategy potentialBetterStrategy = leader.PlanStrategy();
            if (potentialBetterStrategy != null && leader.currentStrategy != null && potentialBetterStrategy.determination > leader.currentStrategy.determination + leader.focusThreshold)
                leader.currentStrategy = potentialBetterStrategy;
        }*/

        if (leader.currentStrategy == null || leader.currentStrategy.plan.Count == 0)
            leader.currentStrategy = leader.PlanStrategy();

        if (leader.currentTactic != leader.currentStrategy.plan.Peek())
        {
            leader.currentTactic = leader.currentStrategy.plan.Peek();
            Debug.Log(leader.currentTactic.targetElement.name);
        }
    }

    // This function is called when the node is in execution
    public override Status Update()
    {
        
        

        return Status.Success;
    }
}
