using UnityEngine;
using System.Collections;
using BehaviourMachine;

public class LeaderAttackNode : ActionNode
{

    EnemyLeader leader;
    Peloton peloton;

    // Use this for initialization
    public override void OnEnable()
    {
        base.OnEnable();
        leader = this.self.gameObject.GetComponent<EnemyLeader>();
    }

    public override void Start()
    {
        peloton = leader.currentTactic.targetElement.GetComponent<Peloton>();
    }

    public override Status Update()
    {
        if (peloton == null || peloton.Size() == 0)
            return Status.Success;

        leader.Move((peloton.transform.position - leader.transform.position).normalized.x,
                    (peloton.transform.position - leader.transform.position).normalized.z);

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
