using UnityEngine;
using Zenject;

namespace Zenject.SpaceFighter
{
    public class EnemyCollisionDetector : MonoBehaviour
    {
        EnemySignals.Hit.Trigger _hitTrigger;

        [PostInject]
        public void Construct(EnemySignals.Hit.Trigger hitTrigger)
        {
            _hitTrigger = hitTrigger;
        }

        public void OnTriggerEnter(Collider other)
        {
            var bullet = other.GetComponent<Bullet>();

            if (bullet != null && bullet.Type != BulletTypes.FromEnemy)
            {
                _hitTrigger.Fire(bullet);
            }
        }
    }
}


