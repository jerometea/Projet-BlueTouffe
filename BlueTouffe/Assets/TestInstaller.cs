using UnityEngine;
using System.Collections;
using Zenject;
using System;

public class TestInstaller : MonoInstaller {

    public GameObject _canvasTouchpad;

    public override void InstallBindings()
    {
        Install();
    }

    void Install()
    {
        Container.Bind<GameObject>("canvas").ToInstance(_canvasTouchpad);
    }

}
