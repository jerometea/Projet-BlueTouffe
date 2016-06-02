using UnityEngine;
using System.Collections;

public class Weapon : MonoBehaviour {

    int fireRate;
    int Damage;
    public LayerMask ToHit;
    int timeToFire;
    int weap = 2; // 1 - snip, 2 - shotgun, 3 - close combat 
    Transform shooter;

	// Use this for initialization1
	void Start () {
        fireRate = 30;
        Damage = 10;
        timeToFire = 0;

        shooter = transform.FindChild( "Shooter" );
        if(shooter == null )
        {
            Debug.LogError( "No shooter detected" );
        }
	}
	
	// Update is called once per frame
	void Update () {
        float x = Input.GetAxisRaw("Horizontal2");
        float y = Input.GetAxisRaw("Vertical2");

        timeToFire++;
        if( x != 0 || y != 0 )
        {
            if(fireRate == 0)
            {
                AdaptInput( x, y );
            } else if( timeToFire % fireRate == 0 || timeToFire == 0 )
            {
                AdaptInput( x, y );
            }
        }
    }

    void AdaptInput( float x, float y )
    {
        float modifiedX = x;
        float modifiedY = y;

        if( x != 0 && y != 0 )
        {
            modifiedX = modifiedX / 2;
            modifiedY = modifiedY / 2;
        }

        if( weap == 2 )
        {
            modifiedX = modifiedX / 1.5f;
            modifiedY = modifiedY / 1.5f;
        }
        else if( weap == 3 )
        {
            modifiedX = modifiedX / 5;
            modifiedY = modifiedY / 5;
        }

       Shoot( modifiedX, modifiedY );
   }


    void Shoot(float x, float y)
    {
        Vector2 shooterPosition = new Vector2(shooter.position.x, shooter.position.y);
        Vector2 direction = new Vector2( shooter.position.x + x, shooter.position.y + y);
        RaycastHit2D hit = Physics2D.Linecast(shooterPosition, direction, ToHit);
        Debug.DrawLine(shooterPosition, direction);

        if( weap == 2 )
        {
            //Vector2 direction2 = new Vector2( shooter.position.x + x, shooter.position.y + y );
            //RaycastHit2D hit2 = Physics2D.Linecast( shooterPosition, direction2, ToHit );
            //Debug.DrawLine( shooterPosition, direction2 );
            
            //Vector2 direction3 = new Vector2(shooter.position.x + x, shooter.position.y + y);
            //RaycastHit2D hit3 = Physics2D.Linecast(shooterPosition, direction3, ToHit);
            //Debug.DrawLine( shooterPosition, direction3 );
        }
        

        if(hit.collider != null )
        {
            Debug.Log( "We hit "+ hit.collider.name + " and did "+ Damage +" damage.");
            Debug.DrawLine( shooterPosition, hit.point, Color.red);

            if( hit.collider.GetComponent<HealthEnemy>() != null )
            {
                HealthEnemy HealthEnemy = hit.collider.GetComponent<HealthEnemy>();
                HealthEnemy.ReceiveDamage( Damage );
             }
            
        }
    }
    public int GetWeapon
    {
        get { return weap; }
    }
}