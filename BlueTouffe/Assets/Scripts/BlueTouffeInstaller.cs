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
        Container.Bind<GameObject>("EndTrigger").ToInstance(_settings.Map.EndTrigger);
        Container.Bind<GameObject>("Character").ToInstance(_settings.Character.Character);
        Container.Bind<GameObject>("Controls").ToInstance(_settings.ControlJoystick.Joystick);
        Container.Bind<GameObject>("Buttons").ToInstance(_settings.Buttons.HelpingButton);


        Container.Bind<GameObject>("Zombie").ToInstance(_settings.Zombie.Zombie);

        Container.Bind<ITickable>().ToSingle<ZombieSpawner>();
        Container.Bind<ZombieSpawner>().ToSingle();

    }

    [Serializable]
    public class Settings
    {
        public MapSettigns Map;
        public CharacterSettings Character;
        public ControlsSettings ControlJoystick;
        public ZombieSettings Zombie;
        public ButtonsSettings Buttons;



        [Serializable]
        public class MapSettigns
        {
            public GameObject Buildings;
            public GameObject FullFloor;
            public GameObject Moutains;
            public GameObject EndTrigger;
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
        [Serializable]
        public class ButtonsSettings
        {
            public GameObject HelpingButton;
        }

    }
}


