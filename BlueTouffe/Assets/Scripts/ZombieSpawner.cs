using UnityEngine;
using System.Collections;
using Zenject;
using System;
using System.Collections.Generic;

public class ZombieSpawner : ITickable {


    Zombie.Factory _zombieFacto;
    List<GameObject> _zombieList;

    public ZombieSpawner ( Zombie.Factory zombieFacto )
    {
        _zombieFacto = zombieFacto;
        _zombieList = new List<GameObject>();
    }

    public void Tick()
    {
        if( _zombieList.Count < 50 && Network.isServer )
        {
            GameObject z = _zombieFacto.Create();
            z.transform.position = RandomPosition();
            _zombieList.Add(z);
        }
    }

    Vector2 RandomPosition()
    {
        return new Vector2(UnityEngine.Random.Range(-3200, -62), UnityEngine.Random.Range(-540, 1200));
    }
}
