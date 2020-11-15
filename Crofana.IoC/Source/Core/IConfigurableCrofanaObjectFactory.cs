using System;
using System.Collections.Generic;
using System.Text;

namespace Crofana.IoC
{
    public interface IConfigurableCrofanaObjectFactory : ICrofanaObjectFactory
    {
        string GetConfigPath(Type type);
    }
}
