using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class RPB : MonoBehaviour {

    public Transform Loading;
    [SerializeField]
    private float currentAmount;
    int rezSpeed;

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	    if(currentAmount < 100)
        { 
            currentAmount += rezSpeed;
        } else
        {
            Destroy( gameObject );
        }
        Loading.GetComponent<Image>().fillAmount = currentAmount / 100;
	}

    public int RezSpeed
    {
        get { return rezSpeed; }
        set { rezSpeed = value; }
    }
}
