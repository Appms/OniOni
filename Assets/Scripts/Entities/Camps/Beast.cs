using UnityEngine;
using System.Collections;

public class Beast : MonoBehaviour {

	public Camp camp;

	protected int health;
	protected int attack;
	protected float crit;
	protected float atkSpeed;

	float atkCooldown = 0f;

	// Use this for initialization
	public virtual void Start () {
	

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
		if (atkCooldown <= 0f){
		                          
			if(other.gameObject.name == Names.PLAYER_MINION || 
			   other.gameObject.name == Names.ENEMY_MINION){

				gameObject.GetComponent<Rigidbody>().velocity += Vector3.up * 20f;
				other.gameObject.GetComponent<Minion>().RecieveDamage(attack);
				
				atkCooldown = atkSpeed;
				
				/*anim.Play("Attack", 1, 0);
				skinnedMesh.SetBlendShapeWeight(1, 100);
				skinnedMesh.SetBlendShapeWeight(2, 0);*/

			}
			else if (other.gameObject.name == Names.PLAYER_LEADER){

				gameObject.GetComponent<Rigidbody>().velocity += Vector3.up * 20f;
				//other.gameObject.GetComponent<PlayerLeader>().RecieveDamage(attack);
				
				atkCooldown = atkSpeed;
				
				/*anim.Play("Attack", 1, 0);
				skinnedMesh.SetBlendShapeWeight(1, 100);
				skinnedMesh.SetBlendShapeWeight(2, 0);*/
			}
			else if (other.gameObject.name == Names.ENEMY_LEADER){
				gameObject.GetComponent<Rigidbody>().velocity += Vector3.up * 20f;
				//other.gameObject.GetComponent<EnemyLeader>().RecieveDamage(attack);
				
				atkCooldown = atkSpeed;
				
				/*anim.Play("Attack", 1, 0);
				skinnedMesh.SetBlendShapeWeight(1, 100);
				skinnedMesh.SetBlendShapeWeight(2, 0);*/
			}

		}
	}
}
