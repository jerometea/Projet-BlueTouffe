using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;

public class SoloMenu : MonoBehaviour
{

    public bool _isReturn;
    public bool _isLevel;
    public int _level;

    void Start()
    {
        
    }

    void Update ()
    {
        if (Input.GetKeyUp(KeyCode.Escape))
        {

            SceneManager.LoadScene(0);
        }
    }

    void OnMouseEnter()
    {
        TextMesh rend = GetComponent<TextMesh>();
        rend.color = Color.red;
    }

    void OnMouseExit()
    {
        TextMesh rend = GetComponent<TextMesh>();
        rend.color = Color.black;
    }
    void OnMouseDown()
    {
        if (_isReturn)
        {
            SceneManager.LoadScene(0);
        }
        else if (_isLevel)
        {
            SceneManager.LoadScene(_level);
        }
    }
}