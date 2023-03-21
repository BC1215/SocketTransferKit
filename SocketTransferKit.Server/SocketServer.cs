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
using Newtonsoft.Json;

namespace SocketTransferKit.Server
{
    public class SocketServer : AppServer<SocketSession, BinaryRequestInfo>
    {
        /// <summary>
        /// 命令到达事件
        /// </summary>
        public event OnCommandArrivedEventHandler OnCommandArrived;

        /// <summary>
        /// 不应直接实例化此类型，请使用TransferStarter
        /// </summary>
        public SocketServer()
            : base(new DefaultReceiveFilterFactory<MyReceiveFilter, BinaryRequestInfo>())
        {

        }

        /// <summary>
        /// 向当前所有已连接到此服务端的客户端发送命令
        /// </summary>
        /// <param name="command"></param>
        public void Broadcast(ICommand command)
        {
            try
            {
                foreach (var socketSession in GetAllSessions())
                {
                    socketSession.SendCommand(command);
                }
            }
            catch (Exception)
            {

            }
        }

        /// <summary>
        /// 触发命令到达事件
        /// </summary>
        /// <param name="clientSession"></param>
        /// <param name="command"></param>
        public void RaiseOnCommandArrivedEvent(SocketSession clientSession, ICommand command)
        {
            switch (command.CommandType)
            {
                case CommandType.ClientHello:
                    //更新客户端会话名称
                    clientSession.SessionClientName = command.Data;
                    break;
                case CommandType.NtpRequestPackage:
                    //ntp协议数据报处理
                    var t2 = DateTime.Now;
                    var ntpPackage = JsonConvert.DeserializeObject<NtpStruct>(command.Data);
                    ntpPackage.T2 = t2;
                    ntpPackage.T3 = DateTime.Now;
                    var rtnJstrNtpPackage = JsonConvert.SerializeObject(ntpPackage);
                    var ntpCommand = new Command(CommandType.NtpReturnPackage) { Data = rtnJstrNtpPackage };
                    clientSession.SendCommand(ntpCommand);
                    break;
                case CommandType.ClientPing:
                    //响应客户端的连接检测请求
                    var pongCommand = new Command(CommandType.ServerPong);
                    clientSession.SendCommand(pongCommand);
                    break;
                default:
                    //触发命令到达事件
                    if (OnCommandArrived != null)
                    {
                        OnCommandArrived(clientSession, command);
                    }
                    break;
            }
        }

        /// <summary>
        /// 新的客户端连接
        /// </summary>
        /// <param name="session"></param>
        protected override void OnNewSessionConnected(SocketSession session)
        {
            //设置回话的字符集
            session.Charset = Encoding.UTF8;
            base.OnNewSessionConnected(session);

            //发送欢迎信息
            var welcomeCommand = new Command(CommandType.ServerWelcome);

            //构建欢迎数据
            var sbWelcomeInfo = new StringBuilder();
            sbWelcomeInfo.Append("welcome to ");
            sbWelcomeInfo.Append(session.AppServer.Name);
            welcomeCommand.Data = sbWelcomeInfo.ToString();

            //发送欢迎命令
            session.SendCommand(welcomeCommand);
        }

    }
}
