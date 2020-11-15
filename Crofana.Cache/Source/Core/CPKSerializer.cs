using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Reflection;
using System.Linq;

using Crofana.Excel;

namespace Crofana.Cache
{
    public class CPKSerializer : ICrofanaEntitySerializer
    {
        private static Type[] EMPTY_TYPE_ARRAY = { };
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
                    target.AddEntity(entity);
                }
                DeserializeEntity(type, fields, ws1[i], entity);
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
            return null;
            /*string text = cell.Text;
            Type type = field.FieldType;
            if (type == typeof(bool))
            {

            }
            else if (type == typeof(byte))
            {

            }
            else if (type == typeof(sbyte))
            {

            }
            else if (type == typeof(ushort))
            {

            }
            else if (type == typeof(short))
            {

            }
            else if (type == typeof(uint))
            {

            }
            else if (type == typeof(int))
            {

            }
            else if (type == typeof(ulong))
            {

            }
            else if (type == typeof(long))
            {

            }
            else if (type == typeof(float))
            {

            }
            else if (type == typeof(double))
            {

            }
            else if (type == typeof(decimal))
            {

            }
            else if (type == typeof(char))
            {

            }
            else if (type.GetGenericTypeDefinition)
            {

            }
            else if (type.is*/
        }
    }
}
