using UnityEngine;
using System.Collections;

public class Totem : MonoBehaviour {

    [Range(-50f, 50f)]
    public float alignment = 0;

    float AREA_RANGE = 30f;

    public GameObject bird;
    public GameObject turtle;

    private MeshRenderer meshRenderer;
    private MeshRenderer birdRenderer;
    private MeshRenderer turtleRenderer;


    // Use this for initialization
    void Start () {
        gameObject.name = Names.TOTEM;
        meshRenderer = GetComponent<MeshRenderer>();
        birdRenderer = bird.GetComponent<MeshRenderer>();
        turtleRenderer = turtle.GetComponent<MeshRenderer>();

        birdRenderer.material.SetFloat("_DissolveFactor", 1);
        turtleRenderer.material.SetFloat("_DissolveFactor", 1);
        turtleRenderer.enabled = false;
        birdRenderer.enabled = false;
        //gameObject.GetComponent<Rigidbody>().constraints = RigidbodyConstraints.FreezeAll;
    }
	
	// Update is called once per frame
    void Update()
    {
        
        if (alignment > 0)
        {
            turtleRenderer.material.SetFloat("_DissolveFactor", 1 - Mathf.Abs((alignment * 2 / 100)));
            birdRenderer.enabled = false;
            turtleRenderer.enabled = true;

            if(alignment == 50) turtleRenderer.material.SetFloat("_B", 0.1f);
            else turtleRenderer.material.SetFloat("_B", 0);
        }
        else if(alignment < 0)
        {
            birdRenderer.material.SetFloat("_DissolveFactor", 1 - Mathf.Abs((alignment * 2 / 100)));
            birdRenderer.enabled = true;
            turtleRenderer.enabled = false;

            if (alignment == -50) turtleRenderer.material.SetFloat("_B", 0.1f);
            else turtleRenderer.material.SetFloat("_B", 0);
        }
        meshRenderer.material.SetFloat("_DissolveFactor", Mathf.Abs((alignment * 2 / 100)));
    }

	void FixedUpdate () {
        RaycastHit[] surroundings = Physics.SphereCastAll(transform.position, AREA_RANGE, Vector3.up, 0f, LayerMask.GetMask("Character"));
        foreach(RaycastHit m in surroundings)
        {
            if (m.collider.gameObject.name.Contains("Minion"))
            {
                Minion minion = m.collider.GetComponent<Minion>();
                if (m.collider.gameObject.tag == "boid" && (minion.peloton.GetTargetElement() == gameObject || (minion.peloton.state == Names.STATE_CONQUER && minion.GetComponent<FollowPeloton>().velocity.magnitude == 0)))
                {
                    float lastAlignment = alignment;
                    if (minion.peloton.GetLeader().name == Names.PLAYER_LEADER) alignment += 0.5f * Time.deltaTime;
                    else if (minion.peloton.GetLeader().name == Names.ENEMY_LEADER) alignment -= 0.5f * Time.deltaTime;

                    //minion.gameObject.GetComponent<Rigidbody>().velocity += Vector3.up * 15f;

                    if (alignment > 50) alignment = 50;
                    else if (alignment < -50) alignment = -50;

                    if (lastAlignment != 50 && alignment == 50 || lastAlignment != -50 && alignment == -50)
                    {
                        //gameObject.GetComponent<Rigidbody>().constraints &= ~RigidbodyConstraints.FreezePositionY;
                        //gameObject.GetComponent<Rigidbody>().velocity = Vector3.up * 40f;
                        //gameObject.GetComponent<Rigidbody>().constraints = RigidbodyConstraints.FreezeAll;
                        Debug.Log("Conquered");
                    }
                }
            }
            else if (m.collider.gameObject.name.Contains("Leader"))
            {
                float lastAlignment = alignment;
                if (m.collider.gameObject.name == Names.PLAYER_LEADER) alignment += 1.5f * Time.deltaTime;
                else if (m.collider.gameObject.name == Names.ENEMY_LEADER) alignment -= 1.5f * Time.deltaTime;

                if (alignment > 50) alignment = 50;
                else if (alignment < -50) alignment = -50;
            }
        }
	}
}
