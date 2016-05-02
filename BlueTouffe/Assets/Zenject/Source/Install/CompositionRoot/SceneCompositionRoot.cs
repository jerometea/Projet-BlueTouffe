#if !NOT_UNITY3D

using System;
using System.Collections.Generic;
using System.Linq;
using ModestTree;
using ModestTree.Util;
using UnityEngine;
using UnityEngine.Serialization;
using Zenject.Internal;
using UnityEngine.SceneManagement;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Zenject
{
    public class SceneCompositionRoot : CompositionRoot
    {
        public static readonly List<Scene> DecoratedScenes = new List<Scene>();

        public static Action<DiContainer> BeforeInstallHooks;
        public static Action<DiContainer> AfterInstallHooks;

        public static DiContainer ParentContainer;

        [FormerlySerializedAs("ParentNewObjectsUnderRoot")]
        [Tooltip("When true, objects that are created at runtime will be parented to the SceneCompositionRoot")]
        [SerializeField]
        bool _parentNewObjectsUnderRoot = false;

        DiContainer _container;
        readonly List<object> _dependencyRoots = new List<object>();

        bool _hasInitialized;

#if UNITY_EDITOR
        bool _validateShutDownAfterwards = true;
#endif

        static bool _autoRun = true;

        public override DiContainer Container
        {
            get
            {
                return _container;
            }
        }

#if UNITY_EDITOR
        public bool IsValidating
        {
            get;
            set;
        }

        public bool ValidateShutDownAfterwards
        {
            get
            {
                return _validateShutDownAfterwards;
            }
            set
            {
                _validateShutDownAfterwards = value;
            }
        }
#else
        public bool IsValidating
        {
            get
            {
                return false;
            }
        }
#endif

        public bool ParentNewObjectsUnderRoot
        {
            get
            {
                return _parentNewObjectsUnderRoot;
            }
            set
            {
                _parentNewObjectsUnderRoot = value;
            }
        }

        public void Awake()
        {
            // We always want to initialize ProjectCompositionRoot as early as possible
            ProjectCompositionRoot.Instance.EnsureIsInitialized();

#if UNITY_EDITOR
            IsValidating = ProjectCompositionRoot.Instance.Container.IsValidating;
#endif

            if (_autoRun)
            {
                Run();
            }
            else
            {
                // True should always be default
                _autoRun = true;
            }
        }

#if UNITY_EDITOR
        public void Run()
        {
            if (IsValidating)
            {
                try
                {
                    RunInternal();

                    Assert.That(_container.IsValidating);

                    _container.ValidateIValidatables();

                    Log.Info("Scene '{0}' Validated Successfully", this.gameObject.scene.name);
                }
                catch (Exception e)
                {
                    Log.ErrorException("Scene '{0}' Failed Validation!".Fmt(this.gameObject.scene.name), e);
                }
            }
            else
            {
                RunInternal();
            }
        }
#else
        public void Run()
        {
            RunInternal();
        }
#endif

        public void RunInternal()
        {
            Assert.That(!_hasInitialized);
            _hasInitialized = true;

            Assert.IsNull(_container);

            var parentContainer = ParentContainer ?? ProjectCompositionRoot.Instance.Container;

            // ParentContainer is optionally set temporarily before calling ZenUtil.LoadScene
            ParentContainer = null;

            _container = parentContainer.CreateSubContainer(IsValidating);

#if !UNITY_EDITOR
            Assert.That(!IsValidating);
#endif

            // This can happen if you run a decorated scene with immediately running a normal scene afterwards
            foreach (var decoratedScene in DecoratedScenes)
            {
                Assert.That(decoratedScene.isLoaded,
                    "Unexpected state in SceneCompositionRoot - found unloaded decorated scene");
            }

            Log.Debug("SceneCompositionRoot: Running installers...");

            _container.IsInstalling = true;

            try
            {
                InstallBindings();
            }
            finally
            {
                _container.IsInstalling = false;
            }

            Log.Debug("SceneCompositionRoot: Injecting components in the scene...");

            InjectComponents();

            Log.Debug("SceneCompositionRoot: Resolving dependency roots...");

            Assert.That(_dependencyRoots.IsEmpty());
            _dependencyRoots.AddRange(_container.ResolveDependencyRoots());

            DecoratedScenes.Clear();

            Log.Debug("SceneCompositionRoot: Initialized successfully");
        }

        void InstallBindings()
        {
            if (_parentNewObjectsUnderRoot)
            {
                _container.DefaultParent = this.transform;
            }
            else
            {
                // This is necessary otherwise we inherit the project root DefaultParent
                _container.DefaultParent = null;
            }

            _container.Bind<CompositionRoot>().FromInstance(this);
            _container.Bind<SceneCompositionRoot>().FromInstance(this);

            InstallSceneBindings();

            if (BeforeInstallHooks != null)
            {
                BeforeInstallHooks(_container);
                // Reset extra bindings for next time we change scenes
                BeforeInstallHooks = null;
            }

            _container.Bind<SceneFacade>().FromComponent(this.gameObject).AsSingle().NonLazy();

            _container.Bind<ZenjectSceneLoader>().AsSingle();

            InstallInstallers();

            if (AfterInstallHooks != null)
            {
                AfterInstallHooks(_container);
                // Reset extra bindings for next time we change scenes
                AfterInstallHooks = null;
            }
        }

        protected override IEnumerable<Component> GetInjectableComponents()
        {
            foreach (var gameObject in GetRootGameObjects())
            {
                foreach (var component in GetInjectableComponents(gameObject))
                {
                    yield return component;
                }
            }

            yield break;
        }

        void InjectComponents()
        {
            // Use ToList in case they do something weird in post inject
            foreach (var component in GetInjectableComponents().ToList())
            {
                Assert.That(!component.GetType().DerivesFrom<MonoInstaller>());

                _container.Inject(component);
            }
        }

        public IEnumerable<GameObject> GetRootGameObjects()
        {
            var scene = this.gameObject.scene;

            // Note: We can't use activeScene.GetRootObjects() here because that apparently fails with an exception
            // about the scene not being loaded yet when executed in Awake
            // We also can't use GameObject.FindObjectsOfType<Transform>() because that does not include inactive game objects
            // So we use Resources.FindObjectsOfTypeAll, even though that may include prefabs.  However, our assumption here
            // is that prefabs do not have their "scene" property set correctly so this should work
            //
            // It's important here that we only inject into root objects that are part of our scene, to properly support
            // multi-scene editing features of Unity 5.x
            //
            // Also, even with older Unity versions, if there is an object that is marked with DontDestroyOnLoad, then it will
            // be injected multiple times when another scene is loaded
            //
            // We also make sure not to inject into the project root objects which are injected by ProjectCompositionRoot.
            return Resources.FindObjectsOfTypeAll<GameObject>()
                .Where(x => x.transform.parent == null
                    && x.GetComponent<ProjectCompositionRoot>() == null
                    && (x.scene == scene || DecoratedScenes.Contains(x.scene)));
        }

        // These methods can be used for cases where you need to create the SceneCompositionRoot entirely in code
        // Note that if you use these methods that you have to call Run() yourself
        // This is useful because it allows you to create a SceneCompositionRoot and configure it how you want
        // and add what installers you want before kicking off the Install/Resolve
        public static SceneCompositionRoot Create()
        {
            return CreateComponent(
                new GameObject("SceneCompositionRoot"));
        }

        public static SceneCompositionRoot CreateComponent(GameObject gameObject)
        {
            _autoRun = false;
            var result = gameObject.AddComponent<SceneCompositionRoot>();
            Assert.That(_autoRun); // Should be reset
            return result;
        }
    }
}

#endif

