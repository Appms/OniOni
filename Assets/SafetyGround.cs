using UnityEngine;
using System.Collections;

public class SafetyGround : MonoBehaviour {

    Vector3 transformPos;

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}

    void OnCollisionEnter(Collision collision)
    {
        foreach (ContactPoint contact in collision.contacts)
        {
            if(!(contact.otherCollider.gameObject.name == Names.FRUIT))
            {
                RaycastHit hit;
                if (Physics.Raycast(contact.point + new Vector3(0, 10f, 0), Vector3.down, out hit))
                {
                    if(contact.otherCollider.gameObject.name.Contains("Minion") || contact.otherCollider.gameObject.name.Contains("Leader"))
                    {
                        transformPos = contact.otherCollider.gameObject.transform.position;
                        contact.otherCollider.gameObject.transform.position += new Vector3(0, (hit.point.y - transformPos.y) + 1f, 0);
                    }
                }
            }
            //contact.otherCollider.gameObject.transform.position += new Vector3(0, 1, 0);
        }
    }
}
