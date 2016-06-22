using UnityEngine;
using System.Collections;

public class CharHealth : MonoBehaviour {
    public float _health = 100;
    Animator _anim;
    bool _playerDead;
    public CircleCollider2D collider;
    int i;
    bool _isAttacked;

    void ChangeColor(Color color)
    {
        gameObject.GetComponent<SpriteRenderer>().color = color;
    }

    void Start() {
        _anim = GetComponent<Animator>();
        i = 0;
    }

    // Update is called once per frame
    void Update() {
        if (_health <= 0 && !_playerDead)
        {
            PlayerDead();
        }
        if (_isAttacked)
        {
            if (i == 1)
            {
                ChangeColor(Color.red);
            }
            else if (i == 7)
            {
                ChangeColor(Color.white);
                _isAttacked = false;
                i = 0;
            }
            i++;
        }
    }

    void PlayerDead()
    {
        // The player is now dead.
        _playerDead = true;

        collider.enabled = false;
        gameObject.layer = 0;
        // Set the animator's dead parameter to true also.
        _anim.SetBool( "isDead", _playerDead );
    }

    public void TakeDamage( float amount )
    {
        // Decrement the player's health by amount.
        _health -= amount;
        _isAttacked = true;
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