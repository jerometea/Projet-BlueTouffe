using UnityEngine;
using System.Collections;

public class CharController : MonoBehaviour {

    public Animator _anim;
    public GameObject _cam;
    public int _distCam = 5;
    float _cross = 1f;

    // Use this for initialization
    void Start () {
        _anim = GetComponent<Animator>();
        _cam = GameObject.Find( "Camera" );
    }
	
	// Update is called once per frame
	void Update () {
        float y = Input.GetAxisRaw("Vertical");
        float x = Input.GetAxisRaw("Horizontal");

        _anim.SetFloat( "speedY", y );
        _anim.SetFloat( "speedX", x );
        if( x != 0 ) _cross = 0.5f;

        if( y > 0)
        {
            transform.Translate( 0, (y * Time.deltaTime) * _cross, 0 );
        } else if (y < 0)
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

        _cam.transform.position = new Vector3( transform.position.x, transform.position.y, transform.position.z - _distCam );
    }
}
