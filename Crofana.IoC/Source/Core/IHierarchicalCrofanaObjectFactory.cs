using System;
using System.Collections.Generic;
using System.Text;

namespace Crofana.IoC
{
    public interface IHierarchicalCrofanaObjectFactory : ICrofanaObjectFactory
    {
        ICrofanaObjectFactory Parent { get; }
    }
}
