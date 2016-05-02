using UnityEngine;
using Zenject;

namespace Zenject.SpaceFighter
{
    public class EnemyHealthWatcher : ITickable
    {
        readonly EnemyKilledSignal.Trigger _killedSignal;
        readonly Explosion.Factory _explosionFactory;
        readonly CompositionRoot _compRoot;
        readonly EnemyModel _model;

        public EnemyHealthWatcher(
            EnemyModel model,
            CompositionRoot compRoot,
            Explosion.Factory explosionFactory,
            EnemyKilledSignal.Trigger killedSignal)
        {
            _killedSignal = killedSignal;
            _explosionFactory = explosionFactory;
            _compRoot = compRoot;
            _model = model;
        }

        public void Tick()
        {
            if (_model.Health <= 0)
            {
                Die();
            }
        }

        void Die()
        {
            var explosion = _explosionFactory.Create();
            explosion.transform.position = _model.Position;

            GameObject.Destroy(_compRoot.gameObject);

            _killedSignal.Fire();
        }
    }
}
