using System;
using System.Collections.Generic;
using System.Text;
using System.Net.Sockets;
using System.Threading;
using System.Collections;
using System.Data;
using System.Net;
using System.IO;
using System.Windows.Forms;
using System.Security.Cryptography;
using TestClient;

namespace WL.SocketClient
{
    public class SocketClient
    {
        public FormMain parent = null;
        /// <summary>
        /// 事件
        /// </summary>
        public EventHandler<SessionEventArgs> eventPrintMessage = null;
        /// <summary>
        /// 实现接口的事件
        /// 异常事件
        /// </summary>
        public event EventHandler<SessionExceptionEventArgs> eventExceptionHandler;

        private DateTime _datDisconnectd = DateTime.Now.AddSeconds(-5.0);
        private DateTime _datConnectd = DateTime.Now;                                                           
        private TcpClient m_socketClient = null;
        private string _strIP = string.Empty;
        private int _nPort = 9633;
        private byte[] SendBuffer = null;
        private byte[] RecvBuffer = null;
        private byte[] ProcBuffer = null; 
        /**
         * 接受和发送缓冲区在解析和发送完之后都会移动数据到队列开始位置
         **/
        private int _nMaxSendSize = 0;
        private int _nTotalSndSize = 0;// 目前未发送的报文长度总和                                                 
        private DateTime datSendBegin = DateTime.Now;//命令的发送时间        
        private string _sendContent = "偷窥,吸食,毒品,吸毒食品,毛泽东,周恩来,刘少奇,朱德,彭德怀,林彪,刘伯承,陈毅,贺龙,聂荣臻,徐向前,罗荣桓,叶剑英,李大钊,陈独秀,孙中山,孙文,孙逸仙,邓小平,陈云,江泽民,李鹏,朱镕基,李瑞环,尉健行,李岚清,胡锦涛,罗干,温家宝,吴邦国,曾庆红,贾庆林,黄菊,吴官正,李长春,吴仪,回良玉,曾培炎,曹刚川,唐家璇,华建敏,陈至立,王乐泉,王刚,王兆国,刘淇,贺国强,郭伯雄,胡耀邦,王乐泉,王兆国,习近平,李克强,张德江,俞正声,刘云山,王岐山,张高丽,刘延东,彭丽媛,习大大,吴帮国,无帮国,无邦国,无帮过,瘟家宝,假庆林,甲庆林,假青林,离长春,习远平,袭近平,李磕墙,贺过墙,和锅枪,布什,布莱尔,小泉,纯一郎,萨马兰奇,安南,阿拉法特,普京,默克尔,克林顿,里根,尼克松,林肯,杜鲁门,赫鲁晓夫,列宁,斯大林,马克思,恩格斯,金正日,金日成,萨达姆,胡志明,西哈努克,希拉克,撒切尔,阿罗约,曼德拉,卡斯特罗,富兰克林,华盛顿,艾森豪威尔,拿破仑,亚历山大,路易,拉姆斯菲尔德,劳拉,鲍威尔,奥巴马,梅德韦杰夫,金正恩,安倍晋三,本拉登,奥马尔,柴玲,达赖,达赖喇嘛,江青,张春桥,姚文元,王洪文,东条英机,希特勒,墨索里尼,冈村秀树,冈村宁次,高丽朴,赵紫阳,王丹,沃尔开西,李洪志,李大师,赖昌星,马加爵,班禅,额尔德尼,山本五十六,阿扁,阿扁万岁,热那亚,薄熙来,周永康,王立军,令计划,默罕默德,徐才厚,粥永康,轴永康,肘永康,周健康,粥健康,周小康,陈良宇,李登辉,连战,宋楚瑜,吕秀莲,郁慕明,蒋介石,蒋中正,蒋经国,马英九,六四,六四运动,美国之音,密宗,民国,民进党,民运,民主,民主潮,摩门教,纳粹,南华早报,南蛮,明慧网,起义,亲民党,瘸腿帮,人民报,法轮功,法轮大法,打倒共产党,台独万岁,圣战,示威,台独,台独分子,台联,台湾民国,台湾岛国,台湾国,台湾独立,太子党,天安门事件,屠杀,小泉,新党,新疆独立,新疆分裂,新疆国,疆独,西藏独立,西藏分裂,西藏国,藏独,藏青会,藏妇会,学潮,学运,一党专政,一中一台,两个中国,一贯道,游行,圆满,造反,真善忍,镇压,政变,政治,政治反对派,政治犯,中共,共产党,反党,反共,政府,民主党,中国之春,转法轮,自焚,共党,共匪,苏家屯,基地组织,塔利班,东亚病夫,支那,高治联,高自联,专政,专制,四人帮,新闻管制,核工业基地,核武器,铀,原子弹,氢弹,导弹,核潜艇,大参考,小参考,国内动态清样,东突,东京热,宗教,迷信,全能神教,雪山狮子旗,道教,多维,佛教,佛祖,释迦牟尼,如来,阿弥陀佛,观世音,普贤,文殊,地藏,河殇,回教,密宗,摩门教,穆罕默德,穆斯林,升天,圣母,圣战,耶和华,耶稣,伊斯兰,真主安拉,白莲教,天主教,基督教,东正教,大法,法轮,法轮功,瘸腿帮,真理教,真善忍,转法轮,自焚,走向圆满,黄大仙,风水,跳大神,神汉,神婆,真理教,大卫教,阎王,黑白无常,牛头马面,藏独,高丽棒子,回回,疆独,蒙古鞑子,台独,台独分子,台联,台湾民国,西藏独立,新疆独立,南蛮,老毛子,回民吃猪肉,蒙古独立,全能神,谋杀,杀人,吸毒,贩毒,赌博,拐卖,走私,卖淫,造反,监狱,强奸,轮奸,抢劫,先奸后杀,击杀,下注,押大,押小,抽头,坐庄,赌马,赌球,筹码,老虎机,轮盘赌,安非他命,大麻,可卡因,海洛因,冰毒,摇头丸,杜冷丁,鸦片,罂粟,迷幻药,白粉,嗑药,卖枪支弹药,K粉,冰粉,枪支弹药,k粉,占领中环,屄,肏,屌,马的,马白勺,妈的,妈白勺,女马ㄉ,女马的,女马白勺,操你,操妳,操他,操人也,操她,操女也,干你,干妳,干他,干人也,干她,干女也,超你,超妳,超他,超人也,超她,超女也,屌你,屌我,屌妳,屌他,屌人也,屌她,屌女也,凸你,凸我,凸妳,凸他,凸人也,凸她,凸女也,插你,插他,插我,插她,插妳,臭你,臭妳,臭他,臭人也,臭她,臭女也,机八,鸡八,G八,Ｇ八,机巴,鸡巴,G巴,Ｇ巴,机叭,鸡叭,G叭,Ｇ叭,机芭,鸡芭,G芭,Ｇ芭,机掰,鸡掰,G掰,Ｇ掰,机Y,机Ｙ,鸡Y,鸡Ｙ,机8,鸡８,靠爸,靠母,哭爸,哭母,靠北,老GY,老ＧＹ,干GY,干ＧＹ,操GY,操ＧＹ,超GY,超ＧＹ,臭GY,臭ＧＹ,干七八,干78,干７８,操七八";

