using System;
using System.Text;

namespace Crofana.Network.Cmtp
{
    public struct CmtpOpCode
    {
        UInt32 code;
        public CmtpOpCode(UInt32 code) => this.code = code;
        public override String ToString() => code.ToString();
        public static implicit operator CmtpOpCode(UInt32 code) => new(code);
        public static implicit operator UInt32(CmtpOpCode code) => code.code;
    }

    internal class CmtpMessage
    {
        #region Auto properties
        public Byte[] Head { get; private set; }
        public Byte[] Body { get; private set; }
        #endregion

        #region Constructors
        public CmtpMessage(Byte[] msg)
        {
            Int32 headSize = 10;
            if (msg.Length < headSize)
            {
                throw new ApplicationException($"Invalid CMTP message, total size < {headSize}.");
            }
            Int32 bodySize = (msg[0] << 8) + msg[1];
            Int32 actualSize = msg.Length - headSize;
            if (bodySize != actualSize)
            {
                throw new ApplicationException($"Invalid CMTP message, head size info {bodySize} is not match actual size {actualSize}.");
            }
            Head = new Byte[headSize];
            Body = new Byte[bodySize];
            Array.Copy(msg, Head, headSize);
            Array.Copy(msg, headSize, Body, 0, bodySize);
        }
        #endregion

        #region Public methods
        public String GetBodyAsString() => Encoding.Default.GetString(Body);
        #endregion

        #region Properties
        public Int32 HeadSize => Head.Length;
        public Int32 BodySize => Body.Length;
        public Int32 TotalSize => HeadSize + BodySize;
        public CmtpOpCode OpCode => (UInt32)((Head[2] << 24) + (Head[3] << 16) + (Head[4] << 8) + Head[5]);
        #endregion
    }
}
