using System.Globalization;
using System.Text.RegularExpressions;
using Menu.Remix;
using MoreSlugcats;
using UnityEngine;
using Expedition;
using System.Collections.Generic;
using System.Linq;

namespace BingoMode.Challenges
{
    // Literally copied from base game, to add the starving thing easily, and to customize which echoes appear
    public class BingoEchoChallenge : Challenge, IBingoChallenge
    {
        public GhostWorldPresence.GhostID ghost;
        public bool starve;

        public override void UpdateDescription()
        {
            description = ChallengeTools.IGT.Translate("Visit the <echo_location> Echo" + (starve ? " while starving" : "")).Replace("<echo_location>", ChallengeTools.IGT.Translate(Region.GetRegionFullName(ghost.value, ExpeditionData.slugcatPlayer)));
            base.UpdateDescription();
        }

        public override void Update()
        {
            base.Update();
            for (int i = 0; i < game.Players.Count; i++)
            {
                if (game.Players[i] != null && game.Players[i].realizedCreature is Player player && player.room != null && (!starve || player.Malnourished))
                {
                    for (int j = 0; j < player.room.updateList.Count; j++)
                    {
                        if (player.room.updateList[j] is Ghost && game.Players[i].world.worldGhost != null && (player.room.updateList[j] as Ghost).onScreenCounter > 30 && game.Players[i].world.worldGhost.ghostID.value == ghost.value)
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

        public override Challenge Generate()
        {
            List<string> list = [];
            for (int i = 0; i < ExtEnum<GhostWorldPresence.GhostID>.values.entries.Count; i++)
            {
                if (ExtEnum<GhostWorldPresence.GhostID>.values.entries[i] != "NoGhost" && (!ModManager.MSC || !(ExtEnum<GhostWorldPresence.GhostID>.values.entries[i] == "MS")) && (!ModManager.MSC || !(ExtEnum<GhostWorldPresence.GhostID>.values.entries[i] == "SL") || !(ExpeditionData.slugcatPlayer != MoreSlugcatsEnums.SlugcatStatsName.Saint)) && SlugcatStats.getSlugcatStoryRegions(ExpeditionData.slugcatPlayer).Contains(ExtEnum<GhostWorldPresence.GhostID>.values.entries[i]))
                {
                    list.Add(ExtEnum<GhostWorldPresence.GhostID>.values.entries[i]);
                }
            }
            return new BingoEchoChallenge
            {
                ghost = new(list[Random.Range(0, list.Count)], false),
                starve = Random.value < 0.5f
            };
        }

        public override bool CombatRequired()
        {
            return false;
        }

        public override bool Duplicable(Challenge challenge)
        {
            return challenge is not BingoEchoChallenge c || c.ghost.value != ghost.value;
        }

        public override string ChallengeName()
        {
            return ChallengeTools.IGT.Translate("Echo Bingoing");
        }

        public override string ToString()
        {
            return string.Concat(
            [
                "BingoEchoChallenge",
                "~",
                ValueConverter.ConvertToString(this.ghost),
                "><",
                starve ? "1" : "0",
                "><",
                completed ? "1" : "0",
                "><",
                hidden ? "1" : "0",
                "><",
                revealed ? "1" : "0"
            ]);
        }

        public override void FromString(string args)
        {
            try
            {
                string[] array = Regex.Split(args, "><");
                ghost = new GhostWorldPresence.GhostID(array[0], false);
                starve = (array[1] == "1");
                completed = (array[2] == "1");
                hidden = (array[3] == "1");
                revealed = (array[4] == "1");
                UpdateDescription();
            }
            catch (System.Exception ex)
            {
                ExpLog.Log("ERROR: BingoEchoChallenge FromString() encountered an error: " + ex.Message);
            }
        }

        public void AddHooks()
        {
        }

        public void RemoveHooks()
        {
        }
    }
}
