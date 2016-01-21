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
        AIManager.staticManager.AddEnemyPeloton(myPeloton);  //Avisar al AIManager
        //gameObject.AddComponent<LeaderMovement>(); // NOT THIS
        //StateMachineMock();
    }

    override public void Update()
    {
        if (goingTo)
            CheckMovementToTarget();
        if (InPosition())
            InteractWithTarget();
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


        //Cuadratic Ponder -> a^2 + 2ab + b^2

        Debug.Log(chosenStrategy.plan.Peek().targetElement);

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
            SimplifyPath();
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
            /*velocity = (waypoint - transform.position).normalized * GetMaxVel();
            transform.position += velocity * Time.deltaTime;*/
            Move((waypoint.x - transform.position.x), (waypoint.z - transform.position.z));

            if (Vector3.Distance(transform.position, waypoint) < 5f)
            {
                path.RemoveAt(0);
                SimplifyPath();
                if (path.Count > 0) waypoint = new Vector3(path[0].x, 0f, path[0].y);
            }
        }
    }


    private void SimplifyPath()
    {
        if (path.Count >= 2)
        {
            RaycastHit hit = new RaycastHit();
            Vector3 nextPoint = new Vector3(path[1].x, 0.5f, path[1].y);

            while (!Physics.Raycast(transform.position, nextPoint - transform.position, out hit, Vector3.Distance(transform.position, nextPoint), LayerMask.GetMask("Level")))
            {
                path.RemoveAt(0);

                if (path.Count < 2) break;

                hit = new RaycastHit();
                nextPoint = new Vector3(path[1].x, transform.position.y, path[1].y);
            }
        }
    }

    private void StateMachineMock()
    {
        if(currentStrategy == null || currentStrategy.plan.Count == 0)
        {
            currentStrategy = PlanStrategy();
        }
        if(currentTactic == null)
        {
            currentTactic = currentStrategy.plan.Pop();
        }
        if (!InPosition())
        {
            SearchPathToTarget();
        }
        else
        {
            InteractWithTarget();
        }
    }

    private void CheckMovementToTarget()
    {
        FollowPath();
        if (Vector3.Distance(currentTactic.targetElement.transform.position, currentTactic.targetPosition) > 10f)
        {
            currentTactic.targetPosition = currentTactic.targetElement.transform.position;
            SearchPathToTarget();
        }
        if (InPosition())
        {
            InteractWithTarget();
        }
    }

    private void InteractWithTarget()
    {
        switch (currentTactic.targetElement.name)
        {
            case Names.TOTEM:
                if (!ReallyNear())
                    Move(currentTactic.targetElement.transform.position.x - transform.position.x, currentTactic.targetElement.transform.position.z - transform.position.z);
                if (currentTactic.targetElement.GetComponent<Totem>().alignment == -50)
                    StateMachineMock();
                break;
            case "Fruit":
                NewOrder(myPeloton.Size(), AIManager.staticManager.GetFruit());
                StateMachineMock();
                break;
            case Names.PLAYER_DOOR:
                if (Random.value > 0.5f)
                    Move(currentTactic.targetElement.transform.position.x - transform.position.x, currentTactic.targetElement.transform.position.z - transform.position.z);
                else
                {
                    NewOrder(myPeloton.Size(), AIManager.staticManager.orangeDoor.gameObject);
                    StateMachineMock();
                }
                break;
            case Names.CAMP:
                Move(currentTactic.targetElement.transform.position.x - transform.position.x, currentTactic.targetElement.transform.position.z - transform.position.z);
                if (currentTactic.targetElement.GetComponent<Camp>().numberOfUnits == 0)
                    StateMachineMock();
                break;
            case Names.ENEMY_PELOTON:
                currentTactic.targetElement.GetComponent<Peloton>().SetObjective(Names.OBJECTIVE_FOLLOW_LEADER, gameObject);
                StateMachineMock();
                break;
            case Names.PLAYER_PELOTON:
                Peloton peloton = currentTactic.targetElement.GetComponent<Peloton>();
                Move((peloton.transform.position - transform.position).normalized.x, (peloton.transform.position - transform.position).normalized.z);
                if (peloton == null || peloton.Size() == 0)
                    StateMachineMock();
                break;
            case Names.PLAYER_LEADER_PELOTON:
                Peloton pelotonL = currentTactic.targetElement.GetComponent<Peloton>();
                Move((pelotonL.transform.position - transform.position).normalized.x, (pelotonL.transform.position - transform.position).normalized.z);
                if (pelotonL == null || pelotonL.Size() == 0)
                    StateMachineMock();
                break;
            case Names.PLAYER_LEADER:
                PlayerLeader pLeader = currentTactic.targetElement.GetComponent<PlayerLeader>();
                Move((pLeader.transform.position - transform.position).normalized.x, (pLeader.transform.position - transform.position).normalized.z);
                if (pLeader.health <= 0)
                    StateMachineMock();
                break;
        }
    }


    private bool InPosition()
    {
        if (currentTactic == null || currentTactic.targetElement == null)
            return false;
        if (Vector3.Distance(transform.position, currentTactic.targetElement.transform.position) > 10f)
            return false;
        return true;
    }
    private bool ReallyNear()
    {
        if (Vector3.Distance(transform.position, currentTactic.targetElement.transform.position) > 1f)
            return false;
        return true;
    }
}
