using UnityEngine;
using System.Collections;

public class Totem : MonoBehaviour {

    [Range(-25f, 25f)]
    public float alignment = 0;

    float AREA_RANGE = 25f; 

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
    void Update()
    {
        gameObject.GetComponent<MeshRenderer>().material.color = new Color(0.5f + 0.5f * (alignment+25f) / 50f, 0.5f * (alignment+25f) / 50f, 1f - (alignment+25f) / 50f);
    }

	void FixedUpdate () {
        RaycastHit[] surroundings = Physics.SphereCastAll(transform.position, AREA_RANGE, Vector3.up, 0f, LayerMask.GetMask("Character"));
        foreach(RaycastHit m in surroundings)
        {
            Minion minion = m.collider.GetComponent<Minion>();
            if (m.collider.gameObject.tag == "boid" && (minion.peloton.GetTargetElement() == gameObject || (minion.peloton.GetObjectiveType() == Names.OBJECTIVE_FOLLOW_LEADER)) && minion.peloton.leader.GetComponent<PlayerLeader>().velocity.magnitude == 0)
            {
                float lastAlignment = alignment;
                if (minion.peloton.GetLeader().name == Names.PLAYER_LEADER) alignment += 1f * Time.deltaTime;
                else if(minion.peloton.GetLeader().name == Names.ENEMY_LEADER)  alignment -= 1f * Time.deltaTime;

                //minion.gameObject.GetComponent<Rigidbody>().velocity += Vector3.up * 15f;

                if (alignment > 25) alignment = 25;
                else if (alignment < -25) alignment = -25;

                if (lastAlignment != 25 && alignment == 25 || lastAlignment != -25 && alignment == -25)
                    gameObject.GetComponent<Rigidbody>().velocity = Vector3.up * 40f;
            }
        }
	}
}
