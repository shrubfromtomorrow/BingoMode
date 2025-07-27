using System.Text;
using System.Text.RegularExpressions;

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
            StringBuilder serialized = base.Serialize(indent);
            if (!serialized.ToString().Contains("__Content__"))
                return serialized.Replace("__Type__", "Bool");
            return new($"{{Bool:{Name} {probability}}}");
        }

        public override void Deserialize(string serialized)
        {
            bool success = float.TryParse(Regex.Match(serialized, @"[01]\.?\d*").Value, out probability);
            if (!success) Plugin.logger.LogError($"Failed to parse decimal value in {serialized}.");
        }
    }
}
