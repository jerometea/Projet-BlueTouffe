using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;

public class EcranAccueil : MonoBehaviour {
    int i = 0;
	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	    if (i < 130)
        {
            transform.localScale += new Vector3(1, 1, 0);
            i++;
        }
        else
        {
            SceneManager.LoadScene(0);
        }
	}
}
