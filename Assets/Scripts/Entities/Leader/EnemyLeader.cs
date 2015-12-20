using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class EnemyLeader : Leader {

    Vector3 lastPosition = new Vector3();
    Strategy currentStrategy;


	override public void Start () {
        base.Start();
        aiManager.AddEnemyPeloton(myPeloton);  //Avisar al AIManager
        //gameObject.AddComponent<LeaderMovement>(); // NOT THIS
    }

    override public void Update()
    {

    }

    override public void FixedUpdate () {
        base.FixedUpdate();
        //velocity = GetComponent<LeaderMovement>().velocity;
        velocity = transform.position - lastPosition;
        lastPosition = transform.position;
    }

    private void PlanStrategy()
    {
        List<Strategy> options = AIManager.staticManager.GetAIStrategies();
        //Cuadratic Ponder -> a^2 + 2ab + b^2
    }
}
