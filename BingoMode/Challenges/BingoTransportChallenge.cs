using System.Globalization;
using System.Text.RegularExpressions;
using Menu.Remix;
using MoreSlugcats;
using UnityEngine;
using Expedition;
using System.Collections.Generic;
using System.Linq;
using CreatureType = CreatureTemplate.Type;

namespace BingoMode.Challenges
{
    using static ChallengeHooks;
    public class BingoTransportChallenge : Challenge, IBingoChallenge
    {
        public string from;
        public string to;
        public List<EntityID> origins = [];
        public CreatureType crit;

        public override void UpdateDescription()
        {
            if (ChallengeTools.creatureNames == null)
            {
                ChallengeTools.CreatureName(ref ChallengeTools.creatureNames);
            }
            description = ChallengeTools.IGT.Translate("Transport a <crit><from><to>")
                .Replace("<crit>", ChallengeTools.creatureNames[crit.index].TrimEnd('s'))
                .Replace("<from>", from != "null" ? (to == "null" ? " out of " : " from ") + Region.GetRegionFullName(from, ExpeditionData.slugcatPlayer) : "")
                .Replace("<to>", to != "null" ? $" to {Region.GetRegionFullName(to, ExpeditionData.slugcatPlayer)}" : "");
            base.UpdateDescription();
        }

        public override bool Duplicable(Challenge challenge)
        {
            return challenge is not BingoTransportChallenge c || (crit != c.crit && (from != c.from || to != c.to));
        }

        public override string ChallengeName()
        {
            return ChallengeTools.IGT.Translate("Creature Transporting");
        }

        public override Challenge Generate()
        {
            // hate this this took me like half an hour
            SlugcatStats.Name slug = ExpeditionData.slugcatPlayer;
            CreatureType[] possible = ChallengeUtils.Transportable.Where(x => slug != MoreSlugcatsEnums.SlugcatStatsName.Saint || !new List<string>(){ "CicadaA", "CicadaB" }.Contains(x.value)).ToArray();
            CreatureType crug = possible[Random.Range(0, possible.Length - (ModManager.MSC && slug != SlugcatStats.Name.Red && slug != MoreSlugcatsEnums.SlugcatStatsName.Spear && slug != MoreSlugcatsEnums.SlugcatStatsName.Artificer ? 0 : 1))];
            List<string> origRegions = ChallengeUtils.CreatureOriginRegions(crug, slug);
            List<string> allRegions = crug == CreatureType.JetFish ? ["SB"] : [.. SlugcatStats.getSlugcatStoryRegions(slug), .. SlugcatStats.getSlugcatOptionalRegions(slug)];
            string fromage = Random.value < 0.5f ? "null" : origRegions[Random.Range(0, origRegions.Count)];
            allRegions.Remove(fromage);
            allRegions.Remove("MS");
            string toto = from == null || Random.value < 0.5f ? allRegions[Random.Range(0, allRegions.Count)] : "null";
            return new BingoTransportChallenge
            {
                from = fromage,
                to = toto,
                crit = crug,
                origins = []
            };
        }

        public void Grabbed(Creature c)
        {
            if (!completed && c.Template.type == crit)
            {
                string rr = c.room.world.region.name;
                if ((rr == from || from == "null") && !origins.Contains(c.abstractCreature.ID))
                {
                    origins.Add(c.abstractCreature.ID);
                    Plugin.logger.LogMessage($"Added {crit} with id {c.abstractCreature.ID}!");
                }
            }
        }

        public void Gated(string regionName)
        {
            Plugin.logger.LogMessage(regionName + " " + ValueConverter.ConvertToString(crit));
            Plugin.logger.LogMessage(from);
            Plugin.logger.LogMessage(to);
            Plugin.logger.LogMessage("---------");
            if (regionName != to && to != "null") return;
            Plugin.logger.LogMessage("went thru");
            bool g = false;
            for (int i = 0; i < game.Players.Count; i++)
            {
                if (game.Players[i] != null && game.Players[i].realizedCreature is Player player && player.room != null)
                {
                    foreach (var cc in player.room.updateList)
                    {
                        if (cc is Creature crib && crib.Template.type == crit && origins.Contains(crib.abstractCreature.ID))
                        {
                            Plugin.logger.LogMessage("WIN");
                            g = true;
                            break;
                        }
                    }
                    if (g) break;
                }
            }

            if (g && !completed)
            {
                CompleteChallenge();
            }
        }

        public override void Reset()
        {
            base.Reset();
            origins.Clear();
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

        // Origins later
        public override string ToString()
        {
            return string.Concat(new string[]
            {
                "Transport",
                "~",
                from,
                "><",
                to,
                "><",
                ValueConverter.ConvertToString(crit),
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
                from = array[0];
                to = array[1];
                crit = new(array[2], false);
                completed = (array[3] == "1");
                hidden = (array[4] == "1");
                revealed = (array[5] == "1");
                UpdateDescription();
            }
            catch (System.Exception ex)
            {
                ExpLog.Log("ERROR: Transport FromString() encountered an error: " + ex.Message);
            }
        }

        public void AddHooks()
        {
            On.Player.SlugcatGrab += Player_SlugcatGrab;
            On.RegionGate.NewWorldLoaded += RegionGate_NewWorldLoaded2;
        }

        public void RemoveHooks()
        {
            On.Player.SlugcatGrab -= Player_SlugcatGrab;
            On.RegionGate.NewWorldLoaded -= RegionGate_NewWorldLoaded2;
        }
    }
}