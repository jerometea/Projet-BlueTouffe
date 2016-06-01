using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;
using LostPolygon.AndroidBluetoothMultiplayer;
using System;

public class MultiplayerMenu : MonoBehaviour
{        
    //============================================================================================================================================================================
    public bool _isReturn;
    public bool _isHeberge;
    public bool _isJoin;

    private BluetoothMultiplayerMode _desiredMode = BluetoothMultiplayerMode.None;

    public Texture2D cursorTexture;
    public CursorMode cursorMode = CursorMode.Auto;
    public Vector2 hotSpot = Vector2.zero;

    private const string kLocalIp = "127.0.0.1"; // An IP for Network.Connect(), must always be 127.0.0.1
    private const int kPort = 28000; // Local server IP. Must be the same for client and server

    void Start()
    {

    }

    void Update()
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
        Cursor.SetCursor(cursorTexture, hotSpot, cursorMode);
    }

    void OnMouseExit()
    {
        TextMesh rend = GetComponent<TextMesh>();
        rend.color = Color.white;
    }

    public void DestroyGameObjects()
    {
        GameObject[] objects = GameObject.FindGameObjectsWithTag("Destroy");
        if (objects != null)
        {
            foreach (GameObject obj in objects)
            {
                Destroy(obj.gameObject);
            }
        }
    }

    void OnMouseDown()
    {
        //float scaleFactor = BluetoothExamplesTools.UpdateScaleMobile();
        // If initialization was successfull, showing the buttons

        // If there is no current Bluetooth connectivity
        BluetoothMultiplayerMode currentMode = AndroidBluetoothMultiplayer.GetCurrentMode();
        if (currentMode == BluetoothMultiplayerMode.None)
        {
            if (_isHeberge)
            {
                // If Bluetooth is enabled, then we can do something right on
                if (AndroidBluetoothMultiplayer.GetIsBluetoothEnabled())
                {
                    AndroidBluetoothMultiplayer.RequestEnableDiscoverability(120);
                    Network.Disconnect(); // Just to be sure
                    AndroidBluetoothMultiplayer.StartServer(kPort);
                    DestroyGameObjects();
                }
                else
                {
                    // Otherwise we have to enable Bluetooth first and wait for callback
                    _desiredMode = BluetoothMultiplayerMode.Server;
                    AndroidBluetoothMultiplayer.RequestEnableDiscoverability(120);
                }
            }

            if (_isJoin)
            {
                // If Bluetooth is enabled, then we can do something right on
                if (AndroidBluetoothMultiplayer.GetIsBluetoothEnabled())
                {
                    Network.Disconnect(); // Just to be sure
                    AndroidBluetoothMultiplayer.ShowDeviceList(); // Open device picker dialog
                    DestroyGameObjects();
                }
                else
                {
                    // Otherwise we have to enable Bluetooth first and wait for callback
                    _desiredMode = BluetoothMultiplayerMode.Client;
                    AndroidBluetoothMultiplayer.RequestEnableBluetooth();
                }
            }
        }
        else
        {
            // Stop all networking
            if (GUI.Button(new Rect(10, 10, 150, 50), currentMode == BluetoothMultiplayerMode.Client ? "Disconnect" : "Stop server"))
            {
                if (Network.peerType != NetworkPeerType.Disconnected)
                    Network.Disconnect();
            }
        }

        if(_isReturn) SceneManager.LoadScene(0);
    }

    //DrawBackButton(scaleFactor);
}