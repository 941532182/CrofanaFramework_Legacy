using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace Crofana.Network.Cmtp
{
    using Core;

    /// <summary>
    /// Internal class CMTP server, user should use NetworkManager instead of CmtpServer.
    /// </summary>
    internal class CmtpServer
    {
        #region Fields
        private UInt64 m_currentSessionHandleId;
        private TcpServer m_server;
        private IDictionary<SessionHandle, Session> m_sessionMap;
        private IDictionary<SocketHandle, Session> m_socketHandle2Session;
        #endregion

        #region Events
        public event Action<SessionHandle> OnConnectionEstablished;
        public event Action<SessionHandle, CmtpMessage> OnCmtpMessageReceived;
        #endregion

        #region Constructors
        public CmtpServer(IPAddress ip, Int32 port, Int32 maxConn)
        {
            m_sessionMap = new ConcurrentDictionary<SessionHandle, Session>();
            m_socketHandle2Session = new ConcurrentDictionary<SocketHandle, Session>();
            m_server = new(ip, port, maxConn);
            m_server.OnConnectionEstablished += HandleConnectionEstablished;
            m_server.OnMessageReceived += HandleMessageReceived;
        }
        #endregion

        #region Internal methods
        #endregion

        #region Callbacks
        private void HandleConnectionEstablished(SocketHandle socketHandle)
        {
            Socket sock = m_server.GetSocket(socketHandle);
            SessionHandle sessionHandle = new(m_currentSessionHandleId++);
            Session session = new(sessionHandle, socketHandle);
            session.OnCmtpMessageReceived += HandleCmtpMessageReceived;
            m_sessionMap[sessionHandle] = session;
            m_socketHandle2Session[socketHandle] = session;
            OnConnectionEstablished?.Invoke(sessionHandle);
            Console.WriteLine($"Connect {sock.RemoteEndPoint}, id: {socketHandle}");
        }

        private void HandleMessageReceived(SocketHandle handle, Byte[] data)
        {
            String info = Encoding.Default.GetString(data);
            //Socket sock = m_server.GetSocket(handle);
            //Console.WriteLine($"Received message, client: {sock.RemoteEndPoint}, message: {info}, length: {data.Length}");
            Session session = m_socketHandle2Session.ContainsKey(handle) ? m_socketHandle2Session[handle] : null;
            if (session != null)
            {
                session.Write(data);
            }
            else
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"Session does not exist! SocketHandle: {handle}");
                Console.ResetColor();
            }
        }

        private void HandleCmtpMessageReceived(Session session, CmtpMessage msg)
        {
            OnCmtpMessageReceived?.Invoke(session.SessionHandle, msg);
        }
        #endregion

        #region Public methods
        public void Start()
        {
            m_server.Start();
        }
        #endregion

        #region Properties
        #endregion
    }
}
