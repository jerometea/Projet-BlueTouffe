using UnityEngine;
using System.Collections;

public class Hero : MonoBehaviour
{
    public bool EnterTrigger;
    public GameObject Zombie;
    // Use this for initialization
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }

    void OnTriggerEnter2D( Collider2D other )
    {
        if ( other.gameObject.tag == "Enemy" )
        {
            EnterTrigger = true;
            Debug.Log("Entrer Hero");

            Zombie = other.gameObject;
            Zombie.GetComponent<Zombie>().Come(gameObject);
        }
    }

    void OnTriggerExit2D( Collider2D other )
    {
        if ( other.gameObject.tag == "Enemy" )
        {
            Zombie.GetComponent<Zombie>().Stop();
        }
    }
}
