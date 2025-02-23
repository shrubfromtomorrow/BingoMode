using BingoMode.BingoSteamworks;
using Expedition;
using Menu.Remix;
using MoreSlugcats;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text.RegularExpressions;
using CreatureType = CreatureTemplate.Type;

namespace BingoMode.BingoChallenges
{
    using static ChallengeHooks;
    public class BingoCreatureGateChallenge : BingoChallenge
    {
        public SettingBox<int> amount;
        public int current;
        public SettingBox<string> crit;
        public Dictionary<EntityID, List<string>> creatureGates = [];

        public override void UpdateDescription()
        {
            if (ChallengeTools.creatureNames == null)
            {
                ChallengeTools.CreatureName(ref ChallengeTools.creatureNames);
            }
            this.description = ChallengeTools.IGT.Translate("Transport the same <crit> through [<current>/<amount>] gates")
                .Replace("<current>", ValueConverter.ConvertToString(current))
                .Replace("<amount>", ValueConverter.ConvertToString(amount.Value))
                .Replace("<crit>", ChallengeTools.creatureNames[new CreatureType(crit.Value).Index].TrimEnd('s'));
            base.UpdateDescription();
        }

        public override Phrase ConstructPhrase()
        {
            return new Phrase([
                new Icon(ChallengeUtils.ItemOrCreatureIconName(crit.Value), 1f, ChallengeUtils.ItemOrCreatureIconColor(crit.Value)),
                new Icon("singlearrow", 1f, UnityEngine.Color.white),
                new Icon("ShortcutGate", 1f, UnityEngine.Color.white),
                new Counter(current, amount.Value)], [3]);
        }

        public override bool Duplicable(Challenge challenge)
        {
            return challenge is not BingoCreatureGateChallenge g || g.crit.Value != crit.Value;
        }

        public override string ChallengeName()
        {
            return ChallengeTools.IGT.Translate("Transporting the same creature through gates");
        }

        public override Challenge Generate()
        {
            List<string> crits = [.. ChallengeUtils.Transportable];
            if (ExpeditionData.slugcatPlayer.value != "Red") crits.Remove("JetFish");
            return new BingoCreatureGateChallenge
            {
                amount = new(UnityEngine.Random.Range(2, 5), "Amount", 0),
                crit = new(crits[UnityEngine.Random.Range(0, crits.Count - ((!ModManager.MSC || ExpeditionData.slugcatPlayer == MoreSlugcatsEnums.SlugcatStatsName.Spear || ExpeditionData.slugcatPlayer == MoreSlugcatsEnums.SlugcatStatsName.Artificer) ? 1 : 0))], "Creature Type", 1, listName: "transport")
            };
        }

        public void Gate(string roomName)
        {
            if (hidden || revealed || TeamsCompleted[SteamTest.team] || completed) return;

            List<AbstractCreature> foundCreatures = [];
            bool addedGateCreatures = false;

            for (int i = 0; i < game.Players.Count; i++)
            {
                if (game.Players[i] != null && game.Players[i].realizedCreature is Player player && player.room != null)
                {
                    if (!addedGateCreatures)
                    {
                        foundCreatures.AddRange(player.room.abstractRoom.creatures.FindAll(x => x.creatureTemplate.type.value == crit.Value));
                        addedGateCreatures = true;
                    }
                    if (player.objectInStomach is AbstractCreature stomacreature && stomacreature.creatureTemplate.type.value == crit.Value)
                    {
                        if (!foundCreatures.Contains(stomacreature)) foundCreatures.Add(stomacreature);
                    }
                }
            }

            if (foundCreatures.Count == 0) return;

            foreach (var gateCrit in foundCreatures)
            {
                EntityID id = gateCrit.ID;
                if (!creatureGates.ContainsKey(id))
                {
                   // Plugin.logger.LogMessage($"Adding {crit.Value} of ID {id} to the dictionary. Room name: {roomName}");
                    creatureGates.Add(id, [roomName]);
                }
                else
                {
                    //Plugin.logger.LogMessage($"{crit.Value} of ID {id} already in dictionary. Room name: {roomName}");
                    if (!creatureGates[id].Contains(roomName))
                    {
                        creatureGates[id].Add(roomName);
                    }
                }
            }
            foundCreatures.Sort(delegate(AbstractCreature one, AbstractCreature two)
            {
                int count1 = creatureGates[one.ID].Count;
                int count2 = creatureGates[two.ID].Count;
                return count2.CompareTo(count1);
            });
            int last = current;
            current = creatureGates[foundCreatures[0].ID].Count;
            UpdateDescription();
            if (current >= amount.Value) CompleteChallenge();
            else if (last != current) ChangeValue();
        }

        public override void Reset()
        {
            base.Reset();

            creatureGates?.Clear();
            creatureGates = [];
            current = 0;
        }

        public override int Points()
        {
            return amount.Value * 10;
        }

        public override bool CombatRequired()
        {
            return false;
        }

        public override bool ValidForThisSlugcat(SlugcatStats.Name slugcat)
        {
            return true;
        }

        public string CreatureGatesToString()
        {
            List<string> joinLater = [];

            foreach (var kvp in creatureGates)
            {
                joinLater.Add(kvp.Key.ToString() + "|" + string.Join("|", kvp.Value));
            }

            if (joinLater.Count == 0) return "empty";

            return string.Join("%", joinLater);
        }

        public Dictionary<EntityID, List<string>> CreatureGatesFromString(string from)
        {
            Dictionary<EntityID, List<string>> gates = [];

            if (from == "empty") return gates;

            string[] gateCrits = from.Split('%');
            for (int i = 0; i < gateCrits.Length; i++)
            {
                string[] split = gateCrits[i].Split('|');
                EntityID id = EntityID.FromString(split[0]);
                List<string> rooms = [];
                for (int r = 1; r < split.Length; r++)
                {
                    rooms.Add(split[r]);
                }
                gates[id] = rooms;
            }

            return gates;
        }

        public override string ToString()
        {
            return string.Concat(new string[]
            {
                "BingoCreatureGateChallenge",
                "~",
                crit.ToString(),
                "><",
                current.ToString(),
                "><",
                amount.ToString(),
                "><",
                CreatureGatesToString(),
                "><",
                completed ? "1" : "0",
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
                current = int.Parse(array[1], NumberStyles.Any, CultureInfo.InvariantCulture);
                amount = SettingBoxFromString(array[2]) as SettingBox<int>;
                creatureGates = CreatureGatesFromString(array[3]);
                completed = (array[4] == "1");
                revealed = (array[5] == "1");
                UpdateDescription();
            }
            catch (Exception ex)
            {
                ExpLog.Log("ERROR: BingoCreatureGateChallenge FromString() encountered an error: " + ex.Message);
                throw ex;
            }
        }

        public override void AddHooks()
        {
            On.RegionGate.NewWorldLoaded += RegionGate_NewWorldLoaded1;
        }

        public override void RemoveHooks()
        {
            On.RegionGate.NewWorldLoaded -= RegionGate_NewWorldLoaded1;
        }

        public override List<object> Settings() => [amount, crit];
    }
}