using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SocketTransferKit.Client;
using SocketTransferKit.Data;
using SocketTransferKit.Server;

namespace UnitTest
{
    [TestClass]
    public class Benchmark
    {
        readonly TransferServerStarter _transferServerStarter = new TransferServerStarter();
        readonly TransferClientStarter _transferClientStarter = new TransferClientStarter();
        [TestMethod]
        public void Command10000x100KB()
        {
            try
            {
                if (_transferServerStarter.Start(1) == null)
                {
                    Assert.Fail("服务端启动失败");
                }
                var clients = _transferClientStarter.Start();
                if (clients == null)
                {
                    Assert.Fail("客户端创建失败");
                }
                var client = clients.First();
                if (!client.Connect())
                {
                    Assert.Fail("客户端连接失败");
                }
                var byteData = new byte[100 * 1024];
                var str = Encoding.UTF8.GetString(byteData);
                var command = new Command(CommandType.UpdateAmpDictionary) { Data = str };
                for (int i = 0; i < 10000; i++)
                {
                    Assert.IsTrue(client.SendCommand(command).Result);
                }
                if (!client.Disconnect())
                {
                    Assert.Fail("客户端断开连接失败");
                }
                if (!_transferServerStarter.Stop())
                {
                    Assert.Fail("服务端停止失败");
                }
            }
            catch (Exception ex)
            {
                Assert.Fail(ex.Message);
            }
        }

        [TestMethod]
        public void Client1000()
        {
            try
            {
                if (_transferServerStarter.Start(1) == null)
                {
                    Assert.Fail("服务端启动失败");
                }
                var configs = new List<ClientConfig>();
                for (int i = 0; i < 1000; i++)
                {
                    configs.Add(new ClientConfig("127.0.0.1", "46613", string.Format("client{0}", i)));
                }
                var clients = _transferClientStarter.Start(configs);
                if (clients == null)
                {
                    Assert.Fail("客户端创建失败");
                }
                foreach (var client in clients)
                {
                    if (!client.Connect())
                    {
                        Assert.Fail("客户端连接失败：{0}", client.ClientConfig.ClientName);
                    }
                }
                foreach (var client in clients)
                {
                    if (!client.Disconnect())
                    {
                        Assert.Fail("客户端断开连接失败");
                    }
                }
                if (!_transferServerStarter.Stop())
                {
                    Assert.Fail("服务端停止失败");
                }
            }
            catch (Exception ex)
            {
                Assert.Fail(ex.Message);
            }
        }
    }
}
