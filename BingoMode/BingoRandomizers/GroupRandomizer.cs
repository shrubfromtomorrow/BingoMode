using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace BingoMode.BingoRandomizer
{
    public abstract class GroupRandomizer<T> : Randomizer<T>
    {
        public List<Weighted<Randomizer<T>>> List = [];
        private int[] _cumulative_weights;

        public override T Random()
        {
            T random = default;
            while (Equals(random, default(T)))
            {
                ComputeWeights();
                int index = Array.BinarySearch(_cumulative_weights, UnityEngine.Random.Range(0, _cumulative_weights.Last()));
                if (index < 0) index = ~index;
                else index++;
                while (List[index].Weight == 0)
                    index++;
                try
                {
                    random = List[index].value.Random();
                    List[index].Advance();
                }
                catch (EmptyWeightsException)
                {
                    List[index].Discard();
                }
            }
            return random;
        }

        public override StringBuilder Serialize(string indent)
        {
            string surindent = indent + INDENT_INCREMENT;
            StringBuilder serializedContent = new();
            foreach (Weighted<Randomizer<T>> randomizer in List)
            {
                serializedContent.AppendLine($"{randomizer.value.Serialize(surindent)}{randomizer.WeightsString}");
            }
            return base.Serialize(indent).Replace("__Content__", serializedContent.ToString());
        }

        public override void Deserialize(string serialized)
        {
            MatchCollection matches = Regex.Matches(serialized, SUBRANDOMIZER_PATTERN);
            foreach (Match match in matches)
            {
                int[] weights = new int[match.Groups[2].Captures.Count];
                for (int i = 0; i < weights.Length; i++)
                    weights[i] = int.Parse(match.Groups[2].Captures[i].Value);
                List.Add(new(InitDeserialize(match.Groups[1].Value), weights));
            }
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
