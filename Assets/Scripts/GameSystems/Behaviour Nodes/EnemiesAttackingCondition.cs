using UnityEngine;
using System.Collections;
using BehaviourMachine;

public class EnemiesAttackingCondition : ConditionNode
{
    public bool pelotonBeingAttacked;
    Peloton peloton;

    public override void OnEnable()
    {
        base.OnEnable();
        peloton = this.self.gameObject.GetComponent<Peloton>();
    }

    public override Status Update()
    {
        pelotonBeingAttacked = peloton.menaces.Count > 0;
        if (pelotonBeingAttacked)
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
        }
    }

    public override void Reset()
    {
        base.Reset();

        // My Reset
        pelotonBeingAttacked = false;
    }
}