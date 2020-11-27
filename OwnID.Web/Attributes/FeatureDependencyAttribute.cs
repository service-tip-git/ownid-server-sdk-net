using System;

namespace OwnID.Web.Attributes
{
    [AttributeUsage(AttributeTargets.Class, Inherited = false)]
    public class FeatureDependencyAttribute : Attribute
    {
        public Type[] Types { get; }
        
        public FeatureDependencyAttribute(params Type[] features)
        {
            Types = features;
        }
    }
}