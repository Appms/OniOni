using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class EnemyLeaderStateMachine : MonoBehaviour {

    EnemyLeader enemyLeader;

    Strategy currentStrategy = null;
    public string currentState = Names.STATE_IDLE;
    int treeLevel = 0;
    bool jobDone = false;
    bool invalidTask = false;

    float REACH_DIST = 10f;

    // Use this for initialization
    void Start () {
        enemyLeader = GetComponent<EnemyLeader>();
	}
	
	// Update is called once per frame
	void Update () {
        if(currentStrategy != null) CheckConcurrence();
        if (treeLevel == 0)
        {
            if (currentState == Names.STATE_IDLE)
            {
                //Clean something?
                jobDone = false;
                invalidTask = false;

                List<Strategy> options = AIManager.staticManager.GetAIStrategies();
                currentStrategy = options[0];
                foreach (Strategy s in options)
                    if (s.determination > currentStrategy.determination)
                        currentStrategy = s;

                //-------------------TRANSITION------------------------
                currentState = Names.STATE_DECIDING;
                treeLevel = 1;
                //-----------------------------------------------------
            }
            else
            {
                Debug.Log("ERROR - Unvalid State for treeLevel 0");
            }
        }
        else if (treeLevel == 1)
        {
            if (currentState == Names.STATE_DECIDING)
            {
                if (jobDone || invalidTask)
                {
                    currentStrategy.plan.Pop();
                    if (currentStrategy.plan.Count <= 0)
                    {
                        currentState = Names.STATE_IDLE;
                        treeLevel = 0;
                    }
                }
                else
                {
                    //-------------------TRANSITION------------------------
                    if (currentStrategy.plan.Peek().requiresLeader)
                    {
                        currentState = Names.STATE_GO_TO;
                    }
                    else //if (!currentStrategy.plan.Peek().requiresLeader)
                    {
                        currentState = Names.STATE_SEND_ORDER;
                    }
                    treeLevel = 2;
                    //------------------------------------------------------
                }
            }
            else
            {
                Debug.Log("ERROR - Unvalid State for treeLevel 1");
            }
        }
        else if (treeLevel == 2)
        {
            if (currentState == Names.STATE_GO_TO)
            {
                if (Vector3.Distance(transform.position, currentStrategy.plan.Peek().targetElement.transform.position) > REACH_DIST)
                {
                    if(!enemyLeader.goingTo || Vector3.Distance(currentStrategy.plan.Peek().targetElement.transform.position, currentStrategy.plan.Peek().targetPosition) > REACH_DIST)
                    {
                        enemyLeader.SearchPath(currentStrategy.plan.Peek().targetElement.transform.position);
                        currentStrategy.plan.Peek().targetPosition = currentStrategy.plan.Peek().targetElement.transform.position;
                    }
                    else if(!enemyLeader.calculatingPath)
                    {
                        enemyLeader.FollowPath();
                    }
                    /*else
                    {
                        //Está calculando el camino
                    }*/
                }
                else
                {
                    //REACHED
                    //-------------------TRANSITION------------------------
                    currentState = GetStateFromTargetElementName(currentStrategy.plan.Peek().targetElement.name);
                    treeLevel = 3;
                    //------------------------------------------------------
                }
            }
            else if (currentState == Names.STATE_SEND_ORDER)
            {
                enemyLeader.NewOrder(currentStrategy.plan.Peek().cantMinions, currentStrategy.plan.Peek().targetElement);
                //-------------------TRANSITION------------------------
                currentState = Names.STATE_DECIDING;
                treeLevel = 1;
                jobDone = true;
                //------------------------------------------------------
            }
            else
            {
                Debug.Log("ERROR - Unvalid State for treeLevel 2");
            }
        }
        else if (treeLevel == 3)
        {
            if (currentState == Names.STATE_CONQUER)
            {
                if (currentStrategy.plan.Peek().targetElement.GetComponent<Totem>().alignment == -50)
                {
                    //-------------------TRANSITION------------------------
                    currentState = Names.STATE_DECIDING;
                    treeLevel = 1;
                    jobDone = true;
                    //------------------------------------------------------
                }
            }
            else if (currentState == Names.STATE_ATTACK){

            }
            else
            {
                Debug.Log("ERROR - Unvalid State for treeLevel 3");
            }
        }
        else
        {
            Debug.Log("ERROR - Unvalid treeLevel");
        }
	}



    private void CheckConcurrence()
    {
        if(currentStrategy.plan.Count <= 0
           || currentStrategy.plan.Peek() == null
           || currentStrategy.plan.Peek().targetElement == null)
        {
            ResetTree();
        }
    }

    private void ResetTree()
    {
        currentStrategy = null;
        currentState = Names.STATE_IDLE;
        treeLevel = 0;
    }



    private string GetStateFromTargetElementName(string targetElementName)
    {
        switch (targetElementName)
        {
            case Names.TOTEM:
                return Names.STATE_CONQUER;
            case "Fruit":
                return Names.STATE_PUSH;
            case Names.PLAYER_DOOR:
                return Names.STATE_ATTACK_DOOR;
            case Names.CAMP:
                return Names.STATE_ATTACK_CAMP;
            case Names.ENEMY_PELOTON:
                return Names.STATE_RECRUIT;
            case Names.PLAYER_PELOTON:
                return Names.STATE_ATTACK;
            case Names.PLAYER_LEADER_PELOTON:
                return Names.STATE_ATTACK;
            case Names.PLAYER_LEADER:
                return Names.STATE_ATTACK;
        }
        Debug.Log("ERROR - Couldn't identify state from targetElement");
        return "ERROR";
    }
}
