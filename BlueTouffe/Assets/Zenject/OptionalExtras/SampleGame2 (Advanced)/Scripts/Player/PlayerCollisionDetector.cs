using UnityEngine;
using Zenject;

namespace Zenject.SpaceFighter
{
    public class PlayerCollisionDetector : MonoBehaviour
    {
        PlayerSignals.Hit.Trigger _hitTrigger;

        [PostInject]
        public void Construct(PlayerSignals.Hit.Trigger hitTrigger)
        {
            _hitTrigger = hitTrigger;
        }

        public void OnTriggerEnter(Collider other)
        {
            var bullet = other.GetComponent<Bullet>();

            if (bullet != null && bullet.Type != BulletTypes.FromPlayer)
            {
                _hitTrigger.Fire(bullet);
            }
        }
    }
}

