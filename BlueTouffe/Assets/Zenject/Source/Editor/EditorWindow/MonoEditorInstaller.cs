using System;
using UnityEngine;

namespace Zenject
{
    public abstract class MonoEditorInstaller : ScriptableObject
    {
        public DiContainer Container
        {
            get;
            set;
        }

        public abstract void InstallBindings();
    }
}
