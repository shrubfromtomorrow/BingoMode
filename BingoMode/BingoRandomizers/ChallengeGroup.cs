using Expedition;
using System.Text;

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
