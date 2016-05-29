using UnityEngine;
using System.Collections;
using Zenject;
using System;

public class BlueTouffeInstaller : MonoInstaller {


    [SerializeField]
    Settings _settings = null;

    public override void InstallBindings()
    {
        InstallBlueTouffe();
    }

    void InstallBlueTouffe()
    {
        Container.Bind<Camera>().ToInstance(_settings.MainCamera);



        Container.Bind<GameObject>("Buildings").ToInstance(_settings.Map.Buildings);
        Container.Bind<GameObject>("FullFloor").ToInstance(_settings.Map.FullFloor);
        Container.Bind<GameObject>("Moutains").ToInstance(_settings.Map.Moutains);
        Container.Bind<GameObject>("MoutainsTop").ToInstance(_settings.Map.MoutainsTop);
        Container.Bind<GameObject>("Character").ToSingleInstance(_settings.Character.Character);

    }

    [Serializable]
    public class Settings
    {
        public Camera MainCamera;
        public MapSettigns Map;
        public CharacterSettings Character;


        [Serializable]
        public class MapSettigns
        {
            public GameObject Buildings;
            public GameObject FullFloor;
            public GameObject Moutains;
            public GameObject MoutainsTop;
        }

        [Serializable]
        public class CharacterSettings
        {
            public GameObject Character;
        }

    }
}


