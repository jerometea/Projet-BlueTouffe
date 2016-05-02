using System;
using System.Collections.Generic;
using ModestTree;

#if !NOT_UNITY3D
using UnityEngine;
#endif


namespace Zenject
{
    public class ConcreteBinderNonGeneric : FromBinderNonGeneric
    {
        public ConcreteBinderNonGeneric(
            BindInfo bindInfo,
            BindFinalizerWrapper finalizerWrapper)
            : base(bindInfo, finalizerWrapper)
        {
            ToSelf();
        }

        // Note that this is the default, so not necessary to call
        public FromBinderNonGeneric ToSelf()
        {
            Assert.IsEqual(BindInfo.ToChoice, ToChoices.Self);

            SubFinalizer = new ScopableBindingFinalizer(
                BindInfo, SingletonTypes.To, null,
                (container, type) => new TransientProvider(
                    type, container, BindInfo.Arguments, BindInfo.ConcreteIdentifier));

            return this;
        }

        public FromBinderNonGeneric To<TConcrete>()
        {
            return To(typeof(TConcrete));
        }

        public FromBinderNonGeneric To(Type concreteType)
        {
            return To(new List<Type>() { concreteType });
        }

        // We don't just use params so that we ensure a min of 1
        public FromBinderNonGeneric To(List<Type> concreteTypes)
        {
            BindingUtil.AssertConcreteTypeListIsNotEmpty(concreteTypes);
            BindingUtil.AssertIsDerivedFromTypes(concreteTypes, BindInfo.ContractTypes);

            BindInfo.ToChoice = ToChoices.Concrete;
            BindInfo.ToTypes = concreteTypes;

            return this;
        }
    }
}
