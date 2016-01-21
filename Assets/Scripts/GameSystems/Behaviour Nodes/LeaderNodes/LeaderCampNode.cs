using UnityEngine;
using System.Collections;
using BehaviourMachine;

public class LeaderCampNode : ActionNode {
	
	EnemyLeader leader;
	Camp camp;
	
	// Use this for initialization
	public override void OnEnable()
	{
		base.OnEnable();
		leader = this.self.gameObject.GetComponent<EnemyLeader>();
	}

	public override void Start()
	{
		camp = leader.currentTactic.targetElement.GetComponent<Camp>();
	}
	
	public override Status Update()
	{
		if (camp.units.Count == 0)
		{
            return Status.Success;
        }

		leader.Move((camp.units[0].transform.position - leader.transform.position).normalized.x,
		            (camp.units[0].transform.position - leader.transform.position).normalized.z);

        leader.myPeloton.SetStateAndTarget(Names.STATE_ATTACK_CAMP, leader.currentTactic.targetElement);


        return Status.Running;
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
