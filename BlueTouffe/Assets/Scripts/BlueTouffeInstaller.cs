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
        Container.Bind<GameObject>("Buildings").ToInstance(_settings.Map.Buildings);
        Container.Bind<GameObject>("FullFloor").ToInstance(_settings.Map.FullFloor);
        Container.Bind<GameObject>("Moutains").ToInstance(_settings.Map.Moutains);
        Container.Bind<GameObject>("MoutainsTop").ToInstance(_settings.Map.MoutainsTop);
        Container.Bind<GameObject>("Character").ToInstance(_settings.Character.Character);
        Container.Bind<GameObject>("Controls").ToInstance(_settings.ControlJoystick.Joystick);


        Container.Bind<GameObject>("Zombie").ToInstance(_settings.Zombie.Zombie);

        Container.Bind<ITickable>().ToSingle<ZombieSpawner>();
        Container.Bind<ZombieSpawner>().ToSingle();
        //Container.BindGameObjectFactory<Zombie.Factory>(_settings.Zombie.Zombie, "Zombie");


    }

    [Serializable]
    public class Settings
    {
        public MapSettigns Map;
        public CharacterSettings Character;
        public ControlsSettings ControlJoystick;
        public ZombieSettings Zombie;



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
        [Serializable]
        public class ZombieSettings
        {
            public GameObject Zombie;
        }
        [Serializable]
        public class ControlsSettings
        {
            public GameObject Joystick;
        }
    }
}


