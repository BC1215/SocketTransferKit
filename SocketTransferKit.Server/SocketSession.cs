using SocketTransferKit.Data;
using SuperSocket.SocketBase;
using SuperSocket.SocketBase.Protocol;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SocketTransferKit.Server
{
    public class SocketSession : AppSession<SocketSession, BinaryRequestInfo>
    {
        private Mutex _sendMutex = new Mutex();
        private bool _clientReady2Receive = false;
        /// <summary>
        /// 会话的客户端名称
        /// </summary>
        public string SessionClientName { get; set; }

        /// <summary>
        /// 向客户端发送命令
        /// </summary>
        /// <param name="command"></param>
        public bool SendCommand(ICommand command)
        {
            try
            {
                var b = command.ToStructByteArray();

                //Console.WriteLine(command.CommandType + "数据包大小：" + b.Length);
                Send(b, 0, b.Length);
                return true;
            }
            catch
            {
                // ignored
                return false;
            }
        }

        /// <summary>
        /// 响应不支持的数据
        /// </summary>
        /// <param name="requestInfo"></param>
        protected override void HandleUnknownRequest(BinaryRequestInfo requestInfo)
        {
            base.HandleUnknownRequest(requestInfo);
            var sb = new StringBuilder();
            sb.Append("不支持的命令[");
            sb.Append(requestInfo.Key);
            sb.Append("]");
            var b = Encoding.UTF8.GetBytes(sb.ToString());
            base.Send(b, 0, b.Length);
            //Console.WriteLine(requestInfo.Body);
        }
    }
}
