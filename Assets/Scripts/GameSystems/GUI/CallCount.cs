using UnityEngine;
using System.Collections;



public class CallCount : MonoBehaviour {

    public GameObject leader;
	
	// Update is called once per frame
	void Update () {

        transform.position = leader.transform.position + Vector3.up * 5;
        transform.LookAt(Camera.main.transform.position);
        transform.Rotate(0f, 180f, 0f);
	}
}
