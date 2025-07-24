using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BingoMode.BingoRandomizer
{
    public class WeightRandomizer<T> : Randomizer<T>
    {
        private List<Weighted<T>> _list = [];
        public List<Weighted<T>> List
        {
            get
            {
                _is_weights_computed = false;
                return _list;
            }
        }
        private bool _is_weights_computed;
        private int[] _cumulative_weights;

        public override T Random()
        {
            if (!_is_weights_computed) ComputeWeights();
            int index = Array.BinarySearch(_cumulative_weights, UnityEngine.Random.Range(0, _cumulative_weights.Last()));
            if (index < 0) index = ~index;
            else index++;
            return _list[index].value;
        }

        public override StringBuilder Serialize(string indent)
        {
            string surindent = indent + INDENT_INCREMENT;
            StringBuilder serialized = base.Serialize("");
            if (!serialized.ToString().Contains("__Content__"))
                return serialized;
            serialized = new($"{{__Type__:{Name}[");
            foreach (Weighted<T> weighted in _list)
            {
                serialized.Append($"{weighted.value}:{weighted.weight},");
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
            foreach (T item in list) randomizer._list.Add(new Weighted<T>(item, 1));
            return randomizer;
        }

        private void ComputeWeights()
        {
            _cumulative_weights = new int[_list.Count];
            int total = 0;
            for (int i = 0; i < _list.Count; i++)
            {
                total += _list[i].weight;
                _cumulative_weights[i] = total;
            }
            _is_weights_computed = true;
        }
    }
}
