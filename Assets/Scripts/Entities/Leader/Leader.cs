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

    int health = 360;
    float happiness = 1;
    string weapon = Names.WEAPON_SWORD;
    int base_atk = 7;
    float crit_chance = 0.43f;
    float atkCooldown = 0f;

    public float defenseBuff = 0;
    public float attackBuff = 0;
    public float movementBuff = 0;
    public float pushBuff = 0;

    public readonly float BUFF_MULTIPLYER = 1.5f;
    const float BUFF_VALUE = 70f;

    public static float BEHIND_DIST = 12f;

    static float BASE_MOVEMENT_SPEED = 30f;

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

        // WEAPON ASSIGNMENT
        switch (Random.Range(0, 2))
        {
            case 0: //Axe
                base_atk = 3*6;
                crit_chance = 0.66f;
                weapon = Names.WEAPON_AXE;
                break;
            case 1: //Sword
                base_atk = 3*7;
                crit_chance = 0.43f;
                weapon = Names.WEAPON_SWORD;
                break;
            case 2: //Club
                base_atk = 3*8;
                crit_chance = 0.11f;
                weapon = Names.WEAPON_CLUB;
                break;
        }
    }

    public virtual void Update()
    {
        DecreaseBuffs();
        ApplyDefenseBuff();
    }

    public virtual void FixedUpdate()
    {
        behind = transform.position + transform.forward * BEHIND_DIST;
        if (Physics.Raycast(transform.position, behind - transform.position, Vector3.Distance(transform.position, behind), LayerMask.GetMask("Level")))
        behind = transform.position - transform.forward * BEHIND_DIST;
    }


    // MY FUNCTIONS --------------------------------------------------

    public void AssignWeapon(string newWeapon)
    {
        weapon = newWeapon;
        switch (weapon)
        {
            case Names.WEAPON_AXE: //Axe
                base_atk = 6 * 3;
                crit_chance = 0.66f;
                break;
            case Names.WEAPON_SWORD: //Sword
                base_atk = 7 * 3;
                crit_chance = 0.43f;
                break;
            case Names.WEAPON_CLUB: //Club
                base_atk = 8 * 3;
                crit_chance = 0.11f;
                break;
        }
    }

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
        newPelotonScript.SetObjective("Interact", targetElement);   //Objetivo

        //Repartimiento de Minions
        List<Minion> leaderPeloton = myPeloton.GetMinionListSorted(targetElement.transform.position);
        for (int i = 0; i < cant; i++)
        {
            newPelotonScript.AddMinion(leaderPeloton[0]);
            myPeloton.RemoveMinion(leaderPeloton[0]);
        }
    }

    private void DecreaseBuffs()
    {
        if (defenseBuff > 0) defenseBuff -= Time.deltaTime;
        if (attackBuff > 0) attackBuff -= Time.deltaTime;
        if (movementBuff > 0) movementBuff -= Time.deltaTime;
        if (pushBuff > 0) pushBuff -= Time.deltaTime;
    }

    public void RecieveBuff(string buffType)
    {
        switch (buffType)
        {
            case Names.DEFENSE_BUFF:
                defenseBuff = BUFF_VALUE;
                break;
            case Names.ATTACK_BUFF:
                attackBuff = BUFF_VALUE;
                break;
            case Names.MOVEMENT_BUFF:
                movementBuff = BUFF_VALUE;
                break;
            case Names.PUSH_BUFF:
                pushBuff = BUFF_VALUE;
                break;
        }
    }

    private void ApplyDefenseBuff()
    {
        health = Mathf.FloorToInt(health * (defenseBuff > 0 ? BUFF_MULTIPLYER : 1f));
    }

    protected float GetMaxVel()
    {
        return BASE_MOVEMENT_SPEED * ((movementBuff > 0) ? 1.5f : 1f);
    }
}
