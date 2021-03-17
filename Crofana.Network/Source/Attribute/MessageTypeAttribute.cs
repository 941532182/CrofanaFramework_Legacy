using System;

namespace Crofana.Network
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
    public class MessageTypeAttribute : Attribute
    {
        public Type MessageType { get; }
        public MessageTypeAttribute(Type messageType) => MessageType = messageType;
    }
}
