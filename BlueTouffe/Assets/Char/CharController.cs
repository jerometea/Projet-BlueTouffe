using UnityEngine;
using UnityEngine.Networking;
using System.Collections;
using System;
using UnityEngine.UI;

public class CharController : MonoBehaviour
{

    Animator _anim;
    GameObject _cam;
    Collider2D _friend;
    public GameObject load;

    public bool _isSaving;

    bool _isRez;
    int _distCam = 0;
    float _cross = 1f;

    float _shootY = 0;
    float _shootX = 0;

    int NeededTimeRez = 100;
    int TimeRez = 0;
    public int rezSpeed = 1;

    public float _speed = 5;

    GameObject _joyRightGameObj;
    Joystick _JoystickRight;

    GameObject _joystickGameObj;
    Joystick _Joystick;

    NetworkView _network;

    Button _buttonRez;
    // Use this for initialization
    void Start()
    {
        _anim = GetComponent<Animator>();
        _cam = GameObject.Find("MainCamera");

        _joystickGameObj = GameObject.Find("Joystick left");
        _Joystick = _joystickGameObj.GetComponent<Joystick>();

        _joyRightGameObj = GameObject.Find("Joystick right");
        _JoystickRight = _joyRightGameObj.GetComponent<Joystick>();

        _network = GetComponent<NetworkView>();

    }

    // Update is called once per frame
    void Update()
    {
        if ( _network.isMine )
        {

            if ( !gameObject.GetComponent<CharHealth>().IsDead )
            {
                _shootY = _JoystickRight.JoystickInput.y;
                _shootX = _JoystickRight.JoystickInput.x;

                if ( _isSaving )
                {
                    Ressurection();
                }
                else
                {
                    if ( _shootY == 0 && _shootX == 0 )
                    {
                        Mouvement();
                    }
                    else
                    {
                        Shooting();
                    }
                    Destroy(GameObject.Find("LoadingCanvas(Clone)"));
                    _anim.SetBool("IsRez", false);
                    TimeRez = 0;
                }
            }
        }


    }

    void OnTriggerEnter2D( Collider2D other )
    {
        if ( other.GetComponent<CharHealth>() != null )
        {
            _friend = other;
        }
    }

    void OnTriggerExit2D( Collider2D other )
    {
        if ( _friend != null && _friend.name == other.name )
            _friend = null;
    }

    void Shooting()
    {

        _anim.SetBool("isShooting", true);
        _anim.SetFloat("shootY", _shootY);
        _anim.SetFloat("shootX", _shootX);

        if ( _shootX > 0 )
        {
            transform.eulerAngles = new Vector2(0, 180);
        }
        else if ( _shootX < 0 )
        {
            transform.eulerAngles = new Vector2(0, 0);
        }
    }

    void Mouvement()
    {
        Vector3 direction = _Joystick.JoystickInput;
        Vector3 directionFixe = direction / (float)(Math.Sqrt(direction.x * direction.x + direction.y * direction.y) / 60);
        float y = directionFixe.y;
        float x = directionFixe.x;
        Weapon weap = gameObject.GetComponent<Weapon>();

        _anim.SetFloat("speedY", y);
        _anim.SetFloat("speedX", x);
        _anim.SetInteger("weap", 3);
        _anim.SetInteger("weap", weap.GetWeapon);
        _anim.SetBool("isShooting", false);

        if ( x != 0 ) _cross = 0.5f;

        if ( y > 0 )
        {
            transform.Translate(0, (y * Time.deltaTime), 0);
            Debug.Log("y : " + y);
        }
        else if ( y < 0 )
        {
            transform.Translate(0, (y * Time.deltaTime), 0);

            Debug.Log(y);

        }

        if ( x > 0 )
        {
            transform.Translate(-x * Time.deltaTime, 0, 0);

            transform.eulerAngles = new Vector2(0, 180);
            Debug.Log("x : " + x);

        }
        else if ( x < 0 )
        {
            transform.Translate(x * Time.deltaTime, 0, 0);

            transform.eulerAngles = new Vector2(0, 0);
            Debug.Log("x : " + x);

        }

        _cam.transform.position = new Vector3(transform.position.x, transform.position.y, transform.position.z - 1);
    }

    void Ressurection()
    {
        _anim.SetBool("isShooting", false);
        if ( _friend != null )
        {
            Debug.Log("plop");

            //if(!_friend.GetComponent<CharHealth>().IsDead)
            if ( _friend.GetComponent<Animator>().GetBool("isDead") )
            {
                if ( TimeRez == 0 )
                {
                    GameObject loading = Instantiate(load, gameObject.transform.position, gameObject.transform.rotation) as GameObject;
                    loading.GetComponentInChildren<RPB>().RezSpeed = rezSpeed;
                    _anim.SetBool("IsRez", true);
                }
                TimeRez += rezSpeed;

                if ( TimeRez == NeededTimeRez )
                {
                    _friend.GetComponent<CharHealth>().PlayerRez();
                }
            }
        }
    }

    public float ShootX
    {
        get { return _shootY; }
    }

    public float ShootY
    {
        get { return _shootX; }
    }

    public void GetHurt()
    {
        TimeRez = 0;
    }
}
