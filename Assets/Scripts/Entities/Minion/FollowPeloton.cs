using UnityEngine;
using System.Collections;

public class FollowPeloton : MonoBehaviour
{
    AIManager aiManager;

    Minion thisMinion;
    public GameObject pelotonObject;
    public Peloton peloton;
    Vector3 pelotonVel = new Vector3();
    GameObject leader;
    Vector3 leaderVel = new Vector3();
    //GameObject[] boids;
    Vector3 velocity = new Vector3();
    Vector3 steering = new Vector3();
    Vector3 followVector;
    Vector3 separationVector;
    Vector3 avoidanceVector;
    Vector3 collisionAvoidance;
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
    bool collide = false;
    Collider obstacle = null;

    Vector3 pelotonPosition = new Vector3();

    private Animator animator;

    // Use this for initialization
    void Start()
    {
        aiManager = GameObject.Find("AIManager").GetComponent<AIManager>();
        thisMinion = gameObject.GetComponent<Minion>();
        pelotonObject = thisMinion.peloton.gameObject;
        peloton = pelotonObject.GetComponent<Peloton>();
        leader = gameObject.GetComponent<Minion>().peloton.GetLeader();

        animator = gameObject.GetComponent<Animator>();
        //boids = GameObject.FindGameObjectsWithTag("boid");
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        pelotonVel = peloton.velocity;
        leaderVel = leader.GetComponent<Leader>().velocity;

        steering = Vector3.zero;

        avoidanceVector = evadeLeader();
        followVector = followPeloton(pelotonObject);
        separationVector = separate();
        if (collide) collisionAvoidance = evadeCollider(obstacle);
        else collisionAvoidance = new Vector3();
        steering += avoidanceVector + collisionAvoidance + followVector + separationVector;
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
            velocity = Vector3.zero;
        }
        else
        {
            transform.position = new Vector3(transform.position.x + velocity.x * Time.deltaTime, transform.position.y, transform.position.z + velocity.z * Time.deltaTime);
            transform.LookAt(transform.position - velocity);  
        }

        float turnAngle = Vector3.Angle(-transform.forward.normalized, (leader.GetComponent<Leader>().transform.position - transform.position).normalized);
        turnAngle *= Mathf.Sign(Vector3.Cross(-transform.forward.normalized, (leader.GetComponent<Leader>().transform.position - transform.position).normalized).y);

        if(turnAngle > 45)
        {
            transform.Rotate(new Vector3(0, turnAngle - 45, 0));
        } else if (turnAngle < -45)
        {
            transform.Rotate(new Vector3(0, turnAngle + 45, 0));
        }
        turnAngle = Mathf.Clamp(turnAngle, -45, 45);

        animator.SetFloat("DirX", Mathf.Clamp(turnAngle / 45, -1, 1));
        animator.SetFloat("Speed", velocity.magnitude / 3 * Vector3.Dot(velocity,steering));

    }

    private Vector3 followPeloton(GameObject peloton)
    {
        
        float distance = Vector3.Distance(peloton.transform.position, transform.position);

        Vector3 desiredVelocity = peloton.transform.position - transform.position;
        desiredVelocity.Normalize();
        desiredVelocity *= MAX_VELOCITY;

        if (distance < MAX_VELOCITY / 2) desiredVelocity /= 2f;

        Vector3 acceleration = desiredVelocity - velocity;

        if (acceleration.magnitude > MAX_ACCEL)
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

        /*for (int i = 0; i < boids.Length; i++)
        {
            boid = boids[i];*/
        foreach (Minion m in aiManager.GetTeamMinions(thisMinion.peloton.leader.name)) {
            boid = m.gameObject;
            separation = transform.position - boid.transform.position;

            if (/*boid != this*/ separation.magnitude > 0 && separation.magnitude <= SEPARATION_RADIUS)
            {
                squaredSeparation = Vector3.Dot(separation, separation);
                separation.Normalize();
                added_force += (separation * Mathf.Pow(Vector3.Distance(transform.position, pelotonPosition), 1 / 10f) / squaredSeparation); // function
                neighborCount++;
            }
        }
        if (neighborCount > 0) added_force *= (MIN_SEPARATION / Mathf.Sqrt(neighborCount));

        if (added_force.magnitude < minAccel) added_force = Vector3.zero; // 

        return added_force;
    }

    private Vector3 evadeLeader()
    {
        Vector3 avoidance_force = new Vector3();

        if (Vector3.Distance(leader.transform.position, transform.position) <= AVOIDANCE_RADIUS)
        {
            avoidance_force = (transform.position - leader.transform.position).normalized;
            avoidance_force.Normalize();

            Vector3 ahead = -(pelotonPosition - leader.transform.position);
            Vector3 orto = new Vector3(-ahead.z, ahead.y, ahead.x).normalized;

            if (Vector3.Distance(transform.position + orto, leader.transform.position) <= Vector3.Distance(transform.position - orto, leader.transform.position)) avoidance_force -= orto;
            else avoidance_force += orto;
            avoidance_force.Normalize();

            avoidance_force *= (MAX_VELOCITY / Mathf.Pow(Vector3.Distance(leader.transform.position + leaderVel, transform.position + velocity) / 2f, 2)) + (MAX_VELOCITY / Mathf.Pow(Vector3.Distance(leader.transform.position, transform.position) / 2f, 2)) / 2f;
        }
        return avoidance_force;
    }

    private Vector3 evadeCollider(Collider other)
    {
        Vector3 avoidance_force = new Vector3();

        RaycastHit hit;
        Physics.Raycast(transform.position, other.transform.position - transform.position, out hit, 15, LayerMask.GetMask("Level"));
        Vector3 myNormal = hit.normal;
        float distance = Vector3.Distance(transform.position, hit.point);

        if (myNormal.magnitude > 0)
        {
            Vector3 orto = new Vector3(-myNormal.z, myNormal.y, myNormal.x).normalized;
            if (Vector3.Distance(transform.position + orto, pelotonObject.transform.position) >= Vector3.Distance(transform.position - orto, pelotonObject.transform.position)) avoidance_force -= orto;
            else avoidance_force += orto * Mathf.Abs(Vector3.Dot(orto, pelotonPosition - transform.position));

            avoidance_force += myNormal.normalized;
            avoidance_force.Normalize();

            float squaredSeparation = Mathf.Pow(Vector3.Distance(transform.position, hit.point), 2);
            avoidance_force *= Mathf.Pow(Vector3.Distance(transform.position, hit.point), 1 / 10f) / squaredSeparation * MIN_SEPARATION/2f;
            //avoidance_force += myNormal.normalized * velocity.magnitude / (MAX_VELOCITY / 2);
        }

        if (avoidance_force.magnitude < minAccel) avoidance_force = Vector3.zero;

        if (avoidance_force.magnitude > MAX_ACCEL*1.5f)
        {
            avoidance_force.Normalize();
            avoidance_force *= MAX_ACCEL*1.5f;
        }

        return avoidance_force;
    }

    private Vector3 drag()
    {
        if (velocity.magnitude > 0)
            return -velocity.normalized * DYNAMIC_DRAG;

        return Vector3.zero;
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.layer.Equals(LayerMask.NameToLayer("Level"))){
            collide = true;
            obstacle = other;
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.gameObject.layer.Equals(LayerMask.NameToLayer("Level"))) {
            collide = false;
            obstacle = null;
        }
    }
}
