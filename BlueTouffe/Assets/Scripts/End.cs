using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using IsoTools;
using UnityEngine.SceneManagement;

public class End : MonoBehaviour {

    GameObject[] _players;
    public List<GameObject> PlayersInEnd = new List<GameObject>();


    // Use this for initialization
    void Start () {

	}
	
	// Update is called once per frame
	void Update () {
        _players = GameObject.FindGameObjectsWithTag("Player");
        Debug.Log("Nb of players :" + _players.Length + " | Nb of players in End:" + PlayersInEnd.Count);
        if (PlayersInEnd.Count >= _players.Length)
        {
            Debug.Log("Fin du jeu");
            SceneManager.LoadScene("EndMenu");
        }
	}

    void OnIsoTriggerEnter(IsoCollider col)
    {
        if (col.gameObject.tag == "Player")
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
        if (col.gameObject.tag == "Player")
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
