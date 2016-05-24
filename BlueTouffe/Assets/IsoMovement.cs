using UnityEngine;
using System.Collections;
using IsoTools;

public class IsoMovement : MonoBehaviour {

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
        var iso_rigidbody = GetComponent<IsoRigidbody>();
        if (iso_rigidbody)
        {
            if (Input.GetKey(KeyCode.LeftArrow))
            {
                iso_rigidbody.velocity = new Vector3(-2, 2, 0);
            }
            if (Input.GetKey(KeyCode.RightArrow))
            {
                iso_rigidbody.velocity = new Vector3(2, -2, 0);
            }
            if (Input.GetKey(KeyCode.UpArrow))
            {
                iso_rigidbody.velocity = new Vector3(2, 2, 0);
            }
            if (Input.GetKey(KeyCode.DownArrow))
            {
                iso_rigidbody.velocity = new Vector3(-2, -2, 0);
            }
            if(Input.GetKey(KeyCode.DownArrow) && Input.GetKey(KeyCode.RightArrow))
            {
                iso_rigidbody.velocity = new Vector3(0, -2, 0);
            }
            if (Input.GetKey(KeyCode.DownArrow) && Input.GetKey(KeyCode.LeftArrow))
            {
                iso_rigidbody.velocity = new Vector3(-2, 0, 0);
            }
            if (Input.GetKey(KeyCode.UpArrow) && Input.GetKey(KeyCode.LeftArrow))
            {
                iso_rigidbody.velocity = new Vector3(0, 2, 0);
            }
            if (Input.GetKey(KeyCode.UpArrow) && Input.GetKey(KeyCode.RightArrow))
            {
                iso_rigidbody.velocity = new Vector3(2, 0, 0);
            }
            if (Input.GetKey(KeyCode.Space))
            {
                iso_rigidbody.AddForce(new Vector3(0, 0, 1), ForceMode.Impulse);
            }
        }
	}
}
