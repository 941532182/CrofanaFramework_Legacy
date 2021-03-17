using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Reflection;
using System.Linq;
using System.Text.RegularExpressions;

using Crofana.Extension;
using Crofana.Excel;

namespace Crofana.Cache
{
    public class CPKSerializer : ICrofanaEntitySerializer
    {
        private static readonly Type[] EMPTY_TYPE_ARRAY = { };

        private Dictionary<Type, Func<string, object>> customConverterMap = new Dictionary<Type, Func<string, object>>();
        private ICrofanaEntityManager cachedTarget;

        public CPKSerializer()
        {
            customConverterMap[typeof(bool)] = x => bool.Parse(x);
            customConverterMap[typeof(byte)] = x => byte.Parse(x);
            customConverterMap[typeof(sbyte)] = x => sbyte.Parse(x);
            customConverterMap[typeof(ushort)] = x => ushort.Parse(x);
            customConverterMap[typeof(short)] = x => short.Parse(x);
            customConverterMap[typeof(uint)] = x => uint.Parse(x);
            customConverterMap[typeof(int)] = x => int.Parse(x);
            customConverterMap[typeof(ulong)] = x => ulong.Parse(x);
            customConverterMap[typeof(long)] = x => long.Parse(x);
            customConverterMap[typeof(float)] = x => float.Parse(x);
            customConverterMap[typeof(double)] = x => double.Parse(x);
            customConverterMap[typeof(decimal)] = x => decimal.Parse(x);
            customConverterMap[typeof(char)] = x => char.Parse(x);
            customConverterMap[typeof(string)] = x => x;
        }

        public void Deserialize(Stream stream, ICrofanaEntityManager target)
        {
            cachedTarget = target;
            WorkBook wb = new WorkBook(stream);
            WorkSheet ws1 = wb[0];
            WorkSheet ws2 = wb[1];
            string typeName = ws2["A1"];
            Assembly[] asms = AppDomain.CurrentDomain.GetAssemblies();
            Type type = asms.Select(x => x.GetType(typeName))
                            .Where(x => x != null)
                            .FirstOrDefault();
            IList<FieldInfo> fields = ws1[0].Select(x => type.GetField(x.Text)).ToList();
            IList<PropertyInfo> props = ws1[0].Select(x => type.GetProperty(x.Text)).ToList();
            for (int i = 1; i < ws1.Count; i++)
            {
                ulong primaryKey = ulong.Parse(ws1[i][0].Text);
                object entity = GetEntityInternal(type, primaryKey);
                DeserializeEntity(type, fields, ws1[i], entity);
                DeserializeEntity(type, props, ws1[i], entity);
            }
            cachedTarget = null;
        }

        private object GetEntityInternal(Type type, ulong primaryKey)
        {
            object entity = cachedTarget.GetEntity(type, primaryKey);
            if (entity == null)
            {
                entity = type.GetConstructor(EMPTY_TYPE_ARRAY).Invoke(null);
                FieldInfo primaryKeyField = type.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)
                                                .Where(x => x.HasAttributeRecursive<PrimaryKeyAttribute>())
                                                .FirstOrDefault();
                if (primaryKeyField != null)
                {
                    primaryKeyField.SetValue(entity, primaryKey);
                    cachedTarget.AddEntity(entity);
                    primaryKeyField.SetValue(entity, 0UL);    // used to see whether the entity is uninitialized
                }
                else
                {
                    PropertyInfo primaryKeyProp = type.GetProperties(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)
                                                      .Where(x => x.HasAttributeRecursive<PrimaryKeyAttribute>())
                                                      .FirstOrDefault();
                    if (primaryKeyProp != null)
                    {
                        primaryKeyProp.SetValue(entity, primaryKey);
                        cachedTarget.AddEntity(entity);
                        primaryKeyProp.SetValue(entity, 0UL);    // used to see whether the entity is uninitialized
                    }
                    else
                    {
                        throw new IllegalCrofanaEntityException(type, IllegalCrofanaEntityException.IllegalReason.NoPrimaryKey);
                    }
                }
            }
            return entity;
        }

        private void DeserializeEntity(Type type, IList<FieldInfo> fields, Row row, object entity)
        {
            for (int i = 0; i < fields.Count; i++)
            {
                fields[i]?.SetValue(entity, CellToFieldValue(row[i], fields[i]));
            }
        }

        private void DeserializeEntity(Type type, IList<PropertyInfo> props, Row row, object entity)
        {
            for (int i = 0; i < props.Count; i++)
            {
                props[i]?.SetValue(entity, CellToPropValue(row[i], props[i]));
            }
        }

        private object CellToFieldValue(Cell cell, FieldInfo field)
        {
            string text = cell.Text;
            Type type = field.FieldType;
            return Convert(type, text, true);
        }

