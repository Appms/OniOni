using UnityEngine;
using System.Collections;
using BehaviourMachine;

public class LeaderRecruitNode : ActionNode {

	EnemyLeader leader;

	// Use this for initialization
	public override void OnEnable()
	{
		base.OnEnable();
		leader = this.self.gameObject.GetComponent<EnemyLeader>();
	}
	
	public override Status Update()
	{
        if (leader.currentTactic.targetElement == null)
            return Status.Success;

        leader.currentTactic.targetElement.GetComponent<Peloton>().SetObjective(Names.OBJECTIVE_FOLLOW_LEADER, leader.gameObject);
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
