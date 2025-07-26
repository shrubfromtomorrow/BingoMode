﻿using BingoMode.BingoRandomizer;
using BingoMode.BingoSteamworks;
using Expedition;
using Menu.Remix;
using MoreSlugcats;
using RWCustom;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using UnityEngine;

namespace BingoMode.BingoChallenges
{
    using static ChallengeHooks;

    public class BingoEchoRandomizer : ChallengeRandomizer
    {
        public Randomizer<string> ghost;
        public Randomizer<bool> starve;
        public Randomizer<bool> specific;
        public Randomizer<int> amount;

        public override Challenge Random()
        {
            BingoEchoChallenge challenge = new();
            challenge.ghost.Value = ghost.Random();
            challenge.starve.Value = starve.Random();
            challenge.specific.Value = specific.Random();
            challenge.amount.Value = amount.Random();
            return challenge;
        }

        public override StringBuilder Serialize(string indent)
        {
            string surindent = indent + INDENT_INCREMENT;
            StringBuilder serializedContent = new();
            serializedContent.AppendLine($"{surindent}ghost-{ghost.Serialize(surindent)}");
            serializedContent.AppendLine($"{surindent}starve-{starve.Serialize(surindent)}");
            serializedContent.AppendLine($"{surindent}specific-{specific.Serialize(surindent)}");
            serializedContent.AppendLine($"{surindent}amount-{amount.Serialize(surindent)}");
            return base.Serialize(indent).Replace("__Type__", "Echo").Replace("__Content__", serializedContent.ToString());
        }

        public override void Deserialize(string serialized)
        {
            Dictionary<string, string> dict = ToDict(serialized);
            ghost = Randomizer<string>.InitDeserialize(dict["ghost"]);
            starve = Randomizer<bool>.InitDeserialize(dict["starve"]);
            specific = Randomizer<bool>.InitDeserialize(dict["specific"]);
            amount = Randomizer<int>.InitDeserialize(dict["amount"]);
        }
    }

    // Literally copied from base game, to add the starving thing easily, and to customize which echoes appear
    public class BingoEchoChallenge : BingoChallenge
    {
        public SettingBox<string> ghost; //GhostWorldPresence.GhostID
        public SettingBox<bool> starve;
        public SettingBox<bool> specific;
        public int current;
        public SettingBox<int> amount;
        public List<string> visited = [];

        public BingoEchoChallenge()
        {
            specific = new(false, "Specific Echo", 0);
            ghost = new("", "Region", 1, listName: "echoes");
            amount = new(0, "Amount", 2);
            starve = new(false, "While Starving", 3);
        }

        public override void UpdateDescription()
        {
            this.description = specific.Value ? 
                ChallengeTools.IGT.Translate("Visit the <echo_location> Echo" + (starve.Value ? " while starving" : ""))
                .Replace("<echo_location>", ChallengeTools.IGT.Translate(Region.GetRegionFullName(ghost.Value, ExpeditionData.slugcatPlayer)))
                :
                ChallengeTools.IGT.Translate("Visit <amount> Echos" + (starve.Value ? " while starving" : ""))
                .Replace("<amount>", specific.Value ? "1" : ValueConverter.ConvertToString(amount.Value));
            base.UpdateDescription();
        }

        public override Phrase ConstructPhrase()
        {
            Phrase phrase = new([[new Icon("echo_icon"), specific.Value ? new Verse(ghost.Value) : new Counter(current, amount.Value)]]);
            if (starve.Value) phrase.InsertWord(new Icon("Multiplayer_Death"), 1);
            return phrase;
        }

        public void SeeGhost(string spectre)
        {
            if (completed || revealed || TeamsCompleted[SteamTest.team] || hidden) return;
            if (specific.Value)
            {
                if (spectre != ghost.Value) return;
                UpdateDescription();
                CompleteChallenge();
            }
            else
            {
                if (visited.Contains(spectre)) return;
                current++;
                visited.Add(spectre);
                UpdateDescription();
                if (current >= amount.Value) CompleteChallenge();
                else ChangeValue();
            }
        }

