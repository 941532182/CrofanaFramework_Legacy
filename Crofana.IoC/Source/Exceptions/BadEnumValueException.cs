using System;

namespace Crofana.IoC
{
    public class BadEnumValueException : Exception
    {
        public BadEnumValueException(Enum @enum) : base(@enum.ToString()) { }
    }
}
