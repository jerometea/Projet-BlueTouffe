using UnityEngine;
using System.Collections;

public class CharHealth : MonoBehaviour {
    public float _health = 100;
    Animator _anim;
    bool _playerDead;
    public CircleCollider2D collider;

    // Use this for initialization
    void Start() {
        _anim = GetComponent<Animator>();
    }

    // Update is called once per frame
    void Update() {
        if( _health <= 0 )
        {
            PlayerDead();
        }
    }

    void PlayerDead()
    {
        // The player is now dead.
        _playerDead = true;

        collider.enabled = false;

        // Set the animator's dead parameter to true also.
        _anim.SetBool( "isDead", _playerDead );
    }

    public void TakeDamage( float amount )
    {
        // Decrement the player's health by amount.
        _health -= amount;
        gameObject.GetComponent<CharController>().GetHurt();
    }

    public bool IsDead
    {
        get { return _playerDead; }
    }

    public void PlayerRez()
    {
        _playerDead = false;
        _health = 100;
        _anim.SetBool( "isDead", _playerDead );
        collider.enabled = true;
    }
}