using UnityEngine;
using System.Collections;

public class HealthEnemy : MonoBehaviour {

    public int MaxHealth;
    int Health;
    bool isDead;
    Animator anim;
    public NetworkView _networView;


    // Use this for initialization
    void Start () {
        Health = MaxHealth;
        anim = GetComponent< Animator > ();
    }
	
	// Update is called once per frame
	void Update () {
        if(!isDead)
            _networView.RPC("SyncroHealthEnnemy", RPCMode.All, Health, isDead);


    }

    public void ReceiveDamage(int damage)
    {
        if(isDead)
        {
            _networView.RPC("Dead", RPCMode.All);

        }
        else if(Health <= 0 || Health - damage <= 0 )
        {
            _networView.RPC("Dead", RPCMode.All);

        }
        else
        {
            Health = Health - damage;
        }
        Debug.Log("health : " + Health + " is dead : " + isDead);

    }

    [RPC]
    void Dead()
    {
        isDead = true;
        anim.SetBool( "IsDead", true );
        gameObject.layer = 0;
        Destroy(gameObject, 2);
        gameObject.GetComponent<BoxCollider2D>().isTrigger = true;
    }

    public bool IsDead
    {
        get { return isDead; }
    }

    //void OnSerializeNetworkView(BitStream stream, NetworkMessageInfo info)
    //{

    //    if (stream.isWriting)
    //    {
    //        var health = Health;
    //        var dead = isDead;
    //        stream.Serialize(ref health);
    //        stream.Serialize(ref dead);
    //    }
    //    else
    //    {
    //        var health = Health;
    //        var dead = isDead;
    //        stream.Serialize(ref health);
    //        stream.Serialize(ref dead);
    //        Health = health;
    //        isDead = dead;
    //    }
    //}

    [RPC]
    void SyncroHealthEnnemy( int healthEnnemy, bool IsEnnemyDead )
    {
        Health = healthEnnemy;
        isDead = IsEnnemyDead;
    }

}
