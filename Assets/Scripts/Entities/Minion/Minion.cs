using UnityEngine;
using System.Collections;

public class Minion : MonoBehaviour {

	FollowPeloton pelotonFollowing;
    public Peloton peloton;
    public Material[] materials;

    static string WEAPON_AXE = "Axe";
    static string WEAPON_SWORD = "Sword";
    static string WEAPON_CLUB = "Club";

    int health = 60;
    float happiness = 1;
    string weapon = WEAPON_SWORD;
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
                weapon = WEAPON_AXE;
                break;
            case 1: //Sword
                base_atk = 7;
                crit_chance = 0.43f;
                weapon = WEAPON_SWORD;
                break;
            case 2: //Club
                base_atk = 8;
                crit_chance = 0.11f;
                weapon = WEAPON_CLUB;
                break;
        }
	}
	

	void Update () {
        if (atkCooldown > 0f) atkCooldown -= Time.deltaTime;
        //else if (atkCooldown < 0f) atkCooldown = 0f;

        if (health <= 0) Sacrifice();
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

    
    private int GetDamageOutput() // --DEPRECATED--
    {
        if (Random.value <= crit_chance) //CRIT!
        {
            crit_flag = true;
            return 2*base_atk;
        }
        return base_atk;
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

    void OnTriggerStay(Collider other)
    {
        if (atkCooldown <= 0f && other.gameObject.name == (gameObject.name == Names.PLAYER_MINION ? Names.ENEMY_MINION : Names.PLAYER_MINION))
        {
            /*other.gameObject.GetComponent<Minion>().RecieveDamage(GetDamageOutput());
            if (crit_flag)
            {
                //Play CRITICAL ATTACK ANIMATION
                crit_flag = false;
            }
            else
            {
                //Play NORMAL ATTACK ANIMATION
            }*/

            // ANIMATION MOCK
            gameObject.GetComponent<Rigidbody>().velocity += Vector3.up * 20f;

            if (IsCriticalStrike())
            {
                //Play CRITICAL ATTACK ANIMATION
                //At end of animation:
                other.gameObject.GetComponent<Minion>().RecieveDamage(2*base_atk);
            }
            else
            {
                //Play NORMAL ATTACK ANIMATION
                //At end of animation:
                other.gameObject.GetComponent<Minion>().RecieveDamage(base_atk);
            }

            atkCooldown = 1f;

            anim.Play("Attack", 1, 0);
            skinnedMesh.SetBlendShapeWeight(1, 100);
            skinnedMesh.SetBlendShapeWeight(2, 0);
        }
    }
}