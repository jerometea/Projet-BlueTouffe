using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using ModestTree;
using ModestTree.Util;
using Zenject.Internal;

#if !NOT_UNITY3D
using UnityEngine;
#endif

namespace Zenject
{
    public delegate bool BindingCondition(InjectContext c);

    // Responsibilities:
    // - Expose methods to configure object graph via Bind() methods
    // - Build object graphs via Resolve() method
    public class DiContainer : IInstantiator, IResolver, IBinder
    {
        public const string DependencyRootIdentifier = "DependencyRoot";

        readonly Dictionary<BindingId, List<ProviderInfo>> _providers = new Dictionary<BindingId, List<ProviderInfo>>();
        readonly HashSet<Type> _installedInstallers = new HashSet<Type>();
        readonly Stack<Type> _installsInProgress = new Stack<Type>();
        readonly DiContainer _parentContainer;
        readonly Stack<LookupId> _resolvesInProgress = new Stack<LookupId>();

        readonly SingletonProviderCreator _singletonProviderCreator;
        readonly SingletonMarkRegistry _singletonMarkRegistry;

        readonly Queue<IBindingFinalizer> _currentBindings = new Queue<IBindingFinalizer>();
        readonly List<IBindingFinalizer> _processedBindings = new List<IBindingFinalizer>();

        bool _isValidating;
        bool _isInstalling;
        bool _hasDisplayedInstallWarning;

        public DiContainer(bool isValidating)
        {
            _isValidating = isValidating;

            _singletonMarkRegistry = new SingletonMarkRegistry();
            _singletonProviderCreator = new SingletonProviderCreator(this, _singletonMarkRegistry);

            // We can't simply call Bind<DiContainer>().FromInstance(this) here because
            // we don't want these bindings to be included in the Clone() below
            // So just directly add to the provider map instead
            var thisProvider = new InstanceProvider(typeof(DiContainer), this);
            var thisContracts = new Type[]
            {
                typeof(DiContainer), typeof(IBinder), typeof(IResolver), typeof(IInstantiator)
            };

            foreach (var contractType in thisContracts)
            {
                var infoList = new List<ProviderInfo>()
                {
                    new ProviderInfo(thisProvider, null)
                };

                var bindingId = new BindingId(contractType, null);

                _providers.Add(bindingId, infoList);
            }
        }

        public DiContainer()
            : this(false)
        {
        }

        // Use the CreateSubContainer method instead of this
        DiContainer(DiContainer parentContainer, bool isValidating)
            : this(isValidating)
        {
            _parentContainer = parentContainer;
        }

        DiContainer(DiContainer parentContainer)
            : this(parentContainer, false)
        {
        }

        public SingletonMarkRegistry SingletonMarkRegistry
        {
            get
            {
                return _singletonMarkRegistry;
            }
        }

        public SingletonProviderCreator SingletonProviderCreator
        {
            get
            {
                return _singletonProviderCreator;
            }
        }

#if !NOT_UNITY3D

        public Transform DefaultParent
        {
            get;
            set;
        }
#endif

        public DiContainer ParentContainer
        {
            get
            {
                return _parentContainer;
            }
        }

        public bool ChecksForCircularDependencies
        {
            get
            {
#if ZEN_MULTITHREADING
                // When multithreading is supported we can't use a static field to track the lookup
                // TODO: We could look at the inject context though
                return false;
#else
                return true;
#endif
            }
        }

        public IEnumerable<Type> InstalledInstallers
        {
            get
            {
                return _installedInstallers;
            }
        }

        // See comment in IBinder.cs for description
        public bool IsValidating
        {
            get
            {
                return _isValidating;
            }
        }

        // When this is true, it will log warnings when Resolve or Instantiate
        // methods are called
        // Used to ensure that Resolve and Instantiate methods are not called
        // during bind phase.  This is important since Resolve and Instantiate
        // make use of the bindings, so if the bindings are not complete then
        // unexpected behaviour can occur
        public bool IsInstalling
        {
            get
            {
                return _isInstalling;
            }
            set
            {
                _isInstalling = value;
            }
        }

        public IEnumerable<BindingId> AllContracts
        {
            get
            {
                FlushBindings();
                return _providers.Keys;
            }
        }

        // DO not run this within Unity!
        // This is only really useful if you are not using any of the Unity bind methods such as
        // FromGameObject, FromPrefab, etc.
        // If you are using those, and you call this method, then it will have side effects like
        // creating game objects
        // Otherwise, it should be safe to call since all the fake instances will be limited to
        // within a cloned copy of the DiContainer and should not have any side effects
        public void Validate()
        {
            var container = CloneForValidate();

            Assert.That(container.IsValidating);

            // It's tempting here to iterate over all the BindingId's in _providers
            // and make sure they can be resolved but that fails once we start
            // using complex conditionals, so this is the best we can do
            container.ResolveDependencyRoots();

            container.ValidateIValidatables();
        }

        public List<object> ResolveDependencyRoots()
        {
            var context = new InjectContext(
                this, typeof(object), DependencyRootIdentifier);
            context.SourceType = InjectSources.Local;
            context.Optional = true;

            return ResolveAll(context).Cast<object>().ToList();
        }

        DiContainer CloneForValidate()
        {
            FlushBindings();

            DiContainer container;

            if (this.ParentContainer == null)
            {
                container = new DiContainer(null, true);
            }
            else
            {
                // Need to clone all parents too
                container = new DiContainer(
                    this.ParentContainer.CloneForValidate(), true);
            }

            foreach (var binding in _processedBindings)
            {
                container._currentBindings.Enqueue(binding);
            }

            container.FlushBindings();
            return container;
        }

        public void ValidateIValidatables()
        {
            Assert.That(IsValidating);

            foreach (var pair in _providers.ToList())
            {
                var bindingId = pair.Key;
                var providers = pair.Value;

                // Validate all IValidatable's
                List<ProviderInfo> validatableProviders;

                var injectContext = new InjectContext(
                    this, bindingId.Type, bindingId.Identifier);

                if (bindingId.Type.DerivesFrom<IValidatable>())
                {
                    validatableProviders = providers;
                }
                else
                {
                    validatableProviders = providers
                        .Where(x => x.Provider.GetInstanceType(injectContext)
                                .DerivesFrom<IValidatable>()).ToList();
                }

                foreach (var provider in validatableProviders)
                {
                    var validatable = (IValidatable)provider.Provider.GetInstance(injectContext);

                    validatable.Validate();
                }
            }
        }

        public DiContainer CreateSubContainer()
        {
            return CreateSubContainer(_isValidating);
        }

        public DiContainer CreateSubContainer(bool isValidating)
        {
            FlushBindings();

            var container = new DiContainer(this, isValidating);
#if !NOT_UNITY3D
            container.DefaultParent = this.DefaultParent;
#endif
            foreach (var binding in _processedBindings.Where(x => x.InheritInSubContainers))
            {
                container._currentBindings.Enqueue(binding);
            }

            container.FlushBindings();
            return container;
        }

        public void RegisterProvider(
            BindingId bindingId, BindingCondition condition, IProvider provider)
        {
            var info = new ProviderInfo(provider, condition);

            if (_providers.ContainsKey(bindingId))
            {
                _providers[bindingId].Add(info);
            }
            else
            {
                _providers.Add(bindingId, new List<ProviderInfo> { info });
            }
        }

        // Wrap IEnumerable<> to avoid LINQ mistakes
        internal List<IProvider> GetAllProviderMatches(InjectContext context)
        {
            Assert.IsNotNull(context);
            return GetProviderMatchesInternal(context).Select(x => x.ProviderInfo.Provider).ToList();
        }

        // Be careful with this method since it is a coroutine
        IEnumerable<ProviderPair> GetProviderMatchesInternal(InjectContext context)
        {
            Assert.IsNotNull(context);
            return GetProvidersForContract(context.GetBindingId(), context.SourceType)
                .Where(x => x.ProviderInfo.Condition == null || x.ProviderInfo.Condition(context));
        }

