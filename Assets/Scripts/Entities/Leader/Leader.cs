﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Leader : MonoBehaviour {

    //LeaderMovement leaderMovement;
    //Cursor cursor; //ONLY PLAYER
    public Peloton myPeloton;
    public Vector3 behind;
    public Vector3 velocity = new Vector3();
    public bool hasFlag = true;
    private GameObject leaderFlag;
    private int flagRadius = 10;

    public int health = 360;
    float happiness = 1;
    string weapon = Names.WEAPON_SWORD;
    int base_atk = 7;
    float crit_chance = 0.43f;
    protected float atkCooldown = 0f;
    public float deathCooldown = 0;
    Vector3 initPos;
    Vector3 initRot;
    int baseHealth = 360;
    float minTargetDist = 30;

    public float defenseBuff = 0;
    public float attackBuff = 0;
    public float movementBuff = 0;
    public float pushBuff = 0;

    public bool defenseBuffApplied = false;

    public readonly float BUFF_MULTIPLYER = 1.5f;
    public readonly float BUFF_VALUE = 70f;

    public static float BEHIND_DIST = 10f;

    static float BASE_MOVEMENT_SPEED = 30f;

	public float drag = 3f;
	public float maxVel = 30f;
	public float accel = 5f;
	public float deadzone = 0.6f;

	protected Animator anim;
	private SkinnedMeshRenderer skinnedMesh;
    protected GameObject leaderTarget;
    public string state;

    SkinnedMeshRenderer meshRenderer;

    // Use this for initialization
    public virtual void Start ()
    {
        meshRenderer = GetComponentInChildren<SkinnedMeshRenderer>();
        initPos = transform.position;
        initRot = transform.eulerAngles;

		anim = GetComponent<Animator>();
		skinnedMesh = transform.FindChild("Onion").GetComponent<SkinnedMeshRenderer>();

        behind = transform.position + transform.forward * -BEHIND_DIST;

        GameObject leaderPeloton = new GameObject();
        leaderPeloton.name = gameObject.name + "Peloton";
        myPeloton = leaderPeloton.AddComponent<Peloton>();
        myPeloton.SetLeader(gameObject);                                    //Leader
        myPeloton.SetObjective(Names.OBJECTIVE_FOLLOW_LEADER, gameObject);  //Objetivo
        myPeloton.transform.position = behind;                              //Posición Inicial
        //aiManager.AddPlayerPeloton(myPeloton);                            //Avisar al AIManager
		
        leaderFlag = GameObject.Find(gameObject.name + "Flag");

        leaderTarget = gameObject;
        //SetLeaderPelotonState(Names.STATE_FOLLOW_LEADER, leaderTarget);
    }

    public virtual void Update()
    {
        DecreaseBuffs();
        ApplyMovementBuff();
        ApplyDefenseBuff();

        if (attackBuff > 0.0) meshRenderer.material.SetFloat("_Atk", 1f);
        else meshRenderer.material.SetFloat("_Atk", 0f);
        if (pushBuff > 0.0) meshRenderer.material.SetFloat("_Push", 1f);
        else meshRenderer.material.SetFloat("_Push", 0f);
        if (defenseBuff > 0.0) meshRenderer.material.SetFloat("_Def", 1f);
        else meshRenderer.material.SetFloat("_Def", 0f);
        if (movementBuff > 0.0) meshRenderer.material.SetFloat("_Speed", 1f);
        else meshRenderer.material.SetFloat("_Speed", 0f);

        if (atkCooldown > 0f) atkCooldown -= Time.deltaTime;
        if (deathCooldown > 0) {
            deathCooldown -= Time.deltaTime;
            LeaderDie();
            if (deathCooldown <= 0) LeaderRespawn();
        }

		//AnimatorStateInfo animState = anim.GetCurrentAnimatorStateInfo(1);
        if (leaderTarget != null && Vector3.Distance(transform.position, leaderTarget.transform.position) > minTargetDist) myPeloton.SetStateAndTarget(Names.STATE_FOLLOW_LEADER, gameObject);
    }

    public virtual void FixedUpdate()
    {
        behind = transform.position + transform.forward * BEHIND_DIST;
        if (Physics.Raycast(transform.position, behind - transform.position, Vector3.Distance(transform.position, behind), LayerMask.GetMask("Level")))
        behind = transform.position - transform.forward * BEHIND_DIST;
        //myPeloton.transform.position = behind;
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
        //GameObject newPeloton = new GameObject();
        GameObject newPeloton = (GameObject)GameObject.Instantiate((GameObject)Resources.Load("Prefabs/Peloton"), myPeloton.transform.position, Quaternion.identity);
        newPeloton.name = gameObject.name == Names.PLAYER_LEADER ? Names.PLAYER_PELOTON : Names.ENEMY_PELOTON;
        //Peloton newPelotonScript = newPeloton.AddComponent<Peloton>();
        Peloton newPelotonScript = newPeloton.GetComponent<Peloton>();
        newPelotonScript.SetLeader(gameObject);                                 //Leader
        newPeloton.transform.position = behind;                                 //Posición Inicial
        AIManager.staticManager.AddPeloton(newPelotonScript);                           //Avisar al AIManager
        newPelotonScript.SetObjective(Names.OBJECTIVE_DEFEND, targetPosition);  //Objetivo

        //Repartimiento de Minions
        List<Minion> leaderPeloton = myPeloton.GetMinionListSorted(targetPosition);
        for (int i = 0; i < cant; i++){
            newPelotonScript.AddMinion(leaderPeloton[0]);
            myPeloton.RemoveMinion(leaderPeloton[0]);
        }
    }
    public bool NewOrder(int cant, GameObject targetElement)
    {
        if (myPeloton.Size() >= cant)
        {
            //GameObject newPeloton = new GameObject();
            GameObject newPeloton = (GameObject)GameObject.Instantiate((GameObject)Resources.Load("Prefabs/Peloton"), myPeloton.transform.position, Quaternion.identity);
            newPeloton.name = gameObject.name == Names.PLAYER_LEADER ? Names.PLAYER_PELOTON : Names.ENEMY_PELOTON;
            //Peloton newPelotonScript = newPeloton.AddComponent<Peloton>();
            Peloton newPelotonScript = newPeloton.GetComponent<Peloton>();
            newPelotonScript.SetLeader(gameObject);                     //Leader
            newPeloton.transform.position = behind;                     //Posición Inicial
            AIManager.staticManager.AddPeloton(newPelotonScript);               //Avisar al AIManager
            string objective = "";

            switch (targetElement.name)
            {
                case Names.ENEMY_PELOTON:
                    objective = Names.OBJECTIVE_ATTACK;
                    break;

                case Names.PLAYER_PELOTON:
                    objective = Names.OBJECTIVE_ATTACK;
                    break;

                case Names.CAMP:
                    objective = Names.OBJECTIVE_ATTACK_CAMP;
                    break;

                case Names.TOTEM:
                    objective = Names.OBJECTIVE_CONQUER;
                    break;

                case Names.FRUIT:
                    objective = Names.OBJECTIVE_PUSH;
                    if (gameObject.name == Names.PLAYER_LEADER) targetElement = GameObject.Find("OrangeObjective");
                    else targetElement = GameObject.Find("PurpleObjective");
                    break;

                case Names.PLAYER_DOOR:
                    if (gameObject.name == Names.PLAYER_LEADER)
                        objective = Names.OBJECTIVE_DEFEND;
                    else objective = Names.OBJECTIVE_ATTACK_DOOR;
                    break;

                case Names.ENEMY_DOOR:
                    if (gameObject.name == Names.ENEMY_LEADER)
                        objective = Names.OBJECTIVE_DEFEND;
                    else objective = Names.OBJECTIVE_ATTACK_DOOR;
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
            return true;
        }
        else
        {
            Debug.Log("Not enough minions to do that!");
            return false;
        }
    }

    private void DecreaseBuffs()
    {
        if (defenseBuff > 0) defenseBuff -= Time.deltaTime;
        if (defenseBuff < 0) defenseBuff = 0;
        if (attackBuff > 0) attackBuff -= Time.deltaTime;
        if (movementBuff > 0) movementBuff -= Time.deltaTime;
        if (pushBuff > 0) pushBuff -= Time.deltaTime;
    }


    protected void PlaceFlag(Vector3 targetPos)
    {
        if (hasFlag)
        {
            leaderFlag.SetActive(false);
			GameObject flag;
            if(gameObject.name == Names.PLAYER_LEADER) flag = (GameObject)GameObject.Instantiate((GameObject)Resources.Load("Prefabs/OrangeFlag"), targetPos + Vector3.up * 1.4f, Quaternion.identity);
			else flag = (GameObject)GameObject.Instantiate((GameObject)Resources.Load("Prefabs/PurpleFlag"), targetPos + Vector3.up * 1.4f, Quaternion.identity);
            flag.transform.Rotate(-70, -90, -180); //random values for now, Blender export issue
            flag.name = gameObject.name + "Flag";
            flag.layer = LayerMask.NameToLayer("Flag");
            hasFlag = false;
        }
    }

    protected void PickFlag()
    {
        GameObject flag = GameObject.Find(gameObject.name + "Flag");
        if (!hasFlag && Vector3.Distance(transform.position, flag.transform.position) < flagRadius){

            /*foreach(Peloton p in AIManager.staticManager.GetPelotonsAtPosition(flag.transform.position, this))
            {
                p.SetObjective(Names.OBJECTIVE_FOLLOW_LEADER, gameObject);
            }*/

            Destroy(flag);
            hasFlag = true;
            leaderFlag.SetActive(true);
        }
    }

    /*private void MinionCall()*/

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

    private void ApplyMovementBuff()
    {
        maxVel = movementBuff > 0 ? BASE_MOVEMENT_SPEED * 1.5f : BASE_MOVEMENT_SPEED;
    }

    private void ApplyDefenseBuff()
    {
        if (!defenseBuffApplied && defenseBuff == BUFF_VALUE)
        {
            health = Mathf.FloorToInt(health * BUFF_MULTIPLYER);
            defenseBuffApplied = true;
        }
        else if (defenseBuffApplied && defenseBuff == 0)
        {
            health = Mathf.FloorToInt(health / BUFF_MULTIPLYER);
            defenseBuffApplied = false;
        }
    }

    protected float GetMaxVel()
    {
        return BASE_MOVEMENT_SPEED * ((movementBuff > 0) ? 1.5f : 1f);
    }

	protected void Attack(){

		atkCooldown = 1f;

		anim.Play("Attack", 1, 0);
		skinnedMesh.SetBlendShapeWeight(1, 100);
		skinnedMesh.SetBlendShapeWeight(2, 0);
	}

	protected int GetDamageOutput()
	{
		int damage = Mathf.FloorToInt(base_atk * (attackBuff > 0 ? BUFF_MULTIPLYER : 1f));
		if(IsCriticalStrike())
				return damage*2;
		return damage;
	}
	private bool IsCriticalStrike()
	{
		return Random.value <= crit_chance;
	}

	public void RecieveDamage(int damage){

        if (deathCooldown == 0)
        {
            health -= damage;
            if (health <= 0)
            {
                deathCooldown = 5;
                LeaderDie();
            }
        }
	}


    // Does actions and determines his Peloton's new objective

    /*void OnTriggerStay(Collider other)
    {
        if (other.name == Names.TOTEM && velocity.magnitude < 15)
        {
            if((name == Names.PLAYER_LEADER && other.gameObject.GetComponent<Totem>().alignment < 50) || (name == Names.ENEMY_LEADER && other.gameObject.GetComponent<Totem>().alignment > -50))
            {
                leaderTarget = other.gameObject;
                myPeloton.SetStateAndTarget(Names.STATE_CONQUER, leaderTarget);
            }
            else myPeloton.SetStateAndTarget(Names.STATE_FOLLOW_LEADER, gameObject);
        }
    }*/

    void LeaderDie()
    {
        meshRenderer.material.SetFloat("_DissolveFactor", 1 - deathCooldown / 5);
        // Play Death Animation
        // Set stuff to idle if needed
        //anim.SetTrigger("Death");
        //anim.SetLayerWeight(1, 0);
        //mat.SetFloat("_DissolveFactor", Mathf.Lerp(mat.GetFloat("_DissolveFactor"), 1, Time.deltaTime * 2));
    }

    void LeaderRespawn()
    {
        meshRenderer.material.SetFloat("_DissolveFactor", 0);
        transform.position = initPos;
        transform.eulerAngles = initRot;
        health = baseHealth;
        deathCooldown = 0;
        myPeloton.SetStateAndTarget(Names.STATE_FOLLOW_LEADER, gameObject);

        if(name == Names.PLAYER_LEADER)
        {
            Camera.main.GetComponent<CameraMovement>().setToDefault();
            GameObject.Find(Names.PLAYER_LEADER).GetComponent<PlayerLeader>().cursor.Disappear();
        }
    }

	

    protected void Rotate()
    {
        Quaternion newRotation = new Quaternion();
        if (velocity.magnitude >= 0.01f)
        {
            newRotation = Quaternion.LookRotation(-velocity);
            transform.rotation = newRotation;
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.layer == LayerMask.NameToLayer("Floor"))
        {
            GetComponent<Rigidbody>().useGravity = false;
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.gameObject.layer == LayerMask.NameToLayer("Floor"))
        {
            GetComponent<Rigidbody>().useGravity = true;
        }
    }
}
