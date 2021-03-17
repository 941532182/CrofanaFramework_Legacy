using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using Crofana.CmtpCompiler;

Array.ForEach(args, arg => Console.WriteLine(arg));

Dictionary<String, Module> moduleMap = new();

// build compilation elements
foreach (var path in Directory.GetFiles("Cmtp/"))
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

// call protoc.exe

// generate controllers

Console.WriteLine("完成，按任意键退出...");
Console.ReadKey();