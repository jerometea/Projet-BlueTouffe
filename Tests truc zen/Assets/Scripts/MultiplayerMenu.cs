using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;
using LostPolygon.AndroidBluetoothMultiplayer;

public class MultiplayerMenu : MonoBehaviour
{

    // ====================================================================================================================================================================================
    //                                      NOT FROM ME                     NOT FROM ME                 NOT FROM ME                 NOT FROM ME             NOT FROM ME
    // ====================================================================================================================================================================================
        private bool _initResult;
private const string kLocalIp = "127.0.0.1"; // An IP for Network.Connect(), must always be 127.0.0.1
private const int kPort = 28000; // Local server IP. Must be the same for client and server
private BluetoothMultiplayerMode _desiredMode = BluetoothMultiplayerMode.None;

    private void Awake()
        {
        // Setting the UUID. Must be unique for every application
        _initResult = AndroidBluetoothMultiplayer.Initialize("8ce255c0-200a-11e0-ac64-0800200c9a66");

        // Enabling verbose logging. See log cat!
        AndroidBluetoothMultiplayer.SetVerboseLog(true);

        // Registering the event delegates
        AndroidBluetoothMultiplayer.ListeningStarted += OnBluetoothListeningStarted;
        AndroidBluetoothMultiplayer.ListeningStopped += OnBluetoothListeningStopped;
        AndroidBluetoothMultiplayer.AdapterEnabled += OnBluetoothAdapterEnabled;
        AndroidBluetoothMultiplayer.AdapterEnableFailed += OnBluetoothAdapterEnableFailed;
        AndroidBluetoothMultiplayer.AdapterDisabled += OnBluetoothAdapterDisabled;
        AndroidBluetoothMultiplayer.DiscoverabilityEnabled += OnBluetoothDiscoverabilityEnabled;
        AndroidBluetoothMultiplayer.DiscoverabilityEnableFailed += OnBluetoothDiscoverabilityEnableFailed;
        AndroidBluetoothMultiplayer.ConnectedToServer += OnBluetoothConnectedToServer;
        AndroidBluetoothMultiplayer.DisconnectedFromServer += OnBluetoothDisconnectedFromServer;
    }


        
    //============================================================================================================================================================================
    public bool _isReturn;
    public bool _isHeberge;
    public bool _isJoin;

    public Texture2D cursorTexture;
    public CursorMode cursorMode = CursorMode.Auto;
    public Vector2 hotSpot = Vector2.zero;

    void ChangeText(string text)
    {
        TextMesh rend = GetComponent<TextMesh>();
        rend.text = text;
    }

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

    void OnMouseDown()
    {
        TextMesh rend = GetComponent<TextMesh>();

        if (_isReturn)
        {
            SceneManager.LoadScene(0);
        }
        else if (_isHeberge)
        {
            if (_initResult)
            {
                if (AndroidBluetoothMultiplayer.GetIsBluetoothEnabled())
                {
                    AndroidBluetoothMultiplayer.RequestEnableDiscoverability(120);
                    Network.Disconnect(); // Just to be sure
                    //ChangeText();
                    AndroidBluetoothMultiplayer.StartServer(kPort);
                }
                else
                {
                    // Otherwise we have to enable Bluetooth first and wait for callback
                    _desiredMode = BluetoothMultiplayerMode.Server;
                    AndroidBluetoothMultiplayer.RequestEnableDiscoverability(120);
                }
            }
            else
            {
                rend.text = "MARCHE PO D:";
            }
        }
        else if (_isJoin)
        {
            if (AndroidBluetoothMultiplayer.GetIsBluetoothEnabled())
            {
                Network.Disconnect(); // Just to be sure
                AndroidBluetoothMultiplayer.ShowDeviceList(); // Open device picker dialog
            }
            else
            {
                _desiredMode = BluetoothMultiplayerMode.Client;
                AndroidBluetoothMultiplayer.RequestEnableBluetooth();
            }
        }
    }

    //=============================================================

    //=============================================================

    #region Bluetooth events

    private void OnBluetoothListeningStarted()
    {
        Debug.Log("Event - ListeningStarted");
        ChangeText("OnBluetoothListeningStarted");
        // Starting Unity networking server if Bluetooth listening started successfully
        Network.InitializeServer(4, kPort, false);
    }

    private void OnBluetoothListeningStopped()
    {
        Debug.Log("Event - ListeningStopped");

        // For demo simplicity, stop server if listening was canceled
        ChangeText("Blue Listening Stop");
        AndroidBluetoothMultiplayer.Stop();
    }

    private void OnBluetoothDisconnectedFromServer(BluetoothDevice device)
    {
        ChangeText("Blue Disconect");
        // Stopping Unity networking on Bluetooth failure
        Network.Disconnect();
    }


    private void OnBluetoothConnectedToServer(BluetoothDevice device)
    {
        

        // Trying to negotiate a Unity networking connection, 
        // when Bluetooth client connected successfully
        Network.Connect(kLocalIp, kPort);
    }

    private void OnBluetoothAdapterDisabled()
    {
        Debug.Log("Event - AdapterDisabled");
    }

    private void OnBluetoothAdapterEnableFailed()
    {
        Debug.Log("Event - AdapterEnableFailed");
    }

    private void OnBluetoothAdapterEnabled()
    {
        Debug.Log("Event - AdapterEnabled");

        // Resuming desired action after enabling the adapter
        switch (_desiredMode)
        {
            case BluetoothMultiplayerMode.Server:
                Network.Disconnect();
                AndroidBluetoothMultiplayer.StartServer(kPort);
                break;
            case BluetoothMultiplayerMode.Client:
                Network.Disconnect();
                AndroidBluetoothMultiplayer.ShowDeviceList();
                break;
        }

        _desiredMode = BluetoothMultiplayerMode.None;
    }

    private void OnBluetoothDiscoverabilityEnableFailed()
    {
        ChangeText("Discovery Failed");
        Debug.Log("Event - DiscoverabilityEnableFailed");
    }

    private void OnBluetoothDiscoverabilityEnabled(int discoverabilityDuration)
    {
        //ChangeText("Discovery");
        Debug.Log(string.Format("Event - DiscoverabilityEnabled: {0} seconds", discoverabilityDuration));
    }

    #endregion Bluetooth events
    #region Network events

    private void OnPlayerDisconnected(NetworkPlayer player)
    {
        Debug.Log("Player disconnected: " + player.GetHashCode());
        Network.RemoveRPCs(player);
        Network.DestroyPlayerObjects(player);
    }

    private void OnFailedToConnect(NetworkConnectionError error)
    {
        Debug.Log("Can't connect to the networking server");

        // Stopping all Bluetooth connectivity on Unity networking disconnect event
        ChangeText("Failed");
        AndroidBluetoothMultiplayer.Stop();
    }

    private void OnDisconnectedFromServer()
    {
        Debug.Log("Disconnected from server");

        // Stopping all Bluetooth connectivity on Unity networking disconnect event
        AndroidBluetoothMultiplayer.Stop();

    }

    private void OnConnectedToServer()
    {
        Debug.Log("Connected to server");
        ChangeText("Connected");
        SceneManager.LoadScene(0);
    }

    private void OnServerInitialized()
    {
        Debug.Log("Server initialized");

        // Instantiating a simple test actor
        if (Network.isServer)
        {
            ChangeText("Init");
        }
    }

    #endregion Network events
}