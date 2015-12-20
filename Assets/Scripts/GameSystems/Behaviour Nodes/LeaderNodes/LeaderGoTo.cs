using UnityEngine;
using System.Collections;
using BehaviourMachine;

public class LeaderGoTo : ActionNode
{
    EnemyLeader leader;
    float MIN_DIST = 20;
    bool alreadyInPlace = false;

    // Called when the owner (BehaviourTree or ActionState) is enabled
    public override void OnEnable()
    {
        base.OnEnable();
        leader = this.self.gameObject.GetComponent<EnemyLeader>();
    }

    // Called when the node starts its execution
    public override void Start()
    {
        if(leader.currentTactic != null && Vector3.Distance(leader.transform.position, leader.currentTactic.targetElement.transform.position) > MIN_DIST)
        {
            leader.SearchPathToTarget();
            leader.state = Names.STATE_GO_TO;
        }
        /*else if (Vector3.Distance(leader.transform.position, leader.currentTactic.targetElement.transform.position) > MIN_DIST)
        {
            leader.SearchPathToTarget();
            leader.state = Names.STATE_GO_TO;
        }*/
        else alreadyInPlace = true;
    }

    // This function is called when the node is in execution
    public override Status Update()
    {
       if (alreadyInPlace)
            return Status.Success;

       if(leader.currentTactic.targetElement != null && Vector3.Distance(leader.currentTactic.targetElement.transform.position, leader.transform.position) > MIN_DIST)
       {
            leader.SearchPathToTarget();
       }
       if (!leader.calculatingPath)
       {
            leader.FollowPath();
            if (!leader.goingTo) return Status.Success;
       }
        return Status.Running;
    }
}
