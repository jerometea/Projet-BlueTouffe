using UnityEngine;
using System.Collections;
using System;

public class Zombie : MonoBehaviour
{
    public bool attack;
    public GameObject Hero;
    public GameObject HeroToAttack;
    public Collider2D HeroCollider;
    public CircleCollider2D TriggerAttack;
    public int speed = 10;
    public Vector3 direction;
    public Vector3 dir;

    Animator anime;

    public float x;
    public float y;

    public int i;
    // Use this for initialization


    void Start()
    {
        anime = GetComponent<Animator>();
        i = 0;
    }

    // Update is called once per frame
    void Update()
    {
        if (Hero != null && Hero.layer == 0) Hero = null;

        if (attack && TriggerAttack.IsTouching(HeroCollider) && HeroToAttack.layer == 9 && !gameObject.GetComponent<HealthEnemy>().IsDead)
        {
            
            Debug.Log("Zombie Attack");
            i++;
            anime.SetBool("IsAttacking", true);
            if (i > 29 && i < 31 && !gameObject.GetComponent<HealthEnemy>().IsDead)
            {
                HeroToAttack.GetComponent<CharHealth>().TakeDamage(10);
                
            }
            if (i > 35 && i < 36)
            {
                HeroToAttack.GetComponent<SpriteRenderer>().color = Color.white;
            }
            if (i >= 60 && !gameObject.GetComponent<HealthEnemy>().IsDead)
            {
                anime.SetBool("IsAttacking", false);
                i = 0;
            }
        }
        else
        {
            anime.SetBool("IsAttacking", false);
        }

        if (Hero != null && gameObject.layer != 0 && !attack)
        {
            Vector3 ht = Hero.transform.position;
            Vector3 t = transform.position;

            direction = (ht - t) / ((int)(Math.Sqrt((ht.y - t.y) * (ht.y - t.y) + (ht.x - t.x) * (ht.x - t.x)) * speed));
            dir = (ht - t) / ((int)(Math.Sqrt((ht.y - t.y) * (ht.y - t.y) + (ht.x - t.x) * (ht.x - t.x))));

            if (x > 0)
            {
                transform.Translate(-direction.x, direction.y, 0);
                transform.eulerAngles = new Vector2(0, 180);

            }
            else if (x < 0)
            {
                transform.Translate(direction.x, direction.y, 0);
                transform.eulerAngles = new Vector2(0, 0);
            }

            x = dir.x;
            y = dir.y;
            
            if (x >= 0.1) x = 1;
            if (y >= 0.1) y = 1;

            anime.SetFloat("SpeedX", dir.x);
            anime.SetFloat("SpeedY", dir.y);
        }
        else
        {
            anime.SetFloat("SpeedX", 0);
            anime.SetFloat("SpeedY", 0);
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        GameObject target = other.gameObject;
        if (!attack && target.layer == 9 && !target.GetComponent<Hero>().IsTriggerDetection(other))
        {
            HeroToAttack = other.gameObject;
            HeroCollider = other;
            attack = true;
            
        }
    }

    void OnTriggerExit2D(Collider2D other)
    {
        GameObject target = other.gameObject;
        if (attack && target.layer == 9 && target == HeroToAttack )
        {
            if (target.GetComponent<Hero>() != null && !target.GetComponent<Hero>().IsTriggerDetection(other))
            {
                HeroToAttack = null;
                attack = false;
                Debug.Log("Zombie !Attack");
                
            }
        }
    }

    public void Come(GameObject Char)
    {
        Hero = Char;
    }

    public void Stop()
    {
        Hero = null;
    }
}
