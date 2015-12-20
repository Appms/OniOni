using UnityEngine;
using System.Collections;
using BehaviourMachine;

public class LeaderGoTo : ActionNode
{
    // Called when the owner (BehaviourTree or ActionState) is enabled
    public override void OnEnable()
    {
        base.OnEnable();
    }

    // Called when the node starts its execution
    public override void Start()
    {
        
    }

    // This function is called when the node is in execution
    public override Status Update()
    {
        return Status.Success;
    }
}
