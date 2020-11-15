using System;

namespace Crofana.IoC
{
    /// <summary>
    /// 此类将采用指定的实例化策略
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = true)]
    public class ScopeAttribute : Attribute
    {
        public Scope Scope { get; }
        public ScopeAttribute(Scope scope) => Scope = scope;
    }
    public enum Scope
    {
        Singleton = 0,
        Prototype = 1,
        Pooled = 2
    }
}