        IEnumerable<ProviderPair> GetProvidersForContract(BindingId bindingId, InjectSources sourceType)
        {
            FlushBindings();

            switch (sourceType)
            {
                case InjectSources.Local:
                {
                    return GetLocalProviders(bindingId).Select(x => new ProviderPair(x, this));
                }
                case InjectSources.Any:
                {
                    var localPairs = GetLocalProviders(bindingId).Select(x => new ProviderPair(x, this));

                    if (_parentContainer == null)
                    {
                        return localPairs;
                    }

                    return localPairs.Concat(
                        _parentContainer.GetProvidersForContract(bindingId, InjectSources.Any));
                }
                case InjectSources.AnyParent:
                {
                    if (_parentContainer == null)
                    {
                        return Enumerable.Empty<ProviderPair>();
                    }

                    return _parentContainer.GetProvidersForContract(bindingId, InjectSources.Any);
                }
                case InjectSources.Parent:
                {
                    if (_parentContainer == null)
                    {
                        return Enumerable.Empty<ProviderPair>();
                    }

                    return _parentContainer.GetProvidersForContract(bindingId, InjectSources.Local);
                }
            }

            throw Assert.CreateException("Invalid source type");
        }

        List<ProviderInfo> GetLocalProviders(BindingId bindingId)
        {
            List<ProviderInfo> localProviders;

            if (_providers.TryGetValue(bindingId, out localProviders))
            {
                return localProviders;
            }

            // If we are asking for a List<int>, we should also match for any localProviders that are bound to the open generic type List<>
            // Currently it only matches one and not the other - not totally sure if this is better than returning both
            if (bindingId.Type.IsGenericType() && _providers.TryGetValue(new BindingId(bindingId.Type.GetGenericTypeDefinition(), bindingId.Identifier), out localProviders))
            {
                return localProviders;
            }

            return new List<ProviderInfo>();
        }

        // See comment in IResolver.cs for description of this method
        public IList ResolveAll(InjectContext context)
        {
            Assert.IsNotNull(context);
            // Note that different types can map to the same provider (eg. a base type to a concrete class and a concrete class to itself)

            FlushBindings();
            CheckForInstallWarning(context);

            var matches = GetProviderMatchesInternal(context).ToList();

            if (matches.Any())
            {
                var instances = matches.SelectMany(x => SafeGetInstances(x.ProviderInfo.Provider, context)).ToArray();

                if (IsValidating)
                {
                    instances = instances.Select(x => x is ValidationMarker ? context.MemberType.GetDefaultValue() : x).ToArray();
                }

                return ReflectionUtil.CreateGenericList(context.MemberType, instances);
            }

            Assert.That(context.Optional,
                "Could not find required dependency with type '{0}' \nObject graph:\n {1}", context.MemberType.Name(), context.GetObjectGraphString());

            return ReflectionUtil.CreateGenericList(context.MemberType, new object[] {});
        }

        void CheckForInstallWarning(InjectContext context)
        {
            Assert.IsNotNull(context);
#if DEBUG || UNITY_EDITOR
            if (!_isInstalling)
            {
                return;
            }

            if (_hasDisplayedInstallWarning)
            {
                return;
            }

            if (context == null)
            {
                // No way to tell whether this is ok or not so just assume ok
                return;
            }

            var rootContext = context.ParentContextsAndSelf.Last();

            if (rootContext.MemberType.DerivesFrom<IInstaller>())
            {
                // Resolving/instantiating/injecting installers is valid during install phase
                return;
            }

#if !NOT_UNITY3D
            if (rootContext.MemberType.DerivesFrom<DecoratorInstaller>())
            {
                return;
            }
#endif

            _hasDisplayedInstallWarning = true;
            // Feel free to comment this out if you are comfortable with this practice
            Log.Warn("Zenject Warning: It is bad practice to call Inject/Resolve/Instantiate before all the Installers have completed!  This is important to ensure that all bindings have properly been installed in case they are needed when injecting/instantiating/resolving.  Detected when operating on type '{0}'", rootContext.MemberType.Name());
#endif
        }

        // See comment in IResolver.cs for description of this method
        public List<Type> ResolveTypeAll(InjectContext context)
        {
            Assert.IsNotNull(context);

            FlushBindings();
            CheckForInstallWarning(context);

            var bindingId = context.GetBindingId();

            if (_providers.ContainsKey(bindingId))
            {
                return _providers[bindingId].Select(x => x.Provider.GetInstanceType(context)).Where(x => x != null).ToList();
            }

            return new List<Type> {};
        }

        // See comment in IBinder.cs for description for this method
        public void Install(IEnumerable<IInstaller> installers)
        {
            foreach (var installer in installers)
            {
                Assert.IsNotNull(installer, "Tried to install a null installer");

                if (installer.IsEnabled)
                {
                    Install(installer);
                }
            }
        }

        // See comment in IBinder.cs for description for this method
        public void InstallExplicit(IInstaller installer, List<TypeValuePair> extraArgs)
        {
            Assert.That(installer.IsEnabled);

            FlushBindings();

            InjectExplicit(installer, extraArgs);
            InstallInstallerInternal(installer);
        }

        // See comment in IBinder.cs for description for this method
        public void Install(IInstaller installer)
        {
            Install(installer, new object[0]);
        }

        public void Install(IInstaller installer, IEnumerable<object> extraArgs)
        {
            Assert.That(installer.IsEnabled);

            FlushBindings();

            try
            {
                Inject(installer, extraArgs);
            }
            catch (Exception e)
            {
                throw new ZenjectException(e,
                    "While installing installer '{0}'", installer.GetType().Name());
            }

            InstallInstallerInternal(installer);
        }

        // See comment in IBinder.cs for description for this method
        public void Install<T>()
            where T : IInstaller
        {
            Install<T>(new object[0]);
        }

        public void Install<T>(IEnumerable<object> extraArgs)
            where T : IInstaller
        {
            Install(typeof(T), extraArgs);
        }

        // See comment in IBinder.cs for description of this method
        public void Install(Type installerType)
        {
            Install(installerType, new object[0]);
        }

        public void Install(Type installerType, IEnumerable<object> extraArgs)
        {
            InstallExplicit(installerType, InjectUtil.CreateArgList(extraArgs));
        }

        // See comment in IBinder.cs for description of this method
        public void InstallExplicit(Type installerType, List<TypeValuePair> extraArgs)
        {
            Assert.That(installerType.DerivesFrom<IInstaller>());

            FlushBindings();

#if !NOT_UNITY3D
            if (installerType.DerivesFrom<MonoInstaller>())
            {
                var gameObj = CreateAndParentPrefabResource("Installers/" + installerType.Name());

                var installer = gameObj.GetComponentInChildren<MonoInstaller>();

                InjectExplicit(installer, extraArgs);
                InstallInstallerInternal(installer);
            }
            else
#endif
            {
                var installer = (IInstaller)InstantiateExplicit(installerType, extraArgs);
                InstallInstallerInternal(installer);
            }
        }

        // See comment in IBinder.cs for description of this method
        public bool HasInstalled<T>()
            where T : IInstaller
        {
            return HasInstalled(typeof(T));
        }

        // See comment in IBinder.cs for description of this method
        public bool HasInstalled(Type installerType)
        {
            FlushBindings();
            return _installedInstallers.Where(x => x == installerType).Any();
        }

        void InstallInstallerInternal(IInstaller installer)
        {
            var installerType = installer.GetType();

            Log.Debug("Installing installer '{0}'", installerType);

            Assert.That(!_installsInProgress.Contains(installerType),
                "Potential infinite loop detected while installing '{0}'", installerType.Name());

            Assert.That(!_installedInstallers.Contains(installerType),
                "Tried installing installer '{0}' twice", installerType.Name());

            _installedInstallers.Add(installerType);
            _installsInProgress.Push(installerType);

            try
            {
                installer.InstallBindings();
                FlushBindings();
            }
            catch (Exception e)
            {
                // This context information is really helpful when bind commands fail
                throw new Exception(
                    "Error occurred while running installer '{0}'".Fmt(installer.GetType().Name()), e);
            }
            finally
            {
                Assert.That(_installsInProgress.Peek().Equals(installerType));
                _installsInProgress.Pop();
            }
        }

