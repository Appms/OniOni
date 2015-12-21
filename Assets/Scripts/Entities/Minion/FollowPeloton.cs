using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class FollowPeloton : MonoBehaviour
{
    AIManager aiManager;

    Minion thisMinion;
    public GameObject pelotonObject;
    public Peloton peloton;
    Vector3 pelotonVel = new Vector3();
    GameObject leader;
    Leader leaderScript;
    Vector3 leaderVel = new Vector3();
    //GameObject[] boids;
    public Vector3 velocity = new Vector3();
    Vector3 steering = new Vector3();
    public Vector3 followVector;
    Vector3 separationVector;
    Vector3 avoidanceVector;
    Vector3 collisionAvoidance;
    float MAX_STEERING = 10;
    float AVOIDANCE_RADIUS = 75;
    float SEPARATION_RADIUS = 15;
    float MIN_SEPARATION = 10;
    float DYNAMIC_DRAG = 0.25f;
    float MAX_ACCEL = 1f;
    float minVel = 0.5f;
    float minAccel = 0.21f; //tested

    public bool separateFromOthers;
    public bool avoidLeader;
    public bool evadeColliders;
    public bool followLeader = true;
	
    List<Collider> obstacles = new List<Collider>();
	
    int elementLayer = 11;
    int levelLayer = 8;
    float movementSpeed = 30f;

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
        leaderScript = AIManager.staticManager.GetLeaderByName(leader.name);

        animator = gameObject.GetComponent<Animator>();
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        ApplyMovementBuff();
        pelotonVel = peloton.velocity;
        leaderVel = leader.GetComponent<Leader>().velocity;

        steering = Vector3.zero;

        avoidanceVector = evadeLeader();
        followVector = followPeloton(pelotonObject);
        separationVector = separate();
        //if (obstacles.Count > 0) collisionAvoidance = evadeCollider(obstacles);
        //else collisionAvoidance = new Vector3();
        steering += (avoidLeader ? avoidanceVector : Vector3.zero) + (evadeColliders ? collisionAvoidance : Vector3.zero) + (followLeader ? followVector : Vector3.zero) + (separateFromOthers? separationVector : Vector3.zero);
        steering += drag();
        steering.y = 0;

        if (steering.magnitude > MAX_STEERING)
        {
            steering.Normalize();
            steering *= MAX_STEERING;
        }

        velocity += steering;

        if (velocity.magnitude > movementSpeed)
        {
            velocity.Normalize();
            velocity *= movementSpeed;
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
        desiredVelocity *= movementSpeed;

        if (distance < movementSpeed / 2) desiredVelocity /= 2f;

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

        foreach (Minion m in aiManager.GetTeamMinions(thisMinion.peloton.leader.name)) {
            boid = m.gameObject;
            separation = transform.position - boid.transform.position;

            if (/*boid != this*/ separation.magnitude > 0 && separation.magnitude <= SEPARATION_RADIUS)
            {
                squaredSeparation = Vector3.Dot(separation, separation);
                separation.Normalize();
                added_force += (separation * Mathf.Pow(Vector3.Distance(transform.position, pelotonPosition), 1 / 10f) / squaredSeparation); 
                neighborCount++;
            }
        }
        if (neighborCount > 0) added_force *= (MIN_SEPARATION / Mathf.Sqrt(neighborCount));

        if (added_force.magnitude < minAccel) added_force = Vector3.zero; 

        return added_force;
    }

    private Vector3 evadeLeader()
    {
        Vector3 avoidance_force = new Vector3();

        if (Vector3.Distance(leader.transform.position, transform.position) <= AVOIDANCE_RADIUS)
        {
            avoidance_force = (transform.position - leader.transform.position).normalized;
            avoidance_force.Normalize();

            Vector3 ahead = peloton.transform.position - transform.position;
            Vector3 orto = new Vector3(-ahead.z, ahead.y, ahead.x).normalized / Mathf.Pow(ahead.magnitude, 1/4f);

            if (Vector3.Distance(transform.position + orto, leader.transform.position) <= Vector3.Distance(transform.position - orto, leader.transform.position)) avoidance_force -= orto;
            else avoidance_force += orto;
            avoidance_force.Normalize();

            //avoidance_force *= (MAX_VELOCITY / Mathf.Pow(Vector3.Distance(leader.transform.position + leaderVel, transform.position + velocity) / 2f, 2)) + (MAX_VELOCITY / Mathf.Pow(Vector3.Distance(leader.transform.position, transform.position) / 2f, 2)) / 2;
            avoidance_force *= (movementSpeed / Mathf.Pow(Vector3.Distance(leader.transform.position + leaderVel, transform.position + velocity) / 2f, 2)) + (movementSpeed / Mathf.Pow(Vector3.Distance(leader.transform.position, transform.position) / 2f, 2)) /2;
        }
        return avoidance_force;
    }

    private Vector3 evadeCollider(List<Collider> obstacles)
    {
        Vector3 avoidance_force = new Vector3();
        Vector3 auxForce;

        foreach (Collider c in obstacles) {

            auxForce = new Vector3();

            RaycastHit hit;
            Physics.Raycast(transform.position, c.transform.position - transform.position, out hit, 25, (1 << levelLayer) | (1 << elementLayer)); // Layer Level and Layer Elements
            Vector3 myNormal = hit.normal;
            float distance = Vector3.Distance(transform.position, hit.point);

            if (myNormal.magnitude > 0)
            {/*
                Vector3 orto = new Vector3(-myNormal.z, myNormal.y, myNormal.x).normalized;
                if (Vector3.Distance(transform.position + orto, pelotonObject.transform.position) >= Vector3.Distance(transform.position - orto, pelotonObject.transform.position)) auxForce -= orto;
                else auxForce += orto * Mathf.Abs(Vector3.Dot(orto, pelotonPosition - transform.position));

                auxForce += myNormal.normalized;
                auxForce.Normalize();

                float squaredSeparation = Mathf.Pow(Vector3.Distance(transform.position, hit.point), 2);
                auxForce *= Mathf.Pow(Vector3.Distance(transform.position, hit.point), 1 / 10f) / squaredSeparation * MIN_SEPARATION / 2f;
                //auxForce += myNormal.normalized * velocity.magnitude / (MAX_VELOCITY / 2);*/

                auxForce += myNormal.normalized;
                float squaredSeparation = Mathf.Pow(distance, 2);
                auxForce *= squaredSeparation / (movementSpeed / 2);

                Vector3 orto = new Vector3(-myNormal.z, myNormal.y, myNormal.x).normalized;
                if (Vector3.Distance(transform.position + orto, pelotonObject.transform.position) >= Vector3.Distance(transform.position - orto, pelotonObject.transform.position)) auxForce -= orto * Vector3.Distance(transform.position, pelotonObject.transform.position) / 5;
                else auxForce += orto * Vector3.Distance(transform.position, pelotonObject.transform.position) / 5;

                avoidance_force += auxForce;
            }
        }

        if (avoidance_force.magnitude < minAccel) avoidance_force = Vector3.zero;

       if (avoidance_force.magnitude > MAX_ACCEL*3)
       {
            avoidance_force.Normalize();
            avoidance_force *= MAX_ACCEL*3;
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
        if ((other.gameObject.layer.Equals(LayerMask.NameToLayer("Level")) /*|| other.gameObject.layer.Equals(LayerMask.NameToLayer("Element"))*/) && !obstacles.Contains(other)){

            obstacles.Add(other); 
        }
    }

    void OnTriggerExit(Collider other)
    {
        if ((other.gameObject.layer.Equals(LayerMask.NameToLayer("Level")) /*|| other.gameObject.layer.Equals(LayerMask.NameToLayer("Element"))*/) && obstacles.Contains(other)) {

            obstacles.Remove(other);
        }
    }

    private void ApplyMovementBuff()
    {
        movementSpeed = peloton.BASE_MOVEMENT_SPEED * (leaderScript.movementBuff > 0 ? leaderScript.BUFF_MULTIPLYER : 1f);
    }
}
