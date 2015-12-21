using UnityEngine;
using System.Collections;

public class Minion : MonoBehaviour {

	public FollowPeloton pelotonFollowing;
    public Peloton peloton;
    public Material[] materials;

    int health = 60;
    float happiness = 1;
    string weapon = Names.WEAPON_SWORD;
    int base_atk = 7;
    float crit_chance = 0.43f;
    bool crit_flag = false; // --DEPRECATED--
    float atkCooldown = 0f;

    private Animator anim;
    private SkinnedMeshRenderer skinnedMesh;

    void Awake () {
        anim = GetComponent<Animator>();
        skinnedMesh = transform.FindChild("Onion").GetComponent<SkinnedMeshRenderer>();

        skinnedMesh.material = materials[(int)Mathf.Round(Random.Range(0, 5))];

        pelotonFollowing = this.gameObject.AddComponent<FollowPeloton>();

        // WEAPON ASSIGNMENT
        switch (Random.Range(0, 2))
        {
            case 0: //Axe
                base_atk = 6;
                crit_chance = 0.66f;
                weapon = Names.WEAPON_AXE;
                break;
            case 1: //Sword
                base_atk = 7;
                crit_chance = 0.43f;
                weapon = Names.WEAPON_SWORD;
                break;
            case 2: //Club
                base_atk = 8;
                crit_chance = 0.11f;
                weapon = Names.WEAPON_CLUB;
                break;
        }
	}
	

	void Update () {
        if (atkCooldown > 0f) atkCooldown -= Time.deltaTime;
        //else if (atkCooldown < 0f) atkCooldown = 0f;

        ApplyDefenseBuff();

        if (health <= 0) Sacrifice();

        MinionStateMachine();
    }

    public void SetPeloton(Peloton p)
    {
        peloton = p;
        pelotonFollowing.pelotonObject = p.gameObject;
        pelotonFollowing.peloton = p;
        gameObject.name = (peloton.leader.name == Names.PLAYER_LEADER ? Names.PLAYER_MINION : Names.ENEMY_MINION);
    }
    public void AbandonPeloton()
    {
        peloton.RemoveMinion(this);
    }

    private void Sacrifice()
    {
        if (skinnedMesh.material.GetFloat("_DissolveFactor") >= 0.8)
        {
            AbandonPeloton();
            // DEATH ANIMATION;
            Destroy(gameObject.GetComponent<FollowPeloton>());
            Destroy(gameObject);
            Destroy(this);
        }

        else skinnedMesh.material.SetFloat("_DissolveFactor", Mathf.Lerp(skinnedMesh.material.GetFloat("_DissolveFactor"), 1, Time.deltaTime * 2));
    }

    
    private int GetDamageOutput()
    {
        Leader leader = AIManager.staticManager.GetLeaderByName(peloton.leader.name);
        return Mathf.FloorToInt(base_atk * (leader.attackBuff > 0 ? leader.BUFF_MULTIPLYER : 1f));
    }

    private bool IsCriticalStrike()
    {
        return Random.value <= crit_chance;
    }

    public void RecieveDamage(int damage)
    {
        health -= damage;
        GetComponent<Rigidbody>().velocity -= transform.forward * 10f; // HURT ANIMATION
        //if (health <= 0) Sacrifice();

        anim.Play(Mathf.PerlinNoise(Time.time, Time.time) >= 0.5f ? "Hurt01" : "Hurt02", 1, 0);
        happiness -= 0.2f;
        anim.SetFloat("Happiness", happiness);
        skinnedMesh.SetBlendShapeWeight(2, 100);
    }
    public void RecieveDamage(int damage, Minion attacker)
    {
        RecieveDamage(damage);

        if(!peloton.menaces.Contains(attacker.peloton))
            peloton.menaces.Add(attacker.peloton);

        peloton.isBeingAttacked = 2.5f;
    }

    void OnTriggerStay(Collider other)
    {
        if (atkCooldown <= 0f)
        {
			if (other.gameObject.name == (gameObject.name == Names.PLAYER_MINION ? Names.ENEMY_MINION : Names.PLAYER_MINION)){
                AttackMinion((Minion)other.gameObject.GetComponent<Minion>());
			}

			else if (other.gameObject.tag == Names.BEAST){
				AttackBeast((Beast)other.gameObject.GetComponent<Beast>());
			}

            else if (other.gameObject.name.Contains(gameObject.name == Names.PLAYER_MINION ? Names.ENEMY_DOOR : Names.PLAYER_DOOR))
            {
                AttackDoor((Door)other.gameObject.GetComponentInParent<Door>());
            }
		}
	}

    private void AttackMinion(Minion minion)
    {
        // ANIMATION MOCK
        gameObject.GetComponent<Rigidbody>().velocity += Vector3.up * 20f;

        if (IsCriticalStrike())
        {
            //Play CRITICAL ATTACK ANIMATION
            minion.RecieveDamage(GetDamageOutput() * 2, this);
        }
        else
        {
            //Play NORMAL ATTACK ANIMATION
            minion.RecieveDamage(GetDamageOutput(), this);
        }

        if(!peloton.victims.Contains(minion.peloton)) peloton.victims.Add(minion.peloton);
        if (!minion.peloton.menaces.Contains(peloton)) minion.peloton.menaces.Add(peloton);

        atkCooldown = 1f;

        anim.Play("Attack", 1, 0);
        skinnedMesh.SetBlendShapeWeight(1, 100);
        skinnedMesh.SetBlendShapeWeight(2, 0);
        transform.LookAt(minion.transform.position);
    }

