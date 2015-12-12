using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class AIManager : MonoBehaviour {

    public static AIManager staticManager;

    public static string PLAYER_LEADER = "PlayerLeader";
    public static string ENEMY_LEADER = "EnemyLeader";
    public static string PLAYER_PELOTON = "PlayerPeloton";
    public static string ENEMY_PELOTON = "EnemyPeloton";

    static float MERGE_DISTANCE = 25f;

    public int MAXIMUM_MINIONS = 15;

    GameObject playerLeader;
    GameObject enemyLeader;
    Leader playerLeaderScript;
    Leader enemyLeaderScript;
    List<Peloton> playerTeam = new List<Peloton>();
    List<Peloton> enemyTeam = new List<Peloton>();

    List<Totem> totems = new List<Totem>();

    Fruit fruitScript;

    void Awake()
    {
        Application.targetFrameRate = 60;

        staticManager = this;

        GameObject[] totemGameObjects = GameObject.FindGameObjectsWithTag("Totem");
        foreach (GameObject t in totemGameObjects)
        {
            totems.Add(t.GetComponent<Totem>());
        }

        fruitScript = GameObject.Find("Fruit").GetComponent<Fruit>();
    }

    // Use this for initialization
    void Start () {
        playerLeader = GameObject.Find(PLAYER_LEADER);
        enemyLeader = GameObject.Find(ENEMY_LEADER);
        playerLeaderScript = playerLeader.GetComponent<PlayerLeader>();
        enemyLeaderScript = enemyLeader.GetComponent<EnemyLeader>();

        // Pelotón inicial del Lider // --DEPRECATED--
        //playerTeam.Add(playerLeaderScript.myPeloton);
        //enemyTeam.Add(enemyLeaderScript.myPeloton);  
    }

    public void AddPlayerPeloton(Peloton peloton)
    {
        playerTeam.Add(peloton);
    }
    public void AddEnemyPeloton(Peloton peloton)
    {
        enemyTeam.Add(peloton);
    }
    public void RemovePlayerPeloton(Peloton p)
    {
        playerTeam.Remove(p);
    }
    public void RemoveEnemyPeloton(Peloton p)
    {
        enemyTeam.Remove(p);
    }

    public void AddPeloton(Peloton peloton)
    {
        if (peloton.gameObject.name == Names.PLAYER_PELOTON) playerTeam.Add(peloton);
        else enemyTeam.Add(peloton);
    }

    public List<Peloton> GetNeighbourPelotons(Peloton peloton)
    {
        List<Peloton> neighbours = new List<Peloton>();
        if(peloton.GetLeader() == playerLeader)
        {
            foreach(Peloton p in playerTeam)
            {
                if (p != peloton && Vector3.Distance(p.transform.position, peloton.transform.position) < MERGE_DISTANCE)
                    neighbours.Add(p);
            }
        }
        else
        {
            foreach (Peloton p in enemyTeam)
            {
                if (Vector3.Distance(p.transform.position, peloton.transform.position) < MERGE_DISTANCE) neighbours.Add(p);
            }
        }
        return neighbours;
    }

    public List<Minion> GetTeamMinions(string leader)
    {
        List<Minion> team = new List<Minion>();
        if(leader == Names.PLAYER_LEADER)
        {
            foreach(Peloton p in playerTeam)
            {
                foreach (Minion m in p.GetMinionList())
                    team.Add(m);
            }
        }
        else
        {
            foreach(Peloton p in enemyTeam)
            {
                foreach (Minion m in p.GetMinionList())
                    team.Add(m);
            }
        }

        return team;
    }

    public int GetTeamMinionsCount(string leader)
    {
        int count = 0;
        if (leader == PLAYER_LEADER)
        {
            foreach (Peloton p in playerTeam)
            {
                count += p.Size();
            }
        }
        else if (leader == ENEMY_LEADER)
        {
            foreach (Peloton p in enemyTeam)
            {
                count += p.Size();
            }
        }

        return count;
    }

    public int GetLeaderMinionsCount(string leader)
    {
        int count = 0;
        if (leader == PLAYER_LEADER) count = playerLeaderScript.myPeloton.Size();

        else if (leader == ENEMY_LEADER) count = enemyLeaderScript.myPeloton.Size();

        return count;
    }

    public List<Minion> GetMinionsInRange(float range, Vector3 position, string leader)
    {
        List<Minion> minions = new List<Minion>();

        if(leader == PLAYER_LEADER)
        {
            foreach(Peloton p in playerTeam)
            {
                foreach(Minion m in p.GetMinionList())
                {
                    if(m.peloton != playerLeaderScript.myPeloton && Vector3.Distance(position, m.transform.position) <= range)
                    {
                        minions.Add(m);
                    }
                }
            }
        }
        else
        {
            foreach (Peloton p in enemyTeam)
            {
                foreach (Minion m in p.GetMinionList())
                {
                    if (m.peloton != enemyLeaderScript.myPeloton && Vector3.Distance(position, m.transform.position) <= range)
                    {
                        minions.Add(m);
                    }
                }
            }
        }

        return minions;
    }


    public int GetAlignedTotemsCount(string owner)
    {
        int count = 0;

        if (owner == PLAYER_LEADER)
        {
            foreach (Totem t in totems)
            {
                if (t.alignment == 25f) count++;
            }
        }
        else if (owner == ENEMY_LEADER)
        {
            foreach (Totem t in totems)
            {
                if (t.alignment == -25f) count++;
            }
        }

        return count;
    }
    public int GetTotemCount()
    {
        return totems.Count;
    }


    public Leader GetLeaderByName(string name)
    {
        if (name == Names.PLAYER_LEADER) return playerLeaderScript;
        /*else if (name == Names.ENEMY_LEADER)*/ return enemyLeaderScript;
    }

    // this should depend on the state, to be implemented
    public List<Peloton> GetPelotonsByObjective(string leader, string objective)
    {
        List<Peloton> pelotons = new List<Peloton>();

        if (leader == Names.PLAYER_LEADER)
        {
            foreach (Peloton p in playerTeam)
            {
                if (p.GetObjectiveType() == objective) pelotons.Add(p);
            }
        }
        /*else
        {
            foreach (Peloton p in enemyTeam)
            {
                if (p.GetObjectiveType() == objective) pelotons.Add(p);
            }
        }*/

        return pelotons;
    }
}
