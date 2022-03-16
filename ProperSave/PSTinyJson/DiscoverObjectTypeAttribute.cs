using System;
using System.Linq;
using System.Reflection;

namespace ProperSave.TinyJson
{
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = false)]
    internal sealed class DiscoverObjectTypeAttribute : Attribute
    {
        private string MemberName { get; }

        public DiscoverObjectTypeAttribute(string memberName)
        {
            MemberName = memberName;
        }

        public Type GetObjectType(object instance)
        {
            if (MemberName == null)
            {
                return null;
            }
            var member = instance?.GetType()?.GetMember(MemberName)?.FirstOrDefault();
            string value;
            switch (member)
            {
                case FieldInfo fieldInfo:
                    value = fieldInfo.GetValue(instance) as string;
                    break;
                case PropertyInfo propertyInfo:
                    value = propertyInfo.GetValue(instance) as string;
                    break;
                default:
                    throw new NotSupportedException();
            }

            return Type.GetType(value, false);
        }
    }
}