	private void AttackBeast(Beast beast){
		gameObject.GetComponent<Rigidbody>().velocity += Vector3.up * 20f;
		if (IsCriticalStrike())
			beast.RecieveDamage(GetDamageOutput() * 2, gameObject.name);
		else
			beast.RecieveDamage(GetDamageOutput(), gameObject.name);
		
		atkCooldown = 1f;
		
		anim.Play("Attack", 1, 0);
		skinnedMesh.SetBlendShapeWeight(1, 100);
		skinnedMesh.SetBlendShapeWeight(2, 0);
        transform.LookAt(beast.transform.position);
	}

    private void AttackDoor(Door door)
    {
        gameObject.GetComponent<Rigidbody>().velocity += Vector3.up * 20f;
        if (IsCriticalStrike())
            door.RecieveDamage(GetDamageOutput() * 2);
        else
            door.RecieveDamage(GetDamageOutput());

        atkCooldown = 1f;

        anim.Play("Attack", 1, 0);
        skinnedMesh.SetBlendShapeWeight(1, 100);
        skinnedMesh.SetBlendShapeWeight(2, 0);
        transform.LookAt(door.transform.position);
    }

    private void ApplyDefenseBuff()
    {
        Leader leader = AIManager.staticManager.GetLeaderByName(peloton.leader.name);
        health = Mathf.FloorToInt(health * (leader.defenseBuff > 0 ? leader.BUFF_MULTIPLYER : 1f));
    }

    private void PromoteToLeader()
    {
        GameObject newLeader;
        if(gameObject.name == Names.PLAYER_MINION)
        {
            newLeader = (GameObject)GameObject.Instantiate((GameObject)Resources.Load("Prefabs/OrangeLeader"), transform.position + Vector3.up*2, Quaternion.identity);
            newLeader.GetComponent<PlayerLeader>().AssignWeapon(weapon);
        }
        else if (gameObject.name == Names.ENEMY_MINION)
        {
            newLeader = (GameObject)GameObject.Instantiate((GameObject)Resources.Load("Prefabs/PurpleLeader"), transform.position + Vector3.up * 2, Quaternion.identity);
            newLeader.GetComponent<EnemyLeader>().AssignWeapon(weapon);
        }
        Sacrifice();
    }

    public void MinionStateMachine()
    {
        switch (peloton.state)
        {
            case Names.STATE_ATTACK:
                pelotonFollowing.separateFromOthers = true;
                pelotonFollowing.avoidLeader = false;
                pelotonFollowing.evadeColliders = false;
                //if (peloton.IsLeaderPeloton()) pelotonFollowing.followLeader = false;
                break;

            case Names.STATE_ATTACK_DOOR:
                pelotonFollowing.separateFromOthers = false;
                pelotonFollowing.avoidLeader = false;
                pelotonFollowing.evadeColliders = true;
                //if (peloton.IsLeaderPeloton()) pelotonFollowing.followLeader = false;
                break;

            case Names.STATE_ATTACK_CAMP:
                pelotonFollowing.separateFromOthers = true;
                pelotonFollowing.avoidLeader = false;
                pelotonFollowing.evadeColliders = false;
                //if (peloton.IsLeaderPeloton()) pelotonFollowing.followLeader = false;
                break;

            case Names.STATE_CONQUER:
                pelotonFollowing.separateFromOthers = true;
                pelotonFollowing.avoidLeader = false;
                pelotonFollowing.evadeColliders = false;
                //transform.LookAt(peloton.targetElement.transform.position);
                //transform.Rotate(0, 180, 0);
                //if (peloton.IsLeaderPeloton()) pelotonFollowing.followLeader = false;
                break;

            case Names.STATE_DEFEND:
                pelotonFollowing.separateFromOthers = true;
                pelotonFollowing.avoidLeader = true;
                pelotonFollowing.evadeColliders = false;
                break;

            case Names.STATE_FOLLOW_LEADER:
                pelotonFollowing.separateFromOthers = true;
                pelotonFollowing.avoidLeader = true;
                pelotonFollowing.evadeColliders = false;
                //if (peloton.IsLeaderPeloton()) pelotonFollowing.followLeader = true;
                break;

            case Names.STATE_GO_TO:
                pelotonFollowing.separateFromOthers = true;
                pelotonFollowing.avoidLeader = false;
                pelotonFollowing.evadeColliders = true;
                break;

            case Names.STATE_PUSH:
                pelotonFollowing.separateFromOthers = false;
                pelotonFollowing.avoidLeader = false;
                pelotonFollowing.evadeColliders = false;
                //transform.LookAt(peloton.targetElement.transform.position);
                //transform.Rotate(0, 180, 0);
                break;
        }
    }
}