        // Try looking up a single provider for a given context
        // Note that this method should not throw zenject exceptions
        internal ProviderLookupResult TryGetUniqueProvider(
            InjectContext context, out IProvider provider)
        {
            Assert.IsNotNull(context);

            // Note that different types can map to the same provider (eg. a base type to a concrete class and a concrete class to itself)
            var providers = GetProviderMatchesInternal(context).ToList();

            if (providers.IsEmpty())
            {
                provider = null;
                return ProviderLookupResult.None;
            }

            if (providers.Count > 1)
            {
                // If we find multiple providers and we are looking for just one, then
                // try to intelligently choose one from the list before giving up

                // First try picking the most 'local' dependencies
                // This will bias towards bindings for the lower level specific containers rather than the global high level container
                // This will, for example, allow you to just ask for a DiContainer dependency without needing to specify [Inject(InjectSources.Local)]
                // (otherwise it would always match for a list of DiContainer's for all parent containers)
                var sortedProviders = providers.Select(x => new { Pair = x, Distance = GetContainerHeirarchyDistance(x.Container) }).OrderBy(x => x.Distance).ToList();

                sortedProviders.RemoveAll(x => x.Distance != sortedProviders[0].Distance);

                if (sortedProviders.Count == 1)
                {
                    // We have one match that is the closest
                    provider = sortedProviders[0].Pair.ProviderInfo.Provider;
                }
                else
                {
                    // Try choosing the one with a condition before giving up and throwing an exception
                    // This is nice because it allows us to bind a default and then override with conditions
                    provider = sortedProviders.Where(x => x.Pair.ProviderInfo.Condition != null).Select(x => x.Pair.ProviderInfo.Provider).OnlyOrDefault();

                    if (provider == null)
                    {
                        return ProviderLookupResult.Multiple;
                    }
                }
            }
            else
            {
                provider = providers.Single().ProviderInfo.Provider;
            }

            Assert.IsNotNull(provider);
            return ProviderLookupResult.Success;
        }

        // See comment in IResolver.cs for description of this method
        public object Resolve(InjectContext context)
        {
            Assert.IsNotNull(context);

            IProvider provider;

            FlushBindings();
            CheckForInstallWarning(context);

            var result = TryGetUniqueProvider(context, out provider);

            Assert.That(result != ProviderLookupResult.Multiple,
                "Found multiple matches when only one was expected for type '{0}'{1}. \nObject graph:\n {2}",
                context.MemberType.Name(),
                (context.ObjectType == null ? "" : " while building object with type '{0}'".Fmt(context.ObjectType.Name())),
                context.GetObjectGraphString());

            if (result == ProviderLookupResult.None)
            {
                // If it's a generic list then try matching multiple instances to its generic type
                if (ReflectionUtil.IsGenericList(context.MemberType))
                {
                    var subType = context.MemberType.GenericArguments().Single();

                    var subContext = context.Clone();
                    subContext.MemberType = subType;

                    return ResolveAll(subContext);
                }

                if (context.Optional)
                {
                    return context.FallBackValue;
                }

                throw Assert.CreateException("Unable to resolve type '{0}'{1}. \nObject graph:\n{2}",
                    context.MemberType.Name() + (context.Identifier == null ? "" : " with ID '{0}'".Fmt(context.Identifier.ToString())),
                    (context.ObjectType == null ? "" : " while building object with type '{0}'".Fmt(context.ObjectType.Name())),
                    context.GetObjectGraphString());
            }

            Assert.That(result == ProviderLookupResult.Success);
            Assert.IsNotNull(provider);

            var instances = SafeGetInstances(provider, context);

            Assert.That(!instances.IsEmpty(), "Provider returned zero instances when one was expected!");
            Assert.That(instances.Count() == 1, "Provider returned multiple instances when one was expected!");

            return instances.First();
        }

        IEnumerable<object> SafeGetInstances(IProvider provider, InjectContext context)
        {
            Assert.IsNotNull(context);

            if (ChecksForCircularDependencies)
            {
                var lookupId = new LookupId(provider, context.GetBindingId());

                // Allow one before giving up so that you can do circular dependencies via postinject or fields
                Assert.That(_resolvesInProgress.Where(x => x.Equals(lookupId)).Count() <= 1,
                    "Circular dependency detected! \nObject graph:\n {0}", context.GetObjectGraphString());

                _resolvesInProgress.Push(lookupId);
                try
                {
                    return provider.GetAllInstances(context);
                }
                finally
                {
                    Assert.That(_resolvesInProgress.Peek().Equals(lookupId));
                    _resolvesInProgress.Pop();
                }
            }
            else
            {
                return provider.GetAllInstances(context);
            }
        }

        int GetContainerHeirarchyDistance(DiContainer container)
        {
            return GetContainerHeirarchyDistance(container, 0);
        }

        int GetContainerHeirarchyDistance(DiContainer container, int depth)
        {
            if (container == this)
            {
                return depth;
            }

            Assert.IsNotNull(_parentContainer);
            return _parentContainer.GetContainerHeirarchyDistance(container, depth + 1);
        }

        public IEnumerable<Type> GetDependencyContracts<TContract>()
        {
            return GetDependencyContracts(typeof(TContract));
        }

        public IEnumerable<Type> GetDependencyContracts(Type contract)
        {
            FlushBindings();

            foreach (var injectMember in TypeAnalyzer.GetInfo(contract).AllInjectables)
            {
                yield return injectMember.MemberType;
            }
        }

        public T InstantiateExplicit<T>(List<TypeValuePair> extraArgs)
        {
            return (T)InstantiateExplicit(typeof(T), extraArgs);
        }

        public object InstantiateExplicit(Type concreteType, List<TypeValuePair> extraArgs)
        {
            bool autoInject = true;

            return InstantiateExplicit(
                autoInject,
                new InjectArgs()
                {
                    TypeInfo = TypeAnalyzer.GetInfo(concreteType),
                    ExtraArgs = extraArgs,
                    Context = new InjectContext(this, concreteType, null),
                    ConcreteIdentifier = null,
                    UseAllArgs = true,
                });
        }

        // See comment in IInstantiator.cs for description of this method
        public object InstantiateExplicit(bool autoInject, InjectArgs args)
        {
#if PROFILING_ENABLED
            using (ProfileBlock.Start("Zenject.Instantiate({0})", args.TypeInfo.Type))
#endif
            {
                return InstantiateInternal(autoInject, args);
            }
        }

        public static bool CanCreateOrInjectDuringValidation(Type type)
        {
            // During validation, do not instantiate or inject anything except for
            // Installers, IValidatable's, or types marked with attribute ZenjectAllowDuringValidation
            // You would typically use ZenjectAllowDuringValidation attribute for data that you
            // inject into factories
            return type.DerivesFrom<IInstaller>()
                || type.DerivesFrom<IValidatable>()
#if !NOT_UNITY3D
                || type.DerivesFrom<CompositionRoot>()
                || type.DerivesFrom<DecoratorInstaller>()
#endif
                || type.HasAttribute<ZenjectAllowDuringValidationAttribute>();
        }

        object InstantiateInternal(bool autoInject, InjectArgs args)
        {
            Type concreteType = args.TypeInfo.Type;

#if !NOT_UNITY3D
            Assert.That(!concreteType.DerivesFrom<UnityEngine.Component>(),
                "Error occurred while instantiating object of type '{0}'. Instantiator should not be used to create new mono behaviours.  Must use InstantiatePrefabForComponent, InstantiatePrefab, or InstantiateComponent.", concreteType.Name());
#endif

            FlushBindings();
            CheckForInstallWarning(args.Context);

            Assert.IsNotNull(args.TypeInfo.InjectConstructor,
                "More than one (or zero) constructors found for type '{0}' when creating dependencies.  Use one [Inject] attribute to specify which to use.", concreteType);

            // Make a copy since we remove from it below
            var paramValues = new List<object>();

            foreach (var injectInfo in args.TypeInfo.ConstructorInjectables)
            {
                object value;

                if (!InjectUtil.PopValueWithType(
                    args.ExtraArgs, injectInfo.MemberType, out value))
                {
                    value = Resolve(injectInfo.CreateInjectContext(
                        this, args.Context, null, args.ConcreteIdentifier));
                }

                if (value is ValidationMarker)
                {
                    Assert.That(IsValidating);
                    paramValues.Add(injectInfo.MemberType.GetDefaultValue());
                }
                else
                {
                    paramValues.Add(value);
                }
            }

