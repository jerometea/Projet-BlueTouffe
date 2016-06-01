using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour {

    public bool _isQuit;
    public bool _isSolo;
    public bool _isMultiplayer;

    public Texture2D cursorTexture;
    public CursorMode cursorMode = CursorMode.Auto;
    public Vector2 hotSpot = Vector2.zero;

    void Start()
    {
       
    }

    void Update()
    {
        if (Input.GetKeyUp(KeyCode.Escape))
        {

            Application.Quit();
        }
    }

    void OnMouseEnter()
    {
        TextMesh rend = GetComponent<TextMesh>();
        rend.color = Color.red;
        Cursor.SetCursor(cursorTexture, hotSpot, cursorMode);
    }

    void OnMouseExit()
    {
        TextMesh rend = GetComponent<TextMesh>();
        rend.color = Color.white;
    }

    void OnMouseDown()
    {
        if (_isQuit)
        {
            Debug.Log("Quit");
            Application.Quit();
        }
        else if (_isSolo)
        {
            SceneManager.LoadScene(2);
        }
        else if (_isMultiplayer)
        {
            SceneManager.LoadScene(1);
        }
    }
}
