using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Leader : MonoBehaviour {

    protected AIManager aiManager;

    //LeaderMovement leaderMovement;
    //Cursor cursor; //ONLY PLAYER
    public Peloton myPeloton;
    public Vector3 behind;
    public Vector3 velocity = new Vector3();

    //ONLY PLAYER
    /*float callTime = 0f;
    float callRadius = 0f;
    static float CALL_RADIUS_SCALE = 30f;
    Projector callArea;*/

    public static float BEHIND_DIST = 12f;

	// Use this for initialization
	public virtual void Start ()
    {
        aiManager = GameObject.Find("AIManager").GetComponent<AIManager>();

        behind = transform.position + transform.forward * -BEHIND_DIST;

        GameObject leaderPeloton = new GameObject();
        leaderPeloton.name = gameObject.name + "Peloton";
        myPeloton = leaderPeloton.AddComponent<Peloton>();
        myPeloton.SetLeader(gameObject);                     //Leader
        myPeloton.SetObjective("FollowLeader", gameObject);  //Objetivo
        myPeloton.transform.position = behind;               //Posición Inicial
        //aiManager.AddPlayerPeloton(myPeloton);               //Avisar al AIManager
    }

    protected void MyStart()
    {
        /*aiManager = GameObject.Find("AIManager").GetComponent<AIManager>();

        gameObject.AddComponent<LeaderMovement>();
        //leaderMovement = gameObject.GetComponent<LeaderMovement>();
        cursor = GameObject.Find("Cursor").GetComponent<Cursor>();
        cursor.SetLeader(gameObject);

        behind = transform.position + transform.forward * -BEHIND_DIST;

        GameObject leaderPeloton = new GameObject();
        leaderPeloton.name = gameObject.name + "Peloton";
        myPeloton = leaderPeloton.AddComponent<Peloton>();
        myPeloton.SetLeader(gameObject);                     //Leader
        myPeloton.SetObjective("FollowLeader", gameObject);  //Objetivo
        myPeloton.transform.position = behind;               //Posición Inicial
        aiManager.AddPlayerPeloton(myPeloton);               //Avisar al AIManager

        //GameObject.Instantiate(myPeloton, behind, Quaternion.identity);

        callArea = gameObject.GetComponentInChildren<Projector>();*/
    }


    public virtual void Update()
    {

    }

    public virtual void FixedUpdate()
    {
        behind = transform.position + transform.forward * BEHIND_DIST;
        if (Physics.Raycast(transform.position, behind - transform.position, Vector3.Distance(transform.position, behind), LayerMask.GetMask("Level")))
        behind = transform.position - transform.forward * BEHIND_DIST;
        //MyFixedUpdate();
    }

    protected void MyFixedUpdate()
    {
        /*behind = transform.position + transform.forward * BEHIND_DIST;
        ManageCursor();
        MinionCall();*/
    }


    /*private void ManageCursor()
    {
        // make cursor appear / disappear
        if (Input.GetKeyDown(KeyCode.LeftShift)) // CHANGE INPUT
        {
            if (cursor.GetCursorActive()) cursor.Disappear();
            else cursor.Appear();
        }
    }*/

    public void NewOrder(int cant, Vector3 targetPosition)
    {
        GameObject newPeloton = new GameObject();
        newPeloton.name = gameObject.name == Names.PLAYER_LEADER ? Names.PLAYER_PELOTON : Names.ENEMY_PELOTON;
        Peloton newPelotonScript = newPeloton.AddComponent<Peloton>();
        newPelotonScript.SetLeader(gameObject);                 //Leader
        newPeloton.transform.position = behind;                 //Posición Inicial
        aiManager.AddPlayerPeloton(newPelotonScript);           //Avisar al AIManager
        newPelotonScript.SetObjective("GoTo", targetPosition);  //Objetivo

        //Repartimiento de Minions
        List<Minion> leaderPeloton = myPeloton.GetMinionListSorted(targetPosition);
        for (int i = 0; i < cant; i++){
            newPelotonScript.AddMinion(leaderPeloton[0]);
            myPeloton.RemoveMinion(leaderPeloton[0]);
        }

    }
    public void NewOrder(int cant, GameObject targetElement)
    {
        GameObject newPeloton = new GameObject();
        newPeloton.name = gameObject.name == Names.PLAYER_LEADER ? Names.PLAYER_PELOTON : Names.ENEMY_PELOTON;
        Peloton newPelotonScript = newPeloton.AddComponent<Peloton>();
        newPelotonScript.SetLeader(gameObject);                     //Leader
        newPeloton.transform.position = behind;                     //Posición Inicial
        aiManager.AddPlayerPeloton(newPelotonScript);               //Avisar al AIManager
        string objective = "";

        switch (targetElement.name)
        {
            case Names.TOTEM:
                objective = Names.INTERACT;
                break;

            case Names.FRUIT:
                objective = Names.PUSH;
                break;
        }

        newPelotonScript.SetObjective(objective, targetElement);   //Objetivo

        //Repartimiento de Minions
        List<Minion> leaderPeloton = myPeloton.GetMinionListSorted(targetElement.transform.position);
        for (int i = 0; i < cant; i++)
        {
            newPelotonScript.AddMinion(leaderPeloton[0]);
            myPeloton.RemoveMinion(leaderPeloton[0]);
        }

    }

    /*private void MinionCall()
    {
        callArea.orthographicSize = 0f;
        if (Input.GetKeyDown(KeyCode.Space))
        {
            callTime = 0f;
            callRadius = 0f;
        }
        else if (Input.GetKey(KeyCode.Space))
        {
            callTime += Time.deltaTime;
            callRadius = callTime * callTime * CALL_RADIUS_SCALE;
            callArea.orthographicSize = callRadius;
        }
        if (Input.GetKeyUp(KeyCode.Space))
        {
            callTime = 0f;

            List<Minion> minionsInRange = aiManager.GetMinionsInRange(callRadius, transform.position, Names.PLAYER_LEADER);
            foreach(Minion m in minionsInRange)
            {
                m.AbandonPeloton();
                this.myPeloton.AddMinion(m);
            }

            Debug.Log("+" + minionsInRange.Count);
            callRadius = 0f;
        }
    }*/
}
