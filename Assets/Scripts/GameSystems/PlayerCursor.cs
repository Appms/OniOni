using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

public class PlayerCursor : MonoBehaviour {

    public Vector3 cameraRelativePos;

    GameObject cursorImage;
    Leader leaderScript;
    bool _cursorActive = false;
    bool _swarmActive = false;
    public int maxRange;
    GameObject leader;
    Vector3 destiny = new Vector3();
    public LayerMask cursorLayerMask;
    int _minionsToSend;
    int cursorVel = 100;
    Vector3 velocity = new Vector3();
    GameObject target;
    Vector3 targetPos;
    int orderRange = 30;
    GameObject cursorText;


    void Start () {

        cursorImage = GameObject.Find("CursorImage");
        cursorText = GameObject.Find("CursorText");
        //Cursor.visible = false;
        //Cursor.lockState = CursorLockMode.Locked;
        Disappear();
    }

	void Update () {

        if(Input.GetJoystickNames().Length != 0) MoveCursor(Input.GetAxis("CursorHorizontal"), Input.GetAxis("CursorVertical"));
        else MoveCursor(Input.GetAxis("Mouse X"), Input.GetAxis("Mouse Y"));
        

        if (_cursorActive)
        {
            if ((Input.GetKeyDown(KeyCode.E) || Input.GetButtonDown("PlusMinion")) && _minionsToSend < leaderScript.myPeloton.Size()) ++_minionsToSend;//_minionsToSend++;
            if ((Input.GetKeyDown(KeyCode.Q) || Input.GetButtonDown("MinusMinion")) && _minionsToSend > 0) --_minionsToSend;//_minionsToSend--;
            if ((Input.GetKeyDown(KeyCode.Mouse1) || Input.GetButton("PlusMinion") && Input.GetButton("MinusMinion")) && _minionsToSend > 0)
                SendOrder(); // CHANGE INPUT

            if (_minionsToSend > 0) cursorText.GetComponent<TextMesh>().text = "-" + _minionsToSend;
            else cursorText.GetComponent<TextMesh>().text = "";

            if (Input.GetKeyDown(KeyCode.LeftControl))  _swarmActive = true;
            if (Input.GetKeyUp(KeyCode.LeftControl)) _swarmActive = false;
        }
    }

    public void Appear() {

        _cursorActive = true;
        _minionsToSend = 0;
        cursorImage.SetActive(true);
        cursorImage.transform.position = leader.transform.position;
    }

    public void Disappear() {

        _cursorActive = false;
        _minionsToSend = 0;
        target = null;
        cursorText.GetComponent<TextMesh>().text = "";
        cursorImage.SetActive(false);
    }

    public bool GetCursorActive() {

        return _cursorActive;
    }

    public void SetLeader(GameObject leader)
    {
        this.leader = leader;
        leaderScript = leader.GetComponent<Leader>();
    }

    void MoveCursor(float h, float v)
    {
        // camera relative
        velocity = (Camera.main.transform.right * h + Vector3.Cross(Camera.main.transform.right, Vector3.up) * v) * cursorVel;
        velocity += leader.GetComponent<PlayerLeader>().velocity;

        RaycastHit hit;

       if(Physics.Raycast(cursorImage.transform.position, velocity, out hit, velocity.magnitude * Time.deltaTime, LayerMask.GetMask("Level"))){

            float angle = 90 - Vector3.Angle(velocity, -hit.normal);
            float cos = Mathf.Cos(Mathf.Deg2Rad * angle);
            Vector3 newVelocity = Vector3.Cross(hit.normal, Vector3.up) * velocity.magnitude * cos;
            if (Vector3.Angle(velocity, newVelocity) > 90f) velocity = -newVelocity;
            else velocity = newVelocity;

            Vector3 newPosition = hit.point + hit.normal * 0.1f;
            cursorImage.transform.position = new Vector3(newPosition.x, cursorImage.transform.position.y, newPosition.z);
        }

        if (Vector3.Distance(leader.transform.position, new Vector3(cursorImage.transform.position.x + velocity.x * Time.deltaTime, cursorImage.transform.position.y, cursorImage.transform.position.z + velocity.z * Time.deltaTime)) <= maxRange)
            cursorImage.transform.position = new Vector3(cursorImage.transform.position.x + velocity.x * Time.deltaTime, cursorImage.transform.position.y, cursorImage.transform.position.z + velocity.z * Time.deltaTime);
        else
            cursorImage.transform.position = targetPos;

        cursorImage.transform.Rotate(Vector3.up * 4);

        /*float limitDist = Camera.main.GetComponent<CameraMovement>().zoom / 3 + orthographicSize/4; 

        float distZ = (cursorImage.transform.position - Camera.main.transform.position).z;
        float bottomBorder = Camera.main.ViewportToWorldPoint(new Vector3(0, 0, distZ)).z + limitDist;
        float topBorder = Camera.main.ViewportToWorldPoint(new Vector3(0, 1, distZ)).z; // WTF??

        //cursorImage.transform.position = new Vector3(cursorImage.transform.position.x, cursorImage.transform.position.y, Mathf.Clamp(cursorImage.transform.position.z, bottomBorder, topBorder));
        if (cursorImage.transform.position.z <= bottomBorder) cursorImage.transform.position = new Vector3(cursorImage.transform.position.x, cursorImage.transform.position.y, bottomBorder);
        //if (cursorImage.transform.position.z >= topBorder) cursorImage.transform.position = new Vector3(cursorImage.transform.position.x, cursorImage.transform.position.y, topBorder );*/

        targetPos = cursorImage.transform.position;
    }

    void SendOrder() {

        RaycastHit[] elements = Physics.SphereCastAll(targetPos, orderRange, Vector3.up, 0, cursorLayerMask);

        if(elements.Length != 0)
        {
            float dist = orderRange;
            for(int i = 0; i<elements.Length; i++)
            {
                if(Vector3.Distance(targetPos, elements[i].collider.ClosestPointOnBounds(targetPos)) <= dist && elements[i].collider.gameObject.name != Names.PLAYER_LEADER)
                {
                    target = elements[i].collider.gameObject;
                    dist = Vector3.Distance(destiny, elements[i].collider.gameObject.transform.position);
                }
            }
        }

        if(target != null)
        {
            //special case for door
            if (target.name.Contains(Names.ENEMY_DOOR)) target = GameObject.Find(Names.ENEMY_DOOR);
            else if (target.name.Contains(Names.PLAYER_DOOR)) target = GameObject.Find(Names.PLAYER_DOOR);
            leaderScript.NewOrder(_minionsToSend, target);
        }

        else leaderScript.NewOrder(_minionsToSend, targetPos);

        _minionsToSend = 0;
        cursorText.GetComponent<TextMesh>().text = "";
        target = null;
    }
}
