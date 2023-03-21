using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SocketTransferKit.Data
{
    /// <summary>
    /// NTP协议数据结构
    /// </summary>
    [Serializable]
    public struct NtpStruct
    {
        /// <summary>
        /// 客户端NTP请求数据报发出时间
        /// </summary>
        public DateTime T1;
        /// <summary>
        /// 服务端接收到数据的时间
        /// </summary>
        public DateTime T2;
        /// <summary>
        /// 服务端处理完成发出NTP返回数据报的时间
        /// </summary>
        public DateTime T3;
        /// <summary>
        /// 客户端接收到数据的时间
        /// </summary>
        public DateTime T4;
    }
}
