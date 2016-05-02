using System;
using Zenject.Commands;

namespace Zenject.SpaceFighter
{
    public static class EnemySignals
    {
        // Triggered when enemy is hit by a bullet
        public class Hit : Signal<Bullet>
        {
            public class Trigger : TriggerBase
            {
            }
        }
    }
}
