using System;
using System.Collections.Generic;
using System.Text;

namespace Crofana.IoC
{
    public interface IAutowireableCrofanaObjectFactory : ICrofanaObjectFactory
    {
        void ProcessDependencyInjection(object crofanaObject);
    }
}