        public SocketClient(int nReceiveBuffSize,int nSendBuffSize,string strIP,int intPort)
        {
            if (nReceiveBuffSize > 0 && nSendBuffSize > 0)
            {
                _nMaxSendSize = nSendBuffSize;
                SendBuffer = new byte[nSendBuffSize];
                RecvBuffer = new byte[nReceiveBuffSize];
                ProcBuffer = new byte[nReceiveBuffSize];
                this._strIP = strIP.Trim();
                this._nPort = intPort;
            }
        }

        #region 处理事件的函数
        protected void OnSessionMonitorHandler(object sender, SessionEventArgs e)
        {
            if (this.eventPrintMessage != null)
            {
                this.eventPrintMessage(this, e);
            }
        }

        protected void OnSessionExceptionHandler(object sender, SessionExceptionEventArgs e)
        {
            if (this.eventExceptionHandler != null)
            {
                this.eventExceptionHandler(this, e);
            }
        }
        #endregion

        /// <summary>
        /// 是否可重连
        /// </summary>
        /// <returns></returns>
        public bool CanReconnect()
        {
            if (m_socketClient != null && m_socketClient.Client != null && m_socketClient.Connected)
                return false;
            else
                return DateTime.Now >= (_datDisconnectd.AddSeconds(1.0));
        }

        /// <summary>
        /// 客户端是否已经连接
        /// </summary>
        /// <returns></returns>
        public bool IsConnect()
        {
            ///  条件为：Socket已经连接+已经登陆验证完毕
            return (m_socketClient!=null && m_socketClient.Client!=null && m_socketClient.Connected);
        }

        /// <summary>
        /// 连接服务器
        /// </summary>
        public void ConnectServer()
        {
            StringBuilder logInfo = new StringBuilder(200);
            if (this.IsConnect())
            {
                this.CloseClientSocket();
                this.DisconnectServer();
                return;
            }

            if (this._strIP.Equals(string.Empty) || this._nPort < 1)
            {
                return;
            }

            try
            {
                _nTotalSndSize = 0;               
                m_socketClient = new TcpClient(this._strIP, this._nPort);
                m_socketClient.ReceiveTimeout = 60 * 1000;//一分钟
                if (m_socketClient.Connected)
                {
                    _datConnectd = DateTime.Now;
                    this.LoopAsyncReceive();/// 循环完，最多处理5个报文就结束（接受，发送都是），禁止启线程
                }
            }
            catch (Exception)
            {
                this.CloseClientSocket();
                this.DisconnectServer();
            }
        }

