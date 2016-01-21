using UnityEngine;
using System.Collections;
using BehaviourMachine;

public class LeaderAttackDoorNode : ActionNode
{
	EnemyLeader leader;
	GameObject door;
	
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
        if (leader.currentTactic.targetElement == null)
            return Status.Success;

        door = GameObject.Find(Names.PLAYER_DOOR);
		
		if (!door.GetComponent<Door>().doorsUp) return Status.Success;
		leader.state = Names.STATE_ATTACK_DOOR;
        leader.myPeloton.SetStateAndTarget(Names.STATE_ATTACK_DOOR, door);

        return Status.Running;
	}
}