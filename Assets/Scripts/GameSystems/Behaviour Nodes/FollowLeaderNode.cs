using UnityEngine;
using System.Collections;
using BehaviourMachine;

public class FollowLeaderNode : ActionNode
{
    Peloton peloton;

    // Called once when the node is created
    public virtual void Awake() { }

    // Called when the owner (BehaviourTree or ActionState) is enabled
    public override void OnEnable()
    {

        base.OnEnable();
        peloton = this.self.gameObject.GetComponent<Peloton>();
    }

    // Called when the node starts its execution
    public override void Start()
    {

    }

    // This function is called when the node is in execution
    public override Status Update()
    {
        peloton.state = Names.STATE_FOLLOW_LEADER;
        peloton.FollowLeader();
        return Status.Success;
    }

    // Called when the node ends its execution
    public override void End()
    {
    }

    // Called when the owner (BehaviourTree or ActionState) is disabled
    public override void OnDisable() { }

    // This function is called to reset the default values of the node
    public override void Reset() { }

    // Called when the script is loaded or a value is changed in the inspector (Called in the editor only)
    public override void OnValidate() { }
}