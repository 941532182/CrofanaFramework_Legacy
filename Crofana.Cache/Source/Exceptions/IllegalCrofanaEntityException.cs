using System;

namespace Crofana.Cache
{
    public class IllegalCrofanaEntityException : Exception
    {
        public static string ReasonToString(IllegalReason reason)
        {
            switch (reason)
            {
                case IllegalReason.NoPrimaryKey:
                    return "没有标记主键";
                default:
                    return "";
            }
        }
        public IllegalCrofanaEntityException(Type type, IllegalReason reason) : base($"{type.FullName} 类型不是合法的Crofana实体类，因为: {ReasonToString(reason)}") { }
        public enum IllegalReason
        {
            NoPrimaryKey = 0
        }
    }
}
