using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Spawner : MonoBehaviour {

    AIManager aiManager;

    static int INITIAL_SPAWN = 10;
    Peloton currentPeloton;

    bool preparingPeloton = false;

    int pelotonSize = 5;
    float pelotonSpawnTime = 15f;
    float minionSpawnTime;
    float minimumCooldown = 1f;
    

    int totemsCount;
    float cooldownSpawn = 0f;

    public GameObject leaderOwner;

	// Use this for initialization
	void Start () {

        minionSpawnTime = pelotonSpawnTime / pelotonSize;

        aiManager = GameObject.Find("AIManager").GetComponent<AIManager>();
        totemsCount = aiManager.GetTotemCount();
        cooldownSpawn = minionSpawnTime - aiManager.GetAlignedTotemsCount(leaderOwner.name) * (minionSpawnTime - minimumCooldown) / totemsCount;

        SpawnPeloton();
        for (int i = 0; i < INITIAL_SPAWN; i++) SpawnMinion();
        DepartPeloton();
	}
	
	void Update () {

        if (Time.timeSinceLevelLoad >= 80f/*11f * 60f*/)
        {
            pelotonSize = 12;
            minionSpawnTime = pelotonSpawnTime / pelotonSize;
            minimumCooldown = 0.5f;
        }
        else if (Time.timeSinceLevelLoad >= 40f/*5f * 60f*/)
        {
            pelotonSize = 8;
            minionSpawnTime = pelotonSpawnTime / pelotonSize;
            minimumCooldown = 0.75f;
        }


        cooldownSpawn -= Time.deltaTime;

        if(cooldownSpawn <= 0)
        {
            if (!preparingPeloton)
            {
                SpawnPeloton();
            }
            else if (currentPeloton == null)
            {
                SpawnPeloton();
            }

            if (aiManager.GetTeamMinionsCount(leaderOwner.name) < aiManager.MAXIMUM_MINIONS)
                SpawnMinion();
            else
                DepartPeloton();

            if(currentPeloton.Size() == pelotonSize)
            {
                DepartPeloton();
            }

            cooldownSpawn = minionSpawnTime - aiManager.GetAlignedTotemsCount(leaderOwner.name) * (minionSpawnTime - minimumCooldown) / totemsCount;
        }
	}

    private void SpawnPeloton()
    {
        preparingPeloton = true;

        GameObject newPeloton = new GameObject();
        //newPeloton.name = leaderOwner.name + "Peloton";
        newPeloton.name = leaderOwner.name == Names.PLAYER_LEADER ? Names.PLAYER_PELOTON : Names.ENEMY_PELOTON;
        currentPeloton = newPeloton.AddComponent<Peloton>();
        currentPeloton.SetLeader(leaderOwner);                //Leader
        newPeloton.transform.position = transform.position;   //Posición Inicial
        aiManager.AddPeloton(currentPeloton);                 //Avisar al AIManager
        currentPeloton.SetObjective("Idle");                  //Objetivo
        //currentPeloton = newPelotonScript;
    }
    private void SpawnMinion()
    {
        GameObject newMinion;
        if(leaderOwner.name == Names.PLAYER_LEADER)
            newMinion = (GameObject)GameObject.Instantiate((GameObject)Resources.Load("Prefabs/OrangeOnioni"), transform.position + new Vector3(Random.Range(-10, 10), 0, Random.Range(-10, 10)), Quaternion.identity);
        else
            newMinion = (GameObject)GameObject.Instantiate((GameObject)Resources.Load("Prefabs/PurpleOnioni"), transform.position + new Vector3(Random.Range(-10, 10), 0, Random.Range(-10, 10)), Quaternion.identity);

        currentPeloton.AddMinion(newMinion.GetComponent<Minion>());
    }
    private void DepartPeloton()
    {
        Leader leaderOwnerScript = AIManager.staticManager.GetLeaderByName(leaderOwner.name);
        //GameObject Flag = GameObject.Find(leaderOwner.name + "Flag");
        if (leaderOwnerScript.hasFlag) currentPeloton.SetObjective("FollowLeader", leaderOwner);
        else currentPeloton.SetObjective("GoTo", GameObject.Find(leaderOwner.name + "Flag").transform.position); //Objetivo
        preparingPeloton = false;
    }
}
