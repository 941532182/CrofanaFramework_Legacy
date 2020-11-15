using System;
using System.Collections.Generic;
using System.Reflection;
using System.Linq;

using Crofana.Extension;

namespace Crofana.IoC
{
    /// <summary>
    /// 标准IoC容器实现，支持依赖注入、生命周期回调、配置作用域
    /// </summary>
    public class StandardCrofanaObjectFactory : ICrofanaObjectFactory
    {

        #region Statics
        private static Type[] EMPTY_TYPE_ARRAY = { };
        #endregion

        #region Fields
        private Dictionary<Type, object> objectMap = new Dictionary<Type, object>();
        private List<Action> cachedPreConstructCallbacks = new List<Action>();
        private List<Action<object>> cachedPostConstructCallbacks = new List<Action<object>>();
        private List<Action<object>> cachedPreInjectCallbacks = new List<Action<object>>();
        private List<Action<object>> cachedPostInjectCallbacks = new List<Action<object>>();
        #endregion

        #region Constructors
        public StandardCrofanaObjectFactory()
        {
            ScanListeners();
        }
        #endregion

        #region ICrofanaObjectFactory
        public object GetObject(Type type)
        {
            if (!type.HasAttributeRecursive<CrofanaObjectAttribute>())
            {
                return null;
            }
            ScopeAttribute scope = type.GetAttributeRecursive<ScopeAttribute>();
            if (scope == null || scope.Scope == Scope.Singleton)
            {
                if (!objectMap.ContainsKey(type))
                {
                    return NewObject(type);
                }
                return objectMap[type];
            }
            return NewObject(type);
        }
        #endregion

        #region Public Methods
        public T GetObject<T>() where T : class => GetObject(typeof(T)) as T;
        #endregion

        #region Private Methods
        private void ScanListeners()
        {
            AppDomain.CurrentDomain
                     .GetAssemblies()
                     .ToList()
                     .ForEach(asm =>
                     {
                         asm.GetTypes()
                            .ToList()
                            .ForEach(type => TryCacheCallbacks(type));
                     });
        }

        private object NewObject(Type type)
        {
            object obj;
            ProcessConstruction(type, out obj);
            if (obj != null)
            {
                TryRegisterObjectMap(obj);
                ProcessDependencyInjection(obj);
            }
            return obj;
        }

        private void ProcessConstruction(Type type, out object obj)
        {
            BroadcastPreConstruct();

            ConstructorInfo ctor = type.GetConstructor(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance, null, EMPTY_TYPE_ARRAY, null);
            if (ctor == null)
            {
                throw new ConstructorNotFoundException(type);
            }
            obj = ctor.Invoke(null);

            BroadcastPostConstruct(obj);
        }

        private void ProcessDependencyInjection(object obj)
        {
            BroadcastPreInject(obj);

            obj.GetType()
               .GetMembers(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)
               .Where(x => (x.MemberType == MemberTypes.Field || x.MemberType == MemberTypes.Property) && x.HasAttributeRecursive<AutowiredAttribute>())
               .ToList()
               .ForEach(x =>
               {
                   if (!x.HasAttributeRecursive<AutowiredAttribute>()) return;
                   if (x.MemberType == MemberTypes.Field)
                   {
                       FieldInfo field = x as FieldInfo;
                       if (field != null && field.FieldType.HasAttributeRecursive<CrofanaObjectAttribute>())
                       {
                           field.SetValue(obj, GetObject(field.FieldType));
                       }
                   }
                   else
                   {
                       PropertyInfo prop = x as PropertyInfo;
                       if (prop.SetMethod == null)
                       {
                           throw new SetterNotFoundException(prop);
                       }
                       if (prop != null && prop.PropertyType.HasAttributeRecursive<CrofanaObjectAttribute>())
                       {
                           prop.SetValue(obj, GetObject(prop.PropertyType));
                       }
                   }
               });

            BroadcastPostInject(obj);
        }

        private void TryRegisterObjectMap(object obj)
        {
            Type type = obj.GetType();
            ScopeAttribute scope = type.GetAttributeRecursive<ScopeAttribute>();
            if (scope == null || scope.Scope == Scope.Singleton)
            {
                objectMap[type] = obj;
            }
        }

        private void TryCacheCallbacks(Type type)
        {
            bool isCOCListener = type.HasAttributeRecursive<CrofanaObjectConstructionListenerAttribute>();
            bool isDIListener = type.HasAttributeRecursive<DependencyInjectionListenerAttribute>();
            if (!isCOCListener && !isDIListener) return;
            type.GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static)
                .ToList()
                .ForEach(x =>
                {
                    object obj = null;
                    if (isCOCListener)
                    {
                        if (x.HasAttributeRecursive<PreConstructAttribute>())
                        {
                            obj = obj != null ? obj : NewObject(type);
                            cachedPreConstructCallbacks.Add(x.CreateDelegate(typeof(Action), obj) as Action);
                            Console.WriteLine($"发现构造前置监听器类型: {type.FullName}");
                        }
                        if (x.HasAttributeRecursive<PostConstructAttribute>())
                        {
                            obj = obj != null ? obj : NewObject(type);
                            cachedPostConstructCallbacks.Add(x.CreateDelegate(typeof(Action<object>), obj) as Action<object>);
                            Console.WriteLine($"发现构造后置监听器类型: {type.FullName}");
                        }
                    }
                    if (isDIListener)
                    {
                        if (x.HasAttributeRecursive<PreInjectAttribute>())
                        {
                            obj = obj != null ? obj : NewObject(type);
                            cachedPreInjectCallbacks.Add(x.CreateDelegate(typeof(Action<object>), obj) as Action<object>);
                            Console.WriteLine($"发现注入前置监听器类型: {type.FullName}");
                        }
                        if (x.HasAttributeRecursive<PostInjectAttribute>())
                        {
                            obj = obj != null ? obj : NewObject(type);
                            cachedPostInjectCallbacks.Add(x.CreateDelegate(typeof(Action<object>), obj) as Action<object>);
                            Console.WriteLine($"发现注入后置监听器类型: {type.FullName}");
                        }
                    }
                });
        }

        private void BroadcastPreConstruct()
        {
            cachedPreConstructCallbacks.ForEach(x => x.Invoke());
        }

        private void BroadcastPostConstruct(object obj)
        {
            cachedPostConstructCallbacks.ForEach(x => x.Invoke(obj));
        }

        private void BroadcastPreInject(object obj)
        {
            cachedPreInjectCallbacks.ForEach(x => x.Invoke(obj));
        }

        private void BroadcastPostInject(object obj)
        {
            cachedPostInjectCallbacks.ForEach(x => x.Invoke(obj));
        }
        #endregion

    }
}
