using UnityEngine;
using System.Collections;

public class Fruit : MonoBehaviour {

    float push_per_minion = 0.5f;
    GameObject fruitMesh;
    GameObject orangeObjective;
    GameObject purpleObjective;
    float angle;
    float velocity;

    void Start()
    {
        fruitMesh = GameObject.Find(Names.FRUIT);
        orangeObjective = GameObject.Find(Names.ORANGE_OBJECTIVE);
        purpleObjective = GameObject.Find(Names.PURPLE_OBJECTIVE);
    }

    public void pushMelon(Peloton peloton)
    {
        int minionCount = peloton.GetMinionList().Count;
        velocity = peloton.Size() * push_per_minion * (peloton.leader.GetComponent<Leader>().pushBuff > 0 ? peloton.leader.GetComponent<Leader>().BUFF_MULTIPLYER : 1f) * Time.deltaTime;

        if (peloton.leader.name == Names.ENEMY_LEADER) velocity *= -1;
        angle = velocity / fruitMesh.GetComponent<SphereCollider>().radius;
        fruitMesh.transform.Rotate(angle, 0, 0);
        transform.position += new Vector3(0, 0, 1) * velocity;

        if (peloton.leader.name == Names.PLAYER_LEADER) peloton.transform.position = orangeObjective.transform.position;
        else peloton.transform.position = purpleObjective.transform.position;
    }
}
