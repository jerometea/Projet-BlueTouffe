using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using IsoTools;

public class End : MonoBehaviour {

    public List<GameObject> PlayersInEnd = new List<GameObject>();


    // Use this for initialization
    void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}

    void OnIsoTriggerEnter(IsoCollider col)
    {
        if (col.gameObject.name == "Char")
        {
            PlayersInEnd.Add(col.gameObject);
            Debug.Log("enter");
        }
        foreach (GameObject g in PlayersInEnd)
        {
            Debug.Log(g);
        }
    }

    void OnIsoTriggerExit(IsoCollider col)
    {
        if (col.gameObject.name == "Char")
        {
            PlayersInEnd.Remove(col.gameObject);
            Debug.Log("exit");
        }
        foreach (GameObject g in PlayersInEnd)
        {
            Debug.Log(g);
        }
    }
}
