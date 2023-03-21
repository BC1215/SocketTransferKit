using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using SocketTransferKit.Data;

namespace SocketTransferKit.Client
{
    /// <summary>
    /// Socket客户端
    /// </summary>
    public class SocketClient : IDisposable
    {
        private readonly ClientConfig _clientConfig;
        /// <summary>
        /// 
        /// </summary>
        public ClientConfig ClientConfig
        {
            get { return _clientConfig; }
        }
        /// <summary>
        /// 退出消息接收循环
        /// </summary>
        private bool _escapeReciveLoop;
        /// <summary>
        /// 退出连接保持循环
        /// </summary>
        private bool _escapeKeepConnected;

        /// <summary>
        /// 是否启用延迟计算
        /// </summary>
        private bool _delayTestEnabled;
        /// <summary>
        /// 是否启用延迟计算
        /// </summary>
        public bool DelayTestEnabled
        {
            get { return _delayTestEnabled; }
            set
            {
                //如果已经启用并且不需要更新值
                if (_delayTestEnabled && (_delayTestEnabled == value))
                {
                    return;//不作处理
                }
                //更新值
                _delayTestEnabled = value;
                //如果需要计算延迟
                if (_delayTestEnabled)
                {
                    //启动延迟计算线程
                    TestDelay();
                }
            }
        }

        /// <summary>
        /// 是否启用心跳机制保证连接
        /// </summary>
        private bool _isKeepConnection;
        /// <summary>
        /// 是否启用心跳机制保证连接
        /// </summary>
        public bool IsKeepConnection
        {
            get { return _isKeepConnection; }
            set { _isKeepConnection = value; }
        }

        private void TestDelay()
        {
            Action<object> act = delegate
            {
                while (true)
                {
                    try
                    {
                        //如果不计算了
                        if (!_delayTestEnabled)
                        {
                            break;//跳出
                        }
                        //获取延迟数据
                        GetDelay();
                        Thread.Sleep(2000);
                    }
                    catch (Exception ex)
                    {
                        DebugLog("获取延迟失败：" + ex.Message);
                    }
                }
            };
            ThreadPool.QueueUserWorkItem(new WaitCallback(act));
        }


        /// <summary>
        /// 与服务端的通信延迟（ms）
        /// </summary>
        private int _delay = -1;
        /// <summary>
        /// 与服务端的通信延迟（ms）
        /// </summary>
        public int Delay
        {
            get { return _delay; }
        }

        /// <summary>
        /// 服务器IP地址
        /// </summary>
        private IPAddress _ip;
        /// <summary>
        /// Socket客户端
        /// </summary>
        private Socket _clientSocket;

        public event ServerConnectedEventHandler ServerConnected;
        public event ServerConnectionLostEventHandler ServerConnectionLost;
        public event OnCommandArrivedEventHandler OnCommandArrived;
        public event OnDelayUpdateEventHandler OnDelayUpdate;
        ////数据头缓冲区
        //private readonly byte[] _headerBuffer = new byte[8];
        ////数据接收缓冲区
        //private readonly byte[] _receiveBodyBuffer = new byte[1024 * 100];

        /// <summary>
        /// 允许服务连接丢失事件发生
        /// 失去连接通知事件标识，防止重复下线
        /// </summary>
        private bool _triggerConnectionLostFlag = false;

        /// <summary>
        /// 构造方法
        /// </summary>
        /// <param name="clientConfig">客户端配置</param>
        public SocketClient(ClientConfig clientConfig)
        {
            _clientConfig = clientConfig;
            //KeepConnection();
        }

        /// <summary>
        /// 发送NTP协议，请求数据命令
        /// </summary>
        private void GetDelay()
        {
            //生成数据结构
            var ntpPackage = new NtpStruct { T1 = DateTime.Now };
            var jstrNtpPackage = JsonConvert.SerializeObject(ntpPackage);
            var command = new Command(CommandType.NtpRequestPackage) { Data = jstrNtpPackage };
            //发送命令
            SendCommand(command);
        }

