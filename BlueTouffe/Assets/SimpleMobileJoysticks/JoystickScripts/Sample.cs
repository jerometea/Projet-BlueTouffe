using UnityEngine;
using System.Collections;

public class Sample : MonoBehaviour {
	[Tooltip("Speed of object rotation.")]
	public float speed = 100;
	[Tooltip("What joystick to use.")]
	public Joystick _Joystick;
	[Tooltip("What touch pad to use.")]
	public TouchPad _TouchPad;
	// Use this for initialization
	void Start () {
		if (_Joystick && _TouchPad) {
			Debug.LogWarning("You have two controllers.");
		}

		if (!_Joystick && !_TouchPad) {
			Debug.LogWarning("You have no controllers.");
		}
	}
	
	// Update is called once per frame
	void Update () {


        if ( _Joystick )
        {
            if ( _Joystick.JoystickInput.x < 0 )
                _Joystick.JoystickInput.x = -1;
            else if ( _Joystick.JoystickInput.x > 0) 
                _Joystick.JoystickInput.x = 1;

            if ( _Joystick.JoystickInput.y < 0 )
                _Joystick.JoystickInput.y = -1;
            else if ( _Joystick.JoystickInput.y > 0 )
                _Joystick.JoystickInput.y = 1;

            transform.Translate(new Vector3(_Joystick.JoystickInput.x, _Joystick.JoystickInput.y, 0) * 1 * Time.deltaTime, Space.World);


        }


        if (_TouchPad)
			transform.Rotate(new Vector3(_TouchPad.TouchPadInput.y,-_TouchPad.TouchPadInput.x,0)* speed * Time.deltaTime, Space.World);
	}
}
