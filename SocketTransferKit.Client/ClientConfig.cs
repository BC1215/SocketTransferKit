using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace SocketTransferKit.Client
{
    public class ClientConfig
    {
        public string RemoteIp { get; set; }
        public string RemotePort { get; set; }
        public string ClientName { get; set; }

        private readonly Regex _regConfigSetSplitter = new Regex(";");
        private readonly Regex _regKeyValueSplitter = new Regex("=");

        /// <summary>
        /// 
        /// </summary>
        /// <param name="configString">配置字符串(例：<![CDATA[clientname=client1;remoteip=192.168.5.211;remoteport=2012]]>)</param>
        /// 
        public ClientConfig(string configString)
        {
            try
            {
                var arrConfigPair = _regConfigSetSplitter.Split(configString);
                //键值对个数检查
                if (arrConfigPair.Length != 3)
                {
                    throw new Exception("config string cannot be recognized");
                }
                foreach (string pair in arrConfigPair)
                {
                    var pairSplitted = _regKeyValueSplitter.Split(pair);
                    //键值对分割检查
                    if (pairSplitted.Length != 2)
                    {
                        throw new Exception("config pair cannot be recognized");
                    }
                    var key = pairSplitted[0];
                    var value = pairSplitted[1];
                    //配置赋值
                    switch (key.ToUpper())
                    {
                        case "REMOTEIP":
                            RemoteIp = value;
                            break;
                        case "REMOTEPORT":
                            RemotePort = value;
                            break;
                        case "CLIENTNAME":
                            ClientName = value;
                            break;
                        default:
                            throw new Exception("least one config pair can not be recognized");
                            break;
                    }
                }
            }
            catch (Exception)
            {
                throw;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="remoteIp">服务端IP</param>
        /// <param name="remotePort">服务端端口</param>
        /// <param name="clientName">客户端名称</param>
        public ClientConfig(string remoteIp, string remotePort, string clientName)
        {
            RemoteIp = remoteIp;
            RemotePort = remotePort;
            ClientName = clientName;
        }
    }
}
