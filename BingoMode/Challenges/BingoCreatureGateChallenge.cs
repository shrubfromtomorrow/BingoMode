using BingoMode.BingoSteamworks;
using Expedition;
using Menu.Remix;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using CreatureType = CreatureTemplate.Type;

namespace BingoMode.Challenges
{
    using static ChallengeHooks;
    public class BingoCreatureGateChallenge : BingoChallenge
    {
        public SettingBox<int> amount;
        public int current;
        public SettingBox<string> crit;
        public List<string> gates = [];

        public override void UpdateDescription()
        {
            if (ChallengeTools.creatureNames == null)
            {
                ChallengeTools.CreatureName(ref ChallengeTools.creatureNames);
            }
            this.description = ChallengeTools.IGT.Translate("Transport a <crit> through [<current>/<amount>] gates")
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
            return challenge is not BingoCreatureGateChallenge;
        }

        public override string ChallengeName()
        {
            return ChallengeTools.IGT.Translate("Transporting a Creature Through Gates");
        }

        public override Challenge Generate()
        {
            return new BingoCreatureGateChallenge
            {
                amount = new(UnityEngine.Random.Range(2, 5), "Amount", 0),
                crit = new(ChallengeUtils.Transportable[UnityEngine.Random.Range(0, ChallengeUtils.Transportable.Length - (ModManager.MSC ? 0 : 1))], "Creature Type", 1, listName: "transport")
            };
        }

        public void Gate(string roomName)
        {
            bool g = false;
            for (int i = 0; i < game.Players.Count; i++)
            {
                if (game.Players[i] != null && game.Players[i].realizedCreature is Player player && player.room != null)
                {
                    /*if (player.grasps != null && player.grasps.Any(x => x != null && x.grabbed is Creature c && c.Template.type.value == crit.Value)) g = true;
                    else */if (player.objectInStomach is AbstractCreature stomacreature && stomacreature.creatureTemplate.type.value == crit.Value) g = true;
                    else if (player.room.abstractRoom.creatures.Any(x => x.creatureTemplate.type.value == crit.Value)) g = true;
                    break;
                }
            }

            if (g && !completed && !TeamsCompleted[SteamTest.team] && !hidden && !revealed && !gates.Contains(roomName))
            {
                gates.Add(roomName);
                current++;
                UpdateDescription();
                if (!RequireSave()) Expedition.Expedition.coreFile.Save(false);

                if (current >= amount.Value) CompleteChallenge();
            }
        }

        public override void Reset()
        {
            base.Reset();

            gates = [];
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
                string.Join("|", gates),
                "><",
                completed ? "1" : "0",
                "><",
                hidden ? "1" : "0",
                "><",
                revealed ? "1" : "0",
                "><",
                TeamsToString()
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
                gates = array[3].Split('|').ToList();
                completed = (array[4] == "1");
                hidden = (array[5] == "1");
                revealed = (array[6] == "1");
                TeamsFromString(array[7]);
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