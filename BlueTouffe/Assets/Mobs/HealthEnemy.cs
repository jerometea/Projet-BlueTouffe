using UnityEngine;
using System.Collections;

public class HealthEnemy : MonoBehaviour {

    int _maxHealth = 30;
    int _health = 30;
    bool isDead;
    Animator anim;

	// Use this for initialization
	void Start () {
        anim = GetComponent< Animator > ();
    }
	
	// Update is called once per frame
	void Update () {
	    
	}

    public void ReceiveDamage(int damage)
    {
        if( damage < 0 ) return;

        if(isDead)
        {
            Dead();
        }
        else if(_health <= 0 || _health - damage <= 0 )
        {
            Dead();
            _health = 0;
        }
        else
        {
            _health = _health - damage;
        }

    }

    void Dead()
    {
        isDead = true;
        gameObject.layer = 0;
        if( anim != null ) anim.SetBool( "IsDead", true );
    }

    public bool IsDead
    {
        get { return isDead; }
    }

    public int Health
    {
        get { return _health; }
    }

    public int MaxHealth
    {
        get { return _maxHealth; }
    }
}
