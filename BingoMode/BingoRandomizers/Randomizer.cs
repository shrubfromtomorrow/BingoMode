using BepInEx;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace BingoMode.BingoRandomizer
{
    public class FailedRandomChallengeException : Exception;

    public abstract class Randomizer<T>
    {
        private enum NameType
        {
            UNNAMED,
            NEW_NAME,
            REFERENCE,
            COPY,
        }
        private string _serializeString;
        private static Dictionary<string, Randomizer<T>> NamedRandomizers = [];
        public string Name { get; set; }
        protected const string INDENT_INCREMENT = "\t";
        // match highest-level matching braces (and everything within)
        // see https://www.regular-expressions.info/balancing.html for more details on balancing groups
        protected const string SUBRANDOMIZER_PATTERN = @"(?:(?'name'\w+)-)?(?'value'\{(?>\{(?<c>)|[^{}]+|\}(?<-c>))*(?(c)(?!))\})\s*\[?(?:\s*(?'weights'\d+)\s*,?\s*)*\]?";
        protected const string LIST_PATTERN = @"\[(?>\[(?<c>)|[^\[\]]+|\](?<-c>))*(?(c)(?!))\]";
        protected const string WEIGHTED_ITEM_PATTERN = @"([^\:\s]+)\s*:\s*(\d+)";

        public abstract T Random();

        public static void ResetNamed() => NamedRandomizers.Clear();
        
        public virtual StringBuilder Serialize(string indent)
        {
            string saveKey = $"{typeof(T)} {Name}";
            if (BingoRandomizationProfile.savedRandomizers.ContainsKey(saveKey))
            {
                bool refEq = ReferenceEquals(this, BingoRandomizationProfile.savedRandomizers[saveKey]);
                return new StringBuilder($"{indent}{{__Type__:{(refEq?"~":"")}{Name}}}");
            }
            else if (!Name.IsNullOrWhiteSpace())
                BingoRandomizationProfile.savedRandomizers.Add(saveKey, this);
            return new StringBuilder($"{indent}{{__Type__:{Name}\n__Content__{indent}}}");
        }

        public static Randomizer<T> InitDeserialize(string serialized)
        {
            Match typeAndName = Regex.Match(serialized, @"(\w+)\s*:[ \t]*([~a-zA-Z_]*)");
            Randomizer<T> deserialized = BingoRandomizationProfile.InstanceFromString<T>(typeAndName.Groups[1].Value);
            string name = "";
            if (typeAndName.Groups.Count > 2)
                name = typeAndName.Groups[2].Value;
            deserialized.Name = name;
            NameType type = CheckName(ref name);
            switch (type)
            {
                case NameType.UNNAMED:
                    deserialized.Deserialize(serialized.Trim('{', '}'));
                    break;
                case NameType.NEW_NAME:
                    deserialized._serializeString = serialized.Trim('{', '}');
                    deserialized.Deserialize(deserialized._serializeString);
                    NamedRandomizers.Add(name, deserialized);
                    break;
                case NameType.REFERENCE:
                    deserialized = NamedRandomizers[name];
                    break;
                case NameType.COPY:
                    deserialized.Deserialize(NamedRandomizers[name]._serializeString);
                    break;
            }
            return deserialized;
        }

        private static NameType CheckName(ref string name)
        {
            if (name.IsNullOrWhiteSpace())
                return NameType.UNNAMED;
            if (name.StartsWith("~"))
            {
                name = name.Substring(1);
                if (!NamedRandomizers.Keys.Contains(name))
                {
                    Plugin.logger.LogError($"trying to copy by reference a randomizer that isn't defined yet ({name})");
                    throw new BingoRandomProfileException();
                }
                return NameType.REFERENCE;
            }
            if (NamedRandomizers.Keys.Contains(name))
                return NameType.COPY;
            else
                return NameType.NEW_NAME;
        }

        public abstract void Deserialize(string serialized);
    }
}
