using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class PlayerLeader : Leader {

    public PlayerCursor cursor;
    public Slider healthSlider;
    AudioSource pelotonSource;

    float callTime = 0f;
    float callRadius = 0f;
    static float CALL_RADIUS_SCALE = 30f;
    Projector callArea;
    GameObject callText;
    private bool _swarmActive = false;

    Radio radio;

    /*public float drag = 3;
    public float maxVel = 30;
    public float accel = 5;
    public float deadzone = 0.6f;*/

    //private Animator animator;

    override public void Start()
    {
        base.Start();
        AIManager.staticManager.AddPeloton(myPeloton);  //Avisar al AIManager

        cursor = GameObject.Find("Cursor").GetComponent<PlayerCursor>();
        cursor.SetLeader(gameObject);

        callArea = gameObject.GetComponentInChildren<Projector>();
        callText = GameObject.Find("CallText");
        healthSlider = GameObject.Find("Slider").GetComponent<Slider>();

        pelotonSource = GetComponent<AudioSource>();

        radio = GameObject.Find(Names.RADIO).GetComponent<Radio>();
    }

    override public void Update()
    {
        base.Update();
        maxVel = GetMaxVel();
        if(deathCooldown == 0 && !AIManager.staticManager.DisableElements) Move(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical"));
        //Rotate();

        ManageCursor();
        MinionCall();
        ManageSwarm();

        ManageFlag();
		ManageAttack();

        healthSlider.value = health;

        if (myPeloton.state == Names.STATE_CONQUER)
        {
            if (pelotonSource.volume < 0.5f) pelotonSource.volume += 0.1f;
            radio.praying = true;
        }
        else
        {
            if (pelotonSource.volume > 0.0f) pelotonSource.volume -= 0.1f;
            if (pelotonSource.volume < 0.0f) pelotonSource.volume = 0;
            radio.praying = false;
        }

        if (myPeloton.state == Names.STATE_ATTACK) radio.combat = true;
        else radio.combat = false;

        //PLACEHOLDER ANIMATION

    }

    private void ManageSwarm()
    {
        /*if (Input.GetKeyDown(KeyCode.LeftControl))
        {
            if (cursor.GetCursorActive())
            {
                if (_swarmActive) _swarmActive = false;
                else _swarmActive = true;
            }
        }

        if (_swarmActive) behind = cursor.GetComponentInChildren<Projector>().transform.position;*/

        if (Mathf.Abs(Input.GetAxis("CursorHorizontal")) > 0.6 || Mathf.Abs(Input.GetAxis("CursorVertical")) > 0.6)
        {
            if (!cursor.GetCursorActive())
            {
                behind = transform.position + (Camera.main.transform.right * Input.GetAxis("CursorHorizontal") + Vector3.Cross(Camera.main.transform.right, Vector3.up) * Input.GetAxis("CursorVertical")).normalized * BEHIND_DIST;
            }
        }
    }

    private void ManageCursor()
    {
        // make cursor appear / disappear
        if (Input.GetKeyDown(KeyCode.LeftShift) || Input.GetButtonDown("Cursor")) // CHANGE INPUT
        {
            if (cursor.GetCursorActive())
            {
                cursor.Disappear();
                _swarmActive = false;
            }
            else cursor.Appear();
        }
    }

    private void MinionCall()
    {
        callArea.orthographicSize = 0f;
        if (Input.GetKeyDown(KeyCode.Space) || Input.GetButtonDown("Recall"))
        {
            callTime = 0f;
            callRadius = 0f;
            callArea.enabled = true;
        }
        else if (Input.GetKey(KeyCode.Space) || Input.GetButton("Recall"))
        {
            callTime += Time.deltaTime;
            callRadius = callTime * callTime * CALL_RADIUS_SCALE;
            callArea.orthographicSize = callRadius;
            callText.GetComponent<TextMesh>().text = "+" + AIManager.staticManager.GetMinionsInRange(callRadius, transform.position, Names.PLAYER_LEADER).Count;
        }
        if (Input.GetKeyUp(KeyCode.Space) || Input.GetButtonUp("Recall"))
        {
            callTime = 0f;

            List<Minion> minionsInRange = AIManager.staticManager.GetMinionsInRange(callRadius, transform.position, Names.PLAYER_LEADER);
            foreach (Minion m in minionsInRange)
            {
                m.AbandonPeloton();
                this.myPeloton.AddMinion(m);
            }

            callRadius = 0f;
            callText.GetComponent<TextMesh>().text = "";

            callArea.enabled = false;
        }
    }

	/*void Move(float horizontal, float vertical)
	{
		//velocity += new Vector3(horizontal, 0, vertical) * accel;
		if (Mathf.Abs(horizontal) < deadzone) horizontal = 0;
		if (Mathf.Abs(vertical) < deadzone) vertical = 0;
		
		velocity += (Camera.main.transform.right * horizontal + Vector3.Cross(Camera.main.transform.right, Vector3.up) * vertical) * accel;
		
		if (velocity.magnitude > maxVel)
		{
			velocity.Normalize();
			velocity *= maxVel;
		}
		
		velocity -= velocity.normalized * drag;
		if (horizontal == 0 && vertical == 0 && velocity.magnitude < drag) velocity *= 0;
		
		transform.position = new Vector3(transform.position.x + velocity.x * Time.deltaTime, transform.position.y, transform.position.z + velocity.z * Time.deltaTime);
		animator.SetFloat("Speed", velocity.magnitude);
	}*/

    /*void Rotate()
    {
        Quaternion newRotation = new Quaternion();
        if (velocity.magnitude != 0)
        {
            newRotation = Quaternion.LookRotation(-velocity);
            transform.rotation = newRotation;
        }
    }*/

    private void ManageFlag()
    {
        if(Input.GetKeyDown(KeyCode.F) || Input.GetButtonDown("Flag")){

            if (hasFlag) PlaceFlag(transform.position);
            else PickFlag();
        }
    }

	private void ManageAttack()
	{
		if(Input.GetKeyDown(KeyCode.Mouse0) || Input.GetButtonDown("Attack")){
			if (atkCooldown <= 0f) Attack();
		}
	}

    void OnTriggerEnter(Collider other)
    {
        AnimatorStateInfo animState = anim.GetCurrentAnimatorStateInfo(1);
        if (animState.shortNameHash == 1080829965/*animState.IsName("Attack")*/)
        {
            if (name == Names.PLAYER_LEADER)
            {
                if (other.name == Names.ENEMY_MINION)
                {
                    leaderTarget = other.gameObject;
                    myPeloton.SetStateAndTarget(Names.STATE_ATTACK, leaderTarget);
                    other.GetComponent<Minion>().RecieveDamage(GetDamageOutput());
                }
                else if (other.name == Names.ENEMY_LEADER)
                {
                    leaderTarget = other.gameObject;
                    myPeloton.SetStateAndTarget(Names.STATE_ATTACK, leaderTarget);
                    other.GetComponent<EnemyLeader>().RecieveDamage(GetDamageOutput());
                }
                else if (other.name.Contains(Names.ENEMY_DOOR))
                {
                    leaderTarget = other.gameObject;
                    myPeloton.SetStateAndTarget(Names.STATE_ATTACK_DOOR, leaderTarget);
                    other.GetComponent<Door>().RecieveDamage(GetDamageOutput());
                }
            }
            /*else if (name == Names.ENEMY_LEADER)
            {
                if (other.name == Names.PLAYER_MINION)
                {
                    leaderTarget = other.gameObject;
                    myPeloton.SetStateAndTarget(Names.STATE_ATTACK, leaderTarget);
                    other.GetComponent<Minion>().RecieveDamage(GetDamageOutput());
                }
                else if (other.name == Names.PLAYER_LEADER)
                {
                    leaderTarget = other.gameObject;
                    myPeloton.SetStateAndTarget(Names.STATE_ATTACK, leaderTarget);
                    other.GetComponent<EnemyLeader>().RecieveDamage(GetDamageOutput());
                }
                else if (other.name.Contains(Names.PLAYER_DOOR))
                {
                    leaderTarget = other.gameObject;
                    myPeloton.SetStateAndTarget(Names.STATE_ATTACK_DOOR, leaderTarget);
                    other.GetComponent<Door>().RecieveDamage(GetDamageOutput());
                }
            }*/

            if (other.name == Names.PEPINO || other.name == Names.PIMIENTO || other.name == Names.MOLEM || other.name == Names.KEAWEE || other.name == Names.CHILI)
            {
                leaderTarget = other.GetComponent<Beast>().camp.gameObject;
                myPeloton.SetStateAndTarget(Names.STATE_ATTACK_CAMP, leaderTarget);
                other.GetComponent<Beast>().RecieveDamage(GetDamageOutput(), name);
            }
        }
    }

    // Does actions and determines his Peloton's new objective
    void OnTriggerStay(Collider other)
    {
        if (other.name == Names.TOTEM && velocity.magnitude < 15 && other.GetComponent<Totem>().alignment < 50)
        {
            leaderTarget = other.gameObject;
            myPeloton.SetStateAndTarget(Names.STATE_CONQUER, leaderTarget);
        }
    }


    /*void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawSphere(behind, 2);
    }*/
}
