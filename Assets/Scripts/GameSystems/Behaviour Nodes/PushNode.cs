using UnityEngine;
using System.Collections;
using BehaviourMachine;

public class PushNode : ActionNode
{
    Peloton peloton;
    Fruit fruit;

    // Called when the owner (BehaviourTree or ActionState) is enabled
    public override void OnEnable()
    {
        base.OnEnable();
        peloton = this.self.gameObject.GetComponent<Peloton>();
        fruit = GameObject.Find("Fruit").GetComponent<Fruit>();
    }

    // Called when the node starts its execution
    public override void Start()
    {

    }

    // This function is called when the node is in execution
    public override Status Update()
    {
        peloton.state = Names.STATE_PUSH;
        peloton.PushFuit(peloton);

        if((peloton.leader.name == Names.PLAYER_LEADER && !fruit.canAdvanceToPurple) || (peloton.leader.name == Names.ENEMY_LEADER && !fruit.canAdvanceToOrange))
        return Status.Success;

        return Status.Running;
    }

    // Called when the node ends its execution
    public override void End()
    {

    }
}