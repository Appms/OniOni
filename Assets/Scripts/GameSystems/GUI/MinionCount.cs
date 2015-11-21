using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class MinionCount : MonoBehaviour {
	
	// Update is called once per frame
	void Update () {

        GetComponent<Text>().text = AIManager.staticManager.GetLeaderMinionsCount(Names.PLAYER_LEADER) + "\n" + AIManager.staticManager.GetTeamMinionsCount(Names.PLAYER_LEADER);
    }
}
