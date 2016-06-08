using UnityEngine;
using System.Collections;

public class ProjController : MonoBehaviour {

    public float speed;
    public int Damage;

	// Use this for initialization
	void Start () {
        Damage = 10;
        Destroy( gameObject, 0.75f );
    }
	
	// Update is called once per frame
	void Update () {

    }

    public void FireInTheHole(Vector2 direction)
    {
        CharController CharC = GameObject.Find("Char").GetComponent<CharController>();
        Rigidbody2D rigi2D = GetComponent<Rigidbody2D>();
        rigi2D.velocity = direction * 3;
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        Debug.Log( "We hit " + other.name + " and did " + Damage + " damage." );
        if( other.GetComponent<HealthEnemy>() != null )
        {
            if( !other.GetComponent<HealthEnemy>().IsDead )
            {
                HealthEnemy HealthEnemy = other.GetComponent<HealthEnemy>();
            HealthEnemy.ReceiveDamage( Damage );
            Destroy( gameObject );
            }
        }
    }
}
