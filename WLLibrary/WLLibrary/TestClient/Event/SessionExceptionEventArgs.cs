using System;
using System.Collections.Generic;
using System.Text;

namespace WL.SocketClient
{
    /// <summary>
    /// 会话异常事件参数类
    /// </summary>
    public class SessionExceptionEventArgs:SessionEventArgs
    {
        private string _exceptionmessage = string.Empty;

        public SessionExceptionEventArgs(string thirdPartyID, Exception ex)
            : base(thirdPartyID, ex.ToString())
        {
            _exceptionmessage = ex.ToString();
        }

        public string ExceptionMessage
        {
            get { return _exceptionmessage; }
        }
    }
}
