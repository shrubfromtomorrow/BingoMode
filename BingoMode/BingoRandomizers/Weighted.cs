using System.Collections.Generic;
using System.Linq;

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

        /// <summary>
        /// Advance this <c>Weighted&lt;<typeparamref name="T"/>&gt;</c>'s weight index and mark it for reset.
        /// </summary>
        public void Advance()
        {
            if (weightIndex + 1 < weights.Length)
            {
                weightIndex++;
                usedWeighted.Add(this);
            }
        }

        /// <summary>
        /// Override this <c>Weighted&lt;<typeparamref name="T"/>&gt;</c>'s <c>Weight</c> to 0 and mark it for reset.
        /// </summary>
        public void Discard()
        {
            discarded = true;
            usedWeighted.Add(this);
        }

        /// <summary>
        /// Reset all <c>Weighted&lt;<typeparamref name="T"/>&gt;</c> marked for reset and unmark them.
        /// </summary>
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
