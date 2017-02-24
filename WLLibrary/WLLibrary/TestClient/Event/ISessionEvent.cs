using System;
using System.Collections.Generic;
using System.Text;

namespace WL.SocketClient
{
    public interface ISessionEvent
    {
        event EventHandler<SessionEventArgs> eventSessionMonitor;//会话状态监控
        event EventHandler<SessionExceptionEventArgs> eventExceptionHandler;//异常事件
    }
}
