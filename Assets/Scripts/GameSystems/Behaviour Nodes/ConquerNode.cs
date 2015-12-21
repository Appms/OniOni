using UnityEngine;
using System.Collections;
using BehaviourMachine;

public class ConquerNode : ActionNode
{
    Peloton peloton;

    // Called when the owner (BehaviourTree or ActionState) is enabled
    public override void OnEnable()
    {
        base.OnEnable();
        peloton = this.self.gameObject.GetComponent<Peloton>();
    }

    // Called when the node starts its execution
    public override void Start()
    {
        //Start praying animation
        peloton.state = Names.STATE_CONQUER;
    }

    // This function is called when the node is in execution
    public override Status Update()
    {
        if (peloton.name == Names.PLAYER_PELOTON && peloton.targetElement.GetComponent<Totem>().alignment == 50)
            return Status.Success;
        else if (peloton.name == Names.ENEMY_PELOTON && peloton.targetElement.GetComponent<Totem>().alignment == -50)
            return Status.Success;

        return Status.Running;
    }
}