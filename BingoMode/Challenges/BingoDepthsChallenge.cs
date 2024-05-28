using Expedition;
using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using CreatureType = CreatureTemplate.Type;

namespace BingoMode.Challenges
{
    using static ChallengeHooks;
    public class BingoDepthsChallenge : BingoChallenge
    {
        public SettingBox<string> crit;

        public override void UpdateDescription()
        {
            if (ChallengeTools.creatureNames == null)
            {
                ChallengeTools.CreatureName(ref ChallengeTools.creatureNames);
            }
            description = ChallengeTools.IGT.Translate("Drop a <crit> into the depths drop room")
                .Replace("<crit>", ChallengeTools.creatureNames[new CreatureType(crit.Value).Index].TrimEnd('s'));
            base.UpdateDescription();
        }

        public override bool Duplicable(Challenge challenge)
        {
            return challenge is not BingoDepthsChallenge;
        }

        public override string ChallengeName()
        {
            return ChallengeTools.IGT.Translate("Dropping a Creature in the Depth Pit");
        }

        public override Challenge Generate()
        {
            return new BingoDepthsChallenge
            {
                crit = new(UnityEngine.Random.value < 0.5f ? "Hazer" : "VultureGrub", "Creature Type", 0, listName: "depths")
            };
        }

        public override void Update()
        {
            base.Update();
            if (completed) return;
            for (int i = 0; i < game.Players.Count; i++)
            {
                if (game.Players[i] != null && game.Players[i].realizedCreature is Player player && player.room != null && player.room.abstractRoom.name.ToLowerInvariant() == "sb_d06")
                {
                    for (int j = 0; j < player.room.updateList.Count; j++)
                    {
                        if (player.room.updateList[j] is Creature c && c.Template.type.value == crit.Value && c.mainBodyChunk != null && c.mainBodyChunk.pos.y < 2266f)
                        {
                            CompleteChallenge();
                        }
                    }
                }
            }
        }

        public override int Points()
        {
            return 20;
        }

        public override bool CombatRequired()
        {
            return false;
        }

        public override bool ValidForThisSlugcat(SlugcatStats.Name slugcat)
        {
            return true;
        }

        public override string ToString()
        {
            return string.Concat(new string[]
            {
                "BingoDepthsChallenge",
                "~",
                crit.ToString(),
                "><",
                completed ? "1" : "0",
                "><",
                hidden ? "1" : "0",
                "><",
                revealed ? "1" : "0"
            });
        }

        public override void FromString(string args)
        {
            try
            {
                string[] array = Regex.Split(args, "><");
                crit = SettingBoxFromString(array[0]) as SettingBox<string>;
                completed = (array[1] == "1");
                hidden = (array[2] == "1");
                revealed = (array[3] == "1");
                UpdateDescription();
            }
            catch (Exception ex)
            {
                ExpLog.Log("ERROR: BingoDepthsChallenge FromString() encountered an error: " + ex.Message);
                throw ex;
            }
        }

        public override void AddHooks()
        {
        }

        public override void RemoveHooks()
        {
        }

        public override List<object> Settings() => [crit];
    }
}