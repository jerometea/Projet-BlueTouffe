using UnityEngine;
using System.Collections;

public class PlayerDisplayScript : MonoBehaviour {

    public string _name;

    public string Name
    {
        get { return _name; }
        set { _name = value; }
    }

    [RPC]
    public void ChangeName (string name)
    {
        for (int i = 0; i < transform.childCount - 1; i++)
        {
            if (transform.GetChild(i).transform.name == "Name")
            {
                transform.GetChild(i).transform.GetComponent<TextMesh>().text = name;
            }
        }
    }

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}
}