        /// <summary>
        /// 发送命令
        /// </summary>
        /// <param name="command"></param>
        public Task<bool> SendCommand(ICommand command)
        {
            return Task.Run(() =>
            {
                try
                {
                    var b = command.ToStructByteArray();
                    _clientSocket.Send(b);
                    return true;
                }
                catch (Exception ex)
                {
                    DebugLog("XXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXX");
                    DebugLog("发送命令异常");
                    if (_clientSocket != null)
                    {
                        DebugLog("local：" + _clientSocket.LocalEndPoint);
                        DebugLog("remote：" + _clientSocket.RemoteEndPoint);
                    }
                    DebugLog(ex.Message);
                    DebugLog("XXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXX");
                    return false;
                }
            });
        }

        /// <summary>
        /// 测试连接可用性
        /// </summary>
        /// <returns></returns>
        public bool TestConnection()
        {
            return TestConnectionSelf();
        }

        private bool TestConnectionSelf()
        {
            try
            {
                if (_clientSocket != null && _clientSocket.Connected)
                {
                    //发送测试数据
                    var command = new Command(CommandType.ClientPing);
                    var b = command.ToStructByteArray();
                    _clientSocket.Send(b);
                    return true;
                }
                DebugLog("client ping");
                return false;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// 初始化Socket对象
        /// </summary>
        private void InitSocket()
        {
            if (_clientSocket != null)
                DisconnectSelf();

            _escapeReciveLoop = false;
            _ip = IPAddress.Parse(_clientConfig.RemoteIp);
            _clientSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp)
            {
                ReceiveBufferSize = 102400000,
                SendBufferSize = 102400000,
                SendTimeout = int.MaxValue,
                ReceiveTimeout = int.MaxValue
            };
        }

        /// <summary>
        /// 重新连接远程计算机,不关心心跳连接
        /// </summary>
        /// <returns></returns>
        private bool ConnectSelf()
        {
            try
            {
                //初始化Socket
                InitSocket();

                //尝试连接
                _clientSocket.Connect(new IPEndPoint(_ip, int.Parse(_clientConfig.RemotePort)));
                //发送信息交换命令
                SendCommand(new Command(CommandType.ClientHello) { Data = _clientConfig.ClientName });
                //启动接受循环
                ReceiveLoop();
                //重置下线通知标识（即又可以下线通知了）
                _triggerConnectionLostFlag = true;

                return true;
            }
            catch (Exception ex)
            {
                DisconnectSelf();

                DebugLog("XXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXX Connect Error");
                DebugLog("failed to connect " + _clientConfig.RemotePort);
                DebugLog(ex.Message);
                DebugLog(ex.StackTrace);

                DebugLog("XXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXX");

                return false;
            }
        }

        /// <summary>
        /// 重新连接远程计算机,关心心跳连接
        /// </summary>
        /// <returns></returns>
        public bool Connect(bool keepConnection = true)
        {
            bool result = ConnectSelf();

            //设置退出连接保持循环
            _escapeKeepConnected = false;
            //设置是否启用心跳机制保证连接
            _isKeepConnection = keepConnection;
            //设置启动心跳
            KeepConnection();

            return result;
        }

        /// <summary>
        /// 断开与远程的连接
        /// </summary>
        /// <returns></returns>
        public bool Disconnect()
        {
            //停止保持连接
            _escapeKeepConnected = true;
            return DisconnectSelf();
        }

        /// <summary>
        /// 退出消息接收循环时，要归位的属性
        /// </summary>
        /// <returns></returns>
        private bool DisconnectSelf()
        {
            try
            {
                //退出消息接收循环
                _escapeReciveLoop = true;
                if (_clientSocket != null)
                {
                    try
                    {
                        _clientSocket.Disconnect(false);
                        _clientSocket.Shutdown(SocketShutdown.Both);
                    }
                    catch (Exception)
                    {
                    }

                    _clientSocket.Close();
                    _clientSocket.Dispose();
                    _clientSocket = null;
                }

                return true;
            }
            catch
            {
                _clientSocket = null;
                GC.Collect();
                return false;
            }
        }

        /// <summary>
        /// 消息休眠时间
        /// </summary>
        private int _threadSleepTimeSpan = 0;
        /// <summary>
        /// 设置消息休眠时间
        /// </summary>
        /// <param name="threadSleepTimeSpan"></param>
        public void SetMessageThreadSleepTimeSpan(int threadSleepTimeSpan)
        {
            _threadSleepTimeSpan = threadSleepTimeSpan;
        }

        /// <summary>
        /// 接收循环
        /// </summary>
        private void ReceiveLoop()
        {
            Console.WriteLine("enter receiveloop");
            //var receiveLeft = 0;
            //var bodyList = new List<byte>();
            //byte[] bodyReceiveBuffer = new byte[1024 * 100]; //设置接收缓冲区
            Action<object> act = delegate
            {
                while (true)
                {
                    if (_escapeReciveLoop)
                    {
                        _escapeReciveLoop = false;
                        break;
                    }
                    //Console.WriteLine("接受循环++++++++++++++++++");
                    try
                    {
                        #region try

                        if (_clientSocket == null || !_clientSocket.Connected)
                        {
                            continue;
                        }

                        //Thread.Sleep(10);
                        Thread.Sleep(_threadSleepTimeSpan);
                        //接收数据头
                        var headerData = ReceiveSpecialBytes(8);

                        //获取命令部分长度
                        var arrBodySeg = new ArraySegment<byte>(headerData.ToArray(), 4, 4);
                        var bodyLength = arrBodySeg.ToArray().ConvertByteArrayToInt();
                        if (bodyLength <= 0)
                        {
                            Debug.Print(bodyLength + " bad body length. body length less or equal than zero.");
                            continue;
                            throw new Exception(bodyLength + " bad body length. body length less or equal than zero.");
                        }
                        //接收数据体（命令）数据
                        if (bodyLength > (200 * 1024 * 1024))
                        {
                            throw new Exception();
                        }
                        var bodyData = ReceiveSpecialBytes(bodyLength);
                        //receiveLeft = bodyLength;
                        //int receiveLength = 0;
                        //bodyList.Clear();
                        //while (receiveLeft > 0)
                        //{
                        //    //如果接收缓冲区长度大于剩余数据长度
                        //    if (bodyReceiveBuffer.Length > receiveLeft)
                        //    {
                        //        //设置接收缓冲区长度为剩余数据大小
                        //        bodyReceiveBuffer = new byte[receiveLeft];
                        //    }
                        //    //接收数据
                        //    receiveLength = _clientSocket.Receive(bodyReceiveBuffer);
                        //    //将接收结果加入结果列表
                        //    bodyList.AddRange(bodyReceiveBuffer.Take(receiveLength));
                        //    //剩余数据大小-=刚接收的数据长度
                        //    receiveLeft -= receiveLength;
                        //}
                        //if (bodyReceiveBuffer.Length != _receiveBodyBuffer.Length)
                        //{
                        //    bodyReceiveBuffer = _receiveBodyBuffer;
                        //}
                        if (bodyLength != bodyData.Count)
                        {
                            //Debug.Print(receiveLength + " bad command. header bodylength not equals to actual received body length");
                            continue;
                            //throw new Exception(receiveLength + " bad command. header bodylength not equals to actual received body length");
                        }
                        //转换为命令
                        //var command = bodyBuffer.ToCommand();
                        var command = bodyData.ToArray().ToCommand();
                        //bodyBuffer = null;
                        //命令类型分治
                        switch (command.CommandType)
                        {
                            case CommandType.ServerWelcome:
                                if (ServerConnected != null)
                                {
                                    ServerConnected(this, command);
                                    DebugLog("连线事件触发");
                                }
                                break;
                            case CommandType.NtpReturnPackage:
                                var t4 = DateTime.Now;
                                var ntpPackage = JsonConvert.DeserializeObject<NtpStruct>(command.Data);
                                ntpPackage.T4 = t4;
                                var delay = (ntpPackage.T4 - ntpPackage.T1) - (ntpPackage.T3 - ntpPackage.T2);

                                _delay = (int)delay.TotalMilliseconds;
                                if (OnDelayUpdate != null)
                                {
                                    OnDelayUpdate(this, _delay);
                                }
                                break;
                            case CommandType.ServerShutdown:
                                DebugLog("receive server shutdown");
                                _clientSocket.Shutdown(SocketShutdown.Both);
                                _clientSocket.Close();
                                _clientSocket.Dispose();
                                break;
                            case CommandType.ServerPong:
                                DebugLog(_clientConfig.RemotePort + " : server pong");
                                break;
                            default:
                                if (OnCommandArrived != null)
                                {
                                    OnCommandArrived(this, command);
                                }
                                break;
                        }

                        #endregion
                    }
                    catch (Exception ex)
                    {
                        DisconnectSelf();

                        DebugLog("ReceiveLoop Error " + ex.Message);
                        DebugLog(ex.StackTrace);

                        break;
                    }
                }
            };
            ThreadPool.QueueUserWorkItem(new WaitCallback(act));
        }

        /// <summary>
        /// 从socket中接收指定长度的数据
        /// </summary>
        /// <param name="length"></param>
        private List<byte> ReceiveSpecialBytes(int length)
        {
            byte[] bodyReceiveBuffer = new byte[1024 * 100]; //设置接收缓冲区
            var receiveLeft = length;
            var bodyList = new List<byte>();
            while (receiveLeft > 0)
            {
                //如果接收缓冲区长度大于剩余数据长度
                if (bodyReceiveBuffer.Length > receiveLeft)
                {
                    //设置接收缓冲区长度为剩余数据大小
                    bodyReceiveBuffer = new byte[receiveLeft];
                }
                //接收数据
                var receiveLength = _clientSocket.Receive(bodyReceiveBuffer);
                //将接收结果加入结果列表
                bodyList.AddRange(bodyReceiveBuffer.Take(receiveLength));
                //剩余数据大小-=刚接收的数据长度
                receiveLeft -= receiveLength;
            }
            return bodyList;
        }

        /// <summary>
        /// 心跳休眠时间
        /// </summary>
        private int _heartSkipTimeSpan = 2000;
        /// <summary>
        /// 设置心跳休眠时间
        /// </summary>
        /// <param name="heartSkipTimeSpan"></param>
        public void SetHeartSkipTimeSpan(int heartSkipTimeSpan)
        {
            _heartSkipTimeSpan = heartSkipTimeSpan;
        }
        /// <summary>
        /// 检查连接状态
        /// 主要两个功能
        /// 1、下线事件通知
        /// 2、心跳功能
        /// </summary>
        private void KeepConnection()
        {
            Action<object> act = delegate
            {
                while (true)
                {
                    if (_escapeKeepConnected)
                    {
                        _escapeKeepConnected = false;
                        break;
                    }

                    //若是Socket为可连接状态
                    //命令检测连接，成功返回
                    if (TestConnectionSelf())
                    {
                        DebugLog("心跳检测连线成功 " + _clientConfig.RemotePort);
                        Thread.Sleep(_heartSkipTimeSpan);
                        continue;
                    }

                    //若命令检测连接，没有成功
                    //判断是否触发“断线”通知
                    if (_triggerConnectionLostFlag && ServerConnectionLost != null)
                    {
                        _triggerConnectionLostFlag = false;
                        ServerConnectionLost(this);
                        DebugLog("断线事件触发 " + _clientConfig.RemotePort);
                    }

                    //触发心跳
                    TiggerHeartSkip();
                    Thread.Sleep(_heartSkipTimeSpan);
                }
            };
            ThreadPool.QueueUserWorkItem(new WaitCallback(act));
        }

        /// <summary>
        /// 触发心跳
        /// </summary>
        private void TiggerHeartSkip()
        {
            //若是保持心跳，则尝试连接
            if (_isKeepConnection)
            {
                if (this.ConnectSelf())
                    DebugLog("心跳重连成功");
                else
                    DebugLog("心跳重连失败 " + _clientConfig.RemotePort);
            }
        }

        /// <summary>
        /// 日志的格式输出
        /// </summary>
        /// <param name="log"></param>
        private void DebugLog(string log)
        {
            var sb = new StringBuilder();
            sb.Append(DateTime.Now.ToString("O"));
            sb.Append(" ");
            sb.Append(_clientConfig.ClientName);
            sb.Append(" (");
            sb.Append(_clientConfig.RemoteIp);
            sb.Append(" :");
            sb.Append(_clientConfig.RemotePort);
            sb.Append(" )");
            sb.Append(" => ");
            sb.Append(log);
            //Console.WriteLine(sb.ToString());
            Debug.Print(sb.ToString());
        }

        /// <summary>
        /// 释放资源
        /// </summary>
        public void Dispose()
        {
            Disconnect();
        }
    }
}
