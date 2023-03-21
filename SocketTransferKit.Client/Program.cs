using System.Diagnostics;
using System.Globalization;
using SocketTransferKit.Data;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SocketTransferKit.Client
{
    class Program
    {
        private static int _reciveCount;
        private static int _sendCount;
        private static bool _waitSendback;
        private static string _testData = new string('a', 1024 * 100);
        static Stopwatch _sw = new Stopwatch();
        static void Main(string[] args)
        {
            var transferStarter = new TransferClientStarter();
            var clients = transferStarter.Start();

            foreach (SocketClient socketClient in clients)
            {
                socketClient.ServerConnected += Program_ServerConnected;
                socketClient.ServerConnectionLost += Program_ServerConnectionLost;
                socketClient.OnCommandArrived += socketClient_OnCommandArrived;
                socketClient.OnDelayUpdate += socketClient_OnDelayUpdate;
                socketClient.DelayTestEnabled = false;
                socketClient.IsKeepConnection = false;
                socketClient.Connect();

                //socketClient.ClientConfig.ClientName;

                if (clients.Count <= 1)
                    Console.Title = socketClient.ClientConfig.ClientName; ;
            }

            //while (true)
            //{
            //    if (clients[0].TestConnection())
            //    {
            //        clients[0].Disconnect();
            //    }
            //    else
            //    {
            //        clients[0].Connect();
            //    }
            //    Console.ReadLine();
            //}
            Console.ReadLine();
            var send = true;
            Action<object> act = delegate
            {
                while (true)
                {
                    foreach (var socketClient in clients)
                    {
                        //Console.WriteLine("clients个数：" + clients.Count);
                        Console.WriteLine("{0} -> 发送第{1}个数据包", DateTime.Now.ToString("O"), ++_sendCount);

                        _waitSendback = true;
                        //socketClient.SendCommand(new Command(CommandType.UpdateAmpDictionary) { Data = _testData }).Wait();
                        socketClient.SendCommand(new Command(CommandType.UpdateAmpDictionary) { Data = string.Empty }).Wait();
                        while (_waitSendback)
                        {

                        }
                        //Thread.Sleep(2000);
                        //Console.ReadLine();
                    }
                    while (!send)
                    {

                    }
                }
            };

            ThreadPool.QueueUserWorkItem(new WaitCallback(act));

            while (true)
            {
                Console.ReadLine();
                send = !send;
            }
        }

        static void socketClient_OnDelayUpdate(SocketClient client, int delay)
        {
            Console.Title = delay + "ms";
        }

        static void socketClient_OnCommandArrived(SocketClient client, ICommand command)
        {
            Console.WriteLine("{0} -> 收到第{1}个回发数据包", DateTime.Now.ToString("O"), ++_reciveCount);
            Console.WriteLine("命令时间:{0}ms", (DateTime.Now - command.CommandTime).TotalMilliseconds);
            _waitSendback = false;
            //reciveCount++;
            ////Console.Title = reciveCount.ToString();
            //Console.Title = "2 client " + reciveCount + " packages " + sw.Elapsed.TotalSeconds.ToString("F7") + " s";
            //if (reciveCount == 2000)
            //{
            //    sw.Stop();
            //}
            //Console.WriteLine("收到回发数据,长度:" + command.ToStructByteArray().Length);
            //Console.WriteLine(command.CommandType + ":" + client.ClientConfig.ClientName + "date:" + DateTime.Now.Second.ToString("F7"));
            //Console.WriteLine((DateTime.Now - command.CommandTime).TotalSeconds);
            // Console.Write(command.Data);
        }

        static void Program_ServerConnectionLost(SocketClient client)
        {
            Console.WriteLine("Server connection lost");
        }

        static void Program_ServerConnected(SocketClient client, ICommand serverWelcome)
        {
            Console.WriteLine("Server connected:" + serverWelcome.Data);
        }
    }
}
