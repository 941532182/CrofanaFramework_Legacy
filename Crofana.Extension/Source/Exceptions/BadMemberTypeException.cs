using System;
using System.Reflection;

namespace Crofana.Extension
{
    class BadMemberTypeException : Exception
    {
        public BadMemberTypeException(MemberInfo member) : base($"{member.ReflectedType.FullName}.{member.Name} 的成员类型 {member.MemberType} 不符合条件") { }
    }
}
