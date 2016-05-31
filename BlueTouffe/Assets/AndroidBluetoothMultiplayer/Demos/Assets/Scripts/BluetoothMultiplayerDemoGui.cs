using UnityEngine;
using System;
using LostPolygon.AndroidBluetoothMultiplayer;
using LostPolygon.AndroidBluetoothMultiplayer.Examples;
using UnityEngine.Networking;
using System.Collections.Generic;
using UnityEngine.UI;
using Zenject;

public class BluetoothMultiplayerDemoGui : BluetoothDemoGuiBase
{
    GameObject _character;

    [PostInject]
    public void Construct([Inject("Character")] GameObject character)
    {

        _character = character;

    }

    public GameObject playerDisplay;
    public GameObject canvasPrefab;
    public NetworkView _view;
    GameObject canvas;
    List<NetworkPlayer> _playerList;
    private const string kLocalIp = "127.0.0.1"; // An IP for Network.Connect(), must always be 127.0.0.1
    private const int kPort = 28000; // Local server IP. Must be the same for client and server

    private bool _initResult;
    private BluetoothMultiplayerMode _desiredMode = BluetoothMultiplayerMode.None;

    private Dictionary<string, GameObject> _PlayerDisplayDico = new Dictionary<string, GameObject>();

    int _numberPlayer;

    private void RefreshPlayerDisplay()
    {
        foreach (string name in _PlayerDisplayDico.Keys)
        {
            _PlayerDisplayDico[name].GetComponent<NetworkView>().RPC("ChangeName", RPCMode.All, name);
        }
    }

    private GameObject CreatPlayerDisplay(string name, int x, int y)
    {
        Debug.Log("Creation of a Player Display : x = " + x + " y = " + _numberPlayer * 100);
        GameObject newItem = Network.Instantiate(playerDisplay, playerDisplay.transform.position, Quaternion.identity, 1) as GameObject;
        newItem.name = "player" + _numberPlayer;
        newItem.GetComponent<Transform>().position = new Vector3(x, _numberPlayer * (-100), 0);
        //newItem.GetComponent<PlayerDisplayScript>()
        for (int i = 0; i < newItem.transform.childCount - 1; i++)
        {
            if (newItem.transform.GetChild(i).transform.name == "Name")
            {
                newItem.transform.GetChild(i).transform.GetComponent<TextMesh>().text = name;
            }
        }
        newItem.GetComponent<NetworkView>().RPC("ChangeName", RPCMode.All, name);
        _PlayerDisplayDico.Add(name, newItem);
        return newItem;
    }

    private void Awake()
    {
        // Setting the UUID. Must be unique for every application
        _initResult = AndroidBluetoothMultiplayer.Initialize("8ce255c0-200a-11e0-ac64-0800200c9a66");
        _numberPlayer = 0;
        _playerList = new List<NetworkPlayer>();

        // Enabling verbose logging. See log cat!
        AndroidBluetoothMultiplayer.SetVerboseLog(true);

        // canvas = Instantiate(canvasPrefab) as GameObject;

        // Registering the event delegates
        AndroidBluetoothMultiplayer.ListeningStarted += OnBluetoothListeningStarted;
        AndroidBluetoothMultiplayer.ListeningStopped += OnBluetoothListeningStopped;
        AndroidBluetoothMultiplayer.AdapterEnabled += OnBluetoothAdapterEnabled;
        AndroidBluetoothMultiplayer.AdapterEnableFailed += OnBluetoothAdapterEnableFailed;
        AndroidBluetoothMultiplayer.AdapterDisabled += OnBluetoothAdapterDisabled;
        AndroidBluetoothMultiplayer.DiscoverabilityEnabled += OnBluetoothDiscoverabilityEnabled;
        AndroidBluetoothMultiplayer.DiscoverabilityEnableFailed += OnBluetoothDiscoverabilityEnableFailed;
        AndroidBluetoothMultiplayer.ConnectedToServer += OnBluetoothConnectedToServer;
        AndroidBluetoothMultiplayer.ConnectionToServerFailed += OnBluetoothConnectionToServerFailed;
        AndroidBluetoothMultiplayer.DisconnectedFromServer += OnBluetoothDisconnectedFromServer;
        AndroidBluetoothMultiplayer.ClientConnected += OnBluetoothClientConnected;
        AndroidBluetoothMultiplayer.ClientDisconnected += OnBluetoothClientDisconnected;
        AndroidBluetoothMultiplayer.DevicePicked += OnBluetoothDevicePicked;
    }

