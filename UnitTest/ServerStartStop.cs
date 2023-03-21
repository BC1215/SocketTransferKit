using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SocketTransferKit.Server;

namespace UnitTest
{
    [TestClass]
    public class ServerStartStop
    {
        readonly TransferServerStarter _transferServerStarter = new TransferServerStarter();
        /// <summary>
        /// 标准模式启停测试
        /// </summary>
        [TestMethod]
        public void server_normal_mode_start_stop()
        {
            Assert.AreNotEqual(_transferServerStarter.Start(), null);
            Assert.IsTrue(_transferServerStarter.Stop());
        }

        /// <summary>
        /// 兼容模式启停测试
        /// </summary>
        [TestMethod]
        public void server_special_mode_start_stop()
        {
            Assert.AreNotEqual(_transferServerStarter.Start(1), null);
            Assert.IsTrue(_transferServerStarter.Stop());
        }
    }
}
