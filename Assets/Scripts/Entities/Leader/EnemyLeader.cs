using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class EnemyLeader : Leader {

    Vector3 lastPosition = new Vector3();
    public Strategy currentStrategy;
    public Tactic currentTactic;
    float time = 0;
    public string objective;

    public float focusThreshold = 20f;


	override public void Start () {
        base.Start();
        aiManager.AddEnemyPeloton(myPeloton);  //Avisar al AIManager
        //gameObject.AddComponent<LeaderMovement>(); // NOT THIS
    }

    override public void Update()
    {
        time += Time.deltaTime;
        if (time >= 5f)
        {
            time = 0f;
            PlanStrategy();
        }
    }

    override public void FixedUpdate () {
        base.FixedUpdate();
        //velocity = GetComponent<LeaderMovement>().velocity;
        velocity = transform.position - lastPosition;
        lastPosition = transform.position;
    }

    public Strategy PlanStrategy()
    {
        List<Strategy> options = AIManager.staticManager.GetAIStrategies();
        Strategy chosenStrategy;
        /*float totalDetermination = 0;

        foreach (Strategy s in options)
            totalDetermination += s.determination;

        foreach (Strategy s in options)
        {
            if (Random.value * totalDetermination <= s.determination)
            {
                currentStrategy = s;
                break;
            }
            totalDetermination -= s.determination;
        }*/

        chosenStrategy = options[0];
        foreach (Strategy s in options)
            if (s.determination > chosenStrategy.determination)
                chosenStrategy = s;




        //DEBUG
        foreach (Strategy s in options)
        {
            Debug.Log("Potential Strategy: " + s.plan.ToArray()[0].targetElement.name + " - Cost: " + s.cost + " Reward: " + s.reward);
            Debug.Log("---------------------------------------------------------------------------------------------");
        }

        Debug.Log("-------------------------------------------------------------------------------------------------");
        Debug.Log("Chosen Strategy - Cost: " + currentStrategy.cost + " Reward: " + currentStrategy.reward);
        foreach (Tactic t in currentStrategy.plan)
            Debug.Log(t.targetElement.name + " id: " + t.GetHashCode());

        //Cuadratic Ponder -> a^2 + 2ab + b^2

        return chosenStrategy;
    }
}
