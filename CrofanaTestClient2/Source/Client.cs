using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Linq;
using System.Threading;
using Google.Protobuf;


Socket sock = new(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
sock.Connect(IPAddress.Parse("127.0.0.1"), 1119);
while (true)
{
    Console.Write("用户名：");
    String account = Console.ReadLine();
    Console.Write("密码：");
    String password = Console.ReadLine();
    Login.CLogin login = new()
    {
        User = new()
        {
            Account = account,
            Password = password,
        },
    };
    Byte[] body = login.ToByteArray();
    UInt16 size = (UInt16)body.Length;
    UInt32 opcode = 1;
    Byte[] head = new Byte[10];
    head[0] = (Byte)(size >> 8);
    head[1] = (Byte)(size & 0xff);
    head[2] = (Byte)(opcode >> 24);
    head[3] = (Byte)((opcode >> 16) & 0xff);
    head[4] = (Byte)((opcode >> 8) & 0xff);
    head[5] = (Byte)(opcode & 0xff);
    sock.Send(head);
    sock.Send(body);
}