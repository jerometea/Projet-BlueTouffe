using System;
using System.Collections.Generic;
using ModestTree;


namespace Zenject
{
    [System.Diagnostics.DebuggerStepThrough]
    public class Facade : IInitializable, IDisposable, ITickable, ILateTickable, IFixedTickable
    {
        [Inject(InjectSources.Local)]
        TickableManager _tickableManager = null;

        [Inject(InjectSources.Local)]
        InitializableManager _initializableManager = null;

        [Inject(InjectSources.Local)]
        DisposableManager _disposablesManager = null;

        // NOTE!  This method must be called explicitly when creating game object roots through factories
        public virtual void Initialize()
        {
            Log.Debug("DependencyRoot: Initializing IInitializable's");

            _initializableManager.Initialize();
        }

        public virtual void Dispose()
        {
            Log.Debug("DependencyRoot: Disposing IDisposable's");

            _disposablesManager.Dispose();
        }

        public virtual void Tick()
        {
            _tickableManager.Update();
        }

        public virtual void LateTick()
        {
            _tickableManager.LateUpdate();
        }

        public virtual void FixedTick()
        {
            _tickableManager.FixedUpdate();
        }
    }
}
