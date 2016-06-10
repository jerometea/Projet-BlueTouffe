using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class End : MonoBehaviour {

    public List<GameObject> PlayersInEnd = new List<GameObject>();


	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}

    void OnTriggerEnter(Collider col)
    {
        if(col.gameObject.tag == "Char")
        {
            PlayersInEnd.Add(col.gameObject);
        }
        foreach(GameObject g in PlayersInEnd)
        {
            Debug.Log("coucou");
        }
    }

    void OnTriggerExit(Collider col)
    {
        if (col.gameObject.tag == "Char")
        {
            PlayersInEnd.Remove(col.gameObject);
        }
        foreach (GameObject g in PlayersInEnd)
        {
            Debug.Log(g);
        }
    }
}
