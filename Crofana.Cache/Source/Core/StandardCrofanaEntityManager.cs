using System;
using System.Collections.Generic;
using System.Reflection;
using System.Linq;
using System.IO;

using Crofana.Extension;

namespace Crofana.Cache
{
    public class StandardCrofanaEntityManager : ICrofanaEntityManager
    {

        #region Statics
        #endregion

        #region Fields
        private Dictionary<Type, Dictionary<ulong, object>> entityMap = new Dictionary<Type, Dictionary<ulong, object>>();
        private Dictionary<Type, MemberInfo> cachedPKMemberMap = new Dictionary<Type, MemberInfo>();
        #endregion

        #region Constructors
        public StandardCrofanaEntityManager()
        {

        }
        #endregion

        #region ICrofanaEntityManager Interface
        public object GetEntity(Type type, ulong primaryKey)
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

        public T GetEntity<T>(ulong primaryKey) where T : class
        {
            return GetEntity(typeof(T), primaryKey) as T;
        }

        public void AddEntity(object obj)
        {
            Type type = obj.GetType();
            if (!entityMap.ContainsKey(type))
            {
                entityMap[type] = new Dictionary<ulong, object>();
                FieldInfo field = type.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)
                                      .Where(x => x.HasAttributeRecursive<PrimaryKeyAttribute>())
                                      .FirstOrDefault();
                if (field != null)
                {
                    cachedPKMemberMap[type] = field;
                }
                else
                {
                    PropertyInfo prop = type.GetProperties(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)
                        .Where(x => x.HasAttributeRecursive<PrimaryKeyAttribute>())
                        .FirstOrDefault();
                    if (prop != null)
                    {
                        cachedPKMemberMap[type] = prop;
                    }
                    throw new IllegalCrofanaEntityException(type, IllegalCrofanaEntityException.IllegalReason.NoPrimaryKey);
                }
            }
            MemberInfo cachedPKMember = cachedPKMemberMap[type];
            if (cachedPKMember.MemberType == MemberTypes.Field)
            {
                FieldInfo field = cachedPKMember as FieldInfo;
                ulong pk = (ulong) field.GetValue(obj);
                entityMap[type][pk] = obj;
            }
            else
            {
                PropertyInfo prop = cachedPKMember as PropertyInfo;
                ulong pk = (ulong) prop.GetValue(obj);
                entityMap[type][pk] = obj;
            }
        }

        public bool RemoveEntity(Type type, ulong primaryKey)
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

        public bool RemoveEntity<T>(ulong primaryKey) where T : class
        {
            return RemoveEntity(typeof(T), primaryKey);
        }
        #endregion

        #region Public Methods
        public void Deserialize(ICrofanaEntitySerializer serializer, Stream stream)
        {
            serializer.Deserialize(stream, this);
        }

        public void NewEntity()
        {

        }
        #endregion

        #region Private Methods
        #endregion

    }
}
