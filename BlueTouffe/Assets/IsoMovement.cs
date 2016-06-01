using UnityEngine;
using System.Collections;
using IsoTools;
using Zenject;

public class IsoMovement : MonoBehaviour {


    float speed = 3;

    Joystick _Joystick;

    NetworkView _networkView;


    GameObject _camera;
    // Use this for initialization

    void Start () {
        //_Joystick = Instantiate(_Joystick);
        _camera = GameObject.Find("MainCamera");
        _Joystick = (Joystick) FindObjectOfType(typeof(Joystick));
    }

    void Awake()
    {
        _networkView = GetComponent<NetworkView>();
    }

    // Update is called once per frame
    void Update () {
        //var iso_rigidbody = GetComponent<IsoRigidbody>();
        //if (iso_rigidbody)
        //{
        //    if (Input.GetKey(KeyCode.LeftArrow))
        //    {
        //        iso_rigidbody.velocity = new Vector3(-2, 2, 0);
        //    }
        //    if (Input.GetKey(KeyCode.RightArrow))
        //    {
        //        iso_rigidbody.velocity = new Vector3(2, -2, 0);
        //    }
        //    if (Input.GetKey(KeyCode.UpArrow))
        //    {
        //        iso_rigidbody.velocity = new Vector3(2, 2, 0);
        //    }
        //    if (Input.GetKey(KeyCode.DownArrow))
        //    {
        //        iso_rigidbody.velocity = new Vector3(-2, -2, 0);
        //    }
        //    if(Input.GetKey(KeyCode.DownArrow) && Input.GetKey(KeyCode.RightArrow))
        //    {
        //        iso_rigidbody.velocity = new Vector3(0, -2, 0);
        //    }
        //    if (Input.GetKey(KeyCode.DownArrow) && Input.GetKey(KeyCode.LeftArrow))
        //    {
        //        iso_rigidbody.velocity = new Vector3(-2, 0, 0);
        //    }
        //    if (Input.GetKey(KeyCode.UpArrow) && Input.GetKey(KeyCode.LeftArrow))
        //    {
        //        iso_rigidbody.velocity = new Vector3(0, 2, 0);
        //    }
        //    if (Input.GetKey(KeyCode.UpArrow) && Input.GetKey(KeyCode.RightArrow))
        //    {
        //        iso_rigidbody.velocity = new Vector3(2, 0, 0);
        //    }
        //    if (Input.GetKey(KeyCode.Space))
        //    {
        //        iso_rigidbody.AddForce(new Vector3(0, 0, 1), ForceMode.Impulse);
        //    }
        //}

        if ( _networkView.isMine )
        {
            if ( _Joystick.JoystickInput.x < 0 )
                _Joystick.JoystickInput.x = -1;
            else if ( _Joystick.JoystickInput.x > 0 )
                _Joystick.JoystickInput.x = 1;

            if ( _Joystick.JoystickInput.y < 0 )
                _Joystick.JoystickInput.y = -1;
            else if ( _Joystick.JoystickInput.y > 0 )
                _Joystick.JoystickInput.y = 1;

            transform.Translate(new Vector3(_Joystick.JoystickInput.x, _Joystick.JoystickInput.y, 0) * speed , Space.World);
            _camera.transform.position = new Vector3(transform.position.x, transform.position.y, transform.position.z - 1);

        }

    }
}
