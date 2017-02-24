using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using WLLibrary.DataStructure;

namespace WLLibrary.Communication
{
    public enum EventType : int
    {
        Event_None          = 0x00000000,
        Event_New           = 0x00000001,
        Event_Receive       = 0x00000002,
        Event_Send          = 0x00000004,
        /// <summary>
        /// 停止指定的通讯通道的收发
        /// </summary>
        Event_StopCommu     = 0x00000008,
        Event_SendComplet   = 0x00000100,
        /// <summary>
        /// 关闭指定的通讯通道
        /// </summary>
        Event_Close         = 0x00000200,
    }

    /// <summary>
    /// 断线原因
    /// </summary>
    public enum CommuBreak
    {
        SERVER_PROC_SLOW = 100,//Server端接收堵塞
        SERVER_SEND_EXPECT = 101,
        SERVER_ANALYSE_EXPECT = 102,
        SERVER_SEND_DATA_SIZE = 103,

        CLIENT = 200,// 客户端引发的断开
        CLIENT_RST = 201,//远端Rst  
        CLIENT_PROC_SLOW = 202,//客户端接收堵塞
        CLIENT_VALID_CONNECT = 203,//空连接，通常是连接建立后一定时间内没有任何数据上送
        CLIENT_DATAGRAM_TOOLONG = 204,//客户端数据过长

        S2S_SERVERID_REPEAT = 300,//RemoteServer登录时，ServerID重复
        S2S_HEART = 301,//Remote心跳超时
    }

    /// <summary>
    /// @brief:记录日志级别
    /// </summary>
    public enum LogLevel
    {
        NONE        =0,
        INFO        = 0x0001,
        DEGBUG      = 0x0002,
        ERROR       = 0x0004,
    }

    /*定期触发*/
    internal class OnTimer : MinHeapElement
    {
        public enum TimerHeapType
        {
            ONCE = 1,//只触发一次
            ALWAYS = 2,//一直触发
        }

        /*Private*/

        private TimerHeapType _type = TimerHeapType.ONCE;
        private int _spanMS = 0;//定时时长,单位:毫秒

        //到期后回调的操作
        private Action<object> _timeoutCallBack = null;

        /*Public*/

        public Action<object> TimeOutCallBack
        {
            get { return this._timeoutCallBack; }
        }

        public TimerHeapType Type
        {
            get { return this._type; }
        }

        public OnTimer()
        {

        }

        public OnTimer(TimerHeapType type, int spanMS, Action<object> callBack)
        {
            this._type = type;
            this._spanMS = spanMS;
            this._timeoutCallBack = callBack;
            base.Weight = DateTime.Now.Ticks + this._spanMS * 10000;
        }

        /// <summary>
        /// @brief:对于TimerHeapType是ALWAYS的，需要重置Weight才能继续加入到MinHeap中
        /// </summary>
        public long RefreshWeight()
        {
            return base.Weight = DateTime.Now.Ticks + this._spanMS * 10000;
        }
    }
}
