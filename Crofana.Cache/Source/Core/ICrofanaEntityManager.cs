using System;

namespace Crofana.Cache
{
    public interface ICrofanaEntityManager
    {
        object GetEntity(Type type, long primaryKey);
        T GetEntity<T>(long primaryKey) where T : class;
        void AddEntity(object obj);
        bool RemoveEntity(Type type, long primaryKey);
        bool RemoveEntity<T>(long primaryKey) where T : class;
    }
}
