using UnityEngine;
using System.Collections;

public class leaderFollowing : MonoBehaviour {

    GameObject leader;
    Vector3 leaderVel = new Vector3();
    GameObject[] boids;
    Vector3 velocity = new Vector3();
    Vector3 steering = new Vector3();
    Vector3 followVector;
    Vector3 separationVector;
    Vector3 avoidanceVector;
    float MAX_VELOCITY = 30;
    float MAX_STEERING = 10;
    float MIN_BEHIND_DIST = 12;
    float AVOIDANCE_RADIUS = 75;
    float SEPARATION_RADIUS = 15;
    float MIN_SEPARATION = 10;
    float DYNAMIC_DRAG = 0.2f;
    float MAX_ACCEL = 1f;
    float minVel = 0.5f;
    float minAccel = 0.21f; //tested

    Vector3 behind = new Vector3();

    Animator animator;

    // Use this for initialization
    void Start () {

        leader = GameObject.FindGameObjectWithTag("Player");
        boids = GameObject.FindGameObjectsWithTag("boid");
        behind = leader.transform.position - new Vector3(MIN_BEHIND_DIST, transform.position.y, MIN_BEHIND_DIST);

        animator = GetComponent<Animator>();
    }
	
	// Update is called once per frame
	void Update () {

        //leaderVel = leader.GetComponent<leaderController>().velocity;
        leaderVel = leader.GetComponent<LeaderClickNGo>().velocity;

        steering = Vector3.zero;

        avoidanceVector = evadeLeader(leader);
        followVector = followBehind(leader);
        separationVector = separate();
        steering += avoidanceVector + followVector + separationVector;
        steering += drag();
        steering.y = 0;

        if (steering.magnitude > MAX_STEERING)
        {
            steering.Normalize();
            steering *= MAX_STEERING;
        }

        velocity += steering;

        if (velocity.magnitude > MAX_VELOCITY)
        {
            velocity.Normalize();
            velocity *= MAX_VELOCITY;
        }

        if (velocity.magnitude <= minVel)
        {
            //transform.LookAt(Vector3.Lerp(transform.position + transform.forward, leader.transform.position, Time.deltaTime));
            //transform.Rotate(0.0f, 180.0f, 0.0f);
            velocity = Vector3.zero;
        }
        else
        {
            Vector3 oldForward = transform.forward;

            transform.position = new Vector3(transform.position.x + velocity.x * Time.deltaTime, transform.position.y, transform.position.z + velocity.z * Time.deltaTime);
            transform.LookAt(transform.position - velocity);

            //Vector3 newForward = transform.forward;

            //float turnAngle = Vector3.Angle(oldForward.normalized, newForward.normalized);
            //turnAngle *= Mathf.Sign(Vector3.Cross(oldForward, newForward).y);
            
            //transform.Rotate(0.0f, 180.0f, 0.0f);

            animator.SetFloat("Speed", velocity.magnitude / 3);
        }

        


        //  transform.position = Vector3.Lerp(transform.position, new Vector3(transform.position.x + velocity.x, transform.position.y, transform.position.z + velocity.z), Time.deltaTime * smooth);

    }

    private Vector3 followBehind(GameObject leader)
    {
        // we need to adjust how behind is calculated
        // depends on character
        if (leaderVel.magnitude > 0.1f)
        {
            behind = leader.transform.position - leaderVel.normalized * MIN_BEHIND_DIST;
            behind.y = transform.position.y;
        }

        float distance = Vector3.Distance(behind, transform.position);

        Vector3 desiredVelocity = behind - transform.position;
        desiredVelocity.Normalize();
        desiredVelocity *= MAX_VELOCITY;

        if (distance < MAX_VELOCITY / 2) desiredVelocity /= 2f;

        Vector3 acceleration = desiredVelocity - velocity;

        if(acceleration.magnitude > MAX_ACCEL)
        {
            acceleration.Normalize();
            acceleration *= MAX_ACCEL;
        }

        if (acceleration.magnitude < minAccel) acceleration *= 0;

        return acceleration;
    }

    private Vector3 separate()
    {
        Vector3 separation = new Vector3();
        Vector3 added_force = new Vector3();
        GameObject boid;
        float squaredSeparation;
        int neighborCount = 0;

        for (int i = 0; i < boids.Length; i++)
        {
            boid = boids[i];
            separation = transform.position - boid.transform.position;

            if (/*boid != this*/ separation.magnitude > 0 && separation.magnitude <= SEPARATION_RADIUS)
            {
                squaredSeparation =/* Mathf.Pow(separation.magnitude, 2);*/ Vector3.Dot(separation, separation);
                separation.Normalize();
                added_force += (separation * Mathf.Pow(Vector3.Distance(transform.position, behind), 1/10f) / squaredSeparation);
                neighborCount++;
            }
        }
        if (neighborCount > 0) added_force *= (MIN_SEPARATION / Mathf.Sqrt(neighborCount));

        if (added_force.magnitude < minAccel) added_force = Vector3.zero; // not magic, I swear

        return added_force;
    }

    private Vector3 evadeLeader(GameObject leader)
    { 
        Vector3 avoidance_force = new Vector3();

        if(Vector3.Distance(leader.transform.position, transform.position) <= AVOIDANCE_RADIUS)
        {
            avoidance_force = (transform.position - leader.transform.position).normalized;
            avoidance_force.Normalize();

            Vector3 ahead = -(behind - leader.transform.position);
            Vector3 orto = new Vector3(-ahead.z, ahead.y, ahead.x).normalized;

            if (Vector3.Distance(transform.position + orto, leader.transform.position) <= Vector3.Distance(transform.position - orto, leader.transform.position)) avoidance_force -= orto;
            else avoidance_force += orto;
            avoidance_force.Normalize();

            avoidance_force *= (MAX_VELOCITY / Mathf.Pow(Vector3.Distance(leader.transform.position + leaderVel, transform.position + velocity) / 2f, 2)) + (MAX_VELOCITY / Mathf.Pow(Vector3.Distance(leader.transform.position, transform.position) / 2f, 2)) / 2f;
        }
        return avoidance_force;
    }

    private Vector3 drag() {

        if (velocity.magnitude > 0)
            return -velocity.normalized * DYNAMIC_DRAG;

        return Vector3.zero;
    }

    void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawSphere(behind, 2);
        
        Gizmos.color = Color.blue;
        Gizmos.DrawLine(transform.position, transform.position + avoidanceVector.normalized * 5f);
        
        Gizmos.color = Color.green;
        Gizmos.DrawLine(transform.position, transform.position + separationVector.normalized * 5f);
        
        Gizmos.color = Color.magenta;
        Gizmos.DrawLine(transform.position, transform.position + followVector.normalized * 5f);
    }
}
