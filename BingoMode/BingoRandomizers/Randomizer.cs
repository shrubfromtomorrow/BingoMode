using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace BingoMode.BingoRandomizer
{
    public abstract class Randomizer<T>
    {
        public string Name { get; set; }
        protected const string INDENT_INCREMENT = "  ";
        // match highest-level matching braces (and everything within)
        // see https://www.regular-expressions.info/balancing.html for more details on balancing groups
        protected const string SUBRANDOMIZER_PATTERN = @"(\{(?>\{(?<c>)|[^{}]+|\}(?<-c>))*(?(c)(?!))\})(\d*)";
        protected const string LIST_PATTERN = @"\[(?>\[(?<c>)|[^\[\]]+|\](?<-c>))*(?(c)(?!))\]";
        protected const string WEIGHTED_ITEM_PATTERN = @"([^\:\s]+)\s*:\s*(\d+)";

        public abstract T Random();

        public virtual StringBuilder Serialize(string indent)
        {
            return new StringBuilder($"{indent}{{__Type__:{Name}\n__Content__{indent}}}");
        }

        public static Randomizer<T> InitDeserialize(string serialized)
        {
            Match typeAndName = Regex.Match(serialized, @"(\w+)\s*:[ \t]*(\w*)");
            Randomizer<T> deserialized = BingoRandomizationProfile.InstanceFromString<T>(typeAndName.Groups[1].Value);
            if (typeAndName.Groups.Count > 2) deserialized.Name = typeAndName.Groups[2].Value;
            deserialized.Deserialize(serialized.Trim('{', '}'));
            return deserialized;
        }

        public abstract void Deserialize(string serialized);
    }
}
