using UnityEngine;
using System.Collections;

public class Borderlands : MonoBehaviour {

    const float RADIUS = 475f;
	
	// Update is called once per frame
	void Update () {
        if (transform.position.magnitude > RADIUS)
            transform.position = transform.position.normalized * RADIUS;
    }
}