        public override void Update()
        {
            base.Update();
            if (Custom.rainWorld.processManager.upcomingProcess != null) return;
            for (int i = 0; i < game.Players.Count; i++)
            {
                if (game.Players[i] != null && game.Players[i].realizedCreature is Player player && player.room != null && (!starve.Value || player.Malnourished))
                {
                    for (int j = 0; j < player.room.updateList.Count; j++)
                    {
                        if (player.room.updateList[j] is Ghost echo && game.Players[i].world.worldGhost != null && (echo.fadeOut > 0f || echo.hasRequestedShutDown))
                        {
                            SeeGhost(game.Players[i].world.worldGhost.ghostID.value);
                            return;
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
                if (ExtEnum<GhostWorldPresence.GhostID>.values.entries[i] != "NoGhost" && (!ModManager.MSC || !(ExtEnum<GhostWorldPresence.GhostID>.values.entries[i] == "MS")) && (!ModManager.MSC || !(ExtEnum<GhostWorldPresence.GhostID>.values.entries[i] == "SL") || !(ExpeditionData.slugcatPlayer != MoreSlugcatsEnums.SlugcatStatsName.Saint)) && ChallengeUtils.GetSortedCorrectListForChallenge("regionsreal").Contains(ExtEnum<GhostWorldPresence.GhostID>.values.entries[i]))
                {
                    list.Add(ExtEnum<GhostWorldPresence.GhostID>.values.entries[i]);
                }
            }
            return new BingoEchoChallenge
            {
                specific = new SettingBox<bool>(Random.value < 0.5f, "Specific Echo", 0),
                ghost = new(list[Random.Range(0, list.Count)], "Region", 1, listName: "echoes"),
                amount = new(Random.Range(2, 7), "Amount", 2),
                starve = new(Random.value < 0.1f, "While Starving", 3)
            };
        }

        public override bool RequireSave() => false;

        public override bool CombatRequired()
        {
            return false;
        }

        public override bool Duplicable(Challenge challenge)
        {
            return challenge is not BingoEchoChallenge c || c.ghost.Value != ghost.Value;
        }

        public override string ChallengeName()
        {
            return ChallengeTools.IGT.Translate("Visiting echoes");
        }

        public override string ToString()
        {
            return string.Concat(
            [
                "BingoEchoChallenge",
                "~",
                specific.ToString(),
                "><",
                ghost.ToString(),
                "><",
                starve.ToString(),
                "><",
                current.ToString(),
                "><",
                amount.ToString(),
                "><",
                completed ? "1" : "0",
                "><",
                revealed ? "1" : "0",
                "><",
                string.Join("|", visited),
            ]);
        }

        public override void FromString(string args)
        {
            try
            {
                string[] array = Regex.Split(args, "><");
                if (array.Length == 8)
                {
                    specific = SettingBoxFromString(array[0]) as SettingBox<bool>;
                    ghost = SettingBoxFromString(array[1]) as SettingBox<string>;
                    starve = SettingBoxFromString(array[2]) as SettingBox<bool>;
                    current = int.Parse(array[3], NumberStyles.Any, CultureInfo.InvariantCulture);
                    amount = SettingBoxFromString(array[4]) as SettingBox<int>;
                    completed = (array[5] == "1");
                    revealed = (array[6] == "1");
                    string[] arr = Regex.Split(array[7], "|");
                    visited = [.. arr];
                }
                // Legacy board echo challenge compatibility
                else if (array.Length == 4)
                {
                    ghost = SettingBoxFromString(array[0]) as SettingBox<string>;
                    starve = SettingBoxFromString(array[1]) as SettingBox<bool>;
                    completed = (array[2] == "1");
                    revealed = (array[3] == "1");
                    specific = SettingBoxFromString("System.Boolean|true|Specific Echo|0|NULL") as SettingBox<bool>;
                    current = 0;
                    amount = SettingBoxFromString("System.Int32|2|Amount|1|NULL") as SettingBox<int>;
                    visited = [];
                }
                UpdateDescription();
            }
            catch (System.Exception ex)
            {
                ExpLog.Log("ERROR: BingoEchoChallenge FromString() encountered an error: " + ex.Message);
                throw ex;
            }
        }

        public override void AddHooks()
        {
            On.Ghost.StartConversation += Ghost_StartConversation;
        }

        public override void RemoveHooks()
        {

            On.Ghost.StartConversation -= Ghost_StartConversation;
        }

        public override List<object> Settings() => [ghost, specific, amount, starve];
    }
}
