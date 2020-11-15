using System;
using System.Text;

namespace Crofana.Extension
{
    public class ConstructorNotFoundException : Exception
    {

        private static string Format(params Type[] paramTypes)
        {
            if (paramTypes == null || paramTypes.Length == 0)
            {
                return "()";
            }
            StringBuilder sb = new StringBuilder();
            sb.Append("( ");
            foreach (var type in paramTypes)
            {
                sb.Append(type.FullName);
                sb.Append(", ");
            }
            sb.Remove(-2, 2);
            sb.Append(" )");
            return sb.ToString();
        }

        public ConstructorNotFoundException(Type targetType, params Type[] paramTypes) : base($"{targetType.FullName} 类型未提供指定形式的构造函数: {Format(paramTypes)}") { }
    }
}
