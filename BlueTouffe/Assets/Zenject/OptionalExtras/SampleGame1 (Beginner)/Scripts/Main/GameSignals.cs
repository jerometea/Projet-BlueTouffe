using System;
using Zenject.Commands;

namespace Zenject.Asteroids
{
    public static class Signals
    {
        public class ShipCrashed : Signal
        {
            public class Trigger : TriggerBase
            {
            }
        }
    }
}
