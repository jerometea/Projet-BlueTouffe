using UnityEngine;
using System.Collections;
using Zenject;
public class ClickOnGo : MonoBehaviour {



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


        GameObject.Find("MainCamera").GetComponent<NetworkView>().RPC("DestroyGameObjects", RPCMode.All);

        //if ( Network.isServer)
        //{
        //    //     Network.Instantiate(ActorPrefab, Vector3.zero, Quaternion.identity, 0);

        //    if (_character == null) Debug.Log("character null");
        //    Network.Instantiate(_character, _character.transform.position, _character.transform.rotation, 0);
        //    if (_fullFloor == null) Debug.Log("floor null");
        //    Network.Instantiate(_fullFloor, _fullFloor.transform.position, _fullFloor.transform.rotation, 0);

        //    if (_buildings == null) Debug.Log("building null");
        //    Network.Instantiate(_buildings, _buildings.transform.position, _buildings.transform.rotation, 0);

        //    if (_moutains == null) Debug.Log("moutains null");
        //    Network.Instantiate(_moutains, _moutains.transform.position, _moutains.transform.rotation, 0);

        //    if (_moutainsTop == null) Debug.Log("moutains top null");
        //    Network.Instantiate(_moutainsTop, _moutainsTop.transform.position, _moutainsTop.transform.rotation, 0);

        //}

        GameObject.Find("MainCamera").GetComponent<NetworkView>().RPC("InstantiateCharacter", RPCMode.All);

    }
}
