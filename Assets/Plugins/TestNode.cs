using UnityEngine;
using System.Collections;
using BehaviourMachine;

public class CustomNode : ActionNode
{
    public int potato;

    // Called once when the node is created
    public virtual void Awake() { }

    // Called when the owner (BehaviourTree or ActionState) is enabled
    public override void OnEnable() { }

    // Called when the node starts its execution
    public override void Start() { }

    // This function is called when the node is in execution
    public override Status Update()
    {
        // Do stuff

        // Never forget to set the node status
        return Status.Running;
    }

    // Called when the node ends its execution
    public override void End() { }

    // Called when the owner (BehaviourTree or ActionState) is disabled
    public override void OnDisable() { }

    // This function is called to reset the default values of the node
    public override void Reset() { }

    // Called when the script is loaded or a value is changed in the inspector (Called in the editor only)
    public override void OnValidate() { }
}