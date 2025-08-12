using BepInEx;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace BingoMode.BingoRandomizer
{
    public class FailedRandomChallengeException : Exception;

    public abstract class Randomizer<T>
    {
        /// <summary>
        /// Represents whether a randomizer is named or not, and how it should be deserialized.
        /// </summary>
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
        protected const string SUBRANDOMIZER_PATTERN = @"(?:(?'name'\w+)[-:])?(?'value'\{(?>\{(?<c>)|[^{}]+|\}(?<-c>))*(?(c)(?!))\})\s*\[?(?:\s*(?'weights'\d+)\s*,?\s*)*\]?";
        protected const string LIST_PATTERN = @"\[(?>\[(?<c>)|[^\[\]]+|\](?<-c>))*(?(c)(?!))\]";
        protected const string WEIGHTED_ITEM_PATTERN = @"([^\:\s]+)\s*:\s*(\d+)";

        /// <summary>
        /// Generate and return a new instance of <typeparamref name="T"/> according to this <c>Randomizer</c>'s rules.
        /// </summary>
        /// <returns>A new randomly generated instance of <typeparamref name="T"/>.</returns>
        public abstract T Random();

        /// <summary>
        /// Clear the list of named randomizers of type <typeparamref name="T"/>. This should be called before attempting to deserialize a profile.
        /// </summary>
        public static void ResetNamed() => NamedRandomizers.Clear();
        
        /// <summary>
        /// Serialize this randomizer and return the resulting string as a <c>StringBuilder</c>.
        /// </summary>
        /// <param name="indent">A string to insert at the beginning of each line, usually a number of whitespace.</param>
        /// <returns>A serialized version of this randomizer as a <c>StringBuilder</c>.</returns>
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

        /// <summary>
        /// Initialize the deserialization of a randomizer of expected type <typeparamref name="T"/> by : <br/>
        /// - creating a new instance matching its serialized type and calling its <c>Deserialize()</c> Method. (unnamed or new name)<br/>
        /// - creating a new instance matching its serialized type and calling the original's <c>Deserialize()</c> Method. (copy name)<br/>
        /// - copying the reference of a previously-defined randomizer. (reference name)
        /// </summary>
        /// <param name="serialized">The entire serialized string of the randomizer to deserialize</param>
        /// <returns>A fully deserialized <c>Randomizer&lt;<typeparamref name="T"/>&gt;</c></returns>
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

        /// <summary>
        /// Determine what type a name is, removes starting <c>~</c> from reference names.
        /// </summary>
        /// <param name="name">The name to get a type for</param>
        /// <returns>A <c>NameType</c> value matching the determined type of the name.</returns>
        /// <exception cref="BingoRandomProfileException"></exception>
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

        /// <summary>
        /// Deserialize <paramref name="serialized"/> into this instance of <c>Randomizer&lt;<typeparamref name="T"/>&gt;</c>.
        /// </summary>
        /// <param name="serialized">The serialized string representation of a <c>Randomizer&lt;<typeparamref name="T"/>&gt;</c>,
        /// external {} excluded.</param>
        public abstract void Deserialize(string serialized);
    }
}
