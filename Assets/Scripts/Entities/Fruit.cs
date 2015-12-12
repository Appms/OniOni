using UnityEngine;
using System.Collections;

public class Fruit : MonoBehaviour {

    static float PUSH_PER_MINION = 1f;

    public void pushMelon(Peloton peloton)
    {
        int minionCount = peloton.GetMinionList().Count;
        float velocity = peloton.Size() * PUSH_PER_MINION * Time.deltaTime;
        if (peloton.leader.name == Names.ENEMY_LEADER) velocity *= -1;
        transform.position += Vector3.forward * velocity;
    }
}
