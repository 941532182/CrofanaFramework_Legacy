using System;
using System.Collections.Generic;
using System.Text;

namespace Crofana.IoC
{
    interface IAutowireableCrofanaObjectFactory : ICrofanaObjectFactory
    {
        void ProcessDependencyInjection(object crofanaObject);
    }
}
