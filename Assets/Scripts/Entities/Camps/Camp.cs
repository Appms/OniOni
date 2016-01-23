using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Camp : MonoBehaviour {

	public const int spawnCooldown = 60;
	public float spawnTimer = 0f;

	public string unitToSpawn;
	public int numberOfUnits;
	public List<Beast> units = new List<Beast>();

	public string buffType;

    AudioSource buffAudio;

	// Use this for initialization
	void Start () {
        gameObject.name = Names.CAMP;
        gameObject.layer = LayerMask.NameToLayer("Element");
        buffAudio = GetComponent<AudioSource>();
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
            newBeast.layer = LayerMask.NameToLayer("Element");
            newBeast.GetComponent<Beast>().camp = this;
			units.Add((Beast)newBeast.GetComponent<Beast>());
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
		Leader buffReciever = (Leader) AIManager.staticManager.GetLeaderByName(leader);
		buffReciever.RecieveBuff(buffType);
        buffAudio.Play();
	}
}
