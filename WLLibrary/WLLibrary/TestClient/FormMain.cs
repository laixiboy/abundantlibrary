using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using WL.SocketClient;
using System.Threading;
using System.Collections;
using System.Configuration;
using System.Diagnostics;
using System.IO;
using System.Text.RegularExpressions;
using System.Security.Cryptography;
using System.Net;
using WLLibrary.DataStructure;
using WLLibrary;

namespace TestClient
{
    /**
     * @func:压测机器人
     * @author:wl
     * @date:2013年11月8日
     **/ 
    public partial class FormMain : Form
    {
        public class SocketClientInfo:MinHeapElement
        {
            public Action timeOutCallBack = null;
            public Int32 timeSpan = 1;//操作间隔，单位秒
        }

        private Async_PrintMessage invokeSessionState = null;
        private delegate void Async_PrintMessage(string thirdPartyID, string strMessage);
        private string m_strIP = "192.168.1.12";
        private int m_nPort = 65432;                            
        private long m_lngRun = 0;/// 开始运行标记,标记线程是否已经运行
        private int m_nMinReceiveSize = 10240;
        private int m_nMinSendSize = 10240;
        
        private MinHeap<SocketClientInfo> _timerTrigger = null;

        private StringBuilder logPrintMessage = new StringBuilder(100);

        public FormMain()
        {
            InitializeComponent();

            if (ConfigurationManager.AppSettings.AllKeys.Contains("ip"))
            {
                this.m_strIP = ConfigurationManager.AppSettings["ip"].ToString().Trim();
            }

            if (ConfigurationManager.AppSettings.AllKeys.Contains("port"))
            {
                this.m_nPort = Convert.ToInt32(ConfigurationManager.AppSettings["port"]);
            }

            this.tb_IPPort.Text = this.m_strIP + ":" + this.m_nPort.ToString();
            invokeSessionState += this.PrintMessage;
        }

        private bool BeginConnect(int maxConnectNum)
        {
            this._timerTrigger = new MinHeap<SocketClientInfo>(maxConnectNum);
            string[] aryIPPort =  tb_IPPort.Text.Split(':');
            this.m_strIP = aryIPPort[0];
            this.m_nPort = Convert.ToInt32(aryIPPort[1]);

            for (int i = 0; i < maxConnectNum; i++)
            {
                SocketClient client = new SocketClient(3 * m_nMinReceiveSize, 2 * m_nMinSendSize, m_strIP, m_nPort);
                client.eventPrintMessage += new EventHandler<SessionEventArgs>(this.OnEventPrintMessage);
                client.parent = this;

                SocketClientInfo socketClientInfo = new SocketClientInfo();
                socketClientInfo.timeOutCallBack = client.TimerHandle;
                socketClientInfo.Weight = TimeHandle.ConvertDatetimeToSec(DateTime.Now) + socketClientInfo.timeSpan;
                this._timerTrigger.Push(socketClientInfo);
            }

            ThreadHandle.StartBackgroundThread(new ThreadStart(this.MainLoop), null);
            return true;
        }

        private void btn_Connect_Click(object sender, EventArgs e)
        {           
            if (Interlocked.Read(ref m_lngRun) > 0)
            {
                Interlocked.Exchange(ref m_lngRun, 0);               
                this.btn_Connect.Text = "(TCP)开 始";
            }
            else
            {
                int maxConnectNum = Convert.ToInt32(this.tb_ConnectNum.Text);
                if (BeginConnect(maxConnectNum))
                {
                    this.btn_Connect.Text = "(TCP)结 束";
                }
                Interlocked.Exchange(ref m_lngRun, 1);
            }
        }        
  
        private void OnEventPrintMessage(object sender, SessionEventArgs e)
        {
            try
            {
                IAsyncResult ar_SessionState = this.BeginInvoke(this.invokeSessionState, new object[] {e.thirdPartyID, e.strMessage.Trim() });
            }
            catch { }
        }

        private void PrintMessage(string thirdPartyID, string strMessage)
        {
            try
            {
                if (strMessage.Equals(string.Empty))
                    return;

                //带时间的命令不显示在单Session的交互LV中
                logPrintMessage.Append(DateTime.Now.ToString("mm:ss->")).Append(thirdPartyID).Append(":").Append(strMessage);
                this.lv_Status.Items.Insert(0, new ListViewItem(logPrintMessage.ToString()));
                logPrintMessage.Remove(0, logPrintMessage.Length);

                //最多20行
                if (this.lv_Status.Items.Count > 29)
                    this.lv_Status.Items.RemoveAt(this.lv_Status.Items.Count - 1);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
        }

        private void MainLoop()
        {
            while (Interlocked.Read(ref this.m_lngRun) > 0)
            {
                try
                {
                    UInt32 timestamp = TimeHandle.ConvertDatetimeToSec(DateTime.Now);
                    if (this._timerTrigger.Count > 0 && this._timerTrigger.Root.Weight <= timestamp)
                    {
                        this._timerTrigger.Root.timeOutCallBack();
                        this._timerTrigger.ChangeWeight(this._timerTrigger.Root.HeapIndex, timestamp + this._timerTrigger.Root.timeSpan);
                    }
                }
                catch
                {

                }
                finally
                {
                    Thread.Sleep(10);
                }
            }
        }
    }
}
