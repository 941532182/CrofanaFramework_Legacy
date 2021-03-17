using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace Crofana.CmtpCompiler
{
    public static class XElementExtensions
    {
        public static Using ToUsing(this XElement self) => self.Name == "using" ? new(self.Attribute("name").Value) : null;
        public static EValue ToEValue(this XElement self) => self.Name == "evalue" ? new(self.Attribute("name").Value, self.Attribute("value").Value) : null;
        public static Field ToField(this XElement self) => self.Name == "field" ? new(self.Attribute("name").Value, self.Attribute("type").Value, self.Attribute("key")?.Value, self.Attribute("value")?.Value) : null;
        public static Enum ToEnum(this XElement self)
        {
            if (self.Name != "enum") return null;
            List<EValue> evalues = new();
            foreach (var element in self.Elements())
            {
                evalues.Add(element.ToEValue());
            }
            return new(self.Attribute("name").Value, evalues);
        }
        public static Struct ToStruct(this XElement self)
        {
            if (self.Name != "struct") return null;
            List<Field> fields = new();
            foreach (var element in self.Elements())
            {
                fields.Add(element.ToField());
            }
            return new(self.Attribute("name").Value, fields);
        }
        public static Rpc ToRpc(this XElement self)
        {
            if (self.Name != "rpc") return null;
            List<Field> fields = new();
            foreach (var element in self.Elements())
            {
                fields.Add(element.ToField());
            }
            return new(self.Attribute("name").Value, UInt32.Parse(self.Attribute("opcode").Value), fields);
        }
        public static Module ToModule(this XElement self)
        {
            if (self.Name != "module") return null;
            List<Using> usings = new();
            List<Enum> enums = new();
            List<Struct> structs = new();
            List<Rpc> rpcs = new();
            foreach (var element in self.Elements())
            {
                if (element.Name == "using")
                {
                    usings.Add(element.ToUsing());
                }
                else if (element.Name == "enum")
                {
                    enums.Add(element.ToEnum());
                }
                else if (element.Name == "struct")
                {
                    structs.Add(element.ToStruct());
                }
                else if (element.Name == "rpc")
                {
                    rpcs.Add(element.ToRpc());
                }
            }
            return new(self.Attribute("name").Value, usings, enums, structs, rpcs);
        }
    }
}
