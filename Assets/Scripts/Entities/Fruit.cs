using UnityEngine;
using System.Collections;

public class Fruit : MonoBehaviour {

    float AREA_RANGE = 50;
    int orangeCount = 0;
    int purpleCount = 0;
    int melonVelocity = 5;
    Vector3 orangeObjective;
    Vector3 purpleObjective;
    RaycastHit[] surroundings;

    Peloton orangePeloton;
    Peloton purplePeloton;

    // Use this for initialization
    void Start () {

        orangeObjective = GameObject.Find(Names.ORANGE_OBJECTIVE).transform.position;
        purpleObjective = GameObject.Find(Names.PURPLE_OBJECTIVE).transform.position;
	}

    // Update is called once per frame
    void Update()
    {
        surroundings = Physics.SphereCastAll(transform.position, AREA_RANGE, Vector3.up, 0f, LayerMask.GetMask("Character"));
        foreach (RaycastHit m in surroundings)
        {
            Minion minion = m.collider.GetComponent<Minion>();
            if (m.collider.gameObject.tag == "boid" && minion.peloton.GetTargetElement() == gameObject)
            {
                if (minion.peloton.GetLeader().name == Names.PLAYER_LEADER)
                {
                    orangeCount++;
                    if (orangePeloton == null) orangePeloton = minion.peloton;
                }
                else
                {
                    purpleCount++;
                    if (purplePeloton == null) purplePeloton = minion.peloton;
                }
            }
        }

        transform.position += new Vector3(0, 0, (-melonVelocity * Mathf.Sqrt(purpleCount) + melonVelocity * Mathf.Sqrt(orangeCount)) * Time.deltaTime);
        purpleCount = 0;
        orangeCount = 0;

        if (orangePeloton != null) orangePeloton.SetObjective(Names.PUSH, orangeObjective);
        orangePeloton = null;

        if (purplePeloton != null) purplePeloton.SetObjective(Names.PUSH, purpleObjective);
        purplePeloton = null;
    }
}