            object newObj;

            if (!IsValidating || CanCreateOrInjectDuringValidation(args.TypeInfo.Type))
            {
                //Log.Debug("Zenject: Instantiating type '{0}'", concreteType.Name());
                try
                {
#if PROFILING_ENABLED
                    using (ProfileBlock.Start("{0}.{0}()", concreteType))
#endif
                    {
                        newObj = args.TypeInfo.InjectConstructor.Invoke(paramValues.ToArray());
                    }
                }
                catch (Exception e)
                {
                    throw Assert.CreateException(
                        e, "Error occurred while instantiating object with type '{0}'", concreteType.Name());
                }
            }
            else
            {
                newObj = new ValidationMarker(args.TypeInfo.Type);
            }

            if (autoInject)
            {
                InjectExplicit(newObj, args);
            }
            else if (args.UseAllArgs && !args.ExtraArgs.IsEmpty())
            {
                throw Assert.CreateException(
                    "Passed unnecessary parameters when injecting into type '{0}'. \nExtra Parameters: {1}\nObject graph:\n{2}",
                    newObj.GetType().Name(), String.Join(",", args.ExtraArgs.Select(x => x.Type.Name()).ToArray()), args.Context.GetObjectGraphString());
            }

            return newObj;
        }

        public void InjectExplicit(object injectable, List<TypeValuePair> extraArgs)
        {
            Type injectableType;

            if (injectable is ValidationMarker)
            {
                injectableType = ((ValidationMarker)injectable).MarkedType;
            }
            else
            {
                injectableType = injectable.GetType();
            }

            InjectExplicit(
                injectable,
                new InjectArgs()
                {
                    ExtraArgs = extraArgs,
                    UseAllArgs = true,
                    TypeInfo = TypeAnalyzer.GetInfo(injectableType),
                    Context = new InjectContext(this, injectableType, null),
                    ConcreteIdentifier = null,
                });
        }

        // See comment in IResolver.cs for description of this method
        public void InjectExplicit(
            object injectable, InjectArgs args)
        {
            Assert.That(injectable != null);

            Type injectableType = args.TypeInfo.Type;

            // Installers are the only things that we instantiate/inject on during validation
            bool isDryRun = IsValidating && !CanCreateOrInjectDuringValidation(args.TypeInfo.Type);

            if (!isDryRun)
            {
                Assert.IsEqual(injectable.GetType(), injectableType);
            }

#if !NOT_UNITY3D
            Assert.That(injectableType != typeof(GameObject),
                "Use InjectGameObject to Inject game objects instead of Inject method");
#endif

            FlushBindings();
            CheckForInstallWarning(args.Context);

            foreach (var injectInfo in args.TypeInfo.FieldInjectables.Concat(
                args.TypeInfo.PropertyInjectables))
            {
                object value;

                if (InjectUtil.PopValueWithType(args.ExtraArgs, injectInfo.MemberType, out value))
                {
                    if (!isDryRun)
                    {
                        if (value is ValidationMarker)
                        {
                            Assert.That(IsValidating);
                        }
                        else
                        {
                            injectInfo.Setter(injectable, value);
                        }
                    }
                }
                else
                {
                    value = Resolve(
                        injectInfo.CreateInjectContext(
                            this, args.Context, injectable, args.ConcreteIdentifier));

                    if (injectInfo.Optional && value == null)
                    {
                        // Do not override in this case so it retains the hard-coded value
                    }
                    else
                    {
                        if (!isDryRun)
                        {
                            if (value is ValidationMarker)
                            {
                                Assert.That(IsValidating);
                            }
                            else
                            {
                                injectInfo.Setter(injectable, value);
                            }
                        }
                    }
                }
            }

            foreach (var method in args.TypeInfo.PostInjectMethods)
            {
#if PROFILING_ENABLED
                using (ProfileBlock.Start("{0}.{1}()", injectableType, method.MethodInfo.Name))
#endif
                {
                    var paramValues = new List<object>();

                    foreach (var injectInfo in method.InjectableInfo)
                    {
                        object value;

                        if (!InjectUtil.PopValueWithType(args.ExtraArgs, injectInfo.MemberType, out value))
                        {
                            value = Resolve(
                                injectInfo.CreateInjectContext(this, args.Context, injectable, args.ConcreteIdentifier));
                        }

                        if (value is ValidationMarker)
                        {
                            Assert.That(IsValidating);
                            paramValues.Add(injectInfo.MemberType.GetDefaultValue());
                        }
                        else
                        {
                            paramValues.Add(value);
                        }
                    }

                    if (!isDryRun)
                    {
                        method.MethodInfo.Invoke(injectable, paramValues.ToArray());
                    }
                }
            }

            if (args.UseAllArgs && !args.ExtraArgs.IsEmpty())
            {
                throw Assert.CreateException(
                    "Passed unnecessary parameters when injecting into type '{0}'. \nExtra Parameters: {1}\nObject graph:\n{2}",
                    injectableType.Name(), String.Join(",", args.ExtraArgs.Select(x => x.Type.Name()).ToArray()), args.Context.GetObjectGraphString());
            }
        }

#if !NOT_UNITY3D

        public GameObject InstantiatePrefabResourceExplicit(
            string resourcePath, List<TypeValuePair> extraArgs)
        {
            return InstantiatePrefabResourceExplicit(
                resourcePath, extraArgs, null);
        }

        public GameObject InstantiatePrefabResourceExplicit(
            string resourcePath, List<TypeValuePair> extraArgs, string groupName)
        {
            return InstantiatePrefabResourceExplicit(
                resourcePath, extraArgs, groupName, true);
        }

        public GameObject InstantiatePrefabResourceExplicit(
            string resourcePath, List<TypeValuePair> extraArgs,
            string groupName, bool useAllArgs)
        {
            var prefab = (GameObject)Resources.Load(resourcePath);
            Assert.IsNotNull(prefab, "Could not find prefab at resource location '{0}'".Fmt(resourcePath));

            return InstantiatePrefabExplicit(
                prefab, extraArgs, groupName, useAllArgs);
        }

        public GameObject InstantiatePrefabExplicit(
            GameObject prefab, List<TypeValuePair> extraArgs)
        {
            return InstantiatePrefabExplicit(
                prefab, extraArgs, null);
        }

        public GameObject InstantiatePrefabExplicit(
            GameObject prefab, List<TypeValuePair> extraArgs,
            string groupName)
        {
            return InstantiatePrefabExplicit(
                prefab, extraArgs, groupName, true);
        }

        public GameObject InstantiatePrefabExplicit(
            GameObject prefab, List<TypeValuePair> extraArgs,
            string groupName, bool useAllArgs)
        {
            FlushBindings();

            var gameObj = CreateAndParentPrefab(prefab, groupName);

            InjectGameObjectExplicit(
                gameObj, true, extraArgs, useAllArgs);

            return gameObj;
        }

        // Don't use this unless you know what you're doing
        // You probably want to use InstantiatePrefab instead
        // This one will only create the prefab and will not inject into it
        public GameObject CreateAndParentPrefabResource(string resourcePath)
        {
            return CreateAndParentPrefabResource(resourcePath, null);
        }

        // Don't use this unless you know what you're doing
        // You probably want to use InstantiatePrefab instead
        // This one will only create the prefab and will not inject into it
        public GameObject CreateAndParentPrefabResource(string resourcePath, string groupName)
        {
            var prefab = (GameObject)Resources.Load(resourcePath);

            Assert.IsNotNull(prefab,
                "Could not find prefab at resource location '{0}'".Fmt(resourcePath));

            return CreateAndParentPrefab(prefab, groupName);
        }

        // Don't use this unless you know what you're doing
        // You probably want to use InstantiatePrefab instead
        // This one will only create the prefab and will not inject into it
        public GameObject CreateAndParentPrefab(GameObject prefab, string groupName)
        {
            FlushBindings();

            GameObject gameObj;

            if (IsValidating)
            {
                // We need to avoid triggering any Awake() method during validation
                // so temporarily disable the prefab so that the object gets
                // instantiated as disabled
                bool wasActive = prefab.activeSelf;

                prefab.SetActive(false);

                try
                {
                    gameObj = (GameObject)GameObject.Instantiate(prefab);
                }
                finally
                {
                    prefab.SetActive(wasActive);
                }
            }
            else
            {
                gameObj = (GameObject)GameObject.Instantiate(prefab);
            }

            gameObj.transform.SetParent(GetTransformGroup(groupName), false);
            return gameObj;
        }

