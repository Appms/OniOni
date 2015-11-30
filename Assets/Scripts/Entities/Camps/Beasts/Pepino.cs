using UnityEngine;
using System.Collections;

public class Pepino : Beast {

	// Use this for initialization
	override public void Start() {
		base.Start();
		health = 200;
		atkSpeed = 1f;
		attack = 18;
		crit = 0.25f;
	}
}
