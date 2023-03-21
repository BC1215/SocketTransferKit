using System;
using System.Runtime.InteropServices;
using System.Text;

namespace SocketTransferKit.Data
{
    /// <summary>
    /// 命令
    /// </summary>
    [Serializable]
    public struct StructCommand : ICommand
    {
        private readonly DateTime _commandTime;
        private readonly CommandType _commandType;

        /// <summary>
        /// ctor
        /// </summary>
        /// <param name="commandType">命令类型</param>
        public StructCommand(CommandType commandType)
        {
            _commandType = commandType;
            _commandTime = DateTime.Now;
            _base64Data = string.Empty;
            byteData = null;
        }

        /// <summary>
        /// 命令类型
        /// </summary>
        public CommandType CommandType
        {
            get { return _commandType; }
        }

        /// <summary>
        /// 命令创建时间
        /// </summary>
        public DateTime CommandTime
        {
            get { return _commandTime; }
        }

        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 1024 * 1024)]
        private string _base64Data;
        /// <summary>
        /// 命令数据
        /// </summary>
        public string Data
        {
            get { return Encoding.UTF8.GetString(Convert.FromBase64String(_base64Data)); }
            set { _base64Data = Convert.ToBase64String(Encoding.UTF8.GetBytes(value)); }
        }

        #region ICommand 成员

        byte[] byteData;
        /// <summary>
        /// 数据字段--另一种数据类型
        /// </summary>
        public byte[] ByteData
        {
            get
            {
                return byteData;
            }
            set
            {
                byteData = value;
            }
        }

        #endregion
    }
}
