using UnityEngine;
using System.Collections;

public class Beast : MonoBehaviour {

	public Camp camp;

	public int health;
    public float atkSpeed;
    public int attack;
    public float crit;
	protected Animator anim;

	float atkCooldown = 0f;

	// Use this for initialization
	public virtual void Start () {
	
		anim = GetComponent<Animator>();
		anim.SetFloat ("Speed", 0);
		transform.LookAt(GameObject.Find(Names.PLAYER_LEADER).transform.position);
		transform.Rotate(15f, 180f, 0f);
        transform.up = Vector3.up;
	}
	
	// Update is called once per frame
	void Update () {
		if (atkCooldown > 0f) atkCooldown -= Time.deltaTime;
	}

	public void RecieveDamage(int damage, string attacker){ //GameObject.name
		health -= damage;

		if (health <= 0)
		{
			// Dissolve Animation¿?
			if(attacker == Names.PLAYER_MINION) attacker = Names.PLAYER_LEADER;
			else if (attacker == Names.ENEMY_MINION) attacker = Names.ENEMY_LEADER;
			camp.OnUnitDeath(this, attacker);
		}
	}

	void OnTriggerStay(Collider other)
	{
        if (other.gameObject.layer != LayerMask.NameToLayer("Floor"))
        {
            transform.LookAt(other.transform.position);
            transform.Rotate(0f, 180f, 0f);
        }

		if (atkCooldown <= 0f){
		                          
			if(other.gameObject.name == Names.PLAYER_MINION || 
			   other.gameObject.name == Names.ENEMY_MINION){

				gameObject.GetComponent<Rigidbody>().velocity += Vector3.up * 20f;
				other.gameObject.GetComponent<Minion>().RecieveDamage(attack);
                transform.LookAt(other.transform.position);
				anim.Play("Attack", 1, 0);
				
				atkCooldown = atkSpeed;
				
				/*anim.Play("Attack", 1, 0);
				skinnedMesh.SetBlendShapeWeight(1, 100);
				skinnedMesh.SetBlendShapeWeight(2, 0);*/

			}
			else if (other.gameObject.name == Names.PLAYER_LEADER){

				gameObject.GetComponent<Rigidbody>().velocity += Vector3.up * 20f;
				other.gameObject.GetComponent<PlayerLeader>().RecieveDamage(attack);
                transform.LookAt(other.transform.position);
				anim.Play("Attack", 1, 0);

                atkCooldown = atkSpeed;
				
				/*anim.Play("Attack", 1, 0);
				skinnedMesh.SetBlendShapeWeight(1, 100);
				skinnedMesh.SetBlendShapeWeight(2, 0);*/
			}
			else if (other.gameObject.name == Names.ENEMY_LEADER){
				gameObject.GetComponent<Rigidbody>().velocity += Vector3.up * 20f;
				other.gameObject.GetComponent<EnemyLeader>().RecieveDamage(attack);
				anim.Play("Attack", 1, 0);
				
				atkCooldown = atkSpeed;
				
				/*anim.Play("Attack", 1, 0);
				skinnedMesh.SetBlendShapeWeight(1, 100);
				skinnedMesh.SetBlendShapeWeight(2, 0);*/
			}

		}
	}
}
