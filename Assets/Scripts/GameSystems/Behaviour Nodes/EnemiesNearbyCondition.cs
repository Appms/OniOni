using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using BehaviourMachine;

public class EnemiesNearbyCondition : ConditionNode
{
    Peloton peloton;
    List<Peloton> nearbyPelotons;

    public override void OnEnable()
    {
        base.OnEnable();
        peloton = this.self.gameObject.GetComponent<Peloton>();
    }

    public override Status Update()
    {
        nearbyPelotons = AIManager.staticManager.GetNearbyEnemies(peloton);
        if (nearbyPelotons.Count > 0 && peloton.state != Names.STATE_ATTACK && peloton.state != Names.STATE_CONQUER && peloton.state != Names.STATE_PUSH)
        {
            // sort list by peloton size
            nearbyPelotons.Sort((c1, c2) => (int)(c1.Size() - c2.Size()));
            if(nearbyPelotons[0].Size() * 2f < peloton.Size())
            {
                //assign victim to peloton for attack
                peloton.victims.Add(nearbyPelotons[0]);
                if (onSuccess.id != 0)
                    owner.root.SendEvent(onSuccess.id);

                return Status.Success;
            }
        }
        return Status.Failure;
    }
}