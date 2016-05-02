using System;
using UnityEngine;
using System.Collections;
using Zenject;
using Zenject.Commands;

namespace Zenject.SpaceFighter
{
    // Main installer for our game
    public class GameInstaller : MonoInstaller
    {
        [SerializeField]
        Settings _settings = null;

        public override void InstallBindings()
        {
            Container.BindAllInterfaces<CameraHandler>().To<CameraHandler>().AsSingle();

            Container.BindSignal<PlayerKilledSignal>();
            Container.BindTrigger<PlayerKilledSignal.Trigger>();

            Container.BindSignal<EnemyKilledSignal>();
            Container.BindTrigger<EnemyKilledSignal.Trigger>();

            Container.BindAllInterfaces<EnemySpawner>().To<EnemySpawner>().AsSingle();

            Container.BindFactory<EnemyTunables, EnemyFacade, EnemyFacade.Factory>()
                .FromSubContainerResolve()
                .ByPrefab<EnemyInstaller>(_settings.EnemyFacadePrefab)
                .UnderGameObjectGroup("Enemies");

            Container.BindAllInterfaces<GameDifficultyHandler>().To<GameDifficultyHandler>().AsSingle();

            Container.Bind<EnemyRegistry>().AsSingle();

            Container.BindFactory<float, float, BulletTypes, Bullet, Bullet.Factory>()
                .FromPrefab(_settings.BulletPrefab)
                .UnderGameObjectGroup("Bullets");

            Container.BindFactory<Explosion, Explosion.Factory>()
                .FromPrefab(_settings.ExplosionPrefab)
                .UnderGameObjectGroup("Explosions");

            Container.Bind<AudioPlayer>().AsSingle();

            Container.BindAllInterfaces<GameRestartHandler>().To<GameRestartHandler>().AsSingle();
        }

        [Serializable]
        public class Settings
        {
            public GameObject EnemyFacadePrefab;
            public GameObject BulletPrefab;
            public GameObject ExplosionPrefab;
        }
    }
}
