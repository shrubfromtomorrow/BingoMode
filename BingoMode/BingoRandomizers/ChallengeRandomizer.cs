using Expedition;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace BingoMode.BingoRandomizer
{
    public abstract class ChallengeRandomizer : Randomizer<Challenge>
    {
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
