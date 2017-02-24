#define PROFILE

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using WLLibrary.Log;

namespace WLLibrary.DataStructure
{
    /**
     * @brief：Min Heap's Element
     * @author:wolan mail:khyusj@163.com
     * @date:2014年9月12日
     **/
    public class MinHeapElement
    {
        public long Weight = 0;
        public int HeapIndex = 0;

        public MinHeapElement()
        {
            this.Weight = 0;
            this.HeapIndex = 0;
        }
    }

    /**
     * @brief：MinHeap For OnTimer Trigger Event
     * @author:wolan
     * @date:2014年9月12日
     **/
    public sealed class MinHeap<T> where T : MinHeapElement, new()
    {
        private const int HEAP0 = 1;

        //private object m_lock = null;
        private int _capacity;//当前容量
        private int _sizeUse;//当前使用的容量    
        private T[] _elements = null;//array index 0 is guaranteed to not be in-use at anytime

        /// <summary>
        /// @brief:返回当前容器中的有效元素个数
        /// </summary>
        public int Count
        {
            get { return _sizeUse-HEAP0; }
        }

        public MinHeap(int capacityInit)
        {
            //m_lock = new object();
            _capacity = capacityInit + HEAP0;
            _sizeUse = HEAP0;
            _elements = new T[_capacity];
        }

        public T Root
        {
            //lock (m_lock)
            get
            {
                if (_sizeUse > HEAP0)
                {
                    return _elements[HEAP0];
                }
                else
                    return default(T);
            }
        }

        /// <summary>
        /// 更改指定索引的负载
        /// </summary>
        /// <param name="nIndex">m_ThreadLoad's Index</param>
        /// <param name="newWeight">新权重</param>
        /// <returns></returns>
        public int ChangeWeight(int idxHeap, long newWeight)
        {
            //lock (m_lock)
            {
                int ret = 0;

                try
                {
                    _elements[idxHeap].Weight = newWeight;
  
                    if (idxHeap > HEAP0 && _elements[idxHeap].Weight <= _elements[idxHeap >> 1].Weight)
                        ret = UpHeap(idxHeap);
                    else
                        ret = DownHeap(idxHeap);
                }
                catch (Exception ex)
                {
                    LogEngine.Write(LOGTYPE.ERROR, "MinHeap::ChangeWeight(", idxHeap.ToString(), ",", newWeight.ToString(), "):", ex.Message);
                }

                return ret;
            }
        }

        public int Push(T t)
        {
            if (this._capacity <= this._sizeUse)
            {
                this._capacity <<= 1;//扩容

                T[] newElements = new T[this._capacity];
                this._elements.CopyTo(newElements, 0);
                this._elements = newElements;

#if PROFILE
                LogEngine.Write(LOGTYPE.INFO, "MinHeap Push:", this._capacity.ToString());
#endif
            }
            int idxNew = this._sizeUse++;
            this._elements[idxNew] = t;
            t.HeapIndex = idxNew;
            if (idxNew > HEAP0)
            {
                //TODO:临时加日志和校验，确定没有问题了，要去掉
                int ret = UpHeap(idxNew);
                if (ret < 1)
                {
                    LogEngine.Write(LOGTYPE.ERROR, "Push Fail");
                }

                return ret;
            }
            else
                return idxNew;
        }

        public bool PopRoot()
        {
            if (this._sizeUse <= HEAP0)
                return true;

            if (this._sizeUse - HEAP0 > 1)
            {
                this._elements[HEAP0] = this._elements[--this._sizeUse];
                if (DownHeap(HEAP0) < 1)
                {
                    //TODO:临时加日志和校验，确定没有问题了，要去掉
                    LogEngine.Write(LOGTYPE.ERROR, "PopRoot Fail");
                    return false;
                }
            }
            else
                this._sizeUse--;

            return true;
        }

        private int UpHeap(int idxHeap)
        {
            T t = _elements[idxHeap];

            for (; ; )
            {
                int nIdx_p = idxHeap >> 1;

                if (nIdx_p < 1 || _elements[nIdx_p].Weight <= t.Weight)
                    break;

                _elements[idxHeap] = _elements[nIdx_p];
                _elements[idxHeap].HeapIndex = idxHeap;
                idxHeap = nIdx_p;
            }

            _elements[idxHeap] = t;
            t.HeapIndex = idxHeap;

            return idxHeap;
        }

        private int DownHeap(int idxHeap)
        {
            T t = _elements[idxHeap];

            for (; ; )
            {
                int nIdx_c = idxHeap << 1;

                if (nIdx_c > _sizeUse - 1)
                    break;

                /// check 2 child nodes which's smaller
                nIdx_c +=
                    nIdx_c + 1 < _sizeUse
                    && _elements[nIdx_c].Weight > _elements[nIdx_c + 1].Weight
                    ? 1 : 0;

                if (t.Weight <= _elements[nIdx_c].Weight)
                    break;

                _elements[idxHeap] = _elements[nIdx_c];
                _elements[idxHeap].HeapIndex = idxHeap;
                idxHeap = nIdx_c;
            }

            _elements[idxHeap] = t;
            t.HeapIndex = idxHeap;

            return idxHeap;
        }
    }
}
