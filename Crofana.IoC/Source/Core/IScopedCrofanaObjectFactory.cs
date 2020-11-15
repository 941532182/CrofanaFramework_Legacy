using System;
using System.Collections.Generic;
using System.Text;

namespace Crofana.IoC
{
    public interface IScopedCrofanaObjectFactory : ICrofanaObjectFactory
    {
        Scope GetScope(Type type);
    }
}
