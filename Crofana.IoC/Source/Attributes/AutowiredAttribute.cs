using System;

namespace Crofana.IoC
{
    /// <summary>
    /// 此成员的值将在对象构造时自动装配
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Field | AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
    public class AutowiredAttribute : Attribute { }
}
