using System;

namespace Crofana.IoC
{
    /// <summary>
    /// 此类在实例化时将根据配置来进行初始化
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
    public class ConfigPathAttribute : Attribute
    {
        public string Path { get; }
        public ConfigPathAttribute(string path) => Path = path;
    }
}
