using System;
using Google.Protobuf;

namespace Crofana.Network
{
    public interface IController
    {
        Int32 StatusCode { get; set; }
        Cmtp.CmtpOpCode OpCode { get; }
        Type MessageType { get; }
        bool Response(IMessage msg);
    }
}
