using UnityEngine;
using System.Collections;

public class CharHealth : MonoBehaviour {
    float _health = 100;
    float _resetAfterDeathTime = 5f;
    Animator _anim;
    bool _playerDead;

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

        // Set the animator's dead parameter to true also.
        _anim.SetBool( "deadBool", _playerDead );
    }

    public void TakeDamage( float amount )
    {
        // Decrement the player's health by amount.
        _health -= amount;
    }

    public bool IsDead
    {
        get { return _playerDead; }
    }
}