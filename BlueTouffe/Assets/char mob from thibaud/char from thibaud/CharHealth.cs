using UnityEngine;
using System.Collections;

public class CharHealth : MonoBehaviour {
    public float _health = 100;
    Animator _anim;
    bool _playerDead;
    public CircleCollider2D collider;
    int i;
    bool _isAttacked;
    public NetworkView nv;

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

        if (nv.isMine)
        nv.RPC("SyncroHealth", RPCMode.All, _health, _playerDead);
    }

    [RPC]
    void SyncroHealth(float health, bool isDead)
    {
        _health = health;
        _playerDead = isDead;
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
        gameObject.layer = 9;
        _playerDead = false;
        _health = 100;
        _anim.SetBool( "isDead", _playerDead );
        collider.enabled = true;
        Debug.Log("Player dead : " + _playerDead);
        Debug.Log("Healt : " + _health);
        Debug.Log("Name : " + name);
        Debug.Log("Rez methode end");
    }

    void OnSerializeNetworkView(BitStream stream, NetworkMessageInfo info)
    {
        if (stream.isWriting)
        {
            var health = _health;
            var dead = _playerDead;
            stream.Serialize(ref health);
            stream.Serialize(ref dead);
        }
        else
        {
            var health = _health;
            var dead = _playerDead;
            stream.Serialize(ref health);
            stream.Serialize(ref dead);
            _health = health;
            _playerDead = dead;
        }
    }
}