using Expedition;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace BingoMode.BingoRandomizer
{
    public abstract class ChallengeRandomizer : Randomizer<Challenge>
    {
        /// <summary>
        /// Organize serialized subrandomizers into a dictionnary, using their property name as a key.
        /// </summary>
        /// <param name="serialized">This <c>ChallengeRandomizer</c>'s serialized string, external {} excluded.</param>
        /// <returns>A dictionnary of serialized randomizers, indexed by their property name.</returns>
        protected Dictionary<string, string> ToDict(string serialized)
        {
            Dictionary<string, string> dict = [];
            MatchCollection matches = Regex.Matches(serialized, SUBRANDOMIZER_PATTERN);
            foreach (Match match in matches)
                dict.Add(match.Groups["name"].Value, match.Groups["value"].Value);
            return dict;
        }
    }
}
