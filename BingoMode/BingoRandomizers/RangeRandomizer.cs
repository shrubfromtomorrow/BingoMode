using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace BingoMode.BingoRandomizer
{
    public class RangeRandomizer : Randomizer<int>
    {
        public int min;
        public int max;

        public override int Random()
        {
            if (min > max) (min, max) = (max, min);
            return UnityEngine.Random.Range(min, max + 1);
        }

        public override StringBuilder Serialize(string indent)
        {
            StringBuilder serialized = base.Serialize("");
            if (!serialized.ToString().Contains("__Content__"))
                return serialized.Replace("__Type__", "Range");
            return new($"{{Range:{Name} {min}, {max}}}");
        }

        public override void Deserialize(string serialized)
        {
            Match match = Regex.Match(serialized, @"(\d+)\s*,\s*(\d+)");
            min = int.Parse(match.Groups[1].Value);
            max = int.Parse(match.Groups[2].Value);
        }
    }
}
