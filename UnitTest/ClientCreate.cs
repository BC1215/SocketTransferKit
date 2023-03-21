using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SocketTransferKit.Client;

namespace UnitTest
{
    [TestClass]
    public class ClientCreate
    {
        readonly TransferClientStarter _transferClientStarter=new TransferClientStarter();
        [TestMethod]
        public void create_client_from_appconfig()
        {
            Assert.AreNotEqual(_transferClientStarter.Start(),null);
        }

        [TestMethod]
        public void create_client_from_config()
        {
            var configs=new List<ClientConfig>
            {
                new ClientConfig("127.0.0.1","46613","client1")
            };
            Assert.AreNotEqual(_transferClientStarter.Start(configs), null);
        }
    }
}
