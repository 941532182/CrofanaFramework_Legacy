using System;

namespace Crofana.Network.Cmtp
{
    internal interface ICmtpBuffer
    {
        /// <summary>
        /// Event fire when the buffer receives a complete CMTP message.
        /// </summary>
        event Action<CmtpMessage> OnCmtpMessageReceived;
        /// <summary>
        /// Writes data to the buffer.
        /// If there is any complete CMTP message, raise event OnCmtpMessageReceived.
        /// </summary>
        /// <param name="data">Data to write.</param>
        /// <returns></returns>
        void Write(Byte[] data);

    }
}
