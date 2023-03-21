using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SocketTransferKit.Data;
using SuperSocket.SocketBase.Command;
using SuperSocket.SocketBase.Protocol;

namespace SocketTransferKit.Server
{
    /// <summary>
    /// 命令输出
    /// </summary>
    public class CommandOut : CommandBase<SocketSession, BinaryRequestInfo>
    {
        /// <summary>
        /// 命令输出命令执行
        /// </summary>
        /// <param name="session"></param>
        /// <param name="requestInfo"></param>
        public override void ExecuteCommand(SocketSession session, BinaryRequestInfo requestInfo)
        {
                //将数据转换为命令实体
                var iCommand = requestInfo.Body.ToCommand();
                //触发命令到达事件
                ((SocketServer)session.AppServer).RaiseOnCommandArrivedEvent(session, iCommand);
        }
    }

}
