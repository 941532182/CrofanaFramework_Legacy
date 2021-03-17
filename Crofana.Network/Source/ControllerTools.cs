using System;
using System.Reflection;

namespace Crofana.Network
{
    public static class ControllerTools
    {
        public static void RegisterResponse(IController controller, Type messageType, Action<Int32, MethodInfo> registerResponseDelegate)
        {
            Type type = controller.GetType();
            MethodInfo[] methods = type.GetMethods(BindingFlags.NonPublic | BindingFlags.Static);
            Console.ForegroundColor = ConsoleColor.Green;
            Array.ForEach(methods, method =>
            {
                Type returnType = method.ReturnType;
                ParameterInfo[] parameters = method.GetParameters();
                RequestMappingAttribute attr = method.GetCustomAttribute<RequestMappingAttribute>();
                if (returnType == typeof(Boolean) &&
                    parameters.Length == 1 &&
                    parameters[0].ParameterType == messageType &&
                    attr != null)
                {
                    registerResponseDelegate.Invoke(attr.StatusCode, method);
                    //m_responses[attr.StatusCode] = method.CreateDelegate<System.Func<Test.Person, System.Boolean>>();
                    Console.WriteLine($"Response discovered: {type.FullName}::{method.Name}");
                }
            });
            Console.ResetColor();
        }
    }
}
