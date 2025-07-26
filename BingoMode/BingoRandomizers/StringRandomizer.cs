using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace BingoMode.BingoRandomizer
{
    public class StringRandomizer : WeightRandomizer<string>
    {
        public override StringBuilder Serialize(string indent)
        {
            return base.Serialize(indent).Replace("__Type__", "String");
        }

        public override void Deserialize(string serialized)
        {
            string stringList = Regex.Match(serialized, LIST_PATTERN).Value;
            stringList = stringList.Substring(1, stringList.Length - 2);
            foreach (Match weighted in Regex.Matches(stringList, @"([^:,]+)\s*:\s*\[?(?:\s*(\d+)\s*,?\s*)+\]?"))
            {
                int[] weights = new int[weighted.Groups[2].Captures.Count];
                for (int i = 0; i < weights.Length; i++)
                    weights[i] = int.Parse(weighted.Groups[2].Captures[i].Value);
                List.Add(new(weighted.Groups[1].Value.Trim(), weights));
            }
        }
    }
}
