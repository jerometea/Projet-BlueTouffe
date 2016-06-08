using UnityEngine;
using UnityEngine.Networking;
using System.Collections;
using System;

public class CharController : MonoBehaviour {

    Animator _anim;
    GameObject _cam;
    int _distCam = 0;
    float _cross = 1f;
    float shootY = 0;
    float shootX = 0;
    public TouchPad _touchMove;

    // Use this for initialization
    void Start () {
        _anim = GetComponent<Animator>();
        _cam = GameObject.Find( "Camera" );
    }

    // Update is called once per frame
    void Update()
    {
        if(!gameObject.GetComponent<CharHealth>().IsDead)
        {
            shootY = Input.GetAxisRaw( "Vertical2" );
            shootX = Input.GetAxisRaw( "Horizontal2" );

            if( shootY == 0 && shootX == 0 )
            {
                Mouvement();
            }
            else
            {
                Shooting();
            }

            if( Input.GetButton( "Fire1" ) )
            {
                _anim.SetBool( "IsRez", true );
            } else
            {
                _anim.SetBool( "IsRez", false );
            }
        }
        transform.Translate(new Vector3(_touchMove.TouchPadInput.x, _touchMove.TouchPadInput.y, 0) * 5 * Time.deltaTime, Space.World);

    }

    void Shooting()
    {

        _anim.SetBool( "isShooting", true );
        _anim.SetFloat( "shootY", shootY );
        _anim.SetFloat( "shootX", shootX );

        if( shootX > 0 )
        {
            transform.eulerAngles = new Vector2( 0, 180 );
        }
        else if( shootX < 0 )
        {
            transform.eulerAngles = new Vector2( 0, 0 );
        }
    }

    void Mouvement()
    {
        float y = Input.GetAxisRaw("Vertical");
        float x = Input.GetAxisRaw("Horizontal");
        Weapon weap = gameObject.GetComponent<Weapon>();

        _anim.SetFloat( "speedY", y );
        _anim.SetFloat( "speedX", x );
        _anim.SetInteger( "weap", 3 );
        _anim.SetInteger( "weap", weap.GetWeapon );
        _anim.SetBool( "isShooting", false );

        if( x != 0 ) _cross = 0.5f;

        if( y > 0 )
        {
            transform.Translate( 0, (y * Time.deltaTime) * _cross, 0 );
        }
        else if( y < 0 )
        {
            transform.Translate( 0, (y * Time.deltaTime) * _cross, 0 );
        }

        if( x > 0 )
        {
            transform.Translate( -x * Time.deltaTime, 0, 0 );
            transform.eulerAngles = new Vector2( 0, 180 );

        }
        else if( x < 0 )
        {
            transform.Translate( x * Time.deltaTime, 0, 0 );
            transform.eulerAngles = new Vector2( 0, 0 );
        }

        _cam.transform.position = new Vector3( transform.position.x, transform.position.y, transform.position.z - 1 );
    }

    public float ShootX
    {
        get { return shootX; }
    }

    public float ShootY
    {
        get { return shootY; }
    }
}
