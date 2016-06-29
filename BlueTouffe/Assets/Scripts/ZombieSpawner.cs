using UnityEngine;
using System.Collections;
using Zenject;
using System;
using System.Collections.Generic;

public class ZombieSpawner : ITickable {

    [Inject("Zombie")]
    GameObject _zombiePrefab;

    List<GameObject> _zombieList;

    public ZombieSpawner ()
    {
        _zombieList = new List<GameObject>();
    }

    public void Start()
    {
            while ( _zombieList.Count < 200 )
            {
                _zombieList.Add((GameObject)Network.Instantiate(_zombiePrefab, RandomPosition(), _zombiePrefab.transform.rotation, 1));
            }
        
    }

    public void Tick()
    {

    }

    Vector2 RandomPosition()
    {
        return new Vector3(UnityEngine.Random.Range(-1635, 2174), UnityEngine.Random.Range(675, -860));
    }
}