    // Don't forget to unregister the event delegates!
    protected override void OnDestroy()
    {
        base.OnDestroy();

        AndroidBluetoothMultiplayer.ListeningStarted -= OnBluetoothListeningStarted;
        AndroidBluetoothMultiplayer.ListeningStopped -= OnBluetoothListeningStopped;
        AndroidBluetoothMultiplayer.AdapterEnabled -= OnBluetoothAdapterEnabled;
        AndroidBluetoothMultiplayer.AdapterEnableFailed -= OnBluetoothAdapterEnableFailed;
        AndroidBluetoothMultiplayer.AdapterDisabled -= OnBluetoothAdapterDisabled;
        AndroidBluetoothMultiplayer.DiscoverabilityEnabled -= OnBluetoothDiscoverabilityEnabled;
        AndroidBluetoothMultiplayer.DiscoverabilityEnableFailed -= OnBluetoothDiscoverabilityEnableFailed;
        AndroidBluetoothMultiplayer.ConnectedToServer -= OnBluetoothConnectedToServer;
        AndroidBluetoothMultiplayer.ConnectionToServerFailed -= OnBluetoothConnectionToServerFailed;
        AndroidBluetoothMultiplayer.DisconnectedFromServer -= OnBluetoothDisconnectedFromServer;
        AndroidBluetoothMultiplayer.ClientConnected -= OnBluetoothClientConnected;
        AndroidBluetoothMultiplayer.ClientDisconnected -= OnBluetoothClientDisconnected;
        AndroidBluetoothMultiplayer.DevicePicked -= OnBluetoothDevicePicked;
    }

