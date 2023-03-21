using System;
using System.Globalization;
using SuperSocket.Common;
using SuperSocket.Facility.Protocol;
using SuperSocket.SocketBase.Protocol;
using SocketTransferKit.Data;

namespace SocketTransferKit.Server
{
    /// <summary>
    /// FixedHeaderReceiveFilter协议
    /// </summary>
    internal class MyReceiveFilter : FixedHeaderReceiveFilter<BinaryRequestInfo>
    {
        public MyReceiveFilter()
            : base(8) //前四字节标志命令类型，后四字节标志消息体长度
        {

        }
        /// <summary>
        /// 返回数据体长度
        /// </summary>
        /// <param name="header">头数据</param>
        /// <param name="offset">消息体长度数据位置偏移</param>
        /// <param name="length"></param>
        /// <returns></returns>
        protected override int GetBodyLengthFromHeader(byte[] header, int offset, int length)
        {
            return header.CloneRange(offset+4, 4).ConvertByteArrayToInt();
        }

        /// <summary>
        /// 解析数据
        /// </summary>
        /// <param name="header"></param>
        /// <param name="bodyBuffer"></param>
        /// <param name="offset"></param>
        /// <param name="length"></param>
        /// <returns></returns>
        protected override BinaryRequestInfo ResolveRequestInfo(ArraySegment<byte> header, byte[] bodyBuffer, int offset,
            int length)
        {
            //var commandTypeHash = header.CloneRange(0, 4).ConvertByteArrayToInt();
            //var commandType = (CommandType)Enum.Parse(typeof(CommandType), commandTypeHash.ToString(CultureInfo.InvariantCulture));

            //固定解析为CommandOut命令
            return new BinaryRequestInfo("CommandOut",
                bodyBuffer.CloneRange(offset, length));
        }
    }
}