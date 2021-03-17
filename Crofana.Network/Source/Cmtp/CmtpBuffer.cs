using System;

namespace Crofana.Network.Cmtp
{
    internal class CmtpBuffer : ICmtpBuffer
    {
        #region Consts
        private const Int32 DEFAULT_BUFFER_SIZE = 4096;
        #endregion

        #region Fields
        Byte[] m_buffer;
        Int32 m_offset;
        #endregion

        #region Events
        public event Action<CmtpMessage> OnCmtpMessageReceived;
        #endregion

        #region Constructors
        public CmtpBuffer(Int32 size = DEFAULT_BUFFER_SIZE)
        {
            m_buffer = new byte[size];
        }
        #endregion

        #region Internal methods
        private void TryPopMessage()
        {
            Int32 headSize = 10;
            if (m_offset < headSize)
            {
                // incomplete CMTP header.
                return;
            }
            Int32 bodySize = (m_buffer[0] << 8) + m_buffer[1];
            Int32 messageSize = (headSize + bodySize);
            if (m_offset < messageSize)
            {
                // incomplete CMTP body.
                return;
            }
            Byte[] data = new Byte[messageSize];
            Array.Copy(m_buffer, data, messageSize);
            CmtpMessage message = new(data);
            Array.Copy(m_buffer, messageSize, m_buffer, 0, m_offset - messageSize);
            m_offset -= messageSize;
            OnCmtpMessageReceived?.Invoke(message);
        }
        #endregion

        #region ICmtpBuffer interface
        public void Write(Byte[] data)
        {
            if (m_buffer.Length - m_offset < data.Length)
            {
                throw new ApplicationException($"Cmtp buffer overflow! buffer size: {m_buffer.Length}, use: {m_offset}, require: {data.Length}.");
            }
            Array.Copy(data, 0, m_buffer, m_offset, data.Length);
            m_offset += data.Length;
            TryPopMessage();
        }
        #endregion
    }
}
