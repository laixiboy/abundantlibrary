using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace WLLibrary
{
    public class RandomHandle
    {
        public static Random GenerateRandom()
        {
            return new Random(GenerateRandomSeed());
        }
        public static int RandGet(int max, Random rnd)
        {
            long rndRet = rnd.Next(Int32.MaxValue) % 1000 * 10000 + rnd.Next(Int32.MaxValue) % 10000;
            rndRet += (long)rnd.Next(Int32.MaxValue) % 100 * 10000000;
            return (Int32)(rndRet % max);
        }
        public static int GenerateRandomSeed()
        {
            byte[] bytes = new byte[4];
            System.Security.Cryptography.RNGCryptoServiceProvider rng = new System.Security.Cryptography.RNGCryptoServiceProvider();
            rng.GetBytes(bytes);
            return BitConverter.ToInt32(bytes, 0);
        }

        /// <summary>
        /// @brief:从T的集合中随机一项
        /// </summary>
        /// <param name="rnd"></param>
        /// <param name="weights">Key:参与随机的对象 value:权重</param>
        /// <returns></returns>
        public static T RandomFromArray<T>(Random rnd, Dictionary<T, int> weights)
        {
            int weightTotal = 0;
            foreach (KeyValuePair<T, int> pair in weights)
            {
                weightTotal += pair.Value;
            }

            int weightRnd = RandGet(weightTotal, rnd);// rnd.Next(weightTotal);
            T ret = default(T);
            foreach (KeyValuePair<T, int> pair in weights)
            {
                if (weightRnd < pair.Value)
                {
                    ret = pair.Key;
                    break;
                }
                else
                {
                    weightRnd -= pair.Value;
                }
            }

            return ret;
        }

        /// <summary>
        /// @brief:从T的集合中随机一项
        /// </summary>
        /// <param name="rnd"></param>
        /// <param name="weights">Key:参与随机的对象 value:权重</param>
        /// <returns></returns>
        public static int RandomFromArray(Random rnd, List<int> weights)
        {
            int weightTotal = 0;
            foreach (int value in weights)
            {
                weightTotal += value;
            }

            int ret = 0;
            int weightRnd = RandGet(weightTotal, rnd);// rnd.Next(weightTotal);
            for (int i = 0; i < weights.Count;i++ )
            {
                if (weightRnd < weights[i])
                {
                    ret = i;
                    break;
                }
                else
                {
                    weightRnd -= weights[i];
                }
            }

            return ret;
        }
    }
}
