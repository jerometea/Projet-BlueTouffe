#if !NOT_UNITY3D

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using UnityEngine;
using ModestTree;

namespace Zenject
{
    public class ZenjectBinding : MonoBehaviour
    {
        [Tooltip("The component to add to the Zenject container")]
        [SerializeField]
        Component[] _components = null;

        [Tooltip("Note: This value is optional and can be ignored in most cases.  This can be useful to differentiate multiple bindings of the same type.  For example, if you have multiple cameras in your scene, you can 'name' them by giving each one a different identifier.  For your main camera you might call it 'Main' then any class can refer to it by using an attribute like [Inject('Main')]")]
        [SerializeField]
        string _identifier = null;

        [Tooltip("Note: This value is optional and can be ignored in most cases.  This value will determine what container the component gets added to.  If unset, the component will be bound on the most 'local' composition root.  In most cases this will be the SceneCompositionRoot, unless this component is underneath a GameObjectCompositionRoot, or ProjectCompositionRoot, in which case it will bind to that instead by default.  You can also override this default by providing the CompositionRoot directly.  This can be useful if you want to bind something that is inside a GameObjectCompositionRoot to the SceneCompositionRoot container.")]
        [SerializeField]
        CompositionRoot _compositionRoot = null;

        [Tooltip("This value is used to determine how to bind this component.  When set to 'Self' is equivalent to calling Container.FromInstance inside an installer. When set to 'AllInterfaces' this is equivalent to calling 'Container.BindAllInterfaces<MyMonoBehaviour>().ToInstance', and similarly for AllInterfacesAndSelf")]
        [SerializeField]
        BindTypes _bindType = BindTypes.Self;

        public CompositionRoot CompositionRoot
        {
            get
            {
                return _compositionRoot;
            }
        }

        public Component[] Components
        {
            get
            {
                return _components;
            }
        }

        public string Identifier
        {
            get
            {
                return _identifier;
            }
        }

        public BindTypes BindType
        {
            get
            {
                return _bindType;
            }
        }

        public void Start()
        {
            // Define this method so we expose the enabled check box
        }

        public enum BindTypes
        {
            Self,
            AllInterfaces,
            AllInterfacesAndSelf,
        }
    }
}

#endif
