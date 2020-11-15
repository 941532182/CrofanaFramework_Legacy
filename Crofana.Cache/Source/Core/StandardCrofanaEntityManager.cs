using System;
using System.Collections.Generic;
using System.Reflection;
using System.Linq;

using Crofana.Extension;

namespace Crofana.Cache
{
    public class StandardCrofanaEntityManager : ICrofanaEntityManager
    {

        #region Statics
        #endregion

        #region Fields
        private Dictionary<Type, Dictionary<long, object>> entityMap = new Dictionary<Type, Dictionary<long, object>>();
        private Dictionary<Type, MemberInfo> cachedPKMemberMap = new Dictionary<Type, MemberInfo>();
        #endregion

        #region Constructors
        public StandardCrofanaEntityManager()
        {

        }
        #endregion

        #region ICrofanaEntityManager Interface
        public object GetEntity(Type type, long primaryKey)
        {
            if (entityMap.ContainsKey(type))
            {
                var typedEntityMap = entityMap[type];
                if (typedEntityMap.ContainsKey(primaryKey))
                {
                    return typedEntityMap[primaryKey];
                }
            }
            return null;
        }

        public T GetEntity<T>(long primaryKey) where T : class
        {
            return GetEntity(typeof(T), primaryKey) as T;
        }

        public void AddEntity(object obj)
        {
            Type type = obj.GetType();
            if (!entityMap.ContainsKey(type))
            {
                entityMap[type] = new Dictionary<long, object>();
                FieldInfo field = type.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)
                                      .Where(x => x.HasAttributeRecursive<PrimaryKeyAttribute>())
                                      .FirstOrDefault();
                if (field != null)
                {
                    cachedPKMemberMap[type] = field;
                    return;
                }
                PropertyInfo prop = type.GetProperties(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)
                                        .Where(x => x.HasAttributeRecursive<PrimaryKeyAttribute>())
                                        .FirstOrDefault();
                if (prop != null)
                {
                    cachedPKMemberMap[type] = prop;
                }
                throw new IllegalCrofanaEntityException(type, IllegalCrofanaEntityException.IllegalReason.NoPrimaryKey);
            }
            MemberInfo cachedPKMember = cachedPKMemberMap[type];
            if (cachedPKMember.MemberType == MemberTypes.Field)
            {
                FieldInfo field = cachedPKMember as FieldInfo;
                long pk = (long) field.GetValue(obj);
                entityMap[type][pk] = obj;
            }
            else
            {
                PropertyInfo prop = cachedPKMember as PropertyInfo;
                long pk = (long)prop.GetValue(obj);
                entityMap[type][pk] = obj;
            }
        }

        public bool RemoveEntity(Type type, long primaryKey)
        {
            if (entityMap.ContainsKey(type))
            {
                var typedEntityMap = entityMap[type];
                if (typedEntityMap.ContainsKey(primaryKey))
                {
                    typedEntityMap.Remove(primaryKey);
                    return true;
                }
            }
            return false;
        }

        public bool RemoveEntity<T>(long primaryKey) where T : class
        {
            return RemoveEntity(typeof(T), primaryKey);
        }
        #endregion

        #region Public Methods
        public void NewEntity()
        {

        }
        #endregion

        #region Private Methods
        #endregion

    }
}
