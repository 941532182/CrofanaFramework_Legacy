using System;
using System.Reflection;

namespace Crofana.Extension
{
    public class SetterNotFoundException : Exception
    {
        public SetterNotFoundException(PropertyInfo prop) : base($"{prop.ReflectedType}.{prop.Name} 属性未提供Setter访问器") { }
    }
}
