using UnityEngine;
using System.Collections;

public class OrderChild : MonoBehaviour {

    public int OrderFromParent;

    // Use this for initialization
    void Start () {
        gameObject.GetComponent<Renderer>().sortingOrder = gameObject.transform.parent.GetComponent<Renderer>().sortingOrder + OrderFromParent;
	}
	
	// Update is called once per frame
	void Update () {
	
	}
}
