using UnityEngine;
using System.Collections;

public class LeaderMovement : MonoBehaviour {

    public Vector3 velocity = new Vector3();
    public float drag = 3;
    public float maxVel = 30;
    public float accel = 5;

    public float deadzone = 0.6f;

    private Animator animator;

    void Start()
    {

        animator = GetComponent<Animator>();
    }

    void FixedUpdate () {

        //basic leader movement
        //transform.rotation = Quaternion.LookRotation(new Vector3(Input.GetAxis("Horizontal"), 0, Input.GetAxis("Vertical")));
        leaderMove(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical"));
        leaderRotate();
	}

    void leaderMove(float horizontal, float vertical)
    {
        //velocity += new Vector3(horizontal, 0, vertical) * accel;
        if (Mathf.Abs(horizontal) < deadzone) horizontal = 0;
        if (Mathf.Abs(vertical) < deadzone) vertical = 0;

        velocity += (Camera.main.transform.right * horizontal + Vector3.Cross(Camera.main.transform.right, Vector3.up) * vertical) * accel;

        if(velocity.magnitude > maxVel) {

            velocity.Normalize();
            velocity *= maxVel;
        }

        velocity -= velocity.normalized * drag;
        if(horizontal == 0 && vertical == 0 && velocity.magnitude < drag) velocity *= 0;

        transform.position = new Vector3(transform.position.x + velocity.x * Time.deltaTime, transform.position.y, transform.position.z + velocity.z * Time.deltaTime);
        animator.SetFloat("Speed", velocity.magnitude);
    }

    void leaderRotate()
    {
        Quaternion newRotation = new Quaternion();
        if (velocity.magnitude != 0)
        {
            newRotation = Quaternion.LookRotation(-velocity);
            transform.rotation = newRotation;
        }
    }
}

