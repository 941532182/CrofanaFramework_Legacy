using System;
using System.Collections.Generic;
using System.Text;

namespace Crofana.IoC
{
    /// <summary>
    /// 此类将将收到CrofanaObject创建相关回调
    /// </summary>
    [CrofanaObject]
    [Scope(Scope.Singleton)]
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
    public class DependencyInjectionListenerAttribute : Attribute { }

    /// <summary>
    /// 此方法将在CrofanaObject执行依赖注入之前执行
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = false, Inherited = false)]
    public class PreInjectAttribute : Attribute { }

    /// <summary>
    /// 此方法将在CrofanaObject执行依赖注入之后执行
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = false, Inherited = false)]
    public class PostInjectAttribute : Attribute { }
}
