﻿using UnityEngine;
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
        if ( _zombieList.Count < 50 )
        {
            while ( _zombieList.Count < 50 )
            {
                _zombieList.Add((GameObject)Network.Instantiate(_zombiePrefab, RandomPosition(), _zombiePrefab.transform.rotation, 1));
                Debug.Log(_zombieList.Count);
            }
        }
    }

    public void Tick()
    {

    }

    Vector2 RandomPosition()
    {
        return new Vector3(UnityEngine.Random.Range(-1080, 2152), UnityEngine.Random.Range(-1293, 516));
    }
}
