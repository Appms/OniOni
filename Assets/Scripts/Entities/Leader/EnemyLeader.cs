using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class EnemyLeader : Leader {

    Vector3 lastPosition = new Vector3();
    public Strategy currentStrategy;
    public Tactic currentTactic;
    float time = 0;

    public bool calculatingPath = false;
    public bool goingTo = false;
    public List<Vector2> path = new List<Vector2>();

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
        Debug.Log("-------------------------------------------------------------------------------------------------");
		if (chosenStrategy != null){
        	Debug.Log("Chosen Strategy: - Cost: " + chosenStrategy.cost + " Reward: " + chosenStrategy.reward);
        	foreach (Tactic t in chosenStrategy.plan)
				Debug.Log(t.targetElement.name + " id: " + t.GetHashCode());
		}

        //Cuadratic Ponder -> a^2 + 2ab + b^2

        return chosenStrategy;
    }

    public void SearchPathToTarget()
    {
        SearchPath(currentTactic.targetElement.transform.position);
    }

    private void SearchPath(Vector3 targetPosition)
    {
        calculatingPath = true;
        //goingTo = true;
        JPSManager.RequestPath(transform.position, targetPosition, OnPathFound);
    }

    private void OnPathFound(List<Vector2> newPath, bool pathSuccessful)
    {
        if (pathSuccessful)
        {
            path = newPath;
            calculatingPath = false;
            goingTo = true;
            //StopCoroutine("FollowPath");
            //StartCoroutine("FollowPath");
        }
    }
    public void FollowPath()
    {
        if (path.Count == 0) goingTo = false;
        else
        {
            Vector3 waypoint = new Vector3(path[0].x, 0f, path[0].y);
            velocity = (waypoint - transform.position).normalized * GetMaxVel();
            transform.position += velocity * Time.deltaTime;

            if (Vector3.Distance(transform.position, waypoint) < 0.5f)
            {
                path.RemoveAt(0);
                if (path.Count > 0) waypoint = new Vector3(path[0].x, 0f, path[0].y);
            }
        }
    }

}
