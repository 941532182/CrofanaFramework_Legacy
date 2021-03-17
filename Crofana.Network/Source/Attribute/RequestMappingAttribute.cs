using System;

namespace Crofana.Network
{
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = false)]
    public class RequestMappingAttribute : Attribute
    {
        public Int32 StatusCode { get; }
        public RequestMappingAttribute() { }
        public RequestMappingAttribute(Int32 statusCode) => StatusCode = statusCode;
    }
}
