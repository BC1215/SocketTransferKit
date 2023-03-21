using SocketTransferKit.Data;
using SuperSocket.SocketBase;
using SuperSocket.SocketEngine;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SocketTransferKit.Server
{
    class Program
    {
        private static int _packagesCount;
        private static int _reciveCount = 0;
        static void Main(string[] args)
        {
            //var a = Process.GetProcessesByName("devenv")[0];

            var transferServer = new TransferServerStarter();
            var servers = transferServer.Start(1,@"E:\BaiChen\209\公用\Socket通信\SocketTransferKit\App.config");
            Console.WriteLine("启动结果: {0}！", servers != null);
            if (servers != null)
            {
                foreach (SocketServer socketServer in servers)
                {
                    Console.WriteLine(socketServer.Name);
                    Console.WriteLine(socketServer.Config.Ip);
                    Console.WriteLine(socketServer.Config.Port);
                    socketServer.OnCommandArrived += socketServer_OnCommandArrived;
                    socketServer.NewSessionConnected += socketServer_NewSessionConnected;
                    socketServer.SessionClosed += socketServer_SessionClosed;
                }

                #region 注释

                //while (true)
                //{
                //    foreach (var socketServer in servers)
                //    {
                //        socketServer.Broadcast(new Command(CommandType.UpdateAMPDictionary) { Data = DateTime.Now.ToString("O") });
                //        Thread.Sleep(1000);
                //    }
                //}

                //Console.ReadLine();
                //var cmd = new Command(CommandType.ServerWelcome) {Data = "aaa"};
                //for (int i = 0; i < 10; i++)
                //{
                //    foreach (var socketServer in servers)
                //    {
                //        socketServer.Broadcast(cmd);
                //        Console.WriteLine(DateTime.Now.Second.ToString("F7"));
                //        Thread.Sleep(1000);
                //    }
                //}

                #endregion

                while (true)
                {
                    var keys = SocketSessions.Keys.ToList();
                    foreach (var key in keys)
                    {
                        if (SocketSessions.ContainsKey(key))
                        {
                            Console.WriteLine(key.SessionID + ":" + SocketSessions[key]);
                        }
                    }
                    Console.Title = "Server clients: " + SocketSessions.Count + " ,packages: " + _packagesCount;

                    Thread.Sleep(1000);
                    Console.Clear();
                }
                Console.WriteLine("按Q停止聊天服务器!");

                while (Console.ReadKey().KeyChar != 'q')
                {
                    Console.WriteLine();
                    continue;
                }

                Console.WriteLine();
                //Stop the appServer
                transferServer.Stop();

                Console.WriteLine("服务器已停止!");
                Console.ReadKey();
            }
            else
            {
                Console.WriteLine("启动失败!");
                Console.ReadKey();
            }
        }

        private static readonly Dictionary<SocketSession, int> SocketSessions = new Dictionary<SocketSession, int>();

        static void socketServer_SessionClosed(SocketSession session, CloseReason value)
        {
            SocketSessions.Remove(session);
            Console.WriteLine("client disconnected：" + session.LocalEndPoint);
        }

        static void socketServer_NewSessionConnected(SocketSession session)
        {
            SocketSessions.Add(session, 0);
            Console.WriteLine("client connected："+session.LocalEndPoint);
        }

        static void socketServer_OnCommandArrived(SocketSession clientSession, ICommand command)
        {
            SocketSessions[clientSession]++;
            clientSession.SendCommand(command);
            _packagesCount++;
            //Console.WriteLine("{0} -> 收到第{1}个数据包,已回发", DateTime.Now.ToString("O"), ++_reciveCount);
            //var serverName = clientSession.AppServer.Name;
            //Console.WriteLine(serverName + " returned the message " + command.Data + " from" +
            //                  clientSession.SessionClientName);
        }
    }
}