        public GameObject CreateEmptyGameObject(string name)
        {
            return CreateEmptyGameObject(name, null);
        }

        public GameObject CreateEmptyGameObject(string name, string groupName)
        {
            FlushBindings();

            var gameObj = new GameObject(name);
            gameObj.transform.SetParent(GetTransformGroup(groupName), false);
            return gameObj;
        }

        public T InstantiatePrefabForComponentExplicit<T>(
            GameObject prefab, List<TypeValuePair> extraArgs)
        {
            return (T)InstantiatePrefabForComponentExplicit(
                typeof(T), prefab, extraArgs);
        }

        // Note: Any arguments that are used will be removed from extraArgs
        public object InstantiatePrefabForComponentExplicit(
            Type componentType, GameObject prefab, List<TypeValuePair> extraArgs)
        {
            return InstantiatePrefabForComponentExplicit(
                componentType, prefab, extraArgs, null);
        }

        public object InstantiatePrefabForComponentExplicit(
            Type componentType, GameObject prefab, List<TypeValuePair> extraArgs,
            string groupName)
        {
            return InstantiatePrefabForComponentExplicit(
                prefab, groupName,
                new InjectArgs()
                {
                    TypeInfo = TypeAnalyzer.GetInfo(componentType),
                    ExtraArgs = extraArgs,
                    Context = new InjectContext(this, componentType, null),
                    ConcreteIdentifier = null,
                    UseAllArgs = true,
                });
        }

        // Note: Any arguments that are used will be removed from extraArgs
        public object InstantiatePrefabForComponentExplicit(
            GameObject prefab, string groupName, InjectArgs args)
        {
            FlushBindings();

            Assert.That(prefab != null, "Null prefab found when instantiating game object");

            var componentType = args.TypeInfo.Type;

            Assert.That(componentType.IsInterface() || componentType.DerivesFrom<Component>(),
                "Expected type '{0}' to derive from UnityEngine.Component", componentType.Name());

            var gameObj = (GameObject)GameObject.Instantiate(prefab);

            gameObj.transform.SetParent(GetTransformGroup(groupName), false);
            gameObj.SetActive(true);

            return InjectGameObjectForComponentExplicit(
                gameObj, componentType, args);
        }

        Transform GetTransformGroup(string groupName)
        {
            if (DefaultParent == null)
            {
                if (groupName == null)
                {
                    return null;
                }

                return (GameObject.Find("/" + groupName) ?? new GameObject(groupName)).transform;
            }

            if (groupName == null)
            {
                return DefaultParent;
            }

            foreach (Transform child in DefaultParent)
            {
                if (child.name == groupName)
                {
                    return child;
                }
            }

            var group = new GameObject(groupName).transform;
            group.SetParent(DefaultParent, false);
            return group;
        }

#endif

        public T Instantiate<T>()
        {
            return Instantiate<T>(new object[0]);
        }

        // See comment in IInstantiator.cs for description of this method
        public T Instantiate<T>(IEnumerable<object> extraArgs)
        {
            return (T)Instantiate(typeof(T), extraArgs);
        }

        public object Instantiate(Type concreteType)
        {
            return Instantiate(concreteType, new object[0]);
        }

        // See comment in IInstantiator.cs for description of this method
        public object Instantiate(
            Type concreteType, IEnumerable<object> extraArgs)
        {
            Assert.That(!extraArgs.ContainsItem(null),
                "Null value given to factory constructor arguments when instantiating object with type '{0}'. In order to use null use InstantiateExplicit", concreteType);

            return InstantiateExplicit(
                concreteType, InjectUtil.CreateArgList(extraArgs));
        }

#if !NOT_UNITY3D
        // See comment in IInstantiator.cs for description of this method
        public TContract InstantiateComponent<TContract>(GameObject gameObject)
            where TContract : Component
        {
            return InstantiateComponent<TContract>(gameObject, new object[0]);
        }

        // See comment in IInstantiator.cs for description of this method
        public TContract InstantiateComponent<TContract>(
            GameObject gameObject, IEnumerable<object> extraArgs)
            where TContract : Component
        {
            return (TContract)InstantiateComponent(typeof(TContract), gameObject, extraArgs);
        }

        public Component InstantiateComponent(
            Type componentType, GameObject gameObject)
        {
            return InstantiateComponent(componentType, gameObject, new object[0]);
        }

        // See comment in IInstantiator.cs for description of this method
        public Component InstantiateComponent(
            Type componentType, GameObject gameObject, IEnumerable<object> extraArgs)
        {
            return InstantiateComponentExplicit(
                componentType, gameObject, InjectUtil.CreateArgList(extraArgs));
        }

        public Component InstantiateComponentExplicit(
            Type componentType, GameObject gameObject, List<TypeValuePair> extraArgs)
        {
            Assert.That(componentType.DerivesFrom<Component>());

            FlushBindings();

            var monoBehaviour = (Component)gameObject.AddComponent(componentType);
            InjectExplicit(monoBehaviour, extraArgs);
            return monoBehaviour;
        }

        public GameObject InstantiatePrefab(GameObject prefab)
        {
            return InstantiatePrefab(prefab, new object[0]);
        }

        public GameObject InstantiatePrefab(
            GameObject prefab, IEnumerable<object> extraArgs)
        {
            return InstantiatePrefab(
                prefab, extraArgs, null);
        }

        public GameObject InstantiatePrefab(
            GameObject prefab, IEnumerable<object> extraArgs, string groupName)
        {
            return InstantiatePrefabExplicit(
                prefab, InjectUtil.CreateArgList(extraArgs),
                groupName);
        }

        // See comment in IInstantiator.cs for description of this method
        public GameObject InstantiatePrefabResource(string resourcePath)
        {
            return InstantiatePrefabResource(resourcePath, new object[0]);
        }

        // See comment in IInstantiator.cs for description of this method
        public GameObject InstantiatePrefabResource(
            string resourcePath, IEnumerable<object> extraArgs)
        {
            return InstantiatePrefabResource(
                resourcePath, extraArgs, null);
        }

        // See comment in IInstantiator.cs for description of this method
        public GameObject InstantiatePrefabResource(
            string resourcePath, IEnumerable<object> extraArgs, string groupName)
        {
            return InstantiatePrefabResourceExplicit(
                resourcePath, InjectUtil.CreateArgList(extraArgs),
                groupName);
        }

        /////////////// InstantiatePrefabForComponent

        public T InstantiatePrefabForComponent<T>(GameObject prefab)
        {
            return InstantiatePrefabForComponent<T>(prefab, new object[0]);
        }

        public T InstantiatePrefabForComponent<T>(
            GameObject prefab, IEnumerable<object> extraArgs)
        {
            return (T)InstantiatePrefabForComponent(
                typeof(T), prefab, extraArgs);
        }

        public object InstantiatePrefabForComponent(
            Type concreteType, GameObject prefab, IEnumerable<object> extraArgs)
        {
            return InstantiatePrefabForComponentExplicit(
                concreteType, prefab,
                InjectUtil.CreateArgList(extraArgs));
        }

        /////////////// InstantiatePrefabForComponent

        // See comment in IInstantiator.cs for description of this method
        public T InstantiatePrefabResourceForComponent<T>(string resourcePath)
        {
            return InstantiatePrefabResourceForComponent<T>(resourcePath, new object[0]);
        }

        // See comment in IInstantiator.cs for description of this method
        public T InstantiatePrefabResourceForComponent<T>(
            string resourcePath, IEnumerable<object> extraArgs)
        {
            return (T)InstantiatePrefabResourceForComponent(typeof(T), resourcePath, extraArgs);
        }

