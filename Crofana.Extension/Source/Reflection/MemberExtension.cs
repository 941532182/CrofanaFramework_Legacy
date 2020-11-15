using System;
using System.Reflection;
using System.Linq;

namespace Crofana.Extension
{
    public static class MemberExtension
    {

        public static bool HasAttribute<T>(this MemberInfo self) where T : Attribute => self.GetCustomAttribute<T>() != null;
        public static bool HasAttributeRecursive<T>(this MemberInfo self, uint maxIteration = 8) where T : Attribute => maxIteration > 0 && (self.HasAttribute<T>() || self.GetCustomAttributes().Any<object>(x => (x != null) ? (x.GetType() != typeof(AttributeUsageAttribute) && x.GetType().HasAttributeRecursive<T>(maxIteration - 1)) : (false)));
        public static T GetAttribute<T>(this MemberInfo self) where T : Attribute => self.GetCustomAttribute<T>();
        public static T GetAttributeRecursive<T>(this MemberInfo self, uint maxIteration = 8) where T : Attribute
        {
            if (maxIteration == 0)
            {
                return null;
            }
            T result = self.GetCustomAttribute<T>();
            if (result == null)
            {
                foreach (var attr in self.GetCustomAttributes())
                {
                    if (attr.GetType() == typeof(AttributeUsageAttribute)) continue;
                    result = attr.GetType().GetAttributeRecursive<T>(maxIteration - 1);
                    if (result != null) break;
                }
            }
            return result;
        }

        public static void SetMemberValue(this MemberInfo self, object obj, object value)
        {
            if (self.MemberType == MemberTypes.Field)
            {
                FieldInfo field = self as FieldInfo;
                field.SetValue(obj, value);
            }
            else if (self.MemberType == MemberTypes.Property)
            {
                PropertyInfo prop = self as PropertyInfo;
                prop.SetValue(obj, value);
            }
            else
            {
                throw new BadMemberTypeException(self);
            }
        }
    }
}
