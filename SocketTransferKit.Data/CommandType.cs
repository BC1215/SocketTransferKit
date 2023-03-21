using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SocketTransferKit.Data
{
    /// <summary>
    /// 命令类型
    /// </summary>
    public enum CommandType
    {
        /// <summary>
        /// 服务端欢迎信息
        /// </summary>
        ServerWelcome = 0,
        /// <summary>
        /// 客户端握手信息
        /// </summary>
        ClientHello = 1,
        /// <summary>
        /// NTP协议请求数据报
        /// </summary>
        NtpRequestPackage = 3,
        /// <summary>
        /// NTP协议返回数据报
        /// </summary>
        NtpReturnPackage = 4,
        /// <summary>
        /// 更新计算程序字典
        /// </summary>
        UpdateTaskCalcDictionary = 5,
        /// <summary>
        /// 更新建模客户端字典
        /// </summary>
        UpdateAmpDictionary = 6,
        /// <summary>
        /// 计算程序异常信息
        /// </summary>
        TaskCalcExceptionMessage = 7,
        /// <summary>
        /// 计算程序日志信息
        /// </summary>
        TaskCalcLogMessage = 8,
        /// <summary>
        /// 更新计算时间（秒）
        /// </summary>
        UpdateCalcTime = 9,
        /// <summary>
        /// 更新仿真时间（秒）
        /// </summary>
        UpdateSimTime = 10,
        /// <summary>
        /// 服务器关机
        /// </summary>
        ServerShutdown = 11,
        /// <summary>
        /// 客户端连接状态探测
        /// </summary>
        ClientPing = 12,
        /// <summary>
        /// 服务端连接状态响应
        /// </summary>
        ServerPong = 13,
        /// <summary>
        /// 更新任务启动IC
        /// </summary>
        UpdateTaskSnapNum = 14,
        /// <summary>
        /// 客户端订阅页面数据
        /// </summary>
        SubscribePageData = 15,
        /// <summary>
        /// 客户端取消订阅页面数据
        /// </summary>
        UnsubscribePageData = 16,



        /// <summary>
        /// 实时数据库向中间程序订阅点数据
        /// </summary>
        DBSSubscribePointData = 200,

        /// <summary>
        /// 保存缓存Int数据
        /// </summary>
        SaveIntCache = 201,
        /// <summary>
        /// 保存缓存Double数据
        /// </summary>
        SaveDoubleCache = 202,

        /// <summary>
        /// 更新请求频率
        /// </summary>
        UpdateFreQuency = 203,

        /// <summary>
        /// 获取频率
        /// </summary>
        GetFreQuency=204,

        /// <summary>
        /// 订阅点数据完成
        /// </summary>
        DBSSubscribePointDataFinish = 205,
        /// <summary>
        /// 更新频率完成
        /// </summary>
        GetFreQuencyFiniSh = 206,

        /// <summary>
        /// 实时数据库派发频率
        /// </summary>
        DbDeliverInterVal=207,

        /// <summary>
        /// 实时数据库派送Double数据
        /// </summary>
        DBServiceDeliverDoubleData = 208,

        /// <summary>
        /// 实时数据库派送Int数据
        /// </summary>
        DBServiceDeliverIntData = 209,
        /// <summary>
        /// 实时数据库派发点名
        /// </summary>
        DBDeliverPoint = 210,
        /// <summary>
        /// 更新请求频率完成
        /// </summary>
        UpdateFreQuencyFinish=211,

        /// <summary>
        /// HDBS向DBS订阅点数据
        /// </summary>
        HDBSSubscribePointData = 400,

        /// <summary>
        /// HDBS取消向DBS订阅点数据
        /// </summary>
        HdbsUnsubscribePointData = 401,
       
        /// <summary>
        /// 历史曲线程序向历史数据库请求数据
        /// </summary>
        HDChartRequestData = 404,
        /// <summary>
        /// 历史数据库派发数据
        /// </summary>
        HDBServiceDeliverData = 405,


        /// <summary>
        /// 设备计算程序派发数据
        /// </summary>
        EqulcDeliverData=600,

        /// <summary>
        /// 发送教控台指令
        /// </summary>
        EqulcChangeCmd=601,

        /// <summary>
        /// 发送强制指令
        /// </summary>
        EqulcForceCmd=602,

       /// <summary>
        /// 干涉获取设备
        /// </summary>
        SCSInterventionCmd=603,
        /// <summary>
        /// 干涉获取设备回发
        /// </summary>

        SCSInterventionCallBackCmd=604
    }
}
