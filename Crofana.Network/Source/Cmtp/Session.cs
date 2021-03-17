using System;

namespace Crofana.Network.Cmtp
{
    using Core;

    /// <summary>
    /// A UInt64 wrapper represents a session.
    /// </summary>
    public struct SessionHandle
    {
        UInt64 id;
        public SessionHandle(UInt64 id) => this.id = id;
        public override string ToString() => id.ToString();
    }

    /// <summary>
    /// Internal class session, user should use SessionHandle instead of Session.
    /// </summary>
    internal class Session
    {

        #region Fields
        private ICmtpBuffer m_buffer;
        #endregion

        #region Properties
        public SessionHandle SessionHandle { get; private set; }
        public SocketHandle SocketHandle { get; private set; }
        public Boolean Logined { get; private set; }
        #endregion

        #region Events
        public event Action<Session, CmtpMessage> OnCmtpMessageReceived;
        #endregion

        #region Constructors
        public Session(SessionHandle sessionHandle, SocketHandle socketHandle)
        {
            m_buffer = new CmtpBuffer();
            m_buffer.OnCmtpMessageReceived += HandleCmtpMessageReceived;
            SessionHandle = sessionHandle;
            SocketHandle = socketHandle;
        }
        #endregion

        #region Callbacks
        private void HandleCmtpMessageReceived(CmtpMessage msg)
        {
            //Console.WriteLine($"CMTP Message: {msg.GetBodyAsString()}, OpCode: {msg.OpCode}, From: {SocketHandle}");
            OnCmtpMessageReceived?.Invoke(this, msg);
        }
        #endregion

        #region Public methods
        public void Write(Byte[] data)
        {
            m_buffer.Write(data);
        }
        #endregion
    }
}
