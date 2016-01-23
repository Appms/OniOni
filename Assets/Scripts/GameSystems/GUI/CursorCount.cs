using UnityEngine;
using System.Collections;

public class CursorCount : MonoBehaviour {

    public GameObject cursor;

    // Update is called once per frame
    void Update()
    {
        if (!AIManager.staticManager.DisableElements)
        {
            transform.position = cursor.transform.position;
            transform.position += cursor.transform.up * 5;
            transform.LookAt(Camera.main.transform.position);
            transform.Rotate(0f, 180f, 0f);
        }
    }
}
