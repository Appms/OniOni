using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class EnemyLeader : Leader {

    Vector3 lastPosition = new Vector3();
    public Strategy currentStrategy;
    float time = 0;
    public string objective;


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

    private void PlanStrategy()
    {
        List<Strategy> options = AIManager.staticManager.GetAIStrategies();
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

        currentStrategy = options[0];
        foreach (Strategy s in options)
            if (s.determination > currentStrategy.determination)
                currentStrategy = s;




        //DEBUG
        foreach (Strategy s in options)
        {
            Debug.Log("Potential Strategy - Cost: " + s.cost + " Reward: " + s.reward);
            foreach (Tactic t in s.plan)
                Debug.Log(t.targetElement.name);
            Debug.Log("---------------------------------------------------------------------------------------------");
        }

        Debug.Log("---------------------------------------------------------------------------------------------");
        Debug.Log("Chosen Strategy - Cost: " + currentStrategy.cost + " Reward: " + currentStrategy.reward);
        foreach (Tactic t in currentStrategy.plan)
            Debug.Log(t.targetElement.name + " id: " + t.GetHashCode());

        //Cuadratic Ponder -> a^2 + 2ab + b^2
    }
}