        // See comment in IInstantiator.cs for description of this method
        public object InstantiatePrefabResourceForComponent(
            Type concreteType, string resourcePath, IEnumerable<object> extraArgs)
        {
            Assert.That(!extraArgs.ContainsItem(null),
            "Null value given to factory constructor arguments when instantiating object with type '{0}'. In order to use null use InstantiatePrefabForComponentExplicit", concreteType);

            return InstantiatePrefabResourceForComponentExplicit(
                concreteType, resourcePath, InjectUtil.CreateArgList(extraArgs));
        }

        public T InstantiatePrefabResourceForComponentExplicit<T>(
            string resourcePath, List<TypeValuePair> extraArgs)
        {
            return (T)InstantiatePrefabResourceForComponentExplicit(
                typeof(T), resourcePath, extraArgs);
        }

        // Note: Any arguments that are used will be removed from extraArgs
        public object InstantiatePrefabResourceForComponentExplicit(
            Type componentType, string resourcePath, List<TypeValuePair> extraArgs)
        {
            return InstantiatePrefabResourceForComponentExplicit(
                resourcePath, null,
                new InjectArgs()
                {
                    TypeInfo = TypeAnalyzer.GetInfo(componentType),
                    ExtraArgs = extraArgs,
                    Context = new InjectContext(this, componentType, null),
                    ConcreteIdentifier = null,
                    UseAllArgs = true,
                });
        }

        // Note: Any arguments that are used will be removed from extraArgs
        public object InstantiatePrefabResourceForComponentExplicit(
            string resourcePath, string groupName, InjectArgs args)
        {
            var prefab = (GameObject)Resources.Load(resourcePath);
            Assert.IsNotNull(prefab,
                "Could not find prefab at resource location '{0}'".Fmt(resourcePath));
            return InstantiatePrefabForComponentExplicit(
                prefab, groupName, args);
        }

#endif

        ////////////// Convenience methods for IResolver ////////////////

#if !NOT_UNITY3D
        // See comment in IResolver.cs for description of this method
        public void InjectGameObject(
            GameObject gameObject)
        {
            InjectGameObject(gameObject, true);
        }

        // See comment in IResolver.cs for description of this method
        public void InjectGameObject(
            GameObject gameObject, bool recursive)
        {
            InjectGameObject(gameObject, recursive, new object[0]);
        }

        // See comment in IResolver.cs for description of this method
        public void InjectGameObject(
            GameObject gameObject, bool recursive, IEnumerable<object> extraArgs)
        {
            InjectGameObjectExplicit(
                gameObject, recursive,
                InjectUtil.CreateArgList(extraArgs));
        }

        public void InjectGameObjectExplicit(
            GameObject gameObject, bool recursive,
            List<TypeValuePair> extraArgs)
        {
            // We have to pass in a null InjectContext here because we aren't
            // asking for any particular type
            InjectGameObjectExplicit(
                gameObject, recursive, extraArgs, true);
        }

        // See comment in IResolver.cs for description of this method
        public void InjectGameObjectExplicit(
            GameObject gameObject, bool recursive,
            List<TypeValuePair> extraArgs, bool useAllArgs)
        {
            FlushBindings();

            // Inject on the children first since the parent objects are more likely to use them in their post inject methods
            foreach (var component in ZenUtilInternal.GetInjectableComponentsBottomUp(
                gameObject, recursive).ToList())
            {
                if (component == null)
                {
                    Log.Warn("Found null component while injecting game object '{0}'.  Possible missing script.", gameObject.name);
                    continue;
                }

                if (component.GetType().DerivesFrom<MonoInstaller>())
                {
                    // Do not inject on installers since these are always injected before they are installed
                    continue;
                }

                InjectExplicit(component,
                    new InjectArgs()
                    {
                        TypeInfo = TypeAnalyzer.GetInfo(component.GetType()),
                        ExtraArgs = extraArgs,
                        Context = new InjectContext(this, component.GetType()),
                        ConcreteIdentifier = null,
                        UseAllArgs = false,
                    });
            }

            if (useAllArgs && !extraArgs.IsEmpty())
            {
                throw Assert.CreateException(
                    "Passed unnecessary parameters when injecting into game object '{0}'. \nExtra Parameters: {1}",
                    gameObject.name, string.Join(",", extraArgs.Select(x => x.Type.Name()).ToArray()));
            }
        }

        public Component InjectGameObjectForComponentExplicit(
            GameObject gameObject, Type componentType,
            InjectArgs args)
        {
            Component requestedScript = null;

            // Inject on the children first since the parent objects are more likely to use them in their post inject methods
            foreach (var component in ZenUtilInternal.GetInjectableComponentsBottomUp(
                gameObject, true).ToList())
            {
                if (component == null)
                {
                    Log.Warn("Found null component while injecting into game object '{0}'.  Possible missing script.", gameObject.name);
                    continue;
                }

                if (component.GetType().DerivesFrom<MonoInstaller>())
                {
                    // Do not inject on installers since these are always injected before they are installed
                    continue;
                }

                if (component.GetType().DerivesFromOrEqual(componentType))
                {
                    Assert.IsNull(requestedScript,
                        "Found multiple matches with type '{0}' when injecting into game object '{1}'", componentType, gameObject.name);
                    requestedScript = component;

                    InjectExplicit(component, args);
                }
                else
                {
                    Inject(component);
                }
            }

            if (requestedScript == null)
            {
                throw Assert.CreateException(
                    "Could not find component with type '{0}' when instantiating new game object", componentType);
            }

            return requestedScript;
        }
#endif

        // See comment in IResolver.cs for description of this method
        public void Inject(object injectable)
        {
            Inject(injectable, new object[0]);
        }

        // See comment in IResolver.cs for description of this method
        public void Inject(object injectable, IEnumerable<object> extraArgs)
        {
            InjectExplicit(
                injectable, InjectUtil.CreateArgList(extraArgs));
        }

        // See comment in IResolver.cs for description of this method
        public List<Type> ResolveTypeAll(Type type)
        {
            return ResolveTypeAll(new InjectContext(this, type, null));
        }

        // See comment in IResolver.cs for description of this method
        public TContract Resolve<TContract>()
        {
            return Resolve<TContract>((string)null);
        }

        // See comment in IResolver.cs for description of this method
        public TContract Resolve<TContract>(string identifier)
        {
            return Resolve<TContract>(new InjectContext(this, typeof(TContract), identifier));
        }

        // See comment in IResolver.cs for description of this method
        public TContract TryResolve<TContract>()
            where TContract : class
        {
            return TryResolve<TContract>((string)null);
        }

        // See comment in IResolver.cs for description of this method
        public TContract TryResolve<TContract>(string identifier)
            where TContract : class
        {
            return (TContract)TryResolve(typeof(TContract), identifier);
        }

        // See comment in IResolver.cs for description of this method
        public object TryResolve(Type contractType)
        {
            return TryResolve(contractType, null);
        }

        // See comment in IResolver.cs for description of this method
        public object TryResolve(Type contractType, string identifier)
        {
            return Resolve(new InjectContext(this, contractType, identifier, true));
        }

        // See comment in IResolver.cs for description of this method
        public object Resolve(Type contractType)
        {
            return Resolve(new InjectContext(this, contractType, null));
        }

        // See comment in IResolver.cs for description of this method
        public object Resolve(Type contractType, string identifier)
        {
            return Resolve(new InjectContext(this, contractType, identifier));
        }

        // See comment in IResolver.cs for description of this method
        public TContract Resolve<TContract>(InjectContext context)
        {
            Assert.IsNotNull(context);

            Assert.IsEqual(context.MemberType, typeof(TContract));
            return (TContract) Resolve(context);
        }

        // See comment in IResolver.cs for description of this method
        public List<TContract> ResolveAll<TContract>()
        {
            return ResolveAll<TContract>((string)null);
        }

        // See comment in IResolver.cs for description of this method
        public List<TContract> ResolveAll<TContract>(bool optional)
        {
            return ResolveAll<TContract>(null, optional);
        }

        // See comment in IResolver.cs for description of this method
        public List<TContract> ResolveAll<TContract>(string identifier)
        {
            return ResolveAll<TContract>(identifier, false);
        }

        // See comment in IResolver.cs for description of this method
        public List<TContract> ResolveAll<TContract>(string identifier, bool optional)
        {
            var context = new InjectContext(this, typeof(TContract), identifier, optional);
            return ResolveAll<TContract>(context);
        }

