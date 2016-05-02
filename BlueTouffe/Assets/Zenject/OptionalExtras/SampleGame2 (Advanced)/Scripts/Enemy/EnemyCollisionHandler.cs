using System;
using UnityEngine;
using Zenject;

namespace Zenject.SpaceFighter
{
    public class EnemyCollisionHandler : IInitializable, IDisposable
    {
        readonly EnemySignals.Hit _hitSignal;
        readonly AudioPlayer _audioPlayer;
        readonly Settings _settings;
        readonly EnemyModel _model;

        public EnemyCollisionHandler(
            EnemyModel model,
            Settings settings,
            AudioPlayer audioPlayer,
            EnemySignals.Hit hitSignal)
        {
            _hitSignal = hitSignal;
            _audioPlayer = audioPlayer;
            _settings = settings;
            _model = model;
        }

        public void Initialize()
        {
            _hitSignal.Event += OnHit;
        }

        public void Dispose()
        {
            _hitSignal.Event -= OnHit;
        }

        void OnHit(Bullet bullet)
        {
            _audioPlayer.Play(_settings.HitSound);
            _model.AddForce(-bullet.MoveDirection * _settings.HitForce);
            _model.TakeDamage(_settings.HealthLoss);

            GameObject.Destroy(bullet.gameObject);
        }

        [Serializable]
        public class Settings
        {
            public float HealthLoss;
            public float HitForce;
            public AudioClip HitSound;
        }
    }
}
