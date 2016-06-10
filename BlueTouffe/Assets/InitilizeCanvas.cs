using UnityEngine;
using System.Collections;
using Zenject;

public class InitilizeCanvas : MonoBehaviour {

    [Inject("canvas")]
    GameObject _canvasTouchpads;


    TouchPad _touchMove;
    TouchPad touchBullet;
    // Use this for initialization
    void Start () {
        Instantiate(_canvasTouchpads);
      
	}
	
	// Update is called once per frame
	void Update () {
	
	}
}
