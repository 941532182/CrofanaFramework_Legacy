using System;
using System.Collections.Generic;

namespace Crofana.CmtpCompiler
{
    public record CompilationElement(String Name);
    public record Using(String Name) : CompilationElement(Name);
    public record EValue(String Name, String Value) : CompilationElement(Name);
    public record Field(String Name, String Type, String Key, String Value) : CompilationElement(Name);
    public record Enum(String Name, List<EValue> EValues) : CompilationElement(Name);
    public record Struct(String Name, List<Field> Fields) : CompilationElement(Name);
    public record Rpc(String Name, UInt32 OpCode, List<Field> Fields) : CompilationElement(Name);
    public record Module(String Name, List<Using> Usings, List<Enum> Enums, List<Struct> Structs, List<Rpc> Rpcs) : CompilationElement(Name);
}
