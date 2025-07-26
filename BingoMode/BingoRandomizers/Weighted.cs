using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BingoMode.BingoRandomizer
{
    public class Weighted<T>(T value, int[] weights)
    {
        public static HashSet<Weighted<T>> usedWeighted = [];
        public T value = value;
        public int[] weights = weights;
        public string WeightsString
        {
            get
            {
                if (weights.Length == 1)
                    return weights[0].ToString();
                return $"[{string.Join(",", weights.Select(x => x.ToString()))}]";
            }
        }
        public int Weight
        {
            get
            {
                if (discarded)
                    return 0;
                return weights[weightIndex];
            }
        }
        private int weightIndex = 0;
        private bool discarded = false;

        public void Advance()
        {
            if (weightIndex + 1 < weights.Length)
            {
                weightIndex++;
                usedWeighted.Add(this);
            }
        }

        public void Discard()
        {
            Plugin.logger.LogInfo(value.ToString());
            discarded = true;
            usedWeighted.Add(this);
        }

        public static void Reset()
        {
            foreach (Weighted<T> weighted in usedWeighted)
            {
                weighted.weightIndex = 0;
                weighted.discarded = false;
            }
            usedWeighted.Clear();
        }
    }
}
