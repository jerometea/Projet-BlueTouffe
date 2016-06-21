using UnityEngine;
using System.Collections;
using System;

public class ProjController : MonoBehaviour
{

    public float speed;
    public int Damage;

    // Use this for initialization
    void Start()
    {
        Damage = 10;
        Destroy(gameObject, 0.75f);
    }

    // Update is called once per frame
    void Update()
    {

    }

    public void FireInTheHole( Vector2 direction )
    {
        Rigidbody2D rigi2D = GetComponent<Rigidbody2D>();
        rigi2D.velocity = direction / (float)(Math.Sqrt(direction.x * direction.x + direction.y * direction.y) / 100);
    }

    void OnTriggerEnter2D( Collider2D other )
    {
        if ( other.GetComponent<HealthEnemy>() != null )
        {
            Debug.Log("We hit " + other.name + " and did " + Damage + " damage.");
            if ( !other.GetComponent<HealthEnemy>().IsDead )
            {
                HealthEnemy HealthEnemy = other.GetComponent<HealthEnemy>();
                HealthEnemy.ReceiveDamage(Damage);
                Destroy(gameObject);
            }
        }
    }
}
