using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BingoMode.BingoRandomizer
{
    public class EmptyWeightsException : FailedRandomChallengeException;

    public class WeightRandomizer<T> : Randomizer<T>
    {
        public List<Weighted<T>> List = [];
        private int[] _cumulative_weights;

        public override T Random()
        {
            ComputeWeights();
            int index = Array.BinarySearch(_cumulative_weights, UnityEngine.Random.Range(0, _cumulative_weights.Last()));
            if (index < 0) index = ~index;
            else index++;
            while (List[index].Weight == 0)
                index++;
            List[index].Advance();
            return List[index].value;
        }

        public override StringBuilder Serialize(string indent)
        {
            string surindent = indent + INDENT_INCREMENT;
            StringBuilder serialized = base.Serialize("");
            if (!serialized.ToString().Contains("__Content__"))
                return serialized;
            serialized = new($"{{__Type__:{Name}[");
            foreach (Weighted<T> weighted in List)
            {
                serialized.Append($"{weighted.value}:{weighted.WeightsString},");
            }
            serialized.Remove(serialized.Length - 2, 1);
            serialized.Append("]}");
            return serialized;
        }

        public override void Deserialize(string serialized)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Generate a new <c>WeightRandomizer&lt;<typeparamref name="T"/>&gt;</c> with uniform
        /// weight distribution from a <c>List&lt;<typeparamref name="T"/>&gt;</c>
        /// </summary>
        /// <param name="list"></param>
        /// <returns></returns>
        public static WeightRandomizer<T> UniformFromList(List<T> list)
        {
            WeightRandomizer<T> randomizer = new();
            foreach (T item in list) randomizer.List.Add(new Weighted<T>(item, [1]));
            return randomizer;
        }

        /// <summary>
        /// Compute cumulative weights for this <c>WeightRandomizer&lt;<typeparamref name="T"/>&gt;</c>.<br/>
        /// Throw an error if all weights are 0.
        /// </summary>
        /// <exception cref="EmptyWeightsException"></exception>
        private void ComputeWeights()
        {
            _cumulative_weights = new int[List.Count];
            int total = 0;
            for (int i = 0; i < List.Count; i++)
            {
                total += List[i].Weight;
                _cumulative_weights[i] = total;
            }
            if (total == 0)
                throw new EmptyWeightsException();
        }
    }
}
