using UnityEngine;
using System.Collections;

public class HealthEnemy : MonoBehaviour {

    int MaxHealth;
    int Health;
    bool isDead;
    Animator anim;

	// Use this for initialization
	void Start () {
        MaxHealth = 40;
        Health = MaxHealth;
        anim = GetComponent< Animator > ();
    }
	
	// Update is called once per frame
	void Update () {
	    
	}

    public void ReceiveDamage(int damage)
    {
        if(isDead)
        {
            Dead();
        }
        else if(Health <= 0 || Health - damage <= 0 )
        {
            Dead();
        }
        else
        {
            Health = Health - damage;
        }

    }

    void Dead()
    {
        isDead = true;
        anim.SetBool( "IsDead", true );
        gameObject.layer = 0;
    }
}
