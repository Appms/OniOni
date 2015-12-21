using UnityEngine;
using System.Collections;
using BehaviourMachine;

public class ChangeObjectiveNode : ActionNode
{
    Peloton peloton;
    public string objective;


    // Called when the owner (BehaviourTree or ActionState) is enabled
    public override void OnEnable()
    {

        base.OnEnable();
        peloton = this.self.gameObject.GetComponent<Peloton>();
    }

    // Called when the node starts its execution
    public override void Start()
    {

    }

    // This function is called when the node is in execution
    public override Status Update()
    {
        switch (objective)
        {
            case Names.OBJECTIVE_DEFEND:
                peloton.SetObjective(objective);
                break;

            case Names.OBJECTIVE_PUSH:
                peloton.SetObjective(objective, GameObject.Find("Fruit"));
                break;

            case Names.OBJECTIVE_ATTACK_DOOR:
                peloton.SetObjective(objective, (peloton.leader.name == Names.PLAYER_LEADER ? GameObject.Find(Names.ENEMY_DOOR) : GameObject.Find(Names.PLAYER_DOOR)));
                break;

            default:
                peloton.SetObjective(objective);
                break;
        }
        
        return Status.Success;
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