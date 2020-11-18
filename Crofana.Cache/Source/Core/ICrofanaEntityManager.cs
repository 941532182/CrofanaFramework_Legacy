using System;

namespace Crofana.Cache
{
    public interface ICrofanaEntityManager
    {
        object GetEntity(Type type, ulong primaryKey);
        T GetEntity<T>(ulong primaryKey) where T : class;
        void AddEntity(object obj);
        bool RemoveEntity(Type type, ulong primaryKey);
        bool RemoveEntity<T>(ulong primaryKey) where T : class;
    }
}
