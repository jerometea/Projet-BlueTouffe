using System.Collections.Generic;
using UnityEngine;

namespace Zenject
{
    [CreateAssetMenu(fileName = "UntitledCompositionRoot", menuName = "Zenject/Editor Window Composition Root", order = 1)]
    public class EditorWindowCompositionRoot : ScriptableObject
    {
        [SerializeField]
        List<MonoEditorInstaller> _installers = null;

        public void Initialize(
            DiContainer container, EditorWindowFacade root)
        {
            foreach (var installer in _installers)
            {
                installer.Container = container;
                installer.InstallBindings();
            }

            container.Bind<TickableManager>().AsSingle();
            container.Bind<InitializableManager>().AsSingle();
            container.Bind<DisposableManager>().AsSingle();
            container.Bind<GuiRenderableManager>().AsSingle();

            container.Bind<EditorWindowFacade>().FromInstance(root).NonLazy();

            container.Inject(root);
        }
    }
}
