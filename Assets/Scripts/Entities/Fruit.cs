using UnityEngine;
using System.Collections;

public class Fruit : MonoBehaviour {

    float push_per_minion = 0.5f;
    GameObject fruitMesh;
    GameObject orangeObjective;
    GameObject purpleObjective;
    GameObject orangeDoor;
    GameObject purpleDoor;
    float angle;
    float velocity;
    public bool canAdvanceToOrange = true;
    public bool canAdvanceToPurple = true;
    public float radius;
    AudioSource melonAudio;
    AudioSource endAudio;
    bool pushed;
    ParticleSystem enemyParticles;
    ParticleSystem playerParticles;
    public Camera enemyCamera;
    public Camera playerCamera;
    public Camera mainCamera;
    bool soundPlayed = false;

    void Start()
    {
        fruitMesh = GameObject.Find(Names.FRUIT);
        orangeObjective = GameObject.Find(Names.ORANGE_OBJECTIVE);
        purpleObjective = GameObject.Find(Names.PURPLE_OBJECTIVE);
        orangeDoor = GameObject.Find(Names.PLAYER_DOOR);
        purpleDoor = GameObject.Find(Names.ENEMY_DOOR);
        radius = fruitMesh.GetComponent<SphereCollider>().radius * 300 + 20;
        melonAudio = GetComponents<AudioSource>()[0];
        endAudio = GetComponents<AudioSource>()[1];

        enemyParticles = GameObject.Find("EnemyExplosion").GetComponent<ParticleSystem>();
        playerParticles = GameObject.Find("PlayerExplosion").GetComponent<ParticleSystem>();
        playerCamera.enabled = false;
        enemyCamera.enabled = false;
    }
    
    void Update()
    {
        pushed = false;
        if (AIManager.staticManager.EndGame)
        {
            if (!endAudio.isPlaying && !soundPlayed)
            {
                endAudio.Play();
                soundPlayed = true;
                fruitMesh.GetComponent<MeshRenderer>().enabled = false;
            }  
        }

        if (transform.position.z >= 370 ){

            mainCamera.enabled = false;
            playerCamera.enabled = true;
            AIManager.staticManager.DisableElements = true;

            if (transform.position.z >= 430)
            {
                AIManager.staticManager.EndGame = true;
                playerParticles.Play();
            }
        }
        else if(transform.position.z <= -370)
        {
            mainCamera.enabled = false;
            enemyCamera.enabled = true;
            AIManager.staticManager.DisableElements = true;
            if (transform.position.z <= -430)
            {
                AIManager.staticManager.EndGame = true;
                enemyParticles.Play();
            }
        }
    }

    void LateUpdate() { 

        if(!pushed && melonAudio.isPlaying) melonAudio.Stop();
    }

    public void pushMelon(Peloton peloton)
    {
        if (!AIManager.staticManager.EndGame)
        {
            pushed = true;
            if (!melonAudio.isPlaying) melonAudio.Play();
            int minionCount = peloton.GetMinionList().Count;
            velocity = peloton.Size() * push_per_minion * (peloton.leader.GetComponent<Leader>().pushBuff > 0 ? peloton.leader.GetComponent<Leader>().BUFF_MULTIPLYER : 1f) * Time.deltaTime;

            if (peloton.leader.name == Names.ENEMY_LEADER && canAdvanceToOrange)
            {
                velocity *= -1;
                angle = velocity / fruitMesh.GetComponent<SphereCollider>().radius;
                fruitMesh.transform.Rotate(angle / 2f, 0, 0);
                transform.position += new Vector3(0, 0, 1) * velocity;
                peloton.transform.position = purpleObjective.transform.position;
            }

            if (peloton.leader.name == Names.PLAYER_LEADER)
            {
                angle = velocity / fruitMesh.GetComponent<SphereCollider>().radius;
                fruitMesh.transform.Rotate(angle / 2f, 0, 0);
                transform.position += new Vector3(0, 0, 1) * velocity;
                peloton.transform.position = orangeObjective.transform.position;
            }

            if (transform.position.z + radius < orangeDoor.transform.position.z && orangeDoor.GetComponent<Door>().doorsUp)
                canAdvanceToOrange = false;
            else canAdvanceToOrange = true;
            if (transform.position.z >= purpleDoor.transform.position.z - radius && purpleDoor.GetComponent<Door>().doorsUp)
                canAdvanceToPurple = false;
            else canAdvanceToPurple = true;
        }
    }
}
