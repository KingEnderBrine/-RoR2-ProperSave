using System;

namespace ProperSave.TinyJson
{
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = false)]
    internal sealed class ObjectTypeFromPropertyAttribute : Attribute
    {
        private string PropertyName { get; }

        public ObjectTypeFromPropertyAttribute(string propertyName)
        {
            PropertyName = propertyName;
        }

        public Type GetObjectType(object instance)
        {
            if (PropertyName == null)
            {
                return null;
            }
            return Type.GetType(instance?.GetType()?.GetProperty(PropertyName)?.GetValue(instance) as string, false);
        }
    }
}
