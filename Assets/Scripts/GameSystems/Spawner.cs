﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using BehaviourMachine;

public class Spawner : MonoBehaviour {

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

        totemsCount = AIManager.staticManager.GetTotemCount();
        cooldownSpawn = minionSpawnTime - AIManager.staticManager.GetAlignedTotemsCount(leaderOwner.name) * (minionSpawnTime - minimumCooldown) / totemsCount;

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
            if (AIManager.staticManager.GetTeamMinionsCount(leaderOwner.name) < AIManager.staticManager.MAXIMUM_MINIONS)
            {
                if (!preparingPeloton || currentPeloton == null)
                {
                    SpawnPeloton();
                }
                SpawnMinion();
            }
            else if (preparingPeloton && currentPeloton != null)
                DepartPeloton();

            if(currentPeloton.Size() == pelotonSize)
            {
                DepartPeloton();
            }

            cooldownSpawn = minionSpawnTime - AIManager.staticManager.GetAlignedTotemsCount(leaderOwner.name) * (minionSpawnTime - minimumCooldown) / totemsCount;
        }
	}

    private void SpawnPeloton()
    {
        preparingPeloton = true;

        //GameObject newPeloton = new GameObject();
        GameObject newPeloton = (GameObject)GameObject.Instantiate((GameObject)Resources.Load("Prefabs/Peloton"), transform.position, Quaternion.identity);
        //newPeloton.name = leaderOwner.name + "Peloton";
        newPeloton.name = leaderOwner.name == Names.PLAYER_LEADER ? Names.PLAYER_PELOTON : Names.ENEMY_PELOTON;
        newPeloton.GetComponent<BehaviourTree>().enabled = false;
        //currentPeloton = newPeloton.AddComponent<Peloton>();
        currentPeloton = newPeloton.GetComponent<Peloton>();
        currentPeloton.SetLeader(leaderOwner);                                      //Leader
        newPeloton.transform.position = transform.position;                         //Posición Inicial
        AIManager.staticManager.AddPeloton(currentPeloton);                                       //Avisar al AIManager
        currentPeloton.SetObjective("Spawning");                                    //Objetivo
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
        currentPeloton.GetComponent<BehaviourTree>().enabled = true;
        if (leaderOwnerScript.hasFlag) currentPeloton.SetObjective(Names.OBJECTIVE_FOLLOW_LEADER, leaderOwner);
        else currentPeloton.SetObjective(Names.OBJECTIVE_DEFEND, GameObject.Find(leaderOwner.name + "Flag").transform.position); //Objetivo
        preparingPeloton = false;
    }
}
