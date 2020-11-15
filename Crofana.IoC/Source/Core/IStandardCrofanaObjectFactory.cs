using System;
using System.Collections.Generic;
using System.Text;

namespace Crofana.IoC
{
    interface IStandardCrofanaObjectFactory : IHierarchicalCrofanaObjectFactory, IAutowireableCrofanaObjectFactory, IConfigurableCrofanaObjectFactory, IScopedCrofanaObjectFactory { }
}
