using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Xml.Linq;
using Crofana.CmtpCompiler;

//Array.ForEach(args, arg => Console.WriteLine(arg));

Compiler compiler = new();
Dictionary<String, Module> moduleMap = new();

String rootPath = args[0].Replace('\\', '/');

// build compilation elements
foreach (var path in Directory.GetFiles($"{rootPath}/Cmtp"))
{
    String ext = Path.GetExtension(path);
    if (ext == ".cmtp")
    {
        Console.WriteLine(Path.GetFileName(path));
        XDocument xd = XDocument.Load(path);
        Module module = xd.Root.ToModule();
        if (module is not null)
        {
            moduleMap[module.Name] = module;
        }
    }
}

// generate proto files
foreach (var pair in moduleMap)
{
    String proto = compiler.CompileToProto(pair.Value);
    using (FileStream fs = new($"{rootPath}/Generated/Proto/{pair.Value.Name}.proto", FileMode.OpenOrCreate, FileAccess.Write))
    {
        fs.Write(Encoding.UTF8.GetBytes(proto));
        fs.Flush();
    }
}

// call protoc.exe
Process protoc = Process.Start("protoc.exe", new string[] { $"--csharp_out={rootPath}/Generated/Message", $"--proto_path={rootPath}/Generated/Proto", $"{rootPath}/Generated/Proto/*.proto" });
protoc.WaitForExit();

// generate controllers
foreach (var pair in moduleMap)
{
    Module module = pair.Value;
    module.Rpcs.ForEach(rpc =>
    {
        String controller = compiler.CompileToController(module, rpc);
        using (FileStream fs = new($"{rootPath}/Generated/Controller/{rpc.Name}Controller.gen.cs", FileMode.OpenOrCreate, FileAccess.Write))
        {
            fs.Write(Encoding.UTF8.GetBytes(controller));
            fs.Flush();
        }
    });
}

Console.WriteLine("完成，按任意键退出...");
Console.ReadKey();