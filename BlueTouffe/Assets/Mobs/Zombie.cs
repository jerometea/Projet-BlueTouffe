using UnityEngine;
using System.Collections;
using System;

public class Zombie : MonoBehaviour
{
    public bool EnterTrigger;
    public GameObject Hero;
    public float speed = 10;
    NetworkView _nw;

    // Use this for initialization
    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {
        if (Hero != null)
        {
            Vector3 ht = Hero.transform.position;
            Vector3 t = transform.position;

            transform.Translate((ht - t) / ((int)(Math.Sqrt((ht.y - t.y) * (ht.y - t.y) + (ht.x - t.x) * (ht.x - t.x)) * speed)));
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (!EnterTrigger && other.gameObject.tag == "Enemy")
        {
            EnterTrigger = true;
            Debug.Log("Entrer Zombie");
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
