using System;

namespace Crofana.Network.Core
{
    /// <summary>
    /// A UInt64 wrapper represents a socket.
    /// </summary>
    internal struct SocketHandle
    {
        UInt64 id;
        public SocketHandle(UInt64 id) => this.id = id;
        public override string ToString() => id.ToString();
    }
}
