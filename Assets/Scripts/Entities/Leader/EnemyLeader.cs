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

    const float WAYPOINT_DIST = 10f;


    override public void Start () {
        base.Start();
        AIManager.staticManager.AddEnemyPeloton(myPeloton);  //Avisar al AIManager
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

        //Debug.Log(chosenStrategy.plan.Peek().targetElement);

        return chosenStrategy;
    }

    public void SearchPathToTarget()
    {
        //SearchPath(currentTactic.targetElement.transform.position);
    }

    public void SearchPath(Vector3 targetPosition)
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
            MoveTo(waypoint);
            //Move((waypoint.x - transform.position.x), (waypoint.z - transform.position.z));

            if (Vector3.Distance(new Vector3(transform.position.x, 0f, transform.position.z), waypoint) < WAYPOINT_DIST)
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

    void OnTriggerStay(Collider other)
    {
        // Does actions and determines his Peloton's new objective
        if (other.name == Names.TOTEM && velocity.magnitude < 15)
        {
            leaderTarget = other.gameObject;
            myPeloton.SetStateAndTarget(Names.STATE_CONQUER, leaderTarget);
        }
        else if (atkCooldown <= 0f)
        {
            if (other.name == Names.PLAYER_MINION)
            {
                leaderTarget = other.gameObject;
                myPeloton.SetStateAndTarget(Names.STATE_ATTACK, leaderTarget);
                other.GetComponent<Minion>().RecieveDamage(GetDamageOutput());
                Attack();
            }
            else if (other.name == Names.PLAYER_LEADER)
            {
                leaderTarget = other.gameObject;
                myPeloton.SetStateAndTarget(Names.STATE_ATTACK, leaderTarget);
                other.GetComponent<PlayerLeader>().RecieveDamage(GetDamageOutput());
                Attack();
            }
            else if (other.name.Contains(Names.PLAYER_DOOR))
            {
                leaderTarget = other.gameObject;
                myPeloton.SetStateAndTarget(Names.STATE_ATTACK_DOOR, leaderTarget);
                other.GetComponent<Door>().RecieveDamage(GetDamageOutput());
                Attack();
            }

            if (other.name == Names.PEPINO || other.name == Names.PIMIENTO || other.name == Names.MOLEM || other.name == Names.KEAWEE || other.name == Names.CHILI)
            {
                leaderTarget = other.GetComponent<Beast>().camp.gameObject;
                myPeloton.SetStateAndTarget(Names.STATE_ATTACK_CAMP, leaderTarget);
                other.GetComponent<Beast>().RecieveDamage(GetDamageOutput(), name);
                Attack();
            }
        }
    }

    public void MoveTo(Vector3 targetLocation)
    {
        Vector2 displacement = new Vector2(targetLocation.x - transform.position.x, targetLocation.z - transform.position.z);
        displacement.Normalize();
        displacement *= 3 * 1.44f;
        Move(displacement.x, displacement.y);
    }
}
