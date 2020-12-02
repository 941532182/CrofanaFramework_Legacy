using System;
using System.Collections.Generic;
using System.Text;

namespace Crofana.IoC
{
    public interface ICrofanaObjectFactory
    {
        object GetObject(Type type);
        T GetObject<T>() where T : class;
    }
}
