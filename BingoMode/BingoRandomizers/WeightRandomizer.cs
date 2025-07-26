using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
            serialized.Append("]}");
            return serialized;
        }

        public override void Deserialize(string serialized)
        {
            throw new NotImplementedException();
        }

        public static WeightRandomizer<T> UniformFromList(List<T> list)
        {
            WeightRandomizer<T> randomizer = new();
            foreach (T item in list) randomizer.List.Add(new Weighted<T>(item, [1]));
            return randomizer;
        }

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
