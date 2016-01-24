using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Peloton : MonoBehaviour {

    public float BASE_MOVEMENT_SPEED = 30f;
    float movementSpeed = 0f;
	
	List<Minion> _minionsList = new List<Minion>();

    public string objective;
    public string state = Names.STATE_FOLLOW_LEADER;
    public GameObject targetElement;
    public GameObject leaderTarget;
    public Vector3 targetPosition;
    public bool calculatingPath = false;
    public bool goingTo = false;

    public GameObject leader;
    Leader leaderScript;

	public List<Vector2> path = new List<Vector2>();
    public Vector3 velocity = new Vector3();

    public float isBeingAttacked = 0;
    public List<Peloton> menaces = new List<Peloton>();
    public List<Peloton> victims = new List<Peloton>();

    public Radio radio;

    AudioSource pelotonSource;
	
	void Start () {

        leaderScript = AIManager.staticManager.GetLeaderByName(leader.name);
        victims = new List<Peloton>();
        menaces = new List<Peloton>();
        gameObject.layer = LayerMask.NameToLayer("Element");

        state = Names.STATE_FOLLOW_LEADER;

        pelotonSource = GetComponent<AudioSource>();

        radio = GameObject.Find(Names.RADIO).GetComponent<Radio>();

        //leader = GameObject.Find("Leader"); // CAMBIARLO
        //gameObject.layer = LayerMask.NameToLayer("Peloton");
    }

    void Update()
    {
        ApplyMovementBuff();

        if (isBeingAttacked > 0)
        {
            isBeingAttacked -= Time.deltaTime;
            if (isBeingAttacked <= 0)
            {
                menaces = new List<Peloton>();
            }
        }

        if(IsLeaderPeloton())
        {
            LeaderPelotonStateMachine(state, leaderTarget);
        }
        else
        {
            if (state == Names.STATE_CONQUER)
            {
                if (pelotonSource.volume < 0.5f) pelotonSource.volume += 0.1f;
                radio.praying = true;
            }
            else
            {
                if(pelotonSource.volume > 0.0f) pelotonSource.volume -= 0.1f;
                if (pelotonSource.volume < 0.0f) pelotonSource.volume = 0;
                radio.praying = false;
            }
        }

        if (!goingTo)
        {
            PelotonStateMachine(objective, targetElement);
        }
    }

	void FixedUpdate ()
    {
        //BEHAVIOUR TREE
        //Watch(); //Nothing implemented yet
        CheckForMerge();
	}
    //-------------------------------------------------------------------------
    //-------------------------------------------------------------------------

    // LEADER
    public GameObject GetLeader() // --DEPRECATED-- leader made public
    {
        return leader;
    }

    public void SetLeader(GameObject newleader)
    {
        leader = newleader;
        gameObject.tag = leader.name == Names.PLAYER_LEADER ? Names.PLAYER_PELOTON : Names.ENEMY_PELOTON;
    }

    public bool IsLeaderPeloton()
    {
        //return leader.GetComponent<Leader>().myPeloton == this;
        return gameObject.name == Names.PLAYER_LEADER_PELOTON || gameObject.name == Names.ENEMY_LEADER_PELOTON;
    }
    //-------------------------------------------------------------------------
    //-------------------------------------------------------------------------

    
    // MINIONS
    public void AddMinion(Minion minion)
    {
		_minionsList.Add(minion);
        minion.SetPeloton(this);
	}

	public void AddMinionList(List<Minion> minionList)
    {
		foreach (Minion m in minionList)
        {
			_minionsList.Add(m);
            m.SetPeloton(this);
        }
	}

	public void RemoveMinion(Minion minion)
    {
		_minionsList.Remove(minion);
        if (_minionsList.Count <= 0) DestroyPeloton();
    }

	public void RemoveAllMinions()
    {
		_minionsList = new List<Minion>();
        if (_minionsList.Count <= 0) DestroyPeloton();
    }

    public List<Minion> GetMinionListSorted(Vector3 position)
    {
        _minionsList.Sort((c1, c2) => (int)(Vector3.Distance(c1.transform.position, position) - Vector3.Distance(c2.transform.position, position)));
        return _minionsList;
    }
    //-------------------------------------------------------------------------
    //-------------------------------------------------------------------------


    // OBJECTIVES
    public void SetObjective(string objectiveName, Vector3 position)
    {
        objective = objectiveName;
        targetPosition = position;
    }

    public void SetObjective(string objectiveName, GameObject element)
    {
        objective = objectiveName;
        targetElement = element;
        targetPosition = element.transform.position;
    }

    public void SetObjective(string idle)
    {
        objective = idle;
    }

    public string GetObjectiveType()
    {
        return objective;
    }

    public Vector3 GetTargetPosition()
    {
        return targetPosition;
    }

    public GameObject GetTargetElement()
    {
        return targetElement;
    }

    public void SetStateAndTarget(string state, GameObject target)
    {
        this.state = state;
        leaderTarget = target;
    }

    //-------------------------------------------------------------------------
    //-------------------------------------------------------------------------
    

    // PATHFINDING
	private void SearchPath(Vector3 targetPosition)
    {
        calculatingPath = true;
        //goingTo = true;
		JPSManager.RequestPath (transform.position, targetPosition, OnPathFound);
	}

    public void SearchPathToTarget()
    {
        SearchPath(targetPosition);
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
            velocity = (waypoint - transform.position).normalized * movementSpeed;
            transform.position += velocity * Time.deltaTime;

            if (this.isActiveAndEnabled && Vector3.Distance(transform.position, waypoint) < 0.5f)
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

            while (this != null && this.isActiveAndEnabled && !Physics.Raycast(transform.position, nextPoint - transform.position, out hit, Vector3.Distance(transform.position, nextPoint), LayerMask.GetMask("Level")))
            {
                path.RemoveAt(0);

                if (path.Count < 2) break;

                hit = new RaycastHit();
                nextPoint = new Vector3(path[1].x, transform.position.y, path[1].y);
            }
        }
    }

    /*IEnumerator FollowPath()
    {
        Vector3 waypoint = new Vector3(path[0].x, 0f, path[0].y);
        while (path.Count > 0)
        {
            velocity = (waypoint - transform.position).normalized * movementSpeed;
            transform.position += velocity * Time.deltaTime;
            
            if (Vector3.Distance(transform.position, waypoint) < 0.5f)
            {
                path.RemoveAt(0);
                if(path.Count > 0) waypoint = new Vector3(path[0].x, 0f, path[0].y);
            }
        }
        velocity = Vector3.zero;
        objective = "Idle";
        yield return null;
    }*/
    //-------------------------------------------------------------------------
    //-------------------------------------------------------------------------

    // AI FLOW

	public void Conquer(GameObject totem)
    {
        // "Build Totem", Push Melon
        transform.position = totem.transform.position;
    }

    // only for direct combat, not to be used for atacking static camps or door
	public void Attack()
    {
        // TO ARMS!
        //transform.position = opponentPeloton.targetElement == gameObject ? (opponentPeloton.transform.position - transform.position) / 2f + transform.position : opponentPeloton.transform.position;
        if (victims[0] != null)
        {
            //Vector3 move = (victims[0].transform.position - transform.position).normalized * movementSpeed;
            //transform.position += move * Time.deltaTime;
            transform.position = victims[0].transform.position;
        }
        else victims.RemoveAt(0);
	}

    public void SupportLeaderAttack(GameObject targetObjective)
    {
        //Vector3 move = (targetObjective.transform.position - transform.position).normalized * movementSpeed;
        //transform.position += move * Time.deltaTime;
        transform.position = targetObjective.transform.position;
    }

    public void AttackCamp(GameObject targetCamp)
    {
        //Vector3 move = (targetCamp.transform.position - transform.position).normalized * movementSpeed ;
        //transform.position += move * Time.deltaTime;

        if(targetCamp.GetComponent<Camp>().units.Count > 0) transform.position = targetCamp.GetComponent<Camp>().units[0].transform.position;

    }

    public void PushFuit(Peloton peloton)
    {
        GameObject.Find("Fruit").GetComponent<Fruit>().pushMelon(peloton);
    }

    public void FollowLeader()
    {
        //transform.position = leader.GetComponent<Leader>().behind;
        //transform.position = Vector3.Lerp(transform.position, leader.GetComponent<Leader>().behind, Time.deltaTime);

        Vector3 move = (leader.GetComponent<Leader>().behind - transform.position).normalized * movementSpeed;
        transform.position += move * Time.deltaTime;
    }

    public void AttackDoor(GameObject door)
    {
        //Vector3 move = (door.transform.position - transform.position).normalized * movementSpeed;
        //transform.position += move * Time.deltaTime;

        transform.position = door.transform.position;
    }

    //-------------------------------------------------------------------------
    //-------------------------------------------------------------------------


    // MERGE
    private void CheckForMerge()
    {
        List<Peloton> neighbours = AIManager.staticManager.GetNeighbourPelotons(this);
        foreach (Peloton p in neighbours)
        {
            if (p.HasSameObjective(this))
            {
                Merge(p);
            }
        }
    }
    private void Merge(Peloton anotherPeloton)
    {
        if (!anotherPeloton.IsLeaderPeloton())
        {
            if (IsLeaderPeloton())
            {
                AddMinionList(anotherPeloton.GetMinionList());
                anotherPeloton.RemoveAllMinions();
                anotherPeloton.DestroyPeloton();
            }
            else if (this.Size() > anotherPeloton.Size() || (this.Size() == anotherPeloton.Size() && this.gameObject.GetInstanceID() > anotherPeloton.gameObject.GetInstanceID()))
            {
                AddMinionList(anotherPeloton.GetMinionList());
                anotherPeloton.RemoveAllMinions();
                anotherPeloton.DestroyPeloton();
            }
        }
	}

    public bool HasSameObjective(Peloton otherObjective)
    {
        if (otherObjective.objective == objective) {
            switch (objective)
            {
                case Names.OBJECTIVE_DEFEND:
                    return Vector3.Distance(otherObjective.targetPosition, targetPosition) < 10f;
                case Names.OBJECTIVE_ATTACK:
                    return otherObjective.targetElement.Equals(targetElement);
                case Names.OBJECTIVE_ATTACK_CAMP:
                    return otherObjective.targetElement.Equals(targetElement);
                case Names.OBJECTIVE_ATTACK_DOOR:
                    return otherObjective.targetElement.Equals(targetElement);
                case Names.OBJECTIVE_FOLLOW_LEADER:
                    return otherObjective.leader == leader;
                case Names.OBJECTIVE_CONQUER:
                    return otherObjective.targetElement.Equals(targetElement);
                case Names.OBJECTIVE_PUSH:
                    return true;
            }
        }
        return false;
    }
    //-------------------------------------------------------------------------
    //-------------------------------------------------------------------------

    //-------------------------------------------------------------------------
    //-------------------------------------------------------------------------


    // AUXILIAR FUNCTIONS

    private void ApplyMovementBuff()
    {
        movementSpeed = BASE_MOVEMENT_SPEED * (leaderScript.movementBuff > 0 ? leaderScript.BUFF_MULTIPLYER : 1f);
    }

    public void DestroyPeloton()
    {
        if (this.Size() == 0 && !IsLeaderPeloton())
        {
            foreach(Peloton p in victims)
            {
                p.menaces.Remove(this);
                p.victims.Remove(this);
            }
            foreach (Peloton p in menaces)
            {
                p.menaces.Remove(this);
                p.victims.Remove(this);
            }
            AIManager.staticManager.RemovePeloton(this);
            StopCoroutine("FollowPath");
            Destroy(this.gameObject);
            //gameObject.GetComponent<Peloton>().enabled = false;
            this.enabled = false;
        }
	}
    
	public int Size()
    {
		return _minionsList.Count;
	}

	public List<Minion> GetMinionList()
    {
		return _minionsList;
	}


    void OnDrawGuizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawSphere(transform.position, 10);
    }

    void LeaderPelotonStateMachine(string state, GameObject target)
    {
        switch (state)
        {
            case Names.STATE_ATTACK:
                if (target != null) SupportLeaderAttack(target);
                else SetStateAndTarget(Names.STATE_FOLLOW_LEADER, gameObject);
                break;

            case Names.STATE_ATTACK_CAMP:
                if (target.GetComponent<Camp>().units.Count > 0) AttackCamp(target);
                else SetStateAndTarget(Names.STATE_FOLLOW_LEADER, gameObject);
                break;

            case Names.STATE_ATTACK_DOOR:
                if (!target.GetComponent<Door>().doorsUp) SetStateAndTarget(Names.STATE_FOLLOW_LEADER, gameObject);
                else AttackDoor(target);
                break;

            case Names.STATE_CONQUER:
                if (name == Names.PLAYER_LEADER_PELOTON && target.GetComponent<Totem>().alignment <= -50) SetStateAndTarget(Names.STATE_FOLLOW_LEADER, gameObject);
                else if (name == Names.ENEMY_LEADER_PELOTON && target.GetComponent<Totem>().alignment >= 50) SetStateAndTarget(Names.STATE_FOLLOW_LEADER, gameObject);
                else Conquer(target);
                break;

            case Names.STATE_FOLLOW_LEADER:
                FollowLeader();
                break;

            default:
                FollowLeader();
                break;
        }
    }

    void PelotonStateMachine(string state, GameObject target)
    {
        switch (state)
        {
            case Names.STATE_ATTACK:
                if (target != null) SupportLeaderAttack(target);
                break;

            case Names.STATE_ATTACK_CAMP:
                if (target.GetComponent<Camp>().units.Count > 0) AttackCamp(target);
                break;

            case Names.STATE_ATTACK_DOOR:
                 AttackDoor(target);
                break;

            case Names.STATE_CONQUER:
                Conquer(target);
                break;

            case Names.STATE_FOLLOW_LEADER:
                FollowLeader();
                break;

            case Names.STATE_DEFEND:
                break;

            case Names.STATE_PUSH:
                break;

            default:
                //FollowLeader();
                break;
        }
    }
}
