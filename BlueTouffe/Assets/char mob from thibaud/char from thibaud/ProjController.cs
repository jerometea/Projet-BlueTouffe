using UnityEngine;
using System.Collections;
using System;

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
        Debug.Log("direction : " + direction);
        GameObject charecter = GameObject.Find("Char");
        Debug.Log("characterrr object" + charecter);

        Rigidbody2D rigi2D = GetComponent<Rigidbody2D>();
        if ( rigi2D == null ) Debug.Log("rigi body 2d is null");
        Debug.Log("rigidbody 2D : " + rigi2D);

        //CharController CharC = GameObject.Find("Char").GetComponent<CharController>();
        //if ( CharC == null ) Debug.Log("char is null");
        //Debug.Log("character : " + CharC);

        rigi2D.velocity = direction / (float)(Math.Sqrt(direction.x * direction.x + direction.y * direction.y) / 100);
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if( other.GetComponent<HealthEnemy>() != null )
        {
            Debug.Log( "We hit " + other.name + " and did " + Damage + " damage." );
            if( !other.GetComponent<HealthEnemy>().IsDead )
            {
                HealthEnemy HealthEnemy = other.GetComponent<HealthEnemy>();
            HealthEnemy.ReceiveDamage( Damage );
            Destroy( gameObject );
            }
        }
    }
}
