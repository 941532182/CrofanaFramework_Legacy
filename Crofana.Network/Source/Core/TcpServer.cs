using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace Crofana.Network.Core
{
    /// <summary>
    /// Internal class TCP server, user should use NetworkManager instead of TcpServer.
    /// </summary>
    internal class TcpServer : IDisposable
    {

        #region Consts
        private const Int32 OPS_TO_PRE_ALLOC = 2;
        private const Int32 DEFAULT_BUFFER_SIZE = 1024;
        #endregion

        #region Fields
        private Int32 m_currentConnectionCount;
        private UInt64 m_currentSocketHandleId;
        private Int32 m_maxConnectionCount;
        private Socket m_serverSocket;
        private Int32 m_bufferSize;
        private Semaphore m_connectionResource;
        private BufferPool m_bufferPool;
        private UInt64 m_totalBytesRead;
#if !DISABLE_EVENT_ARGS_POOL
        private EventArgsPool m_eventArgsPool;
#endif
        private IDictionary<SocketHandle, Socket> m_connectionMap;
        private Boolean m_disposed;
        #endregion

        #region Auto properties
        public Boolean IsRunning { get; private set; }
        public IPAddress Address { get; private set; }
        public Int32 Port { get; private set; }
        public Encoding Encoding { get; set; }

        #endregion

        #region Events
        public event Action<SocketHandle> OnConnectionEstablished;
        public event Action<SocketHandle, Byte[]> OnMessageReceived;
        #endregion

        #region Constructors
        public TcpServer(IPAddress ip, Int32 port, Int32 maxConn)
        {
            m_connectionMap = new ConcurrentDictionary<SocketHandle, Socket>();
            m_bufferSize = DEFAULT_BUFFER_SIZE;
            Address = ip;
            Port = port;
            m_maxConnectionCount = maxConn;
            //ServerSocket = new(Address.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
            m_bufferPool = new(OPS_TO_PRE_ALLOC * m_maxConnectionCount * m_bufferSize, m_bufferSize);
#if !DISABLE_EVENT_ARGS_POOL
            m_eventArgsPool = new(m_maxConnectionCount);
#endif
            m_connectionResource = new(m_maxConnectionCount, m_maxConnectionCount);
            Encoding = Encoding.Default;
        }
        #endregion

        #region Internal methods
        #region Convenient methods
        private SocketAsyncEventArgs GetEventArgs()
        {
#if !DISABLE_EVENT_ARGS_POOL
            SocketAsyncEventArgs e = m_eventArgsPool.Pop();
#else
            SocketAsyncEventArgs e = new();
#endif
            e.Completed += HandleIOCompleted;
            Int32 offset;
            Int32 size;
            Byte[] buffer = m_bufferPool.Pop(out offset, out size);
            e.SetBuffer(buffer, offset, size);
            return e;
        }

        private void ReleaseEventArgs(SocketAsyncEventArgs e)
        {
#if !DISABLE_EVENT_ARGS_POOL
            m_bufferPool.Push(e.Offset);
            e.SetBuffer(null, 0, 0);
            e.Completed -= HandleIOCompleted;
            m_eventArgsPool.Push(e);
#endif
        }

        private SocketHandle AddConnection(Socket sock)
        {
            SocketHandle handle = new(m_currentSocketHandleId++);
            m_connectionMap[handle] = sock;
            OnConnectionEstablished?.Invoke(handle);
            return handle;
        }
        #endregion

        #region Lifecycle
        private void Init()
        {
            m_bufferPool.Init();
#if !DISABLE_EVENT_ARGS_POOL
            m_eventArgsPool.Init();
#endif
        }
        #endregion

        #region Accept
        private void StartAccept(SocketAsyncEventArgs e)
        {
            if (e is null)
            {
                e = new();
                e.Completed += HandleAcceptCompleted;
            }
            else
            {
                e.AcceptSocket = null;
            }
            m_connectionResource.WaitOne();
            if (!m_serverSocket.AcceptAsync(e))
            {
                ProcessAccept(e);
            }
        }

        private void ProcessAccept(SocketAsyncEventArgs e)
        {
            if (e.SocketError == SocketError.Success)
            {
                Socket sock = e.AcceptSocket;
                if (sock.Connected)
                {
                    try
                    {
                        ///////////////////// CRITICAL SECTION START /////////////////////////
                        Interlocked.Increment(ref m_currentConnectionCount);
                        SocketAsyncEventArgs args = GetEventArgs();
                        args.UserToken = AddConnection(sock);
                        /////////////////////  CRITICAL SECTION END  /////////////////////////
                        if (!sock.ReceiveAsync(args))
                        {
                            ProcessReceive(args);
                        }
                    }
                    catch (SocketException ex)
                    {

                    }
                    StartAccept(e);
                }
            }
        }
        #endregion

        #region Send
        private void ProcessSend(SocketAsyncEventArgs e)
        {
            if (e.SocketError == SocketError.Success)
            {
                Socket sock = e.UserToken as Socket;
                // TODO
            }
            else
            {
                CloseConnection(e);
            }
        }
        #endregion

        #region Receive
        private void ProcessReceive(SocketAsyncEventArgs e)
        {
            if (e.BytesTransferred > 0 && e.SocketError == SocketError.Success)
            {
                SocketHandle handle = (SocketHandle)e.UserToken;
                Socket sock = GetSocket(handle);
                if (sock is null)
                {
                    throw new ApplicationException("Socket is null.");
                }
                Interlocked.Add(ref m_totalBytesRead, (UInt64)e.BytesTransferred);
                Byte[] data = new Byte[e.BytesTransferred];
                Array.Copy(e.Buffer, e.Offset, data, 0, data.Length);
                OnMessageReceived?.Invoke(handle, data);
                if (!sock.ReceiveAsync(e))
                {
                    ProcessReceive(e);
                }
            }
            else
            {
                CloseConnection(e);
            }
        }
        #endregion

        #region Dispose
        protected virtual void Dispose(bool disposing)
        {
            if (!m_disposed)
            {
                if (disposing)
                {
                    try
                    {
                        Stop();
                        if (m_serverSocket is not null)
                        {
                            m_serverSocket = null;
                        }
                    }
                    catch (SocketException ex)
                    {

                    }
                }
                m_disposed = true;
            }
        }
        #endregion
        #endregion

        #region Callbacks
        private void HandleAcceptCompleted(Object sender, SocketAsyncEventArgs e) => ProcessAccept(e);

        private void HandleIOCompleted(Object sender, SocketAsyncEventArgs e)
        {
            switch (e.LastOperation)
            {
                case SocketAsyncOperation.Accept:
                    ProcessAccept(e);
                    break;
                case SocketAsyncOperation.Receive:
                    ProcessReceive(e);
                    break;
                default:
                    throw new ArgumentException("The last operation completed on the socket was not a receive or send.");
            }
        }
        #endregion

        #region IDisposable interface
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
        #endregion

        #region Public methods
        #region Lifecycle
        public void Start()
        {
            if (!IsRunning)
            {
                Init();
                IsRunning = true;
                IPEndPoint endPoint = new(Address, Port);
                m_serverSocket = new(endPoint.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
                if (endPoint.AddressFamily == AddressFamily.InterNetwork)
                {
                    m_serverSocket.Bind(endPoint);
                }
                else
                {
                    throw new ApplicationException($"Address family {endPoint.AddressFamily} is not supported.");
                }
                m_serverSocket.Listen(m_maxConnectionCount);
                StartAccept(null);
            }
        }

        public void Stop()
        {
            if (IsRunning)
            {
                m_serverSocket.Close();
                foreach (var pair in m_connectionMap)
                {
                    pair.Value.Shutdown(SocketShutdown.Send);
                }
                IsRunning = false;
            }
        }
        #endregion

        #region Send
        public void Send(SocketAsyncEventArgs e, Byte[] data)
        {
            if (e.SocketError == SocketError.Success)
            {
                Socket sock = e.AcceptSocket;
                if (sock.Connected)
                {
                    Array.Copy(data, 0, e.Buffer, 0, data.Length);
                    if (!sock.SendAsync(e))
                    {
                        ProcessSend(e);
                    }
                    else
                    {
                        CloseConnection(e);
                    }
                }
            }
        }

        public void Send(Socket sock, Byte[] data, Int32 offset, Int32 size, Int32 timeout)
        {
            sock.SendTimeout = 0;
            Int32 startTickCount = Environment.TickCount;
            Int32 sent = 0;
            do
            {
                if (Environment.TickCount > startTickCount + timeout)
                {
                    throw new ApplicationException("Timeout.");
                }
                try
                {
                    sent += sock.Send(data, offset + sent, size - sent, SocketFlags.None);
                }
                catch (SocketException ex)
                {
                    if (ex.SocketErrorCode == SocketError.WouldBlock ||
                        ex.SocketErrorCode == SocketError.IOPending ||
                        ex.SocketErrorCode == SocketError.NoBufferSpaceAvailable)
                    {
                        Thread.Sleep(30);
                    }
                    else
                    {
                        throw ex;
                    }
                }
            } while (sent < size);
        }
        #endregion

        #region Shutdown
        public void CloseConnection(SocketAsyncEventArgs e) => CloseConnection(e.UserToken as Socket, e);

        public void CloseConnection(Socket sock, SocketAsyncEventArgs e)
        {
            try
            {
                sock.Shutdown(SocketShutdown.Send);
            }
            catch (Exception ex)
            {

            }
            finally
            {
                sock.Close();
            }
            ///////////////////// CRITICAL SECTION START /////////////////////////
            Interlocked.Decrement(ref m_currentConnectionCount);
            m_connectionResource.Release();
            ReleaseEventArgs(e);
            /////////////////////  CRITICAL SECTION END  /////////////////////////
        }
        #endregion

        #region Socket
        public Socket GetSocket(SocketHandle handle)
        {
            if (m_connectionMap.ContainsKey(handle))
            {
                return m_connectionMap[handle];
            }
            return null;
        }
        #endregion
        #endregion

        #region Properties
        public UInt64 TotalBytesRead => m_totalBytesRead;
        #endregion

        #region Internal classes
        private class EventArgsPool
        {
            private const Int32 DEFAULT_CAPACITY = 8;
            private Int32 capacity;
            private Stack<SocketAsyncEventArgs> m_pool;
            public EventArgsPool(int capacity = DEFAULT_CAPACITY)
            {
                this.capacity = capacity;
                m_pool = new Stack<SocketAsyncEventArgs>(capacity);
            }
            public void Init()
            {
                for (Int32 i = 0; i < capacity; i++)
                {
                    m_pool.Push(new());
                }
            }
            public SocketAsyncEventArgs Pop()
            {
                if (m_pool.Count == 0)
                {
                    return new();
                }
                lock (m_pool)
                {
                    return m_pool.Pop();
                }
            }
            public void Push(SocketAsyncEventArgs item)
            {
                if (item is null)
                {
                    return;
                }
                lock (m_pool)
                {
                    m_pool.Push(item);
                }
            }
            public Int32 Count => m_pool.Count;
        }

        public class BufferPool
        {
            Int32 m_numBytes;
            Byte[] m_buffer;
            Stack<Int32> m_freeIndexPool;
            Int32 m_currentIndex;
            Int32 m_bufferSize;
            public BufferPool(Int32 totalBytes, Int32 bufferSize)
            {
                m_numBytes = totalBytes;
                m_currentIndex = 0;
                m_bufferSize = bufferSize;
                m_freeIndexPool = new();
            }
            public void Init()
            {
                m_buffer = new byte[m_numBytes];
            }
            public Byte[] Pop(out Int32 offset, out Int32 size)
            {
                if (m_freeIndexPool.Count > 0)
                {
                    (offset, size) = (m_freeIndexPool.Pop(), m_bufferSize);
                }
                else
                {
                    if ((m_numBytes - m_bufferSize) < m_currentIndex)
                    {
                        (offset, size) = (-1, -1);
                        return null;
                    }
                    (offset, size) = (m_currentIndex, m_bufferSize);
                    m_currentIndex += m_bufferSize;
                }
                return m_buffer;
            }
            public void Push(Int32 offset)
            {
                m_freeIndexPool.Push(offset);
            }
        }
        #endregion

    }
}
