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
    public bool hasFlag = true;
    private GameObject leaderFlag;
    private int flagRadius = 10;

    int health = 360;
    float happiness = 1;
    string weapon = Names.WEAPON_SWORD;
    int base_atk = 7;
    float crit_chance = 0.43f;
    protected float atkCooldown = 0f;
	bool attacking = false;

    public float defenseBuff = 0;
    public float attackBuff = 0;
    public float movementBuff = 0;
    public float pushBuff = 0;

    public readonly float BUFF_MULTIPLYER = 1.5f;
    const float BUFF_VALUE = 70f;

    public static float BEHIND_DIST = 12f;

    static float BASE_MOVEMENT_SPEED = 30f;

	private Animator anim;
	private SkinnedMeshRenderer skinnedMesh;

	// Use this for initialization
	public virtual void Start ()
    {
        aiManager = GameObject.Find("AIManager").GetComponent<AIManager>();

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
    }


    public virtual void Update()
    {
        DecreaseBuffs();
        ApplyDefenseBuff();

		if (atkCooldown > 0f) atkCooldown -= Time.deltaTime;

		AnimatorStateInfo animState = anim.GetCurrentAnimatorStateInfo(1);

    }

    public virtual void FixedUpdate()
    {
        behind = transform.position + transform.forward * BEHIND_DIST;
        if (Physics.Raycast(transform.position, behind - transform.position, Vector3.Distance(transform.position, behind), LayerMask.GetMask("Level")))
        behind = transform.position - transform.forward * BEHIND_DIST;
        myPeloton.transform.position = behind;
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
        aiManager.AddPlayerPeloton(newPelotonScript);                           //Avisar al AIManager
        newPelotonScript.SetObjective(Names.OBJECTIVE_DEFEND, targetPosition);  //Objetivo

        //Repartimiento de Minions
        List<Minion> leaderPeloton = myPeloton.GetMinionListSorted(targetPosition);
        for (int i = 0; i < cant; i++){
            newPelotonScript.AddMinion(leaderPeloton[0]);
            myPeloton.RemoveMinion(leaderPeloton[0]);
        }
    }
    public void NewOrder(int cant, GameObject targetElement)
    {
        //GameObject newPeloton = new GameObject();
        GameObject newPeloton = (GameObject)GameObject.Instantiate((GameObject)Resources.Load("Prefabs/Peloton"), myPeloton.transform.position, Quaternion.identity);
        newPeloton.name = gameObject.name == Names.PLAYER_LEADER ? Names.PLAYER_PELOTON : Names.ENEMY_PELOTON;
        //Peloton newPelotonScript = newPeloton.AddComponent<Peloton>();
        Peloton newPelotonScript = newPeloton.GetComponent<Peloton>();
        newPelotonScript.SetLeader(gameObject);                     //Leader
        newPeloton.transform.position = behind;                     //Posición Inicial
        aiManager.AddPlayerPeloton(newPelotonScript);               //Avisar al AIManager
        string objective = "";

        switch (targetElement.name)
        {
            case Names.ENEMY_PELOTON :
                objective = Names.OBJECTIVE_ATTACK;
                break;

            case Names.PLAYER_PELOTON :
                objective = Names.OBJECTIVE_ATTACK;
                break;

            case Names.CAMP :
                objective = Names.OBJECTIVE_ATTACK;
                break;

            case Names.TOTEM :
                objective = Names.OBJECTIVE_CONQUER;
                break;

            case Names.FRUIT :
                objective = Names.OBJECTIVE_PUSH;
                if (gameObject.name == Names.PLAYER_LEADER) targetElement = GameObject.Find("OrangeObjective");
                else targetElement = GameObject.Find("PurpleObjective");
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

    private void DecreaseBuffs()
    {
        if (defenseBuff > 0) defenseBuff -= Time.deltaTime;
        if (attackBuff > 0) attackBuff -= Time.deltaTime;
        if (movementBuff > 0) movementBuff -= Time.deltaTime;
        if (pushBuff > 0) pushBuff -= Time.deltaTime;
    }


    protected void PlaceFlag(Vector3 targetPos)
    {
        if (hasFlag)
        {
            leaderFlag.SetActive(false);
            GameObject flag = (GameObject)GameObject.Instantiate((GameObject)Resources.Load("Prefabs/Flag"), targetPos + Vector3.up * 1.4f, Quaternion.identity);
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

            foreach(Peloton p in AIManager.staticManager.GetPelotonsAtPosition(flag.transform.position, this))
            {
                p.SetObjective(Names.OBJECTIVE_FOLLOW_LEADER, gameObject);
            }

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

    private void ApplyDefenseBuff()
    {
        health = Mathf.FloorToInt(health * (defenseBuff > 0 ? BUFF_MULTIPLYER : 1f));
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

	private int GetDamageOutput()
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
		health -= damage;
	}

	void OnTriggerEnter(Collider other)
	{
		AnimatorStateInfo animState = anim.GetCurrentAnimatorStateInfo(1);
		if(animState.shortNameHash == 1080829965/*animState.IsName("Attack")*/){
			if(name == Names.PLAYER_LEADER){
				if(other.name == Names.ENEMY_MINION){
					other.GetComponent<Minion>().RecieveDamage(GetDamageOutput());
				} else if (other.name == Names.ENEMY_LEADER){
					other.GetComponent<EnemyLeader>().RecieveDamage(GetDamageOutput());
				} 
			} 
			else if(name == Names.ENEMY_LEADER){
				if(other.name == Names.PLAYER_MINION){
					other.GetComponent<Minion>().RecieveDamage(GetDamageOutput());
				} else if (other.name == Names.PLAYER_LEADER){
					other.GetComponent<EnemyLeader>().RecieveDamage(GetDamageOutput());
				} 
			}

			if (other.name == Names.PEPINO || other.name == Names.PIMIENTO /*|| other.name == Names.MOLEM*/){
				other.GetComponent<Beast>().RecieveDamage(GetDamageOutput(), name);
			}
		}
	}
}
