using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Reflection;
using System.Linq;

using Crofana.Extension;
using Crofana.Excel;

namespace Crofana.Cache
{
    public class CPKSerializer : ICrofanaEntitySerializer
    {
        private static Type[] EMPTY_TYPE_ARRAY = { };

        private Dictionary<Type, Func<string, object>> converters = new Dictionary<Type, Func<string, object>>();

        public CPKSerializer()
        {
            converters[typeof(bool)] = x => bool.Parse(x);
            converters[typeof(byte)] = x => byte.Parse(x);
            converters[typeof(sbyte)] = x => sbyte.Parse(x);
            converters[typeof(ushort)] = x => ushort.Parse(x);
            converters[typeof(short)] = x => short.Parse(x);
            converters[typeof(uint)] = x => uint.Parse(x);
            converters[typeof(int)] = x => int.Parse(x);
            converters[typeof(ulong)] = x => ulong.Parse(x);
            converters[typeof(long)] = x => long.Parse(x);
            converters[typeof(float)] = x => float.Parse(x);
            converters[typeof(double)] = x => double.Parse(x);
            converters[typeof(decimal)] = x => decimal.Parse(x);
            converters[typeof(char)] = x => char.Parse(x);
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
            IList<FieldInfo> fields = ws1[1].Select(x => type.GetField(x.Text)).ToList();
            for (int i = 2; i < ws1.Count; i++)
            {
                long primaryKey = long.Parse(ws1[i][0].Text);
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
                fields[i].SetValue(entity, CellToFieldType(row[i], fields[i]));
            }
        }

        private object CellToFieldType(Cell cell, FieldInfo field)
        {
            string text = cell.Text;
            Type type = field.FieldType;
            if (type.IsPrimitive)
            {
                return type.GetMethod("Parse", new Type[] { typeof(string) }).Invoke(null, new object[] { text });
            }
            else if (type.IsSubclassOf(typeof(Enum)))
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

                }
                else if (genericDef == typeof(IDictionary<,>))
                {

                }
            }
            else if (type.HasAttributeRecursive<CrofanaEntityAttribute>())
            {

            }
            return null;
        }
    }
}
