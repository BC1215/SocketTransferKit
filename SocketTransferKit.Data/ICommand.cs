using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SocketTransferKit.Data
{
    /// <summary>
    /// 所有命令必须实现这个接口
    /// </summary>
    public interface ICommand
    {
        /// <summary>
        /// 命令类型
        /// </summary>
        CommandType CommandType { get; }

        /// <summary>
        /// 命令时间
        /// </summary>
        DateTime CommandTime { get; }

        /// <summary>
        /// 数据字段
        /// </summary>
        string Data { get; set; }
        /// <summary>
        /// 数据字段--另一种数据类型
        /// </summary>
        byte[] ByteData { get; set; }
    }
}
