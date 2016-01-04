using UnityEngine;
using System.Collections;

public class FrameLimit : MonoBehaviour {

    [Range(30, 120)]
    public int desiredFrameRate;

	// Use this for initialization
	void Start () {
        Application.targetFrameRate = desiredFrameRate;
	}
	
	// Update is called once per frame
	void Update () {
	
	}
}
