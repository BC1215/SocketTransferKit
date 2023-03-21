using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using SocketTransferKit.Data;

namespace SocketTransferKit.Server
{
    public static class ExtMethods
    {
        /// <summary>
        /// 向当前所有已连接到此服务端的客户端发送命令
        /// </summary>
        /// <param name="socketServer"></param>
        /// <param name="command"></param>
        [Obsolete]
        public static void Broadcast(this SocketServer socketServer,ICommand command)
        {
            
        }
    }
}
