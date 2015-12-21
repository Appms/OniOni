using UnityEngine;
using System.Collections;

public class Door : MonoBehaviour {

    int maxHealth = 1500;
    int health;
    float height;
    public bool doorsUp = true;
    MeshCollider[] colliders;
    MeshRenderer[] renderers;
 
	// Use this for initialization
	void Start () {

        height = gameObject.GetComponentInChildren<MeshCollider>().bounds.size.y;
        health = maxHealth;
	}
	
	// Update is called once per frame
	void Update () {
	
	}

    public void RecieveDamage(int damage)
    {
        if (health > 0)
        {
            health -= damage;
            Debug.Log(health);
            transform.position -= new Vector3(0, (float)damage / (float)maxHealth * (height - 5), 0);
        }

        else
        {
            doorsUp = false;
            colliders = gameObject.GetComponentsInChildren<MeshCollider>();
            foreach (MeshCollider c in colliders)
            {
                c.enabled = false;
            }

            renderers = gameObject.GetComponentsInChildren<MeshRenderer>();
            foreach (MeshRenderer r in renderers)
            {
                r.enabled = false;
            }
        }
    }
}
