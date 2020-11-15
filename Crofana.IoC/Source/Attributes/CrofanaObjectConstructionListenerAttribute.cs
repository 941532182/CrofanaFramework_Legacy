using System;

namespace Crofana.IoC
{
    /// <summary>
    /// 此类将将收到CrofanaObject创建相关回调
    /// </summary>
    [CrofanaObject]
    [Scope(Scope.Singleton)]
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
    public class CrofanaObjectConstructionListenerAttribute : Attribute { }

    /// <summary>
    /// 此方法将在CrofanaObject的构造函数执行之前执行
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = false, Inherited = false)]
    public class PreConstructAttribute : Attribute { }

    /// <summary>
    /// 此方法将在CrofanaObject的构造函数执行之后执行
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = false, Inherited = false)]
    public class PostConstructAttribute : Attribute { }
}
