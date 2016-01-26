using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class AIManager : MonoBehaviour {

    public static AIManager staticManager;

    static float MERGE_DISTANCE = 25f;
    static float WATCH_DISTANCE = 30f;

    public int MAXIMUM_MINIONS = 15;

    GameObject playerLeader;
    GameObject enemyLeader;
    Leader playerLeaderScript;
    Leader enemyLeaderScript;
    List<Peloton> playerTeam = new List<Peloton>();
    List<Peloton> enemyTeam = new List<Peloton>();
    public Door orangeDoor;
    public Door purpleDoor;

    public bool EndGame = false;
    public bool DisableElements = false;

    List<Totem> totems = new List<Totem>();

    List<Camp> camps = new List<Camp>();

    Fruit fruitScript;
    GameObject fruitMesh;
    GameObject orangeObjective;
    GameObject purpleObjective;


    private const float unitAdvantage = 1.2f; // Advantage in proportion of units in a conflict that makes an action decently profitable

    private const float minionWeight = 1f;
    private const float distanceWeight = 0.1f;//0.05f; // 1 minion = 20m

    private const float totemsValue = 8.333f; // 1 totem = 10 minions
    //private const float pushingValue = 0.01f;
    private const float pushBaseValue = 10f;
    private const float pushPonderValue = 3f;

    void Awake()
    {
        Application.targetFrameRate = 30;

        staticManager = this;

        GameObject[] totemGameObjects = GameObject.FindGameObjectsWithTag(Names.TOTEM);
        foreach (GameObject t in totemGameObjects)
        {
            totems.Add(t.GetComponent<Totem>());
        }

        GameObject[] campGameObjects = GameObject.FindGameObjectsWithTag(Names.CAMP);
        foreach (GameObject c in campGameObjects)
        {
            camps.Add(c.GetComponent<Camp>());
        }

        fruitScript = GameObject.Find("Fruit").GetComponent<Fruit>();
        fruitMesh = GameObject.Find(Names.FRUIT);
        orangeObjective = GameObject.Find(Names.ORANGE_OBJECTIVE);
        purpleObjective = GameObject.Find(Names.PURPLE_OBJECTIVE);
        orangeDoor = GameObject.Find(Names.PLAYER_DOOR).GetComponent<Door>();
        purpleDoor = GameObject.Find(Names.ENEMY_DOOR).GetComponent<Door>();
    }

    // Use this for initialization
    void OnEnable () {
        playerLeader = GameObject.Find(Names.PLAYER_LEADER);
        enemyLeader = GameObject.Find(Names.ENEMY_LEADER);
        playerLeaderScript = playerLeader.GetComponent<PlayerLeader>();
        enemyLeaderScript = enemyLeader.GetComponent<EnemyLeader>();
    }

    private void AddPlayerPeloton(Peloton peloton)
    {
        playerTeam.Add(peloton);
    }
    private void AddEnemyPeloton(Peloton peloton)
    {
        enemyTeam.Add(peloton);
    }
    private void RemovePlayerPeloton(Peloton p)
    {
        playerTeam.Remove(p);
    }
    private void RemoveEnemyPeloton(Peloton p)
    {
        enemyTeam.Remove(p);
    }

    public void AddPeloton(Peloton peloton)
    {
        if (peloton.gameObject.name == Names.PLAYER_PELOTON || peloton.gameObject.name == Names.PLAYER_LEADER_PELOTON) playerTeam.Add(peloton);
        else enemyTeam.Add(peloton);
    }
    public void RemovePeloton(Peloton peloton)
    {
        if (peloton.gameObject.name == Names.PLAYER_PELOTON || peloton.gameObject.name == Names.PLAYER_LEADER_PELOTON) playerTeam.Remove(peloton);
        else enemyTeam.Remove(peloton);
    }

    public List<Peloton> GetNeighbourPelotons(Peloton peloton)
    {
        List<Peloton> neighbours = new List<Peloton>();
        if(peloton.gameObject.name == Names.PLAYER_LEADER_PELOTON || peloton.gameObject.name == Names.PLAYER_PELOTON)
        {
            foreach(Peloton p in playerTeam)
            {
                if (p != null && peloton != null && p != peloton && Vector3.Distance(p.transform.position, peloton.transform.position) < MERGE_DISTANCE)
                    neighbours.Add(p);
            }
        }
        else
        {
            foreach (Peloton p in enemyTeam)
            {
                if (p != null && peloton != null && p != peloton && Vector3.Distance(p.transform.position, peloton.transform.position) < MERGE_DISTANCE)
                    neighbours.Add(p);
            }
        }
        return neighbours;
    }

	public GameObject GetFruit()
	{
		return fruitScript.gameObject;
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
        if (leader == Names.PLAYER_LEADER)
        {
            foreach (Peloton p in playerTeam)
            {
                count += p.Size();
            }
        }
        else if (leader == Names.ENEMY_LEADER)
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
        if (leader == Names.PLAYER_LEADER) count = playerLeaderScript.myPeloton.Size();

        else if (leader == Names.ENEMY_LEADER) count = enemyLeaderScript.myPeloton.Size();

        return count;
    }

    public List<Minion> GetMinionsInRange(float range, Vector3 position, string leader)
    {
        List<Minion> minions = new List<Minion>();

        if(leader == Names.PLAYER_LEADER)
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
        else // if(leader == Names.ENEMY_LEADER)
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

        if (owner == Names.PLAYER_LEADER)
        {
            foreach (Totem t in totems)
            {
                if (t.alignment == 50f) count++;
            }
        }
        else if (owner == Names.ENEMY_LEADER)
        {
            foreach (Totem t in totems)
            {
                if (t.alignment == -50f) count++;
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
        else
        {
            foreach (Peloton p in enemyTeam)
            {
                if (p.GetObjectiveType() == objective) pelotons.Add(p);
            }
        }

        return pelotons;
    }

    public List<Peloton> GetNearbyEnemies(Peloton peloton)
    {
        List<Peloton> neighbours = new List<Peloton>();
        if (peloton.GetLeader().name == Names.PLAYER_LEADER)
        {
            foreach (Peloton p in enemyTeam)
            {
                if (p != null && peloton != null && p != peloton && Vector3.Distance(p.transform.position, peloton.transform.position) < WATCH_DISTANCE)
                    neighbours.Add(p);
            }
        }
        else
        {
            foreach (Peloton p in playerTeam)
            {
                if (p != null && peloton != null && p != peloton && Vector3.Distance(p.transform.position, peloton.transform.position) < WATCH_DISTANCE)
                    neighbours.Add(p);
            }
        }
        return neighbours;
    }

    public List<Peloton> GetPelotonsInRange(float range, Vector3 position, string leader)
    {
        List<Peloton> pelotons = new List<Peloton>();

        if(leader == Names.PLAYER_LEADER)
        {
            foreach (Peloton p in playerTeam)
            {
                if (Vector3.Distance(p.transform.position, position) < range) pelotons.Add(p);
            }
        }
        else /*if (leader.name == Names.ENEMY_LEADER)*/
        {
            foreach (Peloton p in enemyTeam)
            {
                if (Vector3.Distance(p.transform.position, position) < range) pelotons.Add(p);
            }
        }

        return pelotons;
    }


















    // ENEMY LEADER AI -----------------------------------------------------------------------------------------

    public List<Strategy> GetAIStrategies()
    {
        List<Strategy> options = new List<Strategy>();

        //  TOTEM STRATEGIES
        List<Strategy> aux = GetTotemStrategies();
        foreach (Strategy ts in aux)
            options.Add(ts);

        // PUSH STRATEGY
        options.Add(GetPushStrategy());

        // ATTACK DOOR STRATEGY
        options.Add(GetAttackDoorStrategy());

        // DEFEND DOOR STRATEGY
        aux = GetDefendDoorStrategy();
        foreach (Strategy ds in aux)
            options.Add(ds);

        // ATTACK NEARBY ENEMIES STRATEGY
        aux = GetAttackNearbyEnemiesStrategy();
        foreach (Strategy ane in aux)
            options.Add(ane);

        return options;
    }

    private List<Strategy> GetTotemStrategies()
    {
        List<Strategy> options = new List<Strategy>();

        int i = 0;
        // TOTEMS
        foreach (Totem t in totems)
        {
            i++;
            bool discard = false;
            List<Peloton> encargados = GetPelotonsByObjective(Names.ENEMY_LEADER, Names.OBJECTIVE_CONQUER);
            foreach (Peloton p in encargados)
            {
                if (p.targetElement == t)
                {
                    discard = true;
                    break;
                }
            }

            if (t.alignment != -50 && !discard)
            {
                float tacticCost, tacticReward;
                int necessaryMinions, remainingMinions;
                Stack<Tactic> strategyPlan = new Stack<Tactic>();
                List<Tactic> gathering = new List<Tactic>();
                Tactic buffTactic;

                necessaryMinions = 5; // Minimum minions to be reasonable
                necessaryMinions += Mathf.FloorToInt(GetMinionsInRange(20f, t.transform.position, Names.PLAYER_LEADER).Count * unitAdvantage);

                if (necessaryMinions > 5)
                {
                    buffTactic = GetBuffTactic(Names.ATTACK_BUFF);
                    if (buffTactic == null)
                        buffTactic = GetBuffTactic(Names.DEFENSE_BUFF);
                }
                else buffTactic = GetBuffTactic(Names.MOVEMENT_BUFF);
                if (buffTactic != null)
                {
                    necessaryMinions += buffTactic.cantMinions;
                }

                // Get those MINIONS!!
                remainingMinions = necessaryMinions - enemyLeaderScript.myPeloton.Size();
                if (remainingMinions > 0) gathering = MinionGathering(remainingMinions, Names.OBJECTIVE_CONQUER, t.gameObject);

                tacticCost = necessaryMinions * minionWeight;
                tacticCost += Vector3.Distance(enemyLeader.transform.position, t.transform.position) * distanceWeight;

                //tacticReward = 1f / (GetAlignedTotemsCount(Names.ENEMY_LEADER) + 1f / int.MaxValue);
                //tacticReward += Mathf.Pow(GetAlignedTotemsCount(Names.PLAYER_LEADER), 2); // Urgencia
                tacticReward = (-GetAlignedTotemsCount(Names.ENEMY_LEADER) + 12) * totemsValue;
                tacticReward += (GetAlignedTotemsCount(Names.PLAYER_LEADER)) * totemsValue;
                //tacticReward *= totemsValue;


                // Constructing STRATEGIES
                strategyPlan.Push(new Tactic(tacticCost, tacticReward, t.gameObject, true, necessaryMinions)); //MainTactic
                if (buffTactic != null) strategyPlan.Push(buffTactic);
                foreach (Tactic tc in gathering) // sub-tactics
                    strategyPlan.Push(tc);

                Strategy newStrategy = new Strategy(strategyPlan);
                options.Add(newStrategy);
            }
        }

        return options;
    }

    private Strategy GetPushStrategy()
    {
        float tacticCost, tacticReward;
        int necessaryMinions, remainingMinions;
        Stack<Tactic> strategyPlan = new Stack<Tactic>();
        List<Tactic> gathering = new List<Tactic>();
        Tactic buffTactic;

        int playerMinionsPushing = 0;
        int enemyMinionsPushing = 0;

        // PUSH MELON
        necessaryMinions = 5; // Minimum minions to be reasonable
        List<Peloton> pelotons = GetPelotonsByObjective(Names.PLAYER_LEADER, Names.OBJECTIVE_PUSH);
        foreach (Peloton p in pelotons) {
            necessaryMinions += Mathf.FloorToInt(p.Size() * unitAdvantage);
            playerMinionsPushing += p.Size();
        }
        remainingMinions = necessaryMinions;
        pelotons = GetPelotonsByObjective(Names.ENEMY_LEADER, Names.OBJECTIVE_PUSH);
        foreach (Peloton p in pelotons)
        {
            remainingMinions -= p.Size();
            enemyMinionsPushing += p.Size();
        }

        buffTactic = GetBuffTactic(Names.PUSH_BUFF);
        if (buffTactic != null)
        {
            remainingMinions += buffTactic.cantMinions;
        }
        else
        {
            buffTactic = GetBuffTactic(Names.MOVEMENT_BUFF);
            if (buffTactic != null)
            {
                remainingMinions += buffTactic.cantMinions;
            }
        }


        // Get those MINIONS!!
        //remainingMinions = necessaryMinions - enemyLeaderScript.myPeloton.Size();
        if (remainingMinions > 0) gathering = MinionGathering(remainingMinions, Names.OBJECTIVE_PUSH, purpleObjective);

        tacticCost = necessaryMinions * minionWeight;
        tacticCost += Vector3.Distance(enemyLeader.transform.position, fruitScript.transform.position) * distanceWeight;

        // Distancia Pusheo
        if (true)//!orangeDoor.doorsUp)
            tacticReward = Mathf.Abs(fruitScript.transform.position.z) / 2f;//3.6f;                                          //tacticReward = Mathf.Pow(fruitScript.transform.position.z, 2) * pushingValue + pushingBaseValue;
        else
            tacticReward = Vector3.Distance(fruitScript.transform.position, orangeDoor.transform.position) / 7.2f;      //tacticReward = 500000f / Mathf.Pow(Vector3.Distance(fruitScript.transform.position, purpleDoor.transform.position), 2);

        //Proporción minions que empujan
        tacticReward += playerMinionsPushing / (enemyMinionsPushing + 1f) * pushPonderValue;  //tacticReward += playerMinionsPushing / (enemyMinionsPushing + 1f / int.MaxValue); //Urgencia

        //Ventaja de partida
        tacticReward += GameAdvantage();

        //Base
        //tacticReward += pushBaseValue;

        // Constructing STRATEGY
        strategyPlan.Push(new Tactic(tacticCost, tacticReward, fruitMesh, false, necessaryMinions));
        if (buffTactic != null) strategyPlan.Push(buffTactic);
        foreach (Tactic tc in gathering) // sub-tactics
            strategyPlan.Push(tc);

        return new Strategy(strategyPlan);
    }

    private Strategy GetAttackDoorStrategy()
    {
        float tacticCost = 0;
        float tacticReward = 0;
        int necessaryMinions = 0;
        Stack<Tactic> strategyPlan = new Stack<Tactic>();
        Tactic buffTactic;

        if (orangeDoor.doorsUp)
        {
            List<Tactic> gathering = GatherAllOptimalMinionsToCall(Names.OBJECTIVE_ATTACK_DOOR);

            foreach (Tactic tc in gathering)
                necessaryMinions += Mathf.FloorToInt(tc.targetElement.GetComponent<Peloton>().Size());

            buffTactic = GetBuffTactic(Names.ATTACK_BUFF);
            if (buffTactic == null)
                buffTactic = GetBuffTactic(Names.DEFENSE_BUFF);
            if (buffTactic == null) buffTactic = GetBuffTactic(Names.MOVEMENT_BUFF);


            tacticCost = necessaryMinions * minionWeight;
            tacticCost += Vector3.Distance(enemyLeader.transform.position, orangeDoor.transform.position) * distanceWeight;
            foreach (Peloton p in GetPelotonsByObjective(Names.ENEMY_LEADER, Names.OBJECTIVE_ATTACK_DOOR))
            {
                tacticCost += p.Size() * minionWeight;
            }


            // Melon position
            tacticReward = Vector3.Distance(purpleDoor.transform.position, fruitScript.transform.position) / 7.2f; //   2 * 360/100      //tacticReward = 1f/Vector3.Distance(fruitScript.transform.position, orangeDoor.transform.position);
            tacticReward += GameAdvantage();

            // Constructing STRATEGY
            strategyPlan.Push(new Tactic(tacticCost, tacticReward, orangeDoor.gameObject, Random.value > 0.5f, necessaryMinions));
            if (buffTactic != null) strategyPlan.Push(buffTactic);
            foreach (Tactic tc in gathering) // sub-tactics
                strategyPlan.Push(tc);
        }

        return new Strategy(strategyPlan);
    }

    private List<Strategy> GetDefendDoorStrategy()
    {
        List<Strategy> options = new List<Strategy>();

        float tacticCost = 0;
        float tacticReward = 0;
        int necessaryMinions = 0;
        int remainingMinions = 0;
        Stack<Tactic> strategyPlan = new Stack<Tactic>();
        List<Tactic> gathering = new List<Tactic>();
        Tactic buffTactic;

        List<Peloton> threats = GetPelotonsByObjective(Names.PLAYER_LEADER, Names.OBJECTIVE_ATTACK_DOOR);
        foreach (Peloton p in threats)
        {
            necessaryMinions = 5;
            necessaryMinions += Mathf.FloorToInt(p.Size() * 1.2f);

            buffTactic = GetBuffTactic(Names.ATTACK_BUFF);
            if (buffTactic == null)
                buffTactic = GetBuffTactic(Names.DEFENSE_BUFF);
            if (buffTactic == null)
                buffTactic = GetBuffTactic(Names.MOVEMENT_BUFF);

            if (buffTactic != null)
                necessaryMinions += buffTactic.cantMinions;

            // Get those MINIONS!!
            remainingMinions = necessaryMinions - enemyLeaderScript.myPeloton.Size();
            if (remainingMinions > 0) gathering = MinionGathering(remainingMinions, Names.OBJECTIVE_ATTACK_DOOR, p.gameObject);

            // Tactic COST
            tacticCost = necessaryMinions * minionWeight;
            tacticCost += Vector3.Distance(enemyLeader.transform.position, purpleDoor.transform.position) * distanceWeight;

            //Tactic REWARD
            tacticReward = p.Size() * minionWeight;
            tacticReward += Vector3.Distance(fruitScript.transform.position, orangeDoor.transform.position) / 7.2f;
            tacticReward += purpleDoor.health / purpleDoor.maxHealth * 100f;

            // Plan Building
            strategyPlan.Push(new Tactic(tacticCost, tacticReward, p.gameObject, true, necessaryMinions));
            strategyPlan.Push(buffTactic);
            foreach (Tactic tc in gathering)
                strategyPlan.Push(tc);

            options.Add(new Strategy(strategyPlan));
        }

        return options;
    }

    private List<Strategy> GetAttackNearbyEnemiesStrategy()
    {
        List<Strategy> options = new List<Strategy>();

        float tacticCost = 0;
        float tacticReward = 0;
        int necessaryMinions = 0;
        int remainingMinions = 0;
        Stack<Tactic> strategyPlan = new Stack<Tactic>();
        List<Tactic> gathering = new List<Tactic>();
        Tactic buffTactic;

        List<Peloton> threats = GetNearbyEnemies(enemyLeaderScript.myPeloton);
        foreach (Peloton p in threats)
        {

            necessaryMinions = 5;
            necessaryMinions += Mathf.FloorToInt(p.Size() * 1.2f);

            // Get those MINIONS!!
            remainingMinions = necessaryMinions - enemyLeaderScript.myPeloton.Size();
            if (remainingMinions > 0) gathering = MinionGathering(remainingMinions, Names.OBJECTIVE_ATTACK, p.gameObject);

            buffTactic = GetBuffTactic(Names.ATTACK_BUFF);
            if (buffTactic == null)
                buffTactic = GetBuffTactic(Names.DEFENSE_BUFF);
            if (buffTactic == null)
                buffTactic = GetBuffTactic(Names.MOVEMENT_BUFF);

            tacticCost = necessaryMinions * minionWeight;
            tacticCost += Vector3.Distance(enemyLeader.transform.position, purpleDoor.transform.position) * distanceWeight;

            tacticReward = p.Size() * minionWeight;
            tacticReward += Vector3.Distance(fruitScript.transform.position, orangeDoor.transform.position) / 7.2f;
            tacticReward += purpleDoor.health / purpleDoor.maxHealth * 100f;

            strategyPlan.Push(new Tactic(tacticCost, tacticReward, p.gameObject, true, necessaryMinions));
            strategyPlan.Push(buffTactic);
            foreach (Tactic tc in gathering)
                strategyPlan.Push(tc);

            options.Add(new Strategy(strategyPlan));
        }

        return options;
    }


    private List<Tactic> MinionGathering(int remainingMinions, string objective, GameObject targetElement)
    {
        List<Tactic> gathering = new List<Tactic>();
        if (remainingMinions > 0)
        {
            gathering = SearchMinionsToCall(remainingMinions, objective, targetElement);
            gathering.Sort((c1, c2) => (int)(c2.determination * (c2.targetElement.GetComponent<Peloton>().objective == Names.OBJECTIVE_DEFEND ? 2f : 1f) - c1.determination * (c1.targetElement.GetComponent<Peloton>().objective == Names.OBJECTIVE_DEFEND ? 2f : 1f))); // Chapuza para priorizar los que están en objetivo defend // Comentario por si acaso esto no tira, ja, ilusa, yolanda... ¿de verdad crees que no me va a tirar..?
            int minionCount = 0;
            int index = 0;
            while (minionCount < remainingMinions && index < gathering.Count)
            {
                minionCount += gathering[index].targetElement.GetComponent<Peloton>().Size();
                index++;
            }
            gathering.RemoveRange(index, gathering.Count - index);
        }

        return TacticRouteOptimization(gathering);
    }

    private List<Tactic> SearchMinionsToCall(int cant, string objective, GameObject targetElement)
    {
        List<Tactic> recruits = new List<Tactic>();
        foreach (Peloton p in enemyTeam)
        {
            if (p != enemyLeaderScript.myPeloton && p.name != Names.ENEMY_LEADER_PELOTON
                && p.objective != objective && p.targetElement != targetElement)
            {
                float minionReward = p.Size() * minionWeight;
                float distanceCost = Vector3.Distance(enemyLeader.transform.position, p.transform.position) * distanceWeight;
                if (minionReward > distanceCost)
                {
                    recruits.Add(new Tactic(distanceCost, minionReward, p.gameObject, true, 0));
                }
            }
        }
        return recruits;
    }

    private List<Tactic> GatherAllOptimalMinionsToCall(string objective)
    {
        List<Tactic> recruits = new List<Tactic>();
        foreach (Peloton p in enemyTeam)
        {
            if (p != enemyLeaderScript.myPeloton && p.name != Names.ENEMY_LEADER_PELOTON && p.objective != objective)
            {
                float minionReward = p.Size() * minionWeight;
                float distanceCost = Vector3.Distance(enemyLeader.transform.position, p.transform.position) * distanceWeight;
                if (minionReward > distanceCost)
                {
                    recruits.Add(new Tactic(distanceCost, minionReward, p.gameObject, true, 0));
                }
            }
        }

        return TacticRouteOptimization(recruits); 
    }

    private Tactic GetBuffTactic(string buffType)
    {
        List<Tactic> campTactics = new List<Tactic>();

        int necessaryMinions;
        float tacticCost;
        float tacticReward;

        

        foreach (Camp c in camps)
        {
            bool discard = false;
            List<Peloton> encargados = GetPelotonsByObjective(Names.ENEMY_LEADER, Names.OBJECTIVE_ATTACK_CAMP);
            foreach (Peloton p in encargados)
            {
                if (p.targetElement == c)
                {
                    discard = true;
                    break;
                }
            }

            if (c.buffType == buffType && c.units.Count > 0 && !discard)
            {
                tacticCost = Vector3.Distance(enemyLeader.transform.position, c.transform.position) * distanceWeight;
                necessaryMinions = 5 * c.units.Count;
                tacticCost += necessaryMinions * minionWeight;
                tacticReward = 25f + 1.5f * GetLeaderMinionsCount(Names.ENEMY_LEADER);
                campTactics.Add(new Tactic(tacticCost, tacticReward, c.gameObject, Random.value * enemyLeaderScript.health > 200, necessaryMinions));
            }
        }

        if(campTactics.Count > 0)
        {
            Tactic finalBuffTactic = campTactics[0];
            foreach (Tactic tc in campTactics)
                if (tc.determination > finalBuffTactic.determination)
                    finalBuffTactic = tc;
            if (finalBuffTactic.determination > 0f) // 50 para que vaya a por el kiwi óptimamente
                return finalBuffTactic;
        }

        return null; //new Tactic(0, 0, null, false, 0);
    }

    private float GameAdvantage()
    {
        float gAdv = 0;

        //gAdv = Mathf.Pow(GetTeamMinionsCount(Names.ENEMY_LEADER), 2) / GetTeamMinionsCount(Names.PLAYER_LEADER) * minionWeight;
        //gAdv += GetAlignedTotemsCount(Names.ENEMY_LEADER) * totemsValue;

        gAdv = (GetTeamMinionsCount(Names.ENEMY_LEADER) - GetTeamMinionsCount(Names.PLAYER_LEADER)) * 3.3f;
        gAdv += GetAlignedTotemsCount(Names.ENEMY_LEADER) * totemsValue;

        if (gAdv < 0) gAdv = 0;

        return gAdv;
    }

    private List<Tactic> TacticRouteOptimization(List<Tactic> plan)
    {
        if (plan.Count == 0)
            return plan;

        List<Tactic> finalPlan = new List<Tactic>();
        Tactic lastTactic = new Tactic(0, 0, enemyLeader, false, 0);
        Tactic nearestTactic = plan[0];

        int iterations = plan.Count;

        for (int i = 0; i < iterations; i++)
        {
            /*foreach (Tactic tc in plan)
            {
                if (Vector3.Distance(tc.targetElement.transform.position, lastTactic.targetElement.transform.position) < Vector3.Distance(nearestTactic.targetElement.transform.position, lastTactic.targetElement.transform.position))
                    nearestTactic = tc;
            }
            finalPlan.Add(nearestTactic);
            plan.Remove(nearestTactic);
            lastTactic = nearestTactic;*/

            int tci = 0;
            while(tci < plan.Count)
            {
                if (Vector3.Distance(plan[tci].targetElement.transform.position, lastTactic.targetElement.transform.position) < Vector3.Distance(nearestTactic.targetElement.transform.position, lastTactic.targetElement.transform.position))
                    nearestTactic = plan[tci];
                tci++;
            }
            finalPlan.Add(nearestTactic);
            plan.Remove(nearestTactic);
            lastTactic = nearestTactic;
        }

        return finalPlan;
    }
}