        private object CellToPropValue(Cell cell, PropertyInfo prop)
        {
            string text = cell.Text;
            Type type = prop.PropertyType;
            return Convert(type, text, true);
        }

        private object Convert(Type type, string text, bool convertContainer)
        {
            if (text.Length == 0)
            {
                return Activator.CreateInstance(type);
            }
            else if (customConverterMap.ContainsKey(type))
            {
                return ConvertCustom(type, text);
            }
            else if (type.IsEnum)
            {
                return ConvertEnum(type, text);
            }
            else if (type.IsValueType || type.HasAttributeRecursive<CrofanaValueObject>())
            {
                return ConvertValueObject(type, text);
            }
            else if (type.HasAttributeRecursive<CrofanaEntityAttribute>())
            {
                return ConvertCrofanaEntity(type, text);
            }
            else if (convertContainer)
            {
                if (type.IsArray)
                {
                    return ConvertArray(type, text);
                }
                if (type.IsGenericType)
                {
                    if (type.GetGenericTypeDefinition() == typeof(IList<>))
                    {
                        return ConvertIList(type, text);
                    }
                    else if (type.GetGenericTypeDefinition() == typeof(IDictionary<,>))
                    {
                        return ConvertIDictionary(type, text);
                    }
                }
            }
            return null;
        }

        private object ConvertCustom(Type type, string text)
        {
            return customConverterMap[type].Invoke(text);
        }

        private object ConvertEnum(Type type, string text)
        {
            return Enum.Parse(type, text, true);
        }

        private object ConvertValueObject(Type type, string text)
        {
            string regex = "^\\{.*\\}$";
            if (!Regex.IsMatch(text, regex))
            {
                throw new FormatNotMatchException(text, regex);
            }

            text = text.Substring(1, text.Length - 2);
            object valueObject = Activator.CreateInstance(type);
            if (valueObject == null) return null;

            string[] split = text.Split('|');
            Array.ForEach(split, element =>
            {
                string[] splitElement = element.Split('=');
                FieldInfo field = type.GetField(splitElement[0]);
                if (field == null) return;
                object value = Convert(field.FieldType, splitElement[1], false);
                field.SetValue(valueObject, value);
            });

            return valueObject;
        }

        private object ConvertCrofanaEntity(Type type, string text)
        {
            if (text == null || text.Length == 0) return null;
            return GetEntityInternal(type, ulong.Parse(text));
        }

        private object ConvertArray(Type type, string text)
        {
            string regex = "^\\[.*\\]$";
            if (!Regex.IsMatch(text, regex))
            {
                throw new FormatNotMatchException(text, regex);
            }

            text = text.Substring(1, text.Length - 2);
            Type elementType = type.GetElementType();
            string[] split = text.Split(',');
            Array arr = Array.CreateInstance(elementType, split.Length);
            int index = 0;
            Array.ForEach(split, element => arr.SetValue(Convert(elementType, element, false), index++));
            return arr;
        }

        private object ConvertIList(Type type, string text)
        {
            string regex = "^\\[.*\\]$";
            if (!Regex.IsMatch(text, regex))
            {
                throw new FormatNotMatchException(text, regex);
            }

            text = text.Substring(1, text.Length - 2);
            Type elementType = type.GetGenericArguments()[0];
            string[] split = text.Split(',');
            Type finalType = typeof(List<>).MakeGenericType(elementType);
            object list = finalType.GetConstructor(EMPTY_TYPE_ARRAY).Invoke(null);
            MethodInfo addMethod = finalType.GetMethod("Add");
            Array.ForEach(split, element => addMethod.Invoke(list, new object[] { Convert(elementType, element, false) }));
            return list;
        }

        private object ConvertIDictionary(Type type, string text)
        {
            string regex = "^\\{.*\\}$";
            if (!Regex.IsMatch(text, regex))
            {
                throw new FormatNotMatchException(text, regex);
            }

            text = text.Substring(1, text.Length - 2);
            Type[] genericTypes = type.GetGenericArguments();
            Type keyType = genericTypes[0];
            Type valueType = genericTypes[1];
            string[] split = text.Split(',');
            Type finalType = typeof(Dictionary<,>).MakeGenericType(keyType, valueType);
            object dict = finalType.GetConstructor(EMPTY_TYPE_ARRAY).Invoke(null);
            PropertyInfo indexer = finalType.GetProperty("Item");
            Array.ForEach(split, element =>
            {
                string[] splitElement = element.Split(':');
                object key = ConvertCustom(keyType, splitElement[0]);
                object value = Convert(valueType, splitElement[1], false);
                indexer.SetValue(dict, value, new object[] { key });
            });
            return dict;
        }

    }
}
