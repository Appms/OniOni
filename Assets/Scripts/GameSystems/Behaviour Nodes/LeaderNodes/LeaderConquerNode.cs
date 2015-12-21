using UnityEngine;
using System.Collections;
using BehaviourMachine;

public class LeaderConquerNode : ActionNode
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
		//Start praying animation
		leader.state = Names.STATE_CONQUER;
	}
	
	// This function is called when the node is in execution
	public override Status Update()
	{
		if (leader.name == Names.PLAYER_PELOTON && leader.currentTactic.targetElement.GetComponent<Totem>().alignment == 50)
			return Status.Success;
		else if (leader.name == Names.ENEMY_PELOTON && leader.currentTactic.targetElement.GetComponent<Totem>().alignment == -50)
			return Status.Success;
		
		return Status.Running;
	}
}