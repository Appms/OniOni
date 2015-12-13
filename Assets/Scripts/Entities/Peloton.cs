using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Peloton : MonoBehaviour {

    public float BASE_MOVEMENT_SPEED = 30f;
    float movementSpeed = 0f;

    AIManager aiManager;
	
	List<Minion> _minionsList = new List<Minion>();

    string objective = "Idle";
    public string state = "Idle";
    GameObject targetElement;
    Vector3 targetPosition;
    bool _hasPath = false;

    public GameObject leader;
    Leader leaderScript;


	public List<Vector2> path = new List<Vector2>();
    public Vector3 velocity = new Vector3();

    public float isBeingAttacked = 0;
    public List<Peloton> menaces = new List<Peloton>();
    public List<Peloton> victims = new List<Peloton>();
	
	void Start () {

        aiManager = GameObject.Find("AIManager").GetComponent<AIManager>();
        leaderScript = AIManager.staticManager.GetLeaderByName(leader.name);
        victims = new List<Peloton>();
        menaces = new List<Peloton>();

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
    }

	void FixedUpdate ()
    {
        //BEHAVIOUR TREE
        //Watch(); //Nothing implemented yet
        Decide();
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
    //-------------------------------------------------------------------------
    //-------------------------------------------------------------------------
    

    // PATHFINDING
	private void GoTo(Vector3 targetPosition)
    {
        _hasPath = true;
		JPSManager.RequestPath (transform.position, targetPosition, OnPathFound);
	}

	private void OnPathFound(List<Vector2> newPath, bool pathSuccessful)
    {
		if (pathSuccessful)
        {
			path = newPath;
            _hasPath = false;
			StopCoroutine("FollowPath");
			StartCoroutine("FollowPath");
		}
	}

	IEnumerator FollowPath()
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
    }
    //-------------------------------------------------------------------------
    //-------------------------------------------------------------------------

    // AI FLOW
    public void Watch()
    {
        // Check in the neighbourhoods for interesting things
	}

	public void Decide()
    {
        // RAIN
        DecisionTreeMock(); // MOCK!!!!!!
    }

	public void Conquer()
    {
        // "Build Totem", Push Melon
	}

	public void Attack()
    {
        // TO ARMS!
        //transform.position = opponentPeloton.targetElement == gameObject ? (opponentPeloton.transform.position - transform.position) / 2f + transform.position : opponentPeloton.transform.position;
        Vector3 move = victims[0].transform.position - transform.position;
        if (move.magnitude > movementSpeed) move = move.normalized * movementSpeed;
        transform.position += move;
        state = Names.STATE_ATTACK;
	}

	public void Defend()
    {
        // Maybe unnecessary and just idle?
	}

    public void FollowLeader()
    {

    }

    //-------------------------------------------------------------------------
    //-------------------------------------------------------------------------


    // MERGE
    private void CheckForMerge()
    {
        List<Peloton> neighbours = aiManager.GetNeighbourPelotons(this);
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
                case "Idle":
                    return true;
                case "FollowLeader":
                    return otherObjective.leader == leader;
                case "GoTo":
                    return Vector3.Distance(otherObjective.targetPosition, targetPosition) < 10f;
                case "Interact":
                    return otherObjective.targetElement.Equals(targetElement);
                case "Push":
                    return otherObjective.targetElement.Equals(targetElement);
            }
        }
        return false;
    }
    //-------------------------------------------------------------------------
    //-------------------------------------------------------------------------


    // DECISION TREE MOCK
    private void DecisionTreeMock()
    {
        switch (objective)
        {
            case "Idle":
                break;
            case "FollowLeader":
                transform.position = leader.GetComponent<Leader>().behind;
                break;
            case "GoTo":
                if(!_hasPath) GoTo(targetPosition);
                break;
            case "Interact":
                /*if (targetElement.tag == (gameObject.tag == Names.PLAYER_PELOTON ? Names.ENEMY_PELOTON : Names.PLAYER_PELOTON)) Attack(targetElement.GetComponent<Peloton>());
                else*/ if (!_hasPath) GoTo(targetElement.transform.position + (transform.position - targetElement.transform.position).normalized * 10f);
                break;
            case "Push":
                if (!_hasPath)
                {
                    if(leader.name == Names.PLAYER_LEADER) GoTo(GameObject.Find(Names.ORANGE_OBJECTIVE).transform.position);
                    else GoTo(GameObject.Find(Names.PURPLE_OBJECTIVE).transform.position);
                }
                break;
        }
    }
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
            if (leader.name == Names.PLAYER_LEADER) aiManager.RemovePlayerPeloton(this);
            else aiManager.RemoveEnemyPeloton(this);
            StopCoroutine("FollowPath");
            Destroy(this.gameObject);
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
}
