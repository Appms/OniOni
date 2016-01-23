using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class EnemyLeaderStateMachine : MonoBehaviour {

    EnemyLeader enemyLeader;

    Strategy currentStrategy = null;
    public int treeLevel = 0;
    public string currentState = Names.STATE_IDLE;
    bool jobDone = false;

    float REACH_DIST = 10f;


    // Use this for initialization
    void Start () {
        enemyLeader = GetComponent<EnemyLeader>();
	}
	
	// Update is called once per frame
	void Update () {
        if (currentStrategy != null) CheckConcurrence();
        else ResetTree();
        if (treeLevel == 0)
        {
            if (currentState == Names.STATE_IDLE)
            {
                //Clean something?
                jobDone = false;

                List<Strategy> options = AIManager.staticManager.GetAIStrategies();
                currentStrategy = options[0];
                foreach (Strategy s in options)
                {
                    if (s.determination > currentStrategy.determination)
                        currentStrategy = s;
                }
                Debug.Log("New Tactic: " + currentStrategy.plan.Peek().targetElement.name + " " + currentStrategy.plan.Peek().targetElement.transform.position);

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
                if (jobDone)
                {
                    currentStrategy.plan.Pop();
                    if (currentStrategy.plan.Count <= 0)
                    {
                        //-------------------TRANSITION------------------------
                        currentState = Names.STATE_IDLE;
                        treeLevel = 0;
                        //-----------------------------------------------------
                    }
                    else
                        Debug.Log("New Tactic: " + currentStrategy.plan.Peek().targetElement.name + " " + currentStrategy.plan.Peek().targetElement.transform.position);
                }
                else
                {
                    enemyLeader.leaderTarget = currentStrategy.plan.Peek().targetElement;

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
                Debug.Log("Order Sent: " + currentStrategy.plan.Peek().targetElement.name + " " + currentStrategy.plan.Peek().targetElement.transform.position);
                if (!enemyLeader.NewOrder(currentStrategy.plan.Peek().cantMinions, currentStrategy.plan.Peek().targetElement))
                    ResetTree();
                else
                {
                    //-------------------TRANSITION------------------------
                    TacticCompleted();
                    //------------------------------------------------------
                }
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
                    TacticCompleted();
                    //------------------------------------------------------
                }
            }
            else if (currentState == Names.STATE_PUSH)
            {
                Debug.Log("ERROR - The Leader shouldn't be pushing the fruit personally.");
                ResetTree();
            }
            else if(currentState == Names.STATE_ATTACK_DOOR)
            {
                if (!currentStrategy.plan.Peek().targetElement.GetComponent<Door>().doorsUp)
                {
                    //-------------------TRANSITION------------------------
                    TacticCompleted();
                    //------------------------------------------------------
                }
                else { enemyLeader.MoveTo(currentStrategy.plan.Peek().targetElement.transform.position); }
            }
            else if(currentState == Names.STATE_RECRUIT)
            {
                //currentStrategy.plan.Peek().targetElement.GetComponent<Peloton>().SetObjective(Names.OBJECTIVE_FOLLOW_LEADER, gameObject);
                foreach (Minion m in currentStrategy.plan.Peek().targetElement.GetComponent<Peloton>().GetMinionList())
                {
                    m.AbandonPeloton();
                    enemyLeader.myPeloton.AddMinion(m);
                }
                //-------------------TRANSITION------------------------
                TacticCompleted();
                //------------------------------------------------------
            }
            else if(currentState == Names.STATE_ATTACK_CAMP)
            {
                if(currentStrategy.plan.Peek().targetElement.GetComponent<Camp>().units.Count <= 0)
                {
                    //-------------------TRANSITION------------------------
                    TacticCompleted();
                    //------------------------------------------------------
                }
                else { enemyLeader.MoveTo(currentStrategy.plan.Peek().targetElement.GetComponent<Camp>().units[0].transform.position); }
            }
            else if (currentState == Names.STATE_ATTACK){
                if (currentStrategy.plan.Peek().targetElement == null)
                {
                    //-------------------TRANSITION------------------------
                    TacticCompleted();
                    //------------------------------------------------------
                }
                else { enemyLeader.MoveTo(currentStrategy.plan.Peek().targetElement.transform.position); }
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
        if(currentStrategy.plan.Count > 0 &&
           (currentStrategy.plan.Peek() == null
           || currentStrategy.plan.Peek().targetElement == null
           || currentStrategy.plan.Peek().targetPosition == null))
        {
            ResetTree();
        }
    }

    private void ResetTree()
    {
        currentStrategy = null;
        currentState = Names.STATE_IDLE;
        treeLevel = 0;
        jobDone = false;
        Debug.Log("Reset Tree.");
    }

    private void TacticCompleted()
    {
        Debug.Log("Tactic completed: " + currentStrategy.plan.Peek().targetElement.name + " " + currentStrategy.plan.Peek().targetElement.transform.position);
        currentState = Names.STATE_DECIDING;
        treeLevel = 1;
        jobDone = true;
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