        // See comment in IResolver.cs for description of this method
        public List<TContract> ResolveAll<TContract>(InjectContext context)
        {
            Assert.IsNotNull(context);
            Assert.IsEqual(context.MemberType, typeof(TContract));
            return (List<TContract>) ResolveAll(context);
        }

        // See comment in IResolver.cs for description of this method
        public IList ResolveAll(Type contractType)
        {
            return ResolveAll(contractType, null);
        }

        // See comment in IResolver.cs for description of this method
        public IList ResolveAll(Type contractType, string identifier)
        {
            return ResolveAll(contractType, identifier, false);
        }

        // See comment in IResolver.cs for description of this method
        public IList ResolveAll(Type contractType, bool optional)
        {
            return ResolveAll(contractType, null, optional);
        }

        // See comment in IResolver.cs for description of this method
        public IList ResolveAll(Type contractType, string identifier, bool optional)
        {
            var context = new InjectContext(this, contractType, identifier, optional);
            return ResolveAll(context);
        }

        ////////////// IBinder ////////////////

        public void UnbindAll()
        {
            FlushBindings();
            _providers.Clear();
        }

        public bool Unbind<TContract>()
        {
            return Unbind<TContract>(null);
        }

        public bool Unbind<TContract>(string identifier)
        {
            FlushBindings();

            var bindingId = new BindingId(typeof(TContract), identifier);

            return _providers.Remove(bindingId);
        }

        // See comment in IBinder.cs for description of this method
        public bool HasBinding(InjectContext context)
        {
            Assert.IsNotNull(context);

            FlushBindings();

            List<ProviderInfo> providers;

            if (!_providers.TryGetValue(context.GetBindingId(), out providers))
            {
                return false;
            }

            return providers.Where(x => x.Condition == null || x.Condition(context)).HasAtLeast(1);
        }

        // See comment in IBinder.cs for description of this method
        public bool HasBinding<TContract>()
        {
            return HasBinding<TContract>(null);
        }

        // See comment in IBinder.cs for description of this method
        public bool HasBinding<TContract>(string identifier)
        {
            return HasBinding(
                new InjectContext(this, typeof(TContract), identifier));
        }

        // Do not use this - it is for internal use only
        public void FlushBindings()
        {
            while (!_currentBindings.IsEmpty())
            {
                var binding = _currentBindings.Dequeue();

                binding.FinalizeBinding(this);

                _processedBindings.Add(binding);
            }
        }

        public BindFinalizerWrapper StartBinding()
        {
            FlushBindings();

            var bindingFinalizer = new BindFinalizerWrapper();
            _currentBindings.Enqueue(bindingFinalizer);
            return bindingFinalizer;
        }

        public ConcreteBinderGeneric<TContract> Rebind<TContract>()
        {
            return Rebind<TContract>(null);
        }

        public ConcreteBinderGeneric<TContract> Rebind<TContract>(string identifier)
        {
            Unbind<TContract>(identifier);
            return Bind<TContract>(identifier);
        }

        public ConcreteBinderGeneric<TContract> Bind<TContract>()
        {
            return Bind<TContract>(null);
        }

        public ConcreteBinderGeneric<TContract> Bind<TContract>(string identifier)
        {
            Assert.That(!typeof(TContract).DerivesFrom<IDynamicFactory>(),
                "You should not use Container.Bind for factory classes.  Use Container.BindFactory instead.");

            var bindInfo = new BindInfo(identifier, typeof(TContract));

            return new ConcreteBinderGeneric<TContract>(
                bindInfo, StartBinding());
        }

        public ConcreteBinderNonGeneric Bind(params Type[] contractTypes)
        {
            return Bind((string)null, contractTypes);
        }

        public ConcreteBinderNonGeneric Bind(string identifier, params Type[] contractTypes)
        {
            Assert.That(contractTypes.All(x => !x.DerivesFrom<IDynamicFactory>()),
                "You should not use Container.Bind for factory classes.  Use Container.BindFactory instead.");

            var bindInfo = new BindInfo(identifier, contractTypes.ToList());
            return new ConcreteBinderNonGeneric(bindInfo, StartBinding());
        }

        // This is equivalent to calling NonLazy() at the end of your bind statement
        // It's only in rare cases where you need to call this instead of NonLazy()
        public void BindRootResolve<TContract>()
        {
            BindRootResolve<TContract>(null);
        }

        public void BindRootResolve<TContract>(string identifier)
        {
            BindRootResolve(identifier, typeof(TContract));
        }

        public void BindRootResolve(params Type[] rootTypes)
        {
            BindRootResolve(null, rootTypes);
        }

        public void BindRootResolve(string identifier, params Type[] rootTypes)
        {
            Bind<object>(DependencyRootIdentifier).To(rootTypes).FromResolve(identifier);
        }

        public ConcreteBinderNonGeneric BindAllInterfaces<T>()
        {
            return BindAllInterfaces<T>(null);
        }

        public ConcreteBinderNonGeneric BindAllInterfaces<T>(string identifier)
        {
            return BindAllInterfaces(identifier, typeof(T));
        }

        public ConcreteBinderNonGeneric BindAllInterfaces(Type type)
        {
            return BindAllInterfaces(null, type);
        }

        public ConcreteBinderNonGeneric BindAllInterfaces(string identifier, Type type)
        {
            // We must only have one dependency root per container
            // We need this when calling this with a GameObjectCompositionRoot
            return Bind(identifier, type.Interfaces().ToArray());
        }

        public ConcreteBinderNonGeneric BindAllInterfacesAndSelf<T>()
        {
            return BindAllInterfacesAndSelf<T>(null);
        }

        public ConcreteBinderNonGeneric BindAllInterfacesAndSelf<T>(string identifier)
        {
            return BindAllInterfacesAndSelf(identifier, typeof(T));
        }

        public ConcreteBinderNonGeneric BindAllInterfacesAndSelf(Type type)
        {
            return BindAllInterfacesAndSelf(null, type);
        }

        public ConcreteBinderNonGeneric BindAllInterfacesAndSelf(string identifier, Type type)
        {
            // We must only have one dependency root per container
            // We need this when calling this with a GameObjectCompositionRoot
            return Bind(
                identifier,
                type.Interfaces().Append(type).ToArray());
        }

        public ScopeBinder BindInstance<TContract>(string identifier, TContract obj)
        {
            return Bind<TContract>(identifier).FromInstance(obj);
        }

        public ScopeBinder BindInstance<TContract>(TContract obj)
        {
            return Bind<TContract>().FromInstance(obj);
        }

        public FactoryToChoiceBinder<TContract> BindIFactory<TContract>()
        {
            return BindIFactory<TContract>(null);
        }

        public FactoryToChoiceBinder<TContract> BindIFactory<TContract>(string identifier)
        {
            var bindInfo = new BindInfo(identifier, typeof(IFactory<TContract>));
            return new FactoryToChoiceBinder<TContract>(
                bindInfo, typeof(Factory<TContract>), StartBinding());
        }

        public FactoryToChoiceBinder<TContract> BindFactory<TContract, TFactory>()
            where TFactory : Factory<TContract>
        {
            return BindFactory<TContract, TFactory>(null);
        }

        public FactoryToChoiceBinder<TContract> BindFactory<TContract, TFactory>(string identifier)
            where TFactory : Factory<TContract>
        {
            var bindInfo = new BindInfo(identifier, typeof(TFactory));
            return new FactoryToChoiceBinder<TContract>(
                bindInfo, typeof(TFactory), StartBinding());
        }

        public FactoryToChoiceBinder<TParam1, TContract> BindIFactory<TParam1, TContract>()
        {
            return BindIFactory<TParam1, TContract>(null);
        }

        public FactoryToChoiceBinder<TParam1, TContract> BindIFactory<TParam1, TContract>(string identifier)
        {
            var bindInfo = new BindInfo(identifier, typeof(IFactory<TParam1, TContract>));
            return new FactoryToChoiceBinder<TParam1, TContract>(
                bindInfo, typeof(Factory<TParam1, TContract>), StartBinding());
        }

        public FactoryToChoiceBinder<TParam1, TContract> BindFactory<TParam1, TContract, TFactory>()
            where TFactory : Factory<TParam1, TContract>
        {
            return BindFactory<TParam1, TContract, TFactory>(null);
        }

