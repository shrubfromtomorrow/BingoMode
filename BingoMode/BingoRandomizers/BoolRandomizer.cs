using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace BingoMode.BingoRandomizer
{
    public class BoolRandomizer : Randomizer<bool>
    {
        public float probability;

        public override bool Random()
        {
            return UnityEngine.Random.value < probability;
        }

        public override StringBuilder Serialize(string indent)
        {
            return new($"{{Bool:{probability}}}");
        }

        public override void Deserialize(string serialized)
        {
            bool success = float.TryParse(Regex.Match(serialized, @"0?\.\d*").Value, out probability);
            if (!success) Plugin.logger.LogError($"Failed to parse decimal value in {serialized}.");
        }
    }
}
