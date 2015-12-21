using UnityEngine;
using System.Collections;
using BehaviourMachine;

public class LeaderObjectiveCondition : ConditionNode
{
    public string objective;
    EnemyLeader leader;

    public override void OnEnable()
    {
        base.OnEnable();
        leader = this.self.gameObject.GetComponent<EnemyLeader>();
    }

    public override Status Update()
    {
        if (leader.currentTactic.targetElement == null) {
            return Status.Failure;
        }

        /*if (leader.objective == objective)
        {
            // Send event?
            if (onSuccess.id != 0)
                owner.root.SendEvent(onSuccess.id);

            // Update status
            return Status.Success;
        }
        else
        {
            // Update status
            return Status.Failure;
        }*/

        switch (leader.currentTactic.targetElement.name){

			case Names.TOTEM:
				if(objective == Names.OBJECTIVE_CONQUER)
					return Status.Success;
				break;
			case "Fruit":
				if(objective == Names.OBJECTIVE_PUSH)
					return Status.Success;
				break;
			case Names.PLAYER_DOOR :
				if(objective == Names.OBJECTIVE_ATTACK_DOOR)
					return Status.Success;
				break;
			case Names.CAMP:
				if(objective == Names.OBJECTIVE_ATTACK_CAMP)
					return Status.Success;
				break;
			case Names.ENEMY_PELOTON:
				if(objective == Names.RECRUIT)
					return Status.Success;
				break;

            case Names.PLAYER_PELOTON:
                if (objective == Names.OBJECTIVE_ATTACK)
                    return Status.Success;
                break;
            case Names.PLAYER_LEADER_PELOTON:
                if (objective == Names.OBJECTIVE_ATTACK)
                    return Status.Success;
                break;
            case Names.PLAYER_LEADER:
                if (objective == Names.OBJECTIVE_ATTACK_LEADER)
                    return Status.Success;
                break;
        }
		return Status.Failure;
    }
}