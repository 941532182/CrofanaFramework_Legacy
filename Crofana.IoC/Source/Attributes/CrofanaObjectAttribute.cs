using System;

namespace Crofana.IoC
{
    /// <summary>
    /// 此类的实例将由CrofanaFramework托管生命周期
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = true)]
    public class CrofanaObjectAttribute : Attribute { }
}
