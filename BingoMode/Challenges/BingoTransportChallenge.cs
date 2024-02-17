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
        public SettingBox<string> from;
        public SettingBox<string> to;
        public SettingBox<string> crit;
        public List<EntityID> origins = []; // Save this later

        public override void UpdateDescription()
        {
            if (ChallengeTools.creatureNames == null)
            {
                ChallengeTools.CreatureName(ref ChallengeTools.creatureNames);
            }
            description = ChallengeTools.IGT.Translate("Transport a <crit><from><to>")
                .Replace("<crit>", ChallengeTools.creatureNames[new CreatureType(crit.Value).Index].TrimEnd('s'))
                .Replace("<from>", from.Value != "null" ? (to.Value == "null" ? " out of " : " from ") + Region.GetRegionFullName(from.Value, ExpeditionData.slugcatPlayer) : "")
                .Replace("<to>", to.Value != "null" ? $" to {Region.GetRegionFullName(to.Value, ExpeditionData.slugcatPlayer)}" : "");
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
            string[] possible = ChallengeUtils.Transportable.Where(x => slug != MoreSlugcatsEnums.SlugcatStatsName.Saint || !new List<string>(){ "CicadaA", "CicadaB" }.Contains(x)).ToArray();
            string crug = possible[Random.Range(0, possible.Length - (ModManager.MSC && slug != SlugcatStats.Name.Red && slug != MoreSlugcatsEnums.SlugcatStatsName.Spear && slug != MoreSlugcatsEnums.SlugcatStatsName.Artificer ? 0 : 1))];
            List<string> origRegions = ChallengeUtils.CreatureOriginRegions(crug, slug);
            List<string> allRegions = crug == "JetFish" ? ["SB"] : [.. SlugcatStats.getSlugcatStoryRegions(slug), .. SlugcatStats.getSlugcatOptionalRegions(slug)];
            string fromage = Random.value < 0.5f ? "null" : origRegions[Random.Range(0, origRegions.Count)];
            allRegions.Remove(fromage);
            allRegions.Remove("MS");
            string toto = fromage == "null" || Random.value < 0.5f ? allRegions[Random.Range(0, allRegions.Count)] : "null";
            return new BingoTransportChallenge
            {
                from = new(fromage, "From Region", 0),
                to = new(toto, "To Region", 1),
                crit = new(crug, "Creature Type", 2),
                origins = []
            };
        }

        public void Grabbed(Creature c)
        {
            if (!completed && c.Template.type.value == crit.Value)
            {
                string rr = c.room.world.region.name;
                if ((rr == from.Value || from.Value == "null") && !origins.Contains(c.abstractCreature.ID))
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
            if (regionName != to.Value && to.Value != "null") return;
            Plugin.logger.LogMessage("went thru");
            bool g = false;
            for (int i = 0; i < game.Players.Count; i++)
            {
                if (game.Players[i] != null && game.Players[i].realizedCreature is Player player && player.room != null)
                {
                    foreach (var cc in player.room.updateList)
                    {
                        if (cc is Creature crib && crib.Template.type.value == crit.Value && origins.Contains(crib.abstractCreature.ID))
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
                "BingoTransportChallenge",
                "~",
                from.ToString(),
                "><",
                to.ToString(),
                "><",
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
                from = SettingBoxFromString(array[0]) as SettingBox<string>;
                to = SettingBoxFromString(array[1]) as SettingBox<string>;
                crit = SettingBoxFromString(array[2]) as SettingBox<string>;
                completed = (array[3] == "1");
                hidden = (array[4] == "1");
                revealed = (array[5] == "1");
                UpdateDescription();
            }
            catch (System.Exception ex)
            {
                ExpLog.Log("ERROR: BingoTransportChallenge FromString() encountered an error: " + ex.Message);
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

        public List<object> Settings() => [from, to, crit];
    }
}