using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Camp : MonoBehaviour {

	public int spawnCooldown = 60;
	public float spawnTimer = 0f;

	public string unitToSpawn;
	public int numberOfUnits;
	List<Beast> units = new List<Beast>();

	public string buffType;

	// Use this for initialization
	void Start () {

	}
	
	// Update is called once per frame
	void Update () {
		if(units.Count == 0){
			spawnTimer -= Time.deltaTime;
			if(spawnTimer <= 0f){
				Respawn();
			}
		}
	}

	private void Respawn(){
		for(int i = 0; i < numberOfUnits; i++){
			GameObject newBeast = (GameObject)GameObject.Instantiate((GameObject)Resources.Load("Prefabs/" + unitToSpawn), transform.position + new Vector3(Random.Range(-10, 10), 0, Random.Range(-10, 10)), Quaternion.identity);
			newBeast.name = unitToSpawn;
			switch(newBeast.name){
				case Names.PEPINO:
					newBeast.GetComponent<Pepino>().camp = this;
					units.Add((Beast)newBeast.GetComponent<Pepino>());
					break;
				case Names.PIMIENTO:
					newBeast.GetComponent<Pimiento>().camp = this;
					units.Add((Beast)newBeast.GetComponent<Pimiento>());
					break;
			}
			//newBeast.GetComponent<Beast>().camp = this;
			//units.Add(newBeast.GetComponent<Beast>());
		}
	}

	public void OnUnitDeath(Beast victim, string killer){
		units.Remove(victim);
		Destroy(victim.gameObject);
		Destroy(victim);

		if(units.Count == 0){
			spawnTimer = spawnCooldown;
			GrantBuff(killer);
		}
	}

	private void GrantBuff(string leader){
		Leader buffReceiver = AIManager.staticManager.GetLeaderByName(leader);
		//buffReceiver.RecieveBuff(buffType);
	}
}
