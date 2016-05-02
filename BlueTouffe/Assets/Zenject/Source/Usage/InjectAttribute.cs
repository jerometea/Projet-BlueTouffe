using System;

namespace Zenject
{
    [AttributeUsage(AttributeTargets.Constructor | AttributeTargets.Parameter | AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = false)]
    public class InjectAttribute : InjectAttributeBase
    {
        public InjectAttribute(string identifier)
        {
            Identifier = identifier;
        }

        public InjectAttribute(InjectSources sourceType)
        {
            SourceType = sourceType;
        }

        public InjectAttribute(string identifier, InjectSources sourceType)
        {
            Identifier = identifier;
            SourceType = sourceType;
        }

        public InjectAttribute()
        {
        }
    }
}

