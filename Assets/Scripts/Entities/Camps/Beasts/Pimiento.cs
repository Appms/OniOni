using UnityEngine;
using System.Collections;

public class Pimiento : Beast {

	// Use this for initialization
	override public void Start() {
		base.Start();
		health = 600;
		atkSpeed = 1.5f;
		attack = 40;
		crit = 0.5f;
	}
}