        public FactoryToChoiceBinder<TParam1, TContract> BindFactory<TParam1, TContract, TFactory>(string identifier)
            where TFactory : Factory<TParam1, TContract>
        {
            var bindInfo = new BindInfo(identifier, typeof(TFactory));
            return new FactoryToChoiceBinder<TParam1, TContract>(
                bindInfo, typeof(TFactory), StartBinding());
        }

        public FactoryToChoiceBinder<TParam1, TParam2, TContract> BindIFactory<TParam1, TParam2, TContract>()
        {
            return BindIFactory<TParam1, TParam2, TContract>(null);
        }

        public FactoryToChoiceBinder<TParam1, TParam2, TContract> BindIFactory<TParam1, TParam2, TContract>(string identifier)
        {
            var bindInfo = new BindInfo(identifier, typeof(IFactory<TParam1, TParam2, TContract>));
            return new FactoryToChoiceBinder<TParam1, TParam2, TContract>(
                bindInfo, typeof(Factory<TParam1, TParam2, TContract>), StartBinding());
        }

        public FactoryToChoiceBinder<TParam1, TParam2, TContract> BindFactory<TParam1, TParam2, TContract, TFactory>()
            where TFactory : Factory<TParam1, TParam2, TContract>
        {
            return BindFactory<TParam1, TParam2, TContract, TFactory>(null);
        }

        public FactoryToChoiceBinder<TParam1, TParam2, TContract> BindFactory<TParam1, TParam2, TContract, TFactory>(string identifier)
            where TFactory : Factory<TParam1, TParam2, TContract>
        {
            var bindInfo = new BindInfo(identifier, typeof(TFactory));
            return new FactoryToChoiceBinder<TParam1, TParam2, TContract>(
                bindInfo, typeof(TFactory), StartBinding());
        }

        public FactoryToChoiceBinder<TParam1, TParam2, TParam3, TContract> BindIFactory<TParam1, TParam2, TParam3, TContract>()
        {
            return BindIFactory<TParam1, TParam2, TParam3, TContract>(null);
        }

        public FactoryToChoiceBinder<TParam1, TParam2, TParam3, TContract> BindIFactory<TParam1, TParam2, TParam3, TContract>(string identifier)
        {
            var bindInfo = new BindInfo(identifier, typeof(IFactory<TParam1, TParam2, TParam3, TContract>));
            return new FactoryToChoiceBinder<TParam1, TParam2, TParam3, TContract>(
                bindInfo, typeof(Factory<TParam1, TParam2, TParam3, TContract>), StartBinding());
        }

        public FactoryToChoiceBinder<TParam1, TParam2, TParam3, TContract> BindFactory<TParam1, TParam2, TParam3, TContract, TFactory>()
            where TFactory : Factory<TParam1, TParam2, TParam3, TContract>
        {
            return BindFactory<TParam1, TParam2, TParam3, TContract, TFactory>(null);
        }

        public FactoryToChoiceBinder<TParam1, TParam2, TParam3, TContract> BindFactory<TParam1, TParam2, TParam3, TContract, TFactory>(string identifier)
            where TFactory : Factory<TParam1, TParam2, TParam3, TContract>
        {
            var bindInfo = new BindInfo(identifier, typeof(TFactory));
            return new FactoryToChoiceBinder<TParam1, TParam2, TParam3, TContract>(
                bindInfo, typeof(TFactory), StartBinding());
        }

        public FactoryToChoiceBinder<TParam1, TParam2, TParam3, TParam4, TContract> BindIFactory<TParam1, TParam2, TParam3, TParam4, TContract>()
        {
            return BindIFactory<TParam1, TParam2, TParam3, TParam4, TContract>(null);
        }

        public FactoryToChoiceBinder<TParam1, TParam2, TParam3, TParam4, TContract> BindIFactory<TParam1, TParam2, TParam3, TParam4, TContract>(string identifier)
        {
            var bindInfo = new BindInfo(identifier, typeof(IFactory<TParam1, TParam2, TParam3, TParam4, TContract>));
            return new FactoryToChoiceBinder<TParam1, TParam2, TParam3, TParam4, TContract>(
                bindInfo, typeof(Factory<TParam1, TParam2, TParam3, TParam4, TContract>), StartBinding());
        }

        public FactoryToChoiceBinder<TParam1, TParam2, TParam3, TParam4, TContract> BindFactory<TParam1, TParam2, TParam3, TParam4, TContract, TFactory>()
            where TFactory : Factory<TParam1, TParam2, TParam3, TParam4, TContract>
        {
            return BindFactory<TParam1, TParam2, TParam3, TParam4, TContract, TFactory>(null);
        }

        public FactoryToChoiceBinder<TParam1, TParam2, TParam3, TParam4, TContract> BindFactory<TParam1, TParam2, TParam3, TParam4, TContract, TFactory>(string identifier)
            where TFactory : Factory<TParam1, TParam2, TParam3, TParam4, TContract>
        {
            var bindInfo = new BindInfo(identifier, typeof(TFactory));
            return new FactoryToChoiceBinder<TParam1, TParam2, TParam3, TParam4, TContract>(
                bindInfo, typeof(TFactory), StartBinding());
        }

        public FactoryToChoiceBinder<TParam1, TParam2, TParam3, TParam4, TParam5, TContract> BindIFactory<TParam1, TParam2, TParam3, TParam4, TParam5, TContract>()
        {
            return BindIFactory<TParam1, TParam2, TParam3, TParam4, TParam5, TContract>(null);
        }

        public FactoryToChoiceBinder<TParam1, TParam2, TParam3, TParam4, TParam5, TContract> BindIFactory<TParam1, TParam2, TParam3, TParam4, TParam5, TContract>(string identifier)
        {
            var bindInfo = new BindInfo(identifier, typeof(IFactory<TParam1, TParam2, TParam3, TParam4, TParam5, TContract>));
            return new FactoryToChoiceBinder<TParam1, TParam2, TParam3, TParam4, TParam5, TContract>(
                bindInfo, typeof(Factory<TParam1, TParam2, TParam3, TParam4, TParam5, TContract>), StartBinding());
        }

        public FactoryToChoiceBinder<TParam1, TParam2, TParam3, TParam4, TParam5, TContract> BindFactory<TParam1, TParam2, TParam3, TParam4, TParam5, TContract, TFactory>()
            where TFactory : Factory<TParam1, TParam2, TParam3, TParam4, TParam5, TContract>
        {
            return BindFactory<TParam1, TParam2, TParam3, TParam4, TParam5, TContract, TFactory>(null);
        }

        public FactoryToChoiceBinder<TParam1, TParam2, TParam3, TParam4, TParam5, TContract> BindFactory<TParam1, TParam2, TParam3, TParam4, TParam5, TContract, TFactory>(string identifier)
            where TFactory : Factory<TParam1, TParam2, TParam3, TParam4, TParam5, TContract>
        {
            var bindInfo = new BindInfo(identifier, typeof(TFactory));
            return new FactoryToChoiceBinder<TParam1, TParam2, TParam3, TParam4, TParam5, TContract>(
                bindInfo, typeof(TFactory), StartBinding());
        }

        ////////////// Types ////////////////

        class ProviderPair
        {
            public ProviderPair(
                ProviderInfo providerInfo,
                DiContainer container)
            {
                ProviderInfo = providerInfo;
                Container = container;
            }

            public ProviderInfo ProviderInfo
            {
                get;
                private set;
            }

            public DiContainer Container
            {
                get;
                private set;
            }
        }

        public enum ProviderLookupResult
        {
            Success,
            Multiple,
            None
        }

        struct LookupId
        {
            public readonly IProvider Provider;
            public readonly BindingId BindingId;

            public LookupId(
                IProvider provider, BindingId bindingId)
            {
                Provider = provider;
                BindingId = bindingId;
            }
        }

        public class ProviderInfo
        {
            public ProviderInfo(IProvider provider, BindingCondition condition)
            {
                Provider = provider;
                Condition = condition;
            }

            public IProvider Provider
            {
                get;
                private set;
            }

            public BindingCondition Condition
            {
                get;
                private set;
            }
        }
    }
}
