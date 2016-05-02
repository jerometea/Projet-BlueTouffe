using System;
using Zenject.Commands;

namespace Zenject.SpaceFighter
{
    public static class PlayerSignals
    {
        // Fired when a bullet hits the player
        public class Hit : Signal<Bullet>
        {
            public class Trigger : TriggerBase
            {
            }
        }
    }
}

