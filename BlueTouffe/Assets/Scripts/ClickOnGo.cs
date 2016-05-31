using UnityEngine;
using System.Collections;
using Zenject;
public class ClickOnGo : MonoBehaviour {

    public GameObject _character;

    public GameObject _buildings;
    public GameObject _fullFloor;
    public GameObject _moutains;
    public GameObject _moutainsTop;

    /*[PostInject]
    public void Construct(
        [Inject("Character")] GameObject character,
        [Inject("Buildings")] GameObject buildings, [Inject("FullFloor")] GameObject fullFloor, [Inject("Moutains")] GameObject moutains, [Inject("MoutainsTop")] GameObject moutainsTop)
    {

        _character = Instantiate(character);
        _buildings = buildings;
        _fullFloor = fullFloor;
        _moutains = moutains;
        _moutainsTop = moutainsTop;
    }*/

    // Use this for initialization
    void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
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

    public void Go()
    {
        Debug.Log("LANCER");
        //DestroyGameObjects();

        if (Network.isServer)
        {
            //     Network.Instantiate(ActorPrefab, Vector3.zero, Quaternion.identity, 0);

            if (_character == null) Debug.Log("character null");
            Network.Instantiate(_character, _character.transform.position, _character.transform.rotation, 0);
            if (_fullFloor == null) Debug.Log("floor null");
            Network.Instantiate(_fullFloor, _fullFloor.transform.position, _fullFloor.transform.rotation, 0);

            if (_buildings == null) Debug.Log("building null");
            Network.Instantiate(_buildings, _buildings.transform.position, _buildings.transform.rotation, 0);

            if (_moutains == null) Debug.Log("moutains null");
            Network.Instantiate(_moutains, _moutains.transform.position, _moutains.transform.rotation, 0);

            if (_moutainsTop == null) Debug.Log("moutains top null");
            Network.Instantiate(_moutainsTop, _moutainsTop.transform.position, _moutainsTop.transform.rotation, 0);


        }

        if (_character == null) Debug.Log("character null");
        Network.Instantiate(_character, _character.transform.position, _character.transform.rotation, 0);

        GameObject.Find("MainCamera").GetComponent<NetworkView>().RPC("InstantiateCharacter", RPCMode.Others);
    }
}
