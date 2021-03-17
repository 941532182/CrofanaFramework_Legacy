using System;

namespace Crofana.Network
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
    public class ControllerAttribute : Attribute
    {
        public UInt32 OpCode { get; }
        public ControllerAttribute(UInt32 code) => OpCode = code;
    }
}
