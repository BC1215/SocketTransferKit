using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SocketTransferKit.Client
{
    /// <summary>
    /// 数据传输客户端启动器
    /// </summary>
    public class TransferClientStarter
    {
        //public List<SocketClient> AllSocketClient { get; set; }
        /// <summary>
        /// （通过配置文件）生成所有Socket客户端，如果全部成功：返回Socket客户端列表，如果任一失败：返回null
        /// </summary>
        /// <returns></returns>
        public List<SocketClient> Start()
        {
            try
            {
                //获取配置文件配置
                var clientConfigKeys = ConfigurationManager.AppSettings.AllKeys.Where(k => k.StartsWith("SocketClient")).ToList();

                var socketClients = new List<SocketClient>();
                //遍历配置
                foreach (string configKey in clientConfigKeys)
                {
                    var clientConfigValue = ConfigurationManager.AppSettings[configKey];//获取配置字符串
                    var clientConfig = new ClientConfig(clientConfigValue);//生成客户端配置
                    var socketClient = new SocketClient(clientConfig);//实例化客户端

                    socketClients.Add(socketClient);
                }

                return socketClients;
            }
            catch (Exception)
            {
                return null;
            }
        }

        /// <summary>
        /// （通过自定配置）生成所有Socket客户端，如果全部成功：返回Socket客户端列表，如果任一失败：返回null
        /// </summary>
        /// <param name="configs"></param>
        /// <returns></returns>
        public List<SocketClient> Start(IEnumerable<ClientConfig> configs)
        {
            try
            {
                var socketClients = new List<SocketClient>();
                //遍历配置
                foreach (var config in configs)
                {
                    //创建客户端
                    var socketClient = new SocketClient(config);
                    socketClients.Add(socketClient);
                }
                return socketClients;
            }
            catch (Exception)
            {
                return null;
            }
        }
    }
}
