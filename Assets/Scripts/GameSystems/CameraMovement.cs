using UnityEngine;
using System.Collections;

public class CameraMovement : MonoBehaviour {

    public GameObject target;

    float initDist = 60;//75;
    float initHeight = 45;//60;
    float dist;
    float height;
    float rotDamping = 3.0f;
    float cursorSensitivity = 20;
    int zoomVel = 15;
    float zoomDamping = 10;
    int initZoom = 55;
    Vector3 initPos;
    Quaternion initRot;

    [Range(10, 80)]
    public float zoom;

    // Use this for initialization
    void Start () {

        dist = initDist;
        height = initHeight;
        zoom = initZoom;
        initPos = transform.position;
        initRot = transform.rotation;
	}
	
	// Update is called once per frame
	void LateUpdate () {

        if(target.GetComponent<Leader>().deathCooldown == 0)
        {
            if (Input.GetAxis("Mouse ScrollWheel") != 0)
                zoom -= Input.GetAxis("Mouse ScrollWheel") * zoomVel;

            else if (Input.GetAxis("CamZoom") != 0)
                zoom -= Input.GetAxis("CamZoom") / 20 * zoomVel;

            zoom = Mathf.Clamp(zoom, 20, 80);

            RotateCam(Input.GetAxis("Mouse X"), target.GetComponent<PlayerLeader>().cursor.GetCursorActive());
            RotateCam(-Input.GetAxis("CamJoystick"), target.GetComponent<PlayerLeader>().cursor.GetCursorActive());
        }
    }

    void RotateCam(float mouseInput, bool cursorActive)
    {
        float currentRotationAngle = transform.eulerAngles.y;
        Quaternion currentRotation = Quaternion.Euler(0, currentRotationAngle, 0);

        if (mouseInput != 0 && !cursorActive && !Input.GetKey(KeyCode.LeftControl))
        {
            //rotation = direction == "r" ? -angle : angle; 
            currentRotationAngle = Mathf.LerpAngle(currentRotationAngle, currentRotationAngle + mouseInput * cursorSensitivity, rotDamping * Time.deltaTime);
            currentRotation = Quaternion.Euler(0, currentRotationAngle, 0);
        }

        height = Mathf.Lerp(height, initHeight * zoom / initZoom, Time.deltaTime * zoomDamping);
        dist = Mathf.Lerp(dist, initDist * zoom / initZoom, Time.deltaTime * zoomDamping);

        Vector3 targetPos = target.transform.position + Vector3.up * height;
        targetPos -= currentRotation * Vector3.forward * dist;
        //transform.position = Vector3.Lerp(transform.position, targetPos, Time.deltaTime);
        transform.position = targetPos;

        transform.LookAt(target.transform.position + Vector3.up * (100-zoom)/10);
    }

    public void setToDefault()
    {
        /*float currentRotationAngle = initAngle;
        Quaternion currentRotation = Quaternion.Euler(0, currentRotationAngle, 0);

        currentRotationAngle = Mathf.LerpAngle(currentRotationAngle, currentRotationAngle, rotDamping * Time.deltaTime);
        currentRotation = Quaternion.Euler(0, currentRotationAngle, 0);*/

        /*Vector3 targetPos = target.transform.position + Vector3.up * initHeight;
        targetPos -= currentRotation * Vector3.forward * initDist;*/

        transform.position = Vector3.Lerp(transform.position, initPos, Time.deltaTime);
        transform.LookAt(target.transform.position + Vector3.up * (100 - initZoom) / 10);

        dist = initDist;
        height = initHeight;
        zoom = initZoom;
        transform.rotation = initRot;
    }
}
