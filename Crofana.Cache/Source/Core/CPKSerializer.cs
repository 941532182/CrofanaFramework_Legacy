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
        private static Type[] EMPTY_TYPE_ARRAY = { };

        private Dictionary<Type, Func<string, object>> customConverterMap = new Dictionary<Type, Func<string, object>>();

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
            WorkBook wb = new WorkBook(stream);
            WorkSheet ws1 = wb[0];
            WorkSheet ws2 = wb[1];
            string typeName = ws2["A1"];
            Assembly[] asms = AppDomain.CurrentDomain.GetAssemblies();
            Type type = asms.Select(x => x.GetType(typeName))
                            .Where(x => x != null)
                            .FirstOrDefault();
            IList<FieldInfo> fields = ws1[0].Select(x => type.GetField(x.Text)).ToList();
            for (int i = 1; i < ws1.Count; i++)
            {
                ulong primaryKey = ulong.Parse(ws1[i][0].Text);
                object entity = target.GetEntity(type, primaryKey);
                if (entity == null)
                {
                    entity = type.GetConstructor(EMPTY_TYPE_ARRAY).Invoke(null);
                    DeserializeEntity(type, fields, ws1[i], entity);
                    target.AddEntity(entity);
                }
                else
                {
                    DeserializeEntity(type, fields, ws1[i], entity);
                }
            }
        }

        private void DeserializeEntity(Type type, IList<FieldInfo> fields, Row row, object entity)
        {
            for (int i = 0; i < fields.Count; i++)
            {
                fields[i].SetValue(entity, CellToFieldValue(row[i], fields[i]));
            }
        }

        private object CellToFieldValue(Cell cell, FieldInfo field)
        {
            string text = cell.Text;
            Type type = field.FieldType;
            if (type.IsPrimitive)
            {
                return type.GetMethod("Parse", new Type[] { typeof(string) }).Invoke(null, new object[] { text });
            }
            else if (type.IsEnum)
            {
                return Enum.Parse(type, text, true);
            }
            else if (type == typeof(string))
            {
                return text;
            }
            else if (type.IsGenericType)
            {
                Type genericDef = type.GetGenericTypeDefinition();
                if (genericDef == typeof(IList<>))
                {
                    Type elementType = type.GetGenericArguments()[0];
                    string[] split = text.Split(',');
                    Type finalType = typeof(List<>).MakeGenericType(elementType);
                    object list = finalType.GetConstructor(EMPTY_TYPE_ARRAY).Invoke(null);
                    MethodInfo addMethod = finalType.GetMethod("Add");
                    if (elementType.IsPrimitive)
                    {
                        foreach (var x in split)
                        {
                            addMethod.Invoke(list, new object[] { elementType.GetMethod("Parse", new Type[] { typeof(string) }).Invoke(null, new object[] { x }) });
                        }
                    }
                    else if (elementType.IsEnum)
                    {
                        foreach (var x in split)
                        {
                            addMethod.Invoke(list, new object[] { Enum.Parse(elementType, x, true) });
                        }
                    }
                    else if (elementType == typeof(string))
                    {
                        foreach (var x in split)
                        {
                            addMethod.Invoke(list, new object[] { x });
                        }
                    }
                    else if (elementType.HasAttributeRecursive<CrofanaEntityAttribute>())
                    {

                    }
                    return list;
                }
                else if (genericDef == typeof(IDictionary<,>))
                {
                    Type[] genericTypes = type.GetGenericArguments();
                    Type keyType = genericTypes[0];
                    Type valueType = genericTypes[1];
                    string[] split = text.Split(',');
                    Type finalType = typeof(Dictionary<,>).MakeGenericType(keyType, valueType);
                    object dict = finalType.GetConstructor(EMPTY_TYPE_ARRAY).Invoke(null);
                    PropertyInfo indexer = finalType.GetProperty("Item");
                    if (keyType.IsPrimitive)
                    {
                        if (valueType.IsPrimitive)
                        {
                            foreach (var x in split)
                            {
                                string[] splitPair = x.Split(':');
                                object key = keyType.GetMethod("Parse", new Type[] { typeof(string) }).Invoke(null, new object[] { splitPair[0] });
                                object value = valueType.GetMethod("Parse", new Type[] { typeof(string) }).Invoke(null, new object[] { splitPair[1] });
                                indexer.SetValue(dict, value, new object[] { key });
                            }
                        }
                        else if (valueType.IsEnum)
                        {
                            foreach (var x in split)
                            {
                                string[] splitPair = x.Split(':');
                                object key = keyType.GetMethod("Parse", new Type[] { typeof(string) }).Invoke(null, new object[] { splitPair[0] });
                                object value = Enum.Parse(valueType, splitPair[1], true);
                                indexer.SetValue(dict, value, new object[] { key });
                            }
                        }
                        else if (valueType == typeof(string))
                        {
                            foreach (var x in split)
                            {
                                string[] splitPair = x.Split(':');
                                object key = keyType.GetMethod("Parse", new Type[] { typeof(string) }).Invoke(null, new object[] { splitPair[0] });
                                object value = splitPair[1];
                                indexer.SetValue(dict, value, new object[] { key });
                            }
                        }
                        else if (valueType.HasAttributeRecursive<CrofanaEntityAttribute>())
                        {

                        }
                    }
                    else if (keyType == typeof(string))
                    {
                        if (valueType.IsPrimitive)
                        {
                            foreach (var x in split)
                            {
                                string[] splitPair = x.Split(':');
                                object key = splitPair[0];
                                object value = valueType.GetMethod("Parse", new Type[] { typeof(string) }).Invoke(null, new object[] { splitPair[1] });
                                indexer.SetValue(dict, value, new object[] { key });
                            }
                        }
                        else if (valueType.IsEnum)
                        {
                            foreach (var x in split)
                            {
                                string[] splitPair = x.Split(':');
                                object key = splitPair[0];
                                object value = Enum.Parse(valueType, splitPair[1], true);
                                indexer.SetValue(dict, value, new object[] { key });
                            }
                        }
                        else if (valueType == typeof(string))
                        {
                            foreach (var x in split)
                            {
                                string[] splitPair = x.Split(':');
                                object key = splitPair[0];
                                object value = splitPair[1];
                                indexer.SetValue(dict, value, new object[] { key });
                            }
                        }
                        else if (valueType.HasAttributeRecursive<CrofanaEntityAttribute>())
                        {

                        }
                    }
                    return dict;
                }
            }
            else if (type.HasAttributeRecursive<CrofanaEntityAttribute>())
            {

            }
            return null;
        }

        private object Convert(Type type, string text, bool convertContainer)
        {
            if (customConverterMap.ContainsKey(type))
            {
                return customConverterMap[type].Invoke(text);
            }
            else if (type.IsEnum)
            {
                return Enum.Parse(type, text, true);
            }
            else if (type.HasAttributeRecursive<CrofanaEntityAttribute>())
            {
                return ConvertCrofanaEntity(type, text);
            }
            else if (convertContainer)
            {
                if (type.IsArray)
                {

                }
                if (type.IsGenericType)
                {
                    if (type.GetGenericTypeDefinition() == typeof(IList<>))
                    {

                    }
                    else if (type.GetGenericTypeDefinition() == typeof(IDictionary<,>))
                    {

                    }
                }
            }
            return null;
        }

        private object ConvertEnum(Type type, string text)
        {
            return Enum.Parse(type, text, true);
        }

        private object ConvertArray(Type elementType, string text)
        {
            return null;
        }

        private object ConvertIList(Type elementType, string text)
        {
            return null;
            string[] split = text.Split(',');
            Type finalType = typeof(List<>).MakeGenericType(elementType);
            object list = finalType.GetConstructor(EMPTY_TYPE_ARRAY).Invoke(null);
            MethodInfo addMethod = finalType.GetMethod("Add");
            if (customConverterMap.ContainsKey(elementType))
            {
                foreach (var element in split)
                {
                    addMethod.Invoke(list, new object[] { customConverterMap[elementType].Invoke(element) });
                }
            }
            else if (elementType.IsEnum)
            {
                foreach (var element in split)
                {
                    addMethod.Invoke(list, new object[] { Enum.Parse(elementType, element, true) });
                }
            }
            else if (elementType == typeof(string))
            {
                foreach (var x in split)
                {
                    addMethod.Invoke(list, new object[] { x });
                }
            }
            else if (elementType.HasAttributeRecursive<CrofanaEntityAttribute>())
            {

            }
            return list;
        }

        private object ConvertIDictionary(Type keyType, Type valueType, string text)
        {
            return null;
        }

        private object ConvertCrofanaEntity(Type type, string text)
        {
            return null;
        }

    }
}
