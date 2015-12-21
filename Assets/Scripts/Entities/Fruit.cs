using UnityEngine;
using System.Collections;

public class Fruit : MonoBehaviour {

    float push_per_minion = 0.5f;
    GameObject fruitMesh;
    GameObject orangeObjective;
    GameObject purpleObjective;
    GameObject orangeDoor;
    GameObject purpleDoor;
    float angle;
    float velocity;
    public bool canAdvanceToOrange = true;
    public bool canAdvanceToPurple = true;
    public float radius;

    void Start()
    {
        fruitMesh = GameObject.Find(Names.FRUIT);
        orangeObjective = GameObject.Find(Names.ORANGE_OBJECTIVE);
        purpleObjective = GameObject.Find(Names.PURPLE_OBJECTIVE);
        orangeDoor = GameObject.Find(Names.PLAYER_DOOR);
        purpleDoor = GameObject.Find(Names.ENEMY_DOOR);
        radius = fruitMesh.GetComponent<SphereCollider>().radius * 300 + 20;
    }

    public void pushMelon(Peloton peloton)
    {
        int minionCount = peloton.GetMinionList().Count;
        velocity = peloton.Size() * push_per_minion * (peloton.leader.GetComponent<Leader>().pushBuff > 0 ? peloton.leader.GetComponent<Leader>().BUFF_MULTIPLYER : 1f) * Time.deltaTime;

        if (peloton.leader.name == Names.ENEMY_LEADER && canAdvanceToOrange)
        {
            velocity *= -1;
            angle = velocity / fruitMesh.GetComponent<SphereCollider>().radius;
            fruitMesh.transform.Rotate(angle / 2f, 0, 0);
            transform.position += new Vector3(0, 0, 1) * velocity;
            peloton.transform.position = purpleObjective.transform.position;
        }

        if (peloton.leader.name == Names.PLAYER_LEADER)
        {
            angle = velocity / fruitMesh.GetComponent<SphereCollider>().radius;
            fruitMesh.transform.Rotate(angle / 2f, 0, 0);
            transform.position += new Vector3(0, 0, 1) * velocity;
            peloton.transform.position = orangeObjective.transform.position;
        }
        
        if (transform.position.z + radius < orangeDoor.transform.position.z && orangeDoor.GetComponent<Door>().doorsUp)
            canAdvanceToOrange = false;
        else canAdvanceToOrange = true;
        if (transform.position.z >= purpleDoor.transform.position.z - radius && purpleDoor.GetComponent<Door>().doorsUp)
            canAdvanceToPurple = false;
        else canAdvanceToPurple = true;
    }
}
