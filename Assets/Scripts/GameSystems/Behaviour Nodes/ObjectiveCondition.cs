using UnityEngine;
using System.Collections;
using BehaviourMachine;

public class ObjectiveCondition : ConditionNode
{
    public string objective;
    Peloton peloton;

    public override void OnEnable()
    {
        base.OnEnable();
        peloton = this.self.gameObject.GetComponent<Peloton>();
    }

    public override Status Update()
    {
        if (peloton.objective == objective)
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
}