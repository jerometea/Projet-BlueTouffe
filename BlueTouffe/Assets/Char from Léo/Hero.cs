using UnityEngine;
using System.Collections;

public class Hero : MonoBehaviour
{
    public bool EnterTrigger;
    public GameObject Zombie;
    public Collider2D TriggerDetection;
    public Collider2D TriggerRez;
    // Use this for initialization
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }

    public bool IsTriggerDetection (Collider2D collider)
    {
        return TriggerDetection == collider;
    }


    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.gameObject.layer == 8)
        {
            EnterTrigger = true;
            Debug.Log("Entrer Hero");

            Zombie = other.gameObject;
            Zombie.GetComponent<Zombie>().Come(gameObject);
        }

        if (other.gameObject.layer == 10 && other.gameObject.GetComponent<Hero>().TriggerRez.IsTouching(TriggerRez))
        {
            GameObject.Find("Buttons").SetActive(true);
        }
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if ((other.gameObject.layer == 8 || other.gameObject.layer == 0) && !TriggerDetection.IsTouching(other))
        {
            if (other.gameObject.GetComponent<Zombie>() != null) other.gameObject.GetComponent<Zombie>().Stop();
        }

        if (other.gameObject.layer == 10 && other.gameObject.GetComponent<Hero>().TriggerRez.IsTouching(TriggerRez))
        {
            GameObject.Find("Buttons").SetActive(false);
        }
    }
}