        /// <summary>
        /// 断开服务器
        /// </summary>
        public void DisconnectServer()
        {
            lock (this)
            {
                if (m_socketClient == null) return;

                try
                {
                    m_socketClient.Close();
                    this._datDisconnectd = DateTime.Now;
                }
                catch (Exception)
                {}
                finally
                {
                    m_socketClient = null;
                }
            }
        }

        /// <summary>
        /// 连接未重置状态下，清理客户端连接
        /// </summary>
        private void ClearConnect()
        {
            lock (this)
            {
                if (m_socketClient == null)
                {
                    return;
                }

                try
                {
                    m_socketClient.Close();
                }
                catch (Exception)
                {
                    
                }
                finally
                {
                    m_socketClient = null;
                }
            }
        }

        /// <summary>
        /// 关闭客户端核心Socket连接
        /// </summary>
        private void CloseClientSocket()
        {
            try
            {
                if (m_socketClient != null && m_socketClient.Client!=null)
                {
                    m_socketClient.Client.Shutdown(SocketShutdown.Both);
                    m_socketClient.Client.Close();
                }
            }
            catch (Exception)
            {

            }
            finally
            {
            }
        }

        /// <summary>
        /// 接受到报文的系统回调
        /// </summary>
        /// <param name="iar"></param>
        private void EndReceiveDatagram(IAsyncResult iar)
        {
            StringBuilder logInfo = new StringBuilder(100);
            try
            {
                if (m_socketClient != null && m_socketClient.Client != null && m_socketClient.Client.Connected)
                {
                    int readBytesLength = m_socketClient.Client.EndReceive(iar);
                    if (readBytesLength < 1)
                    {
                        this.CloseClientSocket();
                        this.DisconnectServer();
                    }
                    else
                    {
                        //收到数据
                    }
                }
                else
                {
                    this.CloseClientSocket();
                    this.DisconnectServer();
                }
            }
            catch(Exception) 
            {
                this.CloseClientSocket();
                this.DisconnectServer();
            }
        }

        private int ProcessSend()
        {
            while (_nTotalSndSize>0)
            {
                try
                {
                    if (this.m_socketClient.Client != null && this.m_socketClient.Client.Connected)
                    {
                        /// 客户端同步发送即可
                        int nRet = this.m_socketClient.Client.Send(SendBuffer, 0, _nTotalSndSize, SocketFlags.None);
                        if (nRet < 1)/// 消息阻塞，立刻断开
                        {
                            this.CloseClientSocket();
                            this.DisconnectServer();
                        }
                        else
                        {
                            Buffer.BlockCopy(SendBuffer,nRet,SendBuffer, 0, _nTotalSndSize - nRet);
                            _nTotalSndSize -= nRet;
                        }

                        return nRet;
                    }
                    else
                        break;
                }
                catch 
                {
                    break;
                }
            }

            return 0;
        }

        /// <summary>
        /// 启动异步接收
        /// </summary>
        private void LoopAsyncReceive()
        {
            try
            {
                if (this.m_socketClient.Client != null && this.m_socketClient.Client.Connected)
                {
                    /// 只取剩余空间的数据
                    this.m_socketClient.Client.BeginReceive(RecvBuffer,0,RecvBuffer.Length,SocketFlags.None, this.EndReceiveDatagram, null);
                }
            }
            catch
            {   
                this.CloseClientSocket();
                this.DisconnectServer();
            }
        }

        public int Send(string sendDataStr)
        {
            try
            {
                if (this.m_socketClient != null && this.m_socketClient.Connected)
                {
                    byte[] sendDataBytes = Encoding.UTF8.GetBytes(sendDataStr);

                    if (sendDataBytes.Length <= (_nMaxSendSize - _nTotalSndSize))
                    {
                        Buffer.BlockCopy(sendDataBytes, 0, this.SendBuffer, _nTotalSndSize, sendDataBytes.Length);
                        _nTotalSndSize = sendDataBytes.Length;

                        int sendRet = ProcessSend();
                        if (sendRet > 0)
                        {
                            datSendBegin = DateTime.Now;
                        }

                        return sendRet;
                    }
                    else
                    {
                        this.CloseClientSocket();
                        this.DisconnectServer();
                    }
                }
            }
            catch
            {
                this.CloseClientSocket();
                this.DisconnectServer();
            }

            return 0;
        }

        /// <summary>
        /// @brief:定时处理
        /// </summary>
        public void TimerHandle()
        {
            if (this.IsConnect())
            {        
                this.Send(this._sendContent);
            }
            else
            {
                if (this.CanReconnect())
                {
                    this.ConnectServer();
                }
            }
        }
    }
}
