using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class LeaderClickNGo : MonoBehaviour {

	float MAX_STEERING = 10f/2f;
	float MAX_VELOCITY = 30f/2f;
	float MIN_VELOCITY = 0.5f/1f;
	float MAX_ACCEL = 1f/1f;
	float MIN_ACCEL = 0.21f/1f;
	float DYNAMIC_DRAG = 0.3f/1f;
	float SLOW_DISTANCE = 5f;
	float GOAL_REACH = 2f;
	Vector3 steering = new Vector3();
	public Vector3 velocity = new Vector3();
	
	/*public float speed = 3.0f;
    [Range(0.0f,1.0f)]
    public float drag = 0.2f;*/


    //JPSManager _jpsManager;
    public List<Vector2> path = new List<Vector2>();
    Vector3 targetPosition = new Vector3();
    Vector3 lastPosition = new Vector3();
	RaycastHit hit = new RaycastHit();

    void Start () {
        //_jpsManager = GameObject.Find("JPSManager").GetComponent<JPSManager>();
    }
	
	void FixedUpdate () {

        lastPosition = transform.position;

		// PATHFINDING
        if (Input.GetKey(KeyCode.Mouse0) || Input.GetKey(KeyCode.Mouse1))
        {
            if(Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out hit))
            {
                if (hit.collider.gameObject.layer == LayerMask.NameToLayer("Floor"))
                {
                    //path = pathfinder.FindPath(transform.position, hit.point);
                    targetPosition = hit.point;
					JPSManager.RequestPath (transform.position, targetPosition, OnPathFound);
                    
                }
            }
        }

		// MOVEMENT
		steering = Vector3.zero;
		
		if (path.Count > 0)
		{
			targetPosition = new Vector3(path[0].x, transform.position.y, path[0].y);

			if (Vector3.Distance(transform.position, targetPosition) < GOAL_REACH/*pathfinder.manager.worldSize.x / pathfinder.manager.gridSize.x*/) {
				path.RemoveAt(0);
                SimplifyPath(); // Eliminate Redundant Fixed-Angle Waypoints
            }
		}

		if(path.Count >= 2){
			Vector3 nextTargetPosition = new Vector3(path[1].x, transform.position.y, path[1].y);
			steering += GoTo(targetPosition, nextTargetPosition);
		}
		else if(path.Count > 0){
			steering += GoTo(targetPosition);
		}
		steering += Drag();
		
		if (steering.magnitude > MAX_STEERING) {
			steering.Normalize();
			steering *= MAX_STEERING;
		}
		
		velocity += steering;
		
		if (velocity.magnitude > MAX_VELOCITY) {
			velocity.Normalize();
			velocity *= MAX_VELOCITY;
		}
		
		if (velocity.magnitude <= MIN_VELOCITY) velocity = Vector3.zero;
		
		transform.position = new Vector3(transform.position.x + velocity.x * Time.deltaTime, transform.position.y, transform.position.z + velocity.z * Time.deltaTime);
		
		
		
		
		
		
		
		
		
		// GOTO
		/*if (path.Count > 0)
        {
            targetPosition = new Vector3(path[0].x, transform.position.y, path[0].y);

            if (Vector3.Distance(transform.position, targetPosition) < 0.5f/*pathfinder.manager.worldSize.x / pathfinder.manager.gridSize.x){
                path.RemoveAt(0);

				// Eliminate Redundant Fixed-Angle Waypoints
				if(path.Count >= 2){
					hit = new RaycastHit();
					Vector3 nextPoint = new Vector3(path[1].x, 0.5f, path[1].y);


					while(!Physics.Raycast(transform.position, nextPoint - transform.position, out hit, Vector3.Distance(transform.position, nextPoint), LayerMask.GetMask("Level"))){
						path.RemoveAt(0);
						
						if(path.Count < 2) break;
						
						hit = new RaycastHit();
						nextPoint = new Vector3(path[1].x, transform.position.y, path[1].y);
					}
				}

                /*if (path.Count >= 2){
                    RaycastHit hit = new RaycastHit();
                    Vector3 nextPoint = new Vector3(path[1].x, transform.position.y, path[1].y);
                    if (!Physics.Raycast(transform.position, nextPoint, out hit, Vector3.Distance(transform.position, nextPoint), LayerMask.GetMask("Level")))
                    {
                        path.RemoveAt(0);
                    }
                }
            }
        }
        //else velocity /= 2f;


		//velocity += Vector3.Normalize(targetPosition - transform.position) * speed - drag * velocity;
		steering += GoTo(targetPosition) + Drag();
		if (steering.magnitude > MAX_STEERING) {
			steering.Normalize();
			steering *= MAX_STEERING;
		}

		velocity += steering;
		
		if (velocity.magnitude > MAX_VELOCITY) {
			velocity.Normalize();
			velocity *= MAX_VELOCITY;
		}
		
		if (velocity.magnitude <= MIN_VELOCITY) velocity = Vector3.zero;
		
		transform.position = new Vector3(transform.position.x + velocity.x * Time.deltaTime, transform.position.y, transform.position.z + velocity.z * Time.deltaTime);
        
		//transform.position = Vector3.Lerp(transform.position, targetPosition, Time.deltaTime * 3f);
        //transform.position += velocity * Time.deltaTime;
        
		if (velocity.magnitude > MIN_VELOCITY) { 
            transform.LookAt(transform.position + velocity);
            transform.Rotate(0.0f, 180.0f, 0.0f);
        }*/

        //velocity = transform.position - lastPosition;

    }

	public void OnPathFound(List<Vector2> newPath, bool pathSuccessful){
		if (pathSuccessful){
			path = newPath;
            SimplifyPath(); // Eliminate Redundant Fixed-Angle Waypoints
            //StopCoroutine("FollowPath");
            //StartCoroutine("FollowPath");
        }
	}
	IEnumerator FollowPath(){
		yield return null;
	}




	private Vector3 GoTo(Vector3 target) {
		//Vector3 target = new Vector3(targetPosition.x, 0f, targetPosition.y);
		
		float distance = Vector3.Distance(target, transform.position);
		
		Vector3 desiredVelocity = target - transform.position;
		desiredVelocity.Normalize();
		desiredVelocity *= MAX_VELOCITY;
		
		if (distance < SLOW_DISTANCE) desiredVelocity /= 2f;
		
		Vector3 acceleration = desiredVelocity - velocity;
		
		if(acceleration.magnitude > MAX_ACCEL)
		{
			acceleration.Normalize();
			acceleration *= MAX_ACCEL;
		}
		
		if (acceleration.magnitude < MIN_ACCEL) acceleration *= 0;
		
		return acceleration;
	}
	private Vector3 GoTo(Vector3 target, Vector3 nextTarget) {
		//Vector3 target = new Vector3(targetPosition.x, 0f, targetPosition.y);
		
		float distance = Vector3.Distance(target, transform.position);
		
		Vector3 desiredVelocity = target - transform.position;
		desiredVelocity.Normalize();
		desiredVelocity *= MAX_VELOCITY;

		//if (distance < SLOW_DISTANCE) desiredVelocity *= Vector3.Dot(target, nextTarget)/2f + 0.5f;
		if (distance < SLOW_DISTANCE*2f){

			desiredVelocity *= Mathf.Pow(Vector3.Dot(target, nextTarget),2)/1.43f + 0.3f;
		}
			

		Vector3 acceleration = desiredVelocity - velocity;
		
		if(acceleration.magnitude > MAX_ACCEL)
		{
			acceleration.Normalize();
			acceleration *= MAX_ACCEL;
		}
		
		if (acceleration.magnitude < MIN_ACCEL) acceleration *= 0;
		
		return acceleration;
	}


	private Vector3 Drag() {
		
		if (velocity.magnitude > 0)
			return -velocity.normalized * DYNAMIC_DRAG;
		
		return Vector3.zero;
	}

    private void SimplifyPath()
    {
        if (path.Count >= 2)
        {
            hit = new RaycastHit();
            Vector3 nextPoint = new Vector3(path[1].x, 0.5f, path[1].y);

            while (!Physics.Raycast(transform.position, nextPoint - transform.position, out hit, Vector3.Distance(transform.position, nextPoint), LayerMask.GetMask("Level")))
            {
                path.RemoveAt(0);

                if (path.Count < 2) break;

                hit = new RaycastHit();
                nextPoint = new Vector3(path[1].x, transform.position.y, path[1].y);
            }
        }
    }
}
