using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using BehaviourMachine;

public class GoToNode : ActionNode
{
    Peloton peloton;
    float MIN_DIST = 20;
    bool alreadyInPlace = false;

    // Called once when the node is created
    public virtual void Awake() { }

    // Called when the owner (BehaviourTree or ActionState) is enabled
    public override void OnEnable()
    {
        base.OnEnable();
        peloton = this.self.gameObject.GetComponent<Peloton>();
    }

    // Called when the node starts its execution
    public override void Start()
    {
        if (peloton.targetElement != null && Vector3.Distance(peloton.transform.position, peloton.targetElement.transform.position) > MIN_DIST)
        {
            peloton.SearchPathToTarget();
            peloton.state = Names.STATE_GO_TO;
        }
        else if (Vector3.Distance(peloton.transform.position, peloton.targetPosition) > MIN_DIST)
        {
            peloton.SearchPathToTarget();
            peloton.state = Names.STATE_GO_TO;
        }

        else alreadyInPlace = true;
    }

    // This function is called when the node is in execution
    public override Status Update()
    {
        if (alreadyInPlace)
            return Status.Success;

        if (peloton.targetElement != null && Vector3.Distance(peloton.targetElement.transform.position, peloton.targetPosition) > MIN_DIST)
        {
            peloton.targetPosition = peloton.targetElement.transform.position;
            peloton.SearchPathToTarget();
        }
        if (!peloton.calculatingPath)
        {
            peloton.FollowPath();
            if (!peloton.goingTo) return Status.Success;
        }
        return Status.Running;
    }

    // Called when the node ends its execution
    public override void End()
    {
        peloton.path = new List<Vector2>();
    }

    // This function is called to reset the default values of the node
    public override void Reset()
    {
        base.Reset();
        alreadyInPlace = false;
    }
}