using System;
using System.Collections.Generic;

using System.Text;
using System.Data;

namespace WL.SocketClient
{
    /// <summary>
    /// 会话事件参数类
    /// </summary>
    public class SessionEventArgs:EventArgs
    {
        string _strmessage = string.Empty;
        string _thirdPartyID = string.Empty;

        public SessionEventArgs(string thirdPartyID, string strMessage)
        {
            this._strmessage = strMessage;
            this._thirdPartyID = thirdPartyID;
        }

        public SessionEventArgs(string thirdPartyID, StringBuilder strMessage)
        {
            this._strmessage = strMessage.ToString();
            strMessage.Remove(0, strMessage.Length);
            this._thirdPartyID = thirdPartyID;
        }

        public String strMessage
        {
            get { return this._strmessage; }
        }

        public string thirdPartyID
        {
            get { return this._thirdPartyID; }
        }
    }

    public class GMServerOrderEventArgs : EventArgs
    {
        DataSet _extra = null;

        public GMServerOrderEventArgs(DataSet extra)
        {
            this._extra = extra;
        }

        public DataSet Extra
        {
            get { return this._extra; }
        }
    }

    public class GMServerOrderStatusEventArgs : EventArgs
    {
        private string _content = string.Empty;
        public string Content
        {
            get { return this._content; }
        }

        public GMServerOrderStatusEventArgs(string content)
        {
            this._content = content;
        }
    }
}
