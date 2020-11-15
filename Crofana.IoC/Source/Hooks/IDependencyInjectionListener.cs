using System;
using System.Collections.Generic;
using System.Text;

namespace Crofana.IoC
{
    public interface IDependencyInjectionListener
    {
        void PreInject(object crofanaObject);
        void PostInject(object crofanaObject);
    }
}