    private void OnGUI()
    {
        float scaleFactor = BluetoothExamplesTools.UpdateScaleMobile();
        // If initialization was successfull, showing the buttons
        if (_initResult)
        {
            // If there is no current Bluetooth connectivity
            BluetoothMultiplayerMode currentMode = AndroidBluetoothMultiplayer.GetCurrentMode();
            if (currentMode == BluetoothMultiplayerMode.None)
            {
                if (GUI.Button(new Rect(10, 10, 150, 50), "Create server"))
                {
                    // If Bluetooth is enabled, then we can do something right on
                    if (AndroidBluetoothMultiplayer.GetIsBluetoothEnabled())
                    {
                        AndroidBluetoothMultiplayer.RequestEnableDiscoverability(120);
                        Network.Disconnect(); // Just to be sure
                        AndroidBluetoothMultiplayer.StartServer(kPort);
                    }
                    else
                    {
                        // Otherwise we have to enable Bluetooth first and wait for callback
                        _desiredMode = BluetoothMultiplayerMode.Server;
                        AndroidBluetoothMultiplayer.RequestEnableDiscoverability(120);
                    }
                }

                if (GUI.Button(new Rect(170, 10, 150, 50), "Connect to server"))
                {
                    // If Bluetooth is enabled, then we can do something right on
                    if (AndroidBluetoothMultiplayer.GetIsBluetoothEnabled())
                    {
                        Network.Disconnect(); // Just to be sure
                        AndroidBluetoothMultiplayer.ShowDeviceList(); // Open device picker dialog
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
        }
        else
        {
            // Show a message if initialization failed for some reason
            GUI.contentColor = Color.black;
            GUI.Label(
                new Rect(10, 10, Screen.width / scaleFactor - 10, 50),
                "Bluetooth not available. Are you running this on Bluetooth-capable " +
                "Android device and AndroidManifest.xml is set up correctly?");




        }

        DrawBackButton(scaleFactor);
    }

    protected override void OnBackToMenu()
    {
        // Gracefully closing all Bluetooth connectivity and loading the menu
        try
        {
            AndroidBluetoothMultiplayer.StopDiscovery();
            AndroidBluetoothMultiplayer.Stop();
        }
        catch
        {
            // 
        }
    }

    #region Bluetooth events

    private void OnBluetoothListeningStarted()
    {
        Debug.Log("Event - ListeningStarted");

        // Starting Unity networking server if Bluetooth listening started successfully
        Network.InitializeServer(4, kPort, false);
    }

    private void OnBluetoothListeningStopped()
    {
        Debug.Log("Event - ListeningStopped");

        // For demo simplicity, stop server if listening was canceled
        AndroidBluetoothMultiplayer.Stop();
    }

    private void OnBluetoothDevicePicked(BluetoothDevice device)
    {
        Debug.Log("Event - DevicePicked: " + BluetoothExamplesTools.FormatDevice(device));

        // Trying to connect to a device user had picked
        AndroidBluetoothMultiplayer.Connect(device.Address, kPort);
    }

    private void OnBluetoothClientDisconnected(BluetoothDevice device)
    {
        Debug.Log("Event - ClientDisconnected: " + BluetoothExamplesTools.FormatDevice(device));
        _numberPlayer--;
    }

    private void OnBluetoothClientConnected(BluetoothDevice device)
    {
        Debug.Log("Event - ClientConnected: " + BluetoothExamplesTools.FormatDevice(device));

    }

    private void OnBluetoothDisconnectedFromServer(BluetoothDevice device)
    {
        Debug.Log("Event - DisconnectedFromServer: " + BluetoothExamplesTools.FormatDevice(device));

        // Stopping Unity networking on Bluetooth failure
        Network.Disconnect();
    }

    private void OnBluetoothConnectionToServerFailed(BluetoothDevice device)
    {
        Debug.Log("Event - ConnectionToServerFailed: " + BluetoothExamplesTools.FormatDevice(device));
    }

    private void OnBluetoothConnectedToServer(BluetoothDevice device)
    {
        Debug.Log("Event - ConnectedToServer: " + BluetoothExamplesTools.FormatDevice(device));

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
        Debug.Log("Event - DiscoverabilityEnableFailed");
    }

    private void OnBluetoothDiscoverabilityEnabled(int discoverabilityDuration)
    {
        Debug.Log(string.Format("Event - DiscoverabilityEnabled: {0} seconds", discoverabilityDuration));
    }

    #endregion Bluetooth events

    #region Network events

    private void OnPlayerConnected(NetworkPlayer player)
    {
        Debug.Log("Player connected: " + player.GetHashCode());
        _numberPlayer++;
        if (CreatPlayerDisplay("Player Client", 0, 100) != null) Debug.Log("Player display created");
        else Debug.Log("Player display creation failed");
        RefreshPlayerDisplay();
    }

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
        AndroidBluetoothMultiplayer.Stop();
    }

    private void OnDisconnectedFromServer()
    {
        Debug.Log("Disconnected from server");

        // Stopping all Bluetooth connectivity on Unity networking disconnect event
        AndroidBluetoothMultiplayer.Stop();

        TestActor[] objects = FindObjectsOfType(typeof(TestActor)) as TestActor[];
        if (objects != null)
        {
            foreach (TestActor obj in objects)
            {
                Destroy(obj.gameObject);
            }
        }
    }

    private void OnConnectedToServer()
    {
        Debug.Log("Connected to server");

    }

    void DestroyGameObjects()
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

    [RPC]
    public void InstantiateCharacter()
    {
        DestroyGameObjects();
        if (_character == null) Debug.Log("character null");
        Network.Instantiate(_character, _character.transform.position, _character.transform.rotation, 0);
        
    }

    private void OnServerInitialized()
    {
        Debug.Log("Server initialized");

        // Instantiating a simple test actor
        if (Network.isServer)
        {
            _numberPlayer = 1;
            CreatPlayerDisplay("Hoster", 0, 100);
            canvas = Instantiate(canvasPrefab) as GameObject;
        }
    }

    #endregion Network events
}
