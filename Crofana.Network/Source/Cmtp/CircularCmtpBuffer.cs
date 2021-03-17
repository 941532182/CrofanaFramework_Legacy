using System;

namespace Crofana.Network.Cmtp
{
    internal class CircularCmtpBuffer : ICmtpBuffer
    {

        #region Consts
        private const Int32 DEFAULT_BUFFER_SIZE = 4096;
        #endregion

        #region Fields
        Byte[] m_buffer;
        #endregion

        #region Events
        public event Action<CmtpMessage> OnCmtpMessageReceived;
        #endregion

        #region Constructors
        public CircularCmtpBuffer(Int32 size = DEFAULT_BUFFER_SIZE)
        {
            throw new ApplicationException("This class is not supported currently.");
            m_buffer = new byte[size];
        }
        #endregion

        #region ICmtpBuffer interface
        public void Write(Byte[] data)
        {

        }
        #endregion

    }
}
