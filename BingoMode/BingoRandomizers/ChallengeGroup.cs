using Expedition;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BingoMode.BingoRandomizer
{
    public class ChallengeGroup : GroupRandomizer<Challenge>
    {
        public override StringBuilder Serialize(string indent)
        {
            return base.Serialize(indent).Replace("__Type__", "ChallengeGroup");
        }
    }
